# User Action Recording Application

A Windows Forms application that records and replays user actions including mouse movements, clicks, and keyboard inputs. This tool is designed to help automate repetitive tasks by recording user interactions and playing them back.

## Features

- **Action Recording**: Records mouse movements, clicks, and keyboard combinations
- **Smart Replay**: Faithfully reproduces recorded actions with timing preservation
- **Safety Mechanisms**: Prevents recursive recording during replay
- **Cooldown System**: Implements post-replay protection to prevent accidental re-recordings
- **Global Hook System**: Captures system-wide mouse and keyboard events

## Technical Implementation

- Built with .NET Windows Forms
- Uses global hooks for mouse and keyboard event capture
- Implements precise timing using Stopwatch for accurate action replay
- Features threshold-based mouse movement recording to optimize performance

## Known Limitations

### Keyboard Events During Replay

Keyboard inputs are intentionally disabled during replay mode. This is a deliberate design choice to prevent potential issues:

1. **Prevention of Recursive Recording**: This prevents the application from recording its own replay actions, which could create an infinite loop
2. **System Stability**: Helps maintain system stability by avoiding concurrent input handling
3. **Predictable Behavior**: Ensures consistent replay behavior across different scenarios

### Other Considerations

- The application implements a cooldown period after replay to prevent immediate re-recording
- Record button clicks are filtered during replay to maintain system integrity
- Mouse movements are optimized with a threshold to prevent excessive data collection

## Usage

1. Launch the application
2. Click the record button to start recording your actions
3. Perform the actions you want to record
4. Click the record button again to stop recording
5. Use the replay functionality to reproduce the recorded actions

## Technical Notes

- Mouse movements are recorded with a threshold of 5 pixels to optimize performance
- The application uses a sophisticated event system to manage recording and replay states
- Implements proper cleanup and resource disposal through IDisposable pattern

## Safety Features

- Built-in protection against accidental re-recording during replay
- Smart filtering of record button clicks during replay
- Post-replay cooldown period to prevent immediate re-recording# RecordUserAction
