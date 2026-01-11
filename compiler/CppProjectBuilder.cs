using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Implementazione di IProjectBuilder per progetti C++.
/// Questo esempio assume un processo di compilazione basato su CMake.
/// Potrebbe essere adattato per MSBuild (Windows) o Makefiles a seconda delle esigenze.
/// </summary>
public class CppProjectBuilder : IProjectBuilder
{
    /// <summary>
    /// Compila il progetto C++ situato al percorso specificato e restituisce l'output della shell.
    /// </summary>
    /// <param name="projectPath">Il percorso della directory del progetto da compilare.</param>
    /// <returns>L'output della shell generato dal comando di compilazione.</returns>
    public async Task<string> Build(string projectPath)
    {
        Console.WriteLine($"Building C++ project at: {projectPath}");

        // Passaggio 1: Configura CMake per generare i file di build
        // Assumiamo che il progetto contenga un CMakeLists.txt nella root.
        Console.WriteLine("C++ CMake configuration started...");
        var configureResult = await ShellExecutor.ExecuteCommandAsync("cmake", ".", projectPath);
        if (configureResult.Contains("Error") || configureResult.Contains("error:"))
        {
            Console.WriteLine("C++ CMake Configure Output (Error suspected):");
            Console.WriteLine(configureResult);
            return $"CMake configuration failed:\n{configureResult}";
        }
        Console.WriteLine("C++ CMake Configure Output:");
        Console.WriteLine(configureResult);

        // Passaggio 2: Compila il progetto usando i file generati da CMake
        Console.WriteLine("C++ CMake build started...");
        var buildResult = await ShellExecutor.ExecuteCommandAsync("cmake", "--build .", projectPath);
        Console.WriteLine("C++ Build Output:");
        Console.WriteLine(buildResult);
        return buildResult;
    }

    /// <summary>
    /// Esegue il progetto C++ situato al percorso specificato in una shell visibile. L'output non viene catturato.
    /// Questo metodo assume una struttura di output CMake standard (es. build/Debug/nome_eseguibile o build/nome_eseguibile).
    /// Potrebbe richiedere personalizzazione a seconda della configurazione specifica del progetto CMake.
    /// </summary>
    /// <param name="projectPath">Il percorso della directory del progetto da eseguire.</param>
    /// <returns>Un Task che si completa quando il processo di esecuzione esterno termina.</returns>
    public async Task Run(string projectPath)
    {
        Console.WriteLine($"Running C++ project at: {projectPath} in a visible shell.");

        // Assumiamo una directory di build standard per CMake.
        // Spesso è 'build' all'interno della root del progetto.
        string buildDirectory = Path.Combine(projectPath, "build");

        // Tentiamo di trovare l'eseguibile. Questo è un punto che potrebbe necessitare di maggiore intelligenza
        // a seconda della complessità del progetto CMake (es. target multipli, nomi dinamici).
        // Per semplicità, cerchiamo un eseguibile generico o assumiamo un nome standard.
        // In un caso reale, potremmo leggere il CMakeLists.txt o configurare il nome dell'eseguibile.
        string executableName = "my_cpp_app"; // Placeholder: questo dovrebbe essere determinato dal progetto CMake.
                                              // Ad esempio, potremmo cercare file *.exe (Windows) o file senza estensione (Linux/macOS)
                                              // all'interno delle sottodirectory di build (Debug, Release).
        string executablePath = null;

        if (Directory.Exists(buildDirectory))
        {
            // Tentiamo di trovare l'eseguibile nella build/Debug o build/Release (Windows)
            // o direttamente in build/ (Linux/macOS)
            string[] possiblePaths = {
                Path.Combine(buildDirectory, "Debug", executableName + ".exe"), // Windows Debug
                Path.Combine(buildDirectory, "Release", executableName + ".exe"), // Windows Release
                Path.Combine(buildDirectory, executableName), // Linux/macOS
                Path.Combine(buildDirectory, executableName + ".out") // Altri casi Linux
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    executablePath = path;
                    break;
                }
            }

            // Se non trovato con il nome placeholder, cerchiamo un eseguibile generico nel root della build o in Debug/Release
            if (executablePath == null)
            {
                var execs = Directory.GetFiles(buildDirectory, "*.exe", SearchOption.AllDirectories); // Windows
                if (execs.Length > 0) executablePath = execs[0];
                else
                {
                    execs = Directory.GetFiles(buildDirectory, "*", SearchOption.AllDirectories); // Linux/macOS (no extension)
                    foreach (var file in execs)
                    {
                        // Very basic check for potential executables (e.g., non-library, non-object files)
                        // This is highly heuristic and might fail in complex scenarios.
                        // A better approach would be to parse CMake build output or have explicit configuration.
                        if (!file.EndsWith(".obj") && !file.EndsWith(".lib") && !file.EndsWith(".dll") &&
                            !file.EndsWith(".so") && !file.EndsWith(".dylib") && !Path.GetFileName(file).Contains("."))
                        {
                            // Check if it's executable (on Unix-like systems)
                            // This is hard to do reliably cross-platform from C#.
                            // For now, we just pick the first likely candidate.
                            executablePath = file;
                            break;
                        }
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(executablePath) || !File.Exists(executablePath))
        {
            Console.WriteLine($"Error: C++ executable not found in '{buildDirectory}'. " +
                              "Please ensure the project builds successfully and the executable path is correct. " +
                              "Consider configuring the exact executable name or path if necessary.");
            // Poiché Run è Task (void), non possiamo restituire una stringa di errore. Logghiamo l'errore.
            return; // Completa il Task senza eseguire il comando.
        }

        Console.WriteLine($"Executing C++ application: {executablePath} in a visible shell.");
        // Esegue l'eseguibile in una shell visibile.
        await ShellExecutor.ExecuteVisibleCommandAsync(executablePath, "", Path.GetDirectoryName(executablePath));
        // L'output sarà visibile nella console separata, non catturato qui.
    }
}