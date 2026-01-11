using System;
using System.Threading.Tasks;

/// <summary>
/// Implementazione di IProjectBuilder per progetti C#.
/// Utilizza il comando 'dotnet build' e 'dotnet run'.
/// </summary>
public class CSharpProjectBuilder : IProjectBuilder
{
    /// <summary>
    /// Compila il progetto C# situato al percorso specificato e restituisce l'output della shell.
    /// </summary>
    /// <param name="projectPath">Il percorso della directory del progetto da compilare.</param>
    /// <returns>L'output della shell generato dal comando di compilazione.</returns>
    public async Task<string> Build(string projectPath)
    {
        Console.WriteLine($"Building C# project at: {projectPath}");
        // Esegue 'dotnet build' nella directory del progetto.
        var result = await ShellExecutor.ExecuteCommandAsync("dotnet", "build", projectPath);
        Console.WriteLine("C# Build Output:");
        Console.WriteLine(result);
        return result;
    }

    /// <summary>
    /// Esegue il progetto C# situato al percorso specificato e restituisce l'output della shell.
    /// </summary>
    /// <param name="projectPath">Il percorso della directory del progetto da eseguire.</param>
    /// <returns>L'output della shell generato dal comando di esecuzione.</returns>
    public async Task Run(string projectPath)
    {
        Console.WriteLine($"Running C# project at: {projectPath} in a visible shell.");
        // Esegue 'dotnet run' nella directory del progetto in una shell visibile.
        await ShellExecutor.ExecuteVisibleCommandAsync("dotnet", "run", projectPath);
        // L'output sarà visibile nella console separata, non catturato qui.
    }
}