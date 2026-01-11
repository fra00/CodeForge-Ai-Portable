using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class ArduinoProjectBuilder : IProjectBuilder
{
    private readonly ArduinoConfig _config;

    public ArduinoProjectBuilder(ArduinoConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config), "Arduino configuration is required.");
    }

    public async Task<string> Build(string projectPath)
    {
        Console.WriteLine($"[BUILD] Starting Arduino build at: {projectPath}");

        if (string.IsNullOrEmpty(_config.Board))
            return "Error: Arduino board not specified.";

        // 1. Prepara lo sketch rinominando il file principale per combaciare con la cartella
        string sketchFilePath = PrepareSketch(projectPath);

        if (sketchFilePath == null)
            return $"Error: No .ino file found in {projectPath}.";

        // 2. Esegui il comando
        string arguments = $"compile --fqbn {_config.Board} \"{sketchFilePath}\"";

        Console.WriteLine($"[EXEC] arduino-cli {arguments}");
        var result = await ShellExecutor.ExecuteCommandAsync("arduino-cli", arguments, projectPath);

        Console.WriteLine("--- Build Output ---");
        Console.WriteLine(result);
        return result;
    }

    public async Task Run(string projectPath)
    {
        Console.WriteLine($"[UPLOAD] Uploading to {_config.Board}...");

        string sketchFilePath = PrepareSketch(projectPath);
        if (sketchFilePath == null) return;

        // Costruiamo il comando arduino-cli
        string arduinoArgs = $"upload -p \"{_config.Port}\" --fqbn {_config.Board} \"{sketchFilePath}\"";

        // MODIFICA QUI: Invochiamo cmd.exe per evitare l'errore Win32 "Operazione annullata"
        // /c esegue il comando e poi chiude la finestra. 
        // Se vuoi che la finestra resti aperta per vedere l'esito, usa /k invece di /c.
        string cmdArguments = $"/c arduino-cli {arduinoArgs} & pause";

        try
        {
            Console.WriteLine($"[EXEC] Opening terminal for upload...");

            // Usiamo "cmd.exe" come comando principale, passando arduino-cli negli argomenti
            await ShellExecutor.ExecuteVisibleCommandAsync("cmd.exe", cmdArguments, projectPath);

            Console.WriteLine("[COMPLETED] Arduino Upload process finished.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Errore durante l'upload visibile: {ex.Message}");
        }
    }

    /// <summary>
    /// Trova il file .ino e lo rinomina affinché abbia lo stesso nome della cartella padre.
    /// Questo è fondamentale per arduino-cli.
    /// </summary>
    private string PrepareSketch(string projectRootPath)
    {
        // Trova il primo file .ino (cerca ovunque nel progetto)
        var inoFile = Directory.GetFiles(projectRootPath, "*.ino", SearchOption.AllDirectories).FirstOrDefault();

        if (inoFile == null) return null;

        // Ottieni info sulla cartella che lo contiene
        DirectoryInfo dirInfo = new DirectoryInfo(Path.GetDirectoryName(inoFile));
        string folderName = dirInfo.Name;

        // Costruisci il percorso desiderato: cartella/cartella.ino
        string expectedFileName = folderName + ".ino";
        string expectedFilePath = Path.Combine(dirInfo.FullName, expectedFileName);

        // Se il file non ha già il nome corretto, lo rinominiamo
        if (!inoFile.Equals(expectedFilePath, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                Console.WriteLine($"[FIX] Renaming {Path.GetFileName(inoFile)} to {expectedFileName}");
                File.Move(inoFile, expectedFilePath);
                return expectedFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to rename file: {ex.Message}");
                return inoFile; // Proviamo comunque col vecchio nome
            }
        }

        return inoFile;
    }
}