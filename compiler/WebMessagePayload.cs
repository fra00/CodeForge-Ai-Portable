using System.Text.Json.Serialization;

public class WebMessagePayload
{
    [JsonPropertyName("action")]
    public string Action { get; set; }

    [JsonPropertyName("environment")]
    public string Environment { get; set; }

    // Contiene lo ZIP del progetto codificato in Base64
    [JsonPropertyName("payload")]
    public string Payload { get; set; }

    /// <summary>
    /// Configurazione specifica per progetti Arduino, se l'ambiente è "arduino".
    /// </summary>
    [JsonPropertyName("arduinoConfig")]
    public ArduinoConfig ArduinoConfig { get; set; }
}

/// <summary>
/// Contiene le configurazioni necessarie per i progetti Arduino (es. board e porta seriale).
/// </summary>
public class ArduinoConfig
{
    [JsonPropertyName("board")]
    public string Board { get; set; } // Es. "arduino:avr:uno"

    [JsonPropertyName("port")]
    public string Port { get; set; } // Es. "/dev/ttyACM0" o "COM3"
}