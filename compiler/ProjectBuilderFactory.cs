using System;

/// <summary>
/// Factory statica per ottenere l'implementazione di IProjectBuilder corretta
/// in base all'ambiente specificato.
/// </summary>
public static class ProjectBuilderFactory
{
    public static IProjectBuilder GetBuilder(string environment, WebMessagePayload payload = null)
    {
        return environment.ToLowerInvariant() switch
        {
            "csharp" => new CSharpProjectBuilder(),
            "cpp" => new CppProjectBuilder(),
            "arduino" => new ArduinoProjectBuilder(payload.ArduinoConfig),
            "esp32" => new ArduinoProjectBuilder(payload.ArduinoConfig), // Added support for ESP32 using the same builder
            _ => throw new ArgumentException($"Unsupported environment: {environment}"),
        };
    }
}