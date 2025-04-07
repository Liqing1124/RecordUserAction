# User Action Recorder

A C# Windows Forms application that records and replays mouse actions with high precision and customizable replay functionality. This tool allows you to automate repetitive mouse tasks by recording your actions once and replaying them multiple times.

## Features

- **Mouse Action Recording**
  - Records mouse clicks with precise coordinates
  - Tracks mouse movements with optimized threshold detection
  - Intelligent filtering of small movements to prevent noise

- **Advanced Replay System**
  - Customizable replay count for multiple iterations
  - Smooth mouse movement interpolation during replay
  - Built-in cooldown system to prevent accidental re-recording

- **Smart Recording Controls**
  - Record button protection during replay
  - Temporary recording buffer support
  - Automatic recording state management

- **Event Notifications**
  - Replay start/complete event handling
  - Real-time status tracking

## Requirements

- Windows operating system
- .NET Framework 4.8 or later
- Windows Forms compatible environment
- MouseKeyHook package (automatically installed via NuGet)

## Installation

1. Clone or download this repository
2. Open the solution in Visual Studio 2019 or later
3. Build the solution (dependencies will be automatically restored via NuGet)
4. Run the application

Alternatively, you can download the pre-built binaries from the releases section.

## Usage

### Starting a Recording

1. Launch the application
2. Click the "Record" button to start capturing actions
3. Perform the mouse actions you want to record
4. Click the "Stop Recording" button to finish recording

### Replaying Actions

1. Set the desired number of replay iterations using the numeric control
2. Click the "Replay" button
3. The system will automatically replay your recorded actions with the specified number of iterations
4. Wait for the replay to complete (the status will update when finished)

### Safety Features

- Built-in cooldown period after replays to prevent accidental re-recording
- Protection against accidental record button clicks during replay
- Automatic cleanup of resources when closing the application

## Technical Implementation

- Uses the `GlobalHook` class for system-wide mouse input capture
- Implements `IDisposable` for proper resource management
- Maintains separate timers for recording and temporary buffers
- Includes threshold-based movement filtering to reduce noise
- Uses low-level Windows API calls for precise mouse simulation
- Thread-safe replay execution in a separate thread
- Efficient action timing system using Stopwatch

## Project Structure

- **Form1.cs**: Main application UI and event handling
- **UserActionRecorder.cs**: Core recording and replay functionality
- **SimulateMouse.cs**: Low-level mouse simulation utilities

## Limitations

- Currently only supports mouse actions (clicks and movements)
- Designed for use within the same screen resolution as recorded
- May require elevated permissions for some applications

## License

This project is available under the MIT License. See the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
