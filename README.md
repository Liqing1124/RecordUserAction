# User Action Recorder

A C# Windows Forms application that records and replays user actions including mouse movements, clicks, and keyboard inputs with high precision and customizable replay functionality.

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

## Technical Details

- Built with C# and Windows Forms
- Uses low-level Windows hooks for input capture
- Implements global mouse and keyboard event handling
- Thread-safe replay execution
- Efficient action timing system using Stopwatch

## Usage

1. **Starting a Recording**
   - Launch the application
   - Click the Record button to start capturing actions
   - Perform the actions you want to record
   - Click the Record button again to stop recording

2. **Replaying Actions**
   - Set the desired number of replay iterations
   - Start the replay
   - The system will automatically handle timing and movement interpolation

3. **Safety Features**
   - Built-in cooldown period after replays
   - Protection against accidental record button clicks during replay
   - Automatic cleanup of resources

## Requirements

- Windows operating system
- .NET 9.0 or later
- Windows Forms compatible environment

## Implementation Notes

- Uses the `GlobalHook` class for system-wide input capture
- Implements `IDisposable` for proper resource management
- Maintains separate timers for recording and temporary buffers
- Includes threshold-based movement filtering