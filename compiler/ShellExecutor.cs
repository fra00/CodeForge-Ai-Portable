using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Fornisce metodi per eseguire comandi shell in modo asincrono, sia in background (catturando l'output)
/// che in una finestra separata (visibile, senza catturare l'output).
/// </summary>
public static class ShellExecutor
{
    /// <summary>
    /// Esegue un comando shell in background e restituisce l'output combinato di stdout e stderr.
    /// Non crea una finestra e reindirizza l'output.
    /// </summary>
    /// <param name="command">Il comando da eseguire (es. "dotnet", "cmake").</param>
    /// <param name="args">Gli argomenti del comando (es. "build", "--build .").</param>
    /// <param name="workingDirectory">La directory in cui eseguire il comando. Se null, usa la directory corrente del processo.</param>
    /// <returns>L'output combinato del comando (stdout + stderr).</returns>
    public static async Task<string> ExecuteCommandAsync(string command, string args, string workingDirectory = null)
    {
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        using (var process = new Process())
        {
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false; // Non usare la shell per l'esecuzione
            process.StartInfo.RedirectStandardOutput = true; // Reindirizza stdout
            process.StartInfo.RedirectStandardError = true;  // Reindirizza stderr
            process.StartInfo.CreateNoWindow = true;         // Non creare una finestra per il processo

            if (!string.IsNullOrWhiteSpace(workingDirectory) && Directory.Exists(workingDirectory))
            {
                process.StartInfo.WorkingDirectory = workingDirectory;
                Console.WriteLine($"Executing (headless) '{command} {args}' in '{workingDirectory}'");
            }
            else
            {
                Console.WriteLine($"Executing (headless) '{command} {args}' in current directory.");
            }

            // Aggiungi handler per l'output asincrono
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    outputBuilder.AppendLine(e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    errorBuilder.AppendLine(e.Data);
                }
            };

            process.Start();

            // Inizia a leggere l'output asincronamente
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Attendi la fine del processo
            await process.WaitForExitAsync();

            // Combina l'output
            string output = outputBuilder.ToString().Trim();
            string error = errorBuilder.ToString().Trim();

            if (!string.IsNullOrEmpty(error))
            {
                return $"Error executing command:\n{error}\nOutput:\n{output}";
            }

            return output;
        }
    }

    /// <summary>
    /// Esegue un comando shell in una finestra separata e visibile. L'output non viene catturato dall'applicazione.
    /// </summary>
    /// <param name="command">Il comando da eseguire (es. "notepad", "my_app.exe").</param>
    /// <param name="args">Gli argomenti del comando.</param>
    /// <param name="workingDirectory">La directory in cui eseguire il comando.</param>
    /// <returns>Un Task che si completa quando il processo esterno termina.</returns>
    public static async Task ExecuteVisibleCommandAsync(string command, string args, string workingDirectory = null)
    {
        using (var process = new Process())
        {
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = true; // Usa la shell per l'esecuzione, consentendo una finestra visibile
            process.StartInfo.CreateNoWindow = false; // Crea una finestra visibile per il processo
            // NON reindirizzare l'output/errore quando UseShellExecute è true, altrimenti lancerà un'eccezione.
            // L'output sarà visibile nella finestra della shell creata.

            if (!string.IsNullOrWhiteSpace(workingDirectory) && Directory.Exists(workingDirectory))
            {
                process.StartInfo.WorkingDirectory = workingDirectory;
                Console.WriteLine($"Executing (visible) '{command} {args}' in '{workingDirectory}'");
            }
            else
            {
                Console.WriteLine($"Executing (visible) '{command} {args}' in current directory.");
            }

            process.Start();
            await process.WaitForExitAsync();
        }
    }
}