using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Diagnostics; // Required for Process, though ShellExecutor abstracts it
using System.Threading.Tasks;

// Assicurati che WebMessagePayload sia definito o referenziato in questo contesto.
// Se non è già in un file separato, includilo qui o nel file in cui è definito.
// Per questo esempio, assumiamo che sia già definito in WebMessagePayload.cs
// public class WebMessagePayload { /* ... content from previous response ... */ }

/// <summary>
/// Classe principale per elaborare il payload del messaggio web,
/// decomprimere il progetto, compilarlo e restituire l'output.
/// </summary>
public class ProjectProcessor
{
    private readonly string _baseTempDirectory;

    public ProjectProcessor()
    {
        // Definisce una directory temporanea base all'interno della directory di base dell'applicazione.
        // Ciò assicura che le cartelle temporanee siano "all'interno del progetto corrente"
        // in un percorso gestibile e noto all'applicazione.
        _baseTempDirectory = Path.Combine(AppContext.BaseDirectory, "tmp_projects");
        Directory.CreateDirectory(_baseTempDirectory); // Assicurati che la directory base esista
    }

    /// <summary>
    /// Elabora un payload di messaggio web: decodifica, decomprime, compila e pulisce.
    /// </summary>
    /// <param name="payload">Il payload contenente l'azione, l'ambiente e il payload Base64 dello zip.</param>
    /// <returns>L'output della shell del processo di compilazione/esecuzione.</returns>
    /// <exception cref="ArgumentException">Se il payload o l'ambiente non sono validi.</exception>
    /// <exception cref="FormatException">Se la stringa Base64 è malformata.</exception>
    public async Task<string> ProcessPayload(WebMessagePayload payload)
    {
        if (string.IsNullOrEmpty(payload.Payload))
        {
            throw new ArgumentException("Payload cannot be empty.", nameof(payload.Payload));
        }
        if (string.IsNullOrEmpty(payload.Environment))
        {
            throw new ArgumentException("Environment cannot be empty.", nameof(payload.Environment));
        }
        if (string.IsNullOrEmpty(payload.Action))
        {
            throw new ArgumentException("Action cannot be empty.", nameof(payload.Action));
        }

        // 1. Converte il Payload da Base64 a binario (byte array)
        byte[] zipBytes;
        try
        {
            zipBytes = Convert.FromBase64String(payload.Payload);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Invalid Base64 string in payload.", nameof(payload.Payload), ex);
        }

        // 2. Crea una cartella temporanea univoca all'interno della directory _baseTempDirectory
        string uniqueProjectDir = Path.Combine(_baseTempDirectory, $"proj_{Guid.NewGuid().ToString("N")}");
        Console.WriteLine($"Creating unique temporary directory: {uniqueProjectDir}");
        Directory.CreateDirectory(uniqueProjectDir);

        try
        {
            // 3. Decomprime lo zip binario nella cartella temporanea
            await using (var memoryStream = new MemoryStream(zipBytes))
            {
                using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
                archive.ExtractToDirectory(uniqueProjectDir, true); // Sovrascrive file esistenti se necessario
            }
            Console.WriteLine($"Project successfully unzipped to: {uniqueProjectDir}");

            // 4. Utilizza la factory per ottenere il builder corretto in base all'ambiente
            var builder = ProjectBuilderFactory.GetBuilder(payload.Environment,payload);

            // 5. Esegue il comando di compilazione tramite il builder
            string buildOutput = await builder.Build(uniqueProjectDir);
            StringBuilder finalOutput = new StringBuilder($"--- Build Output ---\n{buildOutput}\n");

            // 6. Se l'azione richiede l'esecuzione, esegue il progetto
            if (payload.Action.Equals("run", StringComparison.OrdinalIgnoreCase) ||
                payload.Action.Equals("build_and_run", StringComparison.OrdinalIgnoreCase))
            {
                if (buildOutput.Contains("error:", StringComparison.OrdinalIgnoreCase) ||
                    buildOutput.Contains("failed", StringComparison.OrdinalIgnoreCase) && !payload.Environment.Equals("cpp", StringComparison.OrdinalIgnoreCase)) // C++ CMake configure can have "warnings"
                {
                    // If build failed, don't attempt to run.
                    finalOutput.AppendLine("\n--- Run Skipped ---");
                    finalOutput.AppendLine("Project build failed, skipping execution.");
                }
                else
                {
                    finalOutput.AppendLine("\n--- Run Initiated ---");
                    finalOutput.AppendLine("Project execution started in a separate, visible shell.");
                    finalOutput.AppendLine("Output will not be captured by this application.");

                    // The Run method now executes in a visible shell and does not return output.
                    await builder.Run(uniqueProjectDir);
                }
            }

            return finalOutput.ToString();
        }
        finally
        {
            // Pulisce la cartella temporanea alla fine dell'elaborazione (anche in caso di errori)
            if (Directory.Exists(uniqueProjectDir))
            {
                try
                {
                    // Elimina la directory ricorsivamente
                    Directory.Delete(uniqueProjectDir, true);
                    Console.WriteLine($"Cleaned up temporary directory: {uniqueProjectDir}");
                }
                catch (IOException ex)
                {
                    // Registra un avviso se non è possibile eliminare la directory
                    Console.WriteLine($"Warning: Could not delete temporary directory {uniqueProjectDir}. {ex.Message}");
                }
            }
        }
    }
}