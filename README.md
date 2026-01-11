# CodeForgePortable - Project Processor

## 🚀 Overview

CodeForgePortable is a robust .NET C# application designed to serve as a versatile backend for processing, compiling, and conditionally executing various software projects. It acts as an orchestration layer, receiving project payloads (Base64-encoded ZIPs), extracting them, and then delegating compilation and execution tasks to environment-specific builders using external command-line tools. This system is ideal for scenarios requiring dynamic project processing, such as online IDEs, code sandboxes, or automated build pipelines for diverse programming languages and embedded platforms.

## ✨ Features

*   **Dynamic Project Processing:** Accepts Base64-encoded ZIP archives containing project source code.
*   **Environment-Agnostic Compilation:** Supports various programming environments:
    *   **C#:** Uses `dotnet CLI` for building and running.
    *   **C++:** Leverages `CMake` for configuring and building, with basic executable detection for running.
    *   **Arduino:** Utilizes `arduino-cli` for compiling and uploading sketches to Arduino boards.
    *   **ESP32:** Leverages `arduino-cli` for compiling and uploading sketches to ESP32 devices (requires ESP32 core installed for `arduino-cli`).
*   **Modular Architecture:** Easily extensible to add support for new programming languages or build systems.
*   **Asynchronous Operations:** Fully leverages `async`/`await` for non-blocking I/O operations, ensuring responsiveness.
*   **Flexible Shell Execution:**
    *   **Headless Execution:** Captures standard output and error for build processes.
    *   **Visible Execution:** Launches processes in a new, visible shell window for interactive execution or device uploads (e.g., Arduino/ESP32 upload progress).
*   **Automated Cleanup:** Ensures temporary project directories are removed after processing, regardless of success or failure.

## 🏗️ Architecture

The project is structured around key object-oriented design patterns to ensure modularity, flexibility, and extensibility:

*   **Strategy Pattern (`IProjectBuilder`):** Defines a common interface (`Build`, `Run`) for all project types. Concrete implementations (e.g., `CSharpProjectBuilder`, `CppProjectBuilder`, `ArduinoProjectBuilder`) encapsulate the specific logic for each environment.
*   **Factory Method Pattern (`ProjectBuilderFactory`):** Provides a static method to instantiate the correct `IProjectBuilder` implementation based on the requested environment, decoupling the client (`ProjectProcessor`) from concrete builder types.
*   **Orchestrator / Facade (`ProjectProcessor`):** Acts as the primary entry point, orchestrating the entire workflow from payload reception and decompression to delegating build/run tasks and managing cleanup.
*   **Shell Abstraction (`ShellExecutor`):** A static utility class that abstracts the complexities of executing external shell commands, offering both background (output captured) and visible (output displayed to user) execution modes.

## 🛠️ Technologies Used

*   **Language:** C#
*   **Framework:** .NET 8
*   **External CLIs:**
    *   `.NET SDK` (for C# projects)
    *   `arduino-cli` (for Arduino/ESP32 projects)
    *   `CMake` (for C++ projects)
*   **Libraries:** `System.IO`, `System.IO.Compression`, `System.Diagnostics`
*   **Data Serialization:** `System.Text.Json` (for `WebMessagePayload`)

## ⚡ Prerequisites

To compile and run projects using CodeForgePortable, the following external tools must be installed and accessible in the system's PATH:

*   **.NET SDK 8.0 or higher:** Required for building and running the CodeForgePortable application itself, and for C# projects.
    *   [Download .NET SDK](https://dotnet.microsoft.com/download)
*   **Arduino CLI:** Essential for compiling and uploading Arduino and ESP32 sketches.
    *   [Install arduino-cli](https://arduino.github.io/arduino-cli/latest/installation/)
    *   **For ESP32 support:** Ensure the ESP32 core is installed via `arduino-cli`. E.g., `arduino-cli core update-index && arduino-cli core install esp32:esp32`.
*   **CMake:** Required for configuring and building C++ projects.
    *   [Download CMake](https://cmake.org/download/)

## 🚀 How to Use

The core entry point for processing projects is the `ProjectProcessor` class.

### `WebMessagePayload` Structure

The system expects an input object conforming to the `WebMessagePayload` structure:

```csharp
public class WebMessagePayload
{
    public string Action { get; set; } // e.g., "build", "run", "build_and_run"
    public string Environment { get; set; } // e.g., "csharp", "cpp", "arduino", "esp32"
    public string Payload { get; set; } // Base64 encoded ZIP archive of the project
    public ArduinoConfig ArduinoConfig { get; set; } // Optional, for "arduino" or "esp32" environments
}

public class ArduinoConfig
{
    public string Board { get; set; } // Fully Qualified Board Name, e.g., "arduino:avr:uno", "esp32:esp32:esp32dev"
    public string Port { get; set; } // Serial port, e.g., "COM3", "/dev/ttyACM0"
}
```

### Example Usage (Conceptual)

```csharp
using System;
using System.IO;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Example: Assume you have a Base64 encoded ZIP of an Arduino project
        string base64ZipPayload = "UEsDBBQAAAAA..."; // Replace with actual Base64 data

        var payload = new WebMessagePayload
        {
            Action = "build_and_run",
            Environment = "arduino",
            Payload = base64ZipPayload,
            ArduinoConfig = new ArduinoConfig
            {
                Board = "arduino:avr:uno", // or "esp32:esp32:esp32dev"
                Port = "COM3" // or appropriate port for your system
            }
        };

        var processor = new ProjectProcessor();
        try
        {
            string output = await processor.ProcessPayload(payload);
            Console.WriteLine("--- Processing Complete ---");
            Console.WriteLine(output);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
```

## 📂 Project Structure

```
.
├── .llmContext/                  # LLM-specific context/cache
├── ArduinoProjectBuilder.cs      # IProjectBuilder implementation for Arduino/ESP32 projects
├── CSharpProjectBuilder.cs       # IProjectBuilder implementation for C# projects
├── CppProjectBuilder.cs          # IProjectBuilder implementation for C++ (CMake) projects
├── IProjectBuilder.cs            # Interface defining build/run operations
├── ProjectBuilderFactory.cs      # Static factory for obtaining IProjectBuilder instances
├── ProjectProcessor.cs           # Main orchestrator for handling web message payloads
├── ShellExecutor.cs              # Utility for executing shell commands (headless/visible)
├── WebMessagePayload.cs          # Data structure for incoming project requests
└── README.md                     # This file
```

## 🤝 Contributing

Contributions are welcome! Please feel free to open issues or submit pull requests.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.