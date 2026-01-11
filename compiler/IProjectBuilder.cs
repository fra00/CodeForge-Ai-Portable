using System.Threading.Tasks;

/// <summary>
/// Interfaccia per la logica di compilazione specifica per ogni tipo di progetto.
/// </summary>
public interface IProjectBuilder
{
    /// <summary>
    /// Compila il progetto situato al percorso specificato e restituisce l'output della shell.
    /// </summary>
    /// <param name="projectPath">Il percorso della directory del progetto da compilare.</param>
    /// <returns>L'output della shell generato dal comando di compilazione.</returns>
    Task<string> Build(string projectPath);

    /// <summary>
    /// Esegue il progetto situato al percorso specificato in una shell visibile. L'output non viene catturato.
    /// </summary>
    /// <param name="projectPath">Il percorso della directory del progetto da eseguire.</param>
    /// <returns>Un Task che si completa quando il processo di esecuzione esterno termina.</returns>
    Task Run(string projectPath);
}