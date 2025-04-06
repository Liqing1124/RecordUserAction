using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Drawing;

namespace RecordUserAction
{
    public class UserAction
    {
        public enum ActionType
        {
            MouseClick,
            MouseMove,
            TabChange
        }

        public ActionType Type { get; set; }
        public Point Location { get; set; } // For mouse clicks

        public string TabName { get; set; } // For tab changes
        public long TimestampMs { get; set; } // Time since recording started
    }

    public class UserActionRecorder
    {
        private List<UserAction> _recordedActions = new List<UserAction>();
        private List<UserAction> _tempRecordBuffer = new List<UserAction>();
        private bool _isRecording = false;
        private bool _isReplaying = false;
        private bool _isInPostReplayCooldown = false;
        private bool _isRecordingDuringReplay = false;
        private Stopwatch _recordingTimer = new Stopwatch();
        private Stopwatch _tempRecordTimer = new Stopwatch();
        private Form _targetForm;
        private Button _recordButton; // Reference to the record button
        
        // Events to notify the form about replay status
        public event EventHandler ReplayStarted;
        public event EventHandler ReplayCompleted;

        // For global mouse and keyboard hooks
        private GlobalHook _mouseHook;

        private Point _lastMousePosition = Point.Empty;
        private const int MOUSE_MOVE_THRESHOLD = 5; // Minimum pixel distance to record a movement

        public UserActionRecorder(Form targetForm)
        {
            _targetForm = targetForm;
            _mouseHook = new GlobalHook(GlobalHook.HookType.Mouse);

            _mouseHook.MouseClick += OnMouseClick;
            _mouseHook.MouseMove += OnMouseMove;
            
            // Find and store reference to the record button
            foreach (Control control in targetForm.Controls)
            {
                if (control is Button button && button.Name == "btnRecord")
                {
                    _recordButton = button;
                    break;
                }
            }
        }

        public void StartRecording()
        {
            if (_isRecording) return;

            if (_isReplaying)
            {
                _tempRecordBuffer.Clear();
                _tempRecordTimer.Reset();
                _tempRecordTimer.Start();
                _isRecordingDuringReplay = true;
                return;
            }

            _recordedActions.Clear();
            _recordingTimer.Reset();
            _recordingTimer.Start();
            _isRecording = true;

            // Start the hooks
            _mouseHook.Install();
            _keyboardHook.Install();
        }

        public void StopRecording()
        {
            if (_isRecordingDuringReplay)
            {
                _tempRecordTimer.Stop();
                _isRecordingDuringReplay = false;
                return;
            }

            if (!_isRecording) return;

            _recordingTimer.Stop();
            _isRecording = false;

            // Stop the hooks
            _mouseHook.Uninstall();
            _keyboardHook.Uninstall();
        }

        public bool IsReplaying
        {
            get { return _isReplaying; }
        }

        // Modified to accept replay count
        public void ReplayActions(int replayCount = 1)
        {
            if (_isRecording || _isReplaying || _recordedActions.Count == 0 || replayCount < 1) return;

            _isReplaying = true;
            ReplayStarted?.Invoke(this, EventArgs.Empty); // Fire started event once

            // Pass replayCount to the thread
            Thread replayThread = new Thread(new ParameterizedThreadStart(ReplayActionsThread));
            replayThread.SetApartmentState(ApartmentState.STA);
            replayThread.Start(replayCount);
        }

        // Modified to accept replay count object and loop
        private void ReplayActionsThread(object replayCountObj)
        {
            int replayCount = (int)replayCountObj;
            Stopwatch replayTimer = new Stopwatch();

            
            for (int i = 0; i < replayCount; i++)
            {
                replayTimer.Restart(); // Restart timer for each replay cycle
                int actionIndex = 0;
                long lastFrameTime = 0;

                while (actionIndex < _recordedActions.Count)
                {
                    UserAction action = _recordedActions[actionIndex];
                    long currentTime = replayTimer.ElapsedMilliseconds;

                    // Maintain consistent frame timing (optional, adjust as needed)
                    // if (currentTime - lastFrameTime < 16) // ~60fps frame time
                    // {
                    //     Thread.Sleep(1);
                    //     continue;
                    // }

                    // Wait until it's time to perform this action relative to the start of this cycle
                    if (currentTime < action.TimestampMs)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    lastFrameTime = currentTime;

                    // Perform the action
                    PerformAction(action, actionIndex);
                    actionIndex++;
                }
                replayTimer.Stop(); // Stop timer for this cycle

                // Optional: Add a small delay between replays if desired
                if (i < replayCount - 1)
                {
                    Thread.Sleep(500); // 500ms delay between replays
                }
            }

            // Add a final safety delay after all replays are done
            Thread.Sleep(500); 
            
            // Set the post-replay cooldown flag
            _isInPostReplayCooldown = true;
            
            // Notify that replay is complete
            _isReplaying = false;
            
            // Start a timer to clear the cooldown flag after a delay
            System.Threading.Timer cooldownTimer = null;
            cooldownTimer = new System.Threading.Timer((state) => {
                _isInPostReplayCooldown = false;
                cooldownTimer?.Dispose();
            }, null, 2000, Timeout.Infinite); // 2 second cooldown
            _targetForm.Invoke(new Action(() => {
                ReplayCompleted?.Invoke(this, EventArgs.Empty);
            }));
        }

        private void PerformAction(UserAction action, int actionIndex)
        {
            switch (action.Type)
            {
                case UserAction.ActionType.MouseClick:
                    // Check if this click is on the record button during replay or cooldown period
                    if (IsClickOnRecordButton(action.Location) && (_isReplaying || _isInPostReplayCooldown))
                    {
                        // Skip clicking the record button during replay or cooldown period
                        Console.WriteLine("Skipping click on record button during replay or cooldown period");
                    }
                    else
                    {
                        // Add a small delay before clicking to ensure the application is ready
                        Thread.Sleep(10);
                        SimulateMouse.Click(action.Location.X, action.Location.Y);
                    }
                    break;
                    
                case UserAction.ActionType.MouseMove:
                    // Implement smooth mouse movement with interpolation
                    if (actionIndex > 0 && _recordedActions[actionIndex - 1].Type == UserAction.ActionType.MouseMove)
                    {
                        var prevAction = _recordedActions[actionIndex - 1];
                        var timeDiff = action.TimestampMs - prevAction.TimestampMs;
                        var steps = Math.Max(1, timeDiff / 16); // ~60fps
                        
                        for (int i = 1; i <= steps; i++)
                        {
                            var t = i / steps;
                            var x = prevAction.Location.X + (action.Location.X - prevAction.Location.X) * t;
                            var y = prevAction.Location.Y + (action.Location.Y - prevAction.Location.Y) * t;
                            SimulateMouse.MoveTo((int)x, (int)y);
                            Thread.Sleep(1);
                        }
                    }
                    else
                    {
                        SimulateMouse.MoveTo(action.Location.X, action.Location.Y);
                    }
                    break;

                case UserAction.ActionType.KeyPress:
                    SimulateKeyboard.KeyPress(action.KeyCode);
                    break;
                
                case UserAction.ActionType.KeyDown:
                    SimulateKeyboard.KeyDown(action.KeyCode);
                    break;
                    
                case UserAction.ActionType.KeyUp:
                    SimulateKeyboard.KeyUp(action.KeyCode);
                    break;
                    
                case UserAction.ActionType.KeyCombination:
                    SimulateKeyboard.KeyCombination(action.KeyCode, action.CtrlKey, action.ShiftKey, action.AltKey);
                    break;

                case UserAction.ActionType.TabChange:
                    // Handle tab change if needed
                    break;
            }
        }

        private bool IsClickOnRecordButton(Point location)
        {
            if (_recordButton != null)
            {
                // Convert the button's bounds to screen coordinates
                Rectangle buttonBounds = _recordButton.Bounds;
                Point formLocation = _targetForm.PointToScreen(Point.Empty);
                buttonBounds.Offset(formLocation);

                // Check if the click is within the button's bounds
                return buttonBounds.Contains(location);
            }

            return false;
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (!_isRecording || _isReplaying || _isInPostReplayCooldown) return;

            _recordedActions.Add(new UserAction
            {
                Type = UserAction.ActionType.MouseClick,
                Location = new Point(e.X, e.Y),
                TimestampMs = _recordingTimer.ElapsedMilliseconds
            });
        }
        
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isRecording || _isReplaying || _isInPostReplayCooldown) return;
            
            // Only record mouse movements if they exceed the threshold distance from the last recorded position
            // This prevents recording too many small movements
            if (_lastMousePosition != Point.Empty)
            {
                int deltaX = Math.Abs(e.X - _lastMousePosition.X);
                int deltaY = Math.Abs(e.Y - _lastMousePosition.Y);
                
                if (deltaX < MOUSE_MOVE_THRESHOLD && deltaY < MOUSE_MOVE_THRESHOLD)
                    return;
            }
            
            _lastMousePosition = new Point(e.X, e.Y);
            
            _recordedActions.Add(new UserAction
            {
                Type = UserAction.ActionType.MouseMove,
                Location = new Point(e.X, e.Y),
                TimestampMs = _recordingTimer.ElapsedMilliseconds
            });
        }



        public void Dispose()
        {
            StopRecording();
            _mouseHook?.Dispose();

        }
    }

    // Event args for key combinations
    public class KeyCombinationEventArgs : EventArgs
    {
        public Keys KeyCode { get; set; }
        public bool CtrlKey { get; set; }
        public bool ShiftKey { get; set; }
        public bool AltKey { get; set; }
    }

    // Helper class for global hooks
    public class GlobalHook : IDisposable
    {
        public enum HookType
        {
            Mouse,
            Keyboard
        }

        private IntPtr _hookId = IntPtr.Zero;
        private HookType _hookType;
        private HookProc _hookProc;

        public event MouseEventHandler MouseClick;
        public event MouseEventHandler MouseMove;
        public event KeyPressEventHandler KeyPress;
        public event EventHandler<KeyEventArgs> KeyDown;
        public event EventHandler<KeyEventArgs> KeyUp;
        public event EventHandler<KeyCombinationEventArgs> KeyCombination;

        // Track modifier key states
        private bool _isCtrlDown = false;
        private bool _isShiftDown = false;
        private bool _isAltDown = false;

        public GlobalHook(HookType hookType)
        {
            _hookType = hookType;
            _hookProc = HookCallback;
        }

        public void Install()
        {
            if (_hookId != IntPtr.Zero) return;

            if (_hookType == HookType.Mouse)
            {
                _hookId = SetWindowsHookEx(WH_MOUSE_LL, _hookProc, GetModuleHandle(null!), 0);
            }
            else
            {
                _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, GetModuleHandle(null!), 0);
            }
        }

        public void Uninstall()
        {
            if (_hookId == IntPtr.Zero) return;

            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (_hookType == HookType.Mouse)
                {
                    if (wParam == (IntPtr)WM_LBUTTONDOWN)
                    {
                        MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                        MouseClick?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 1, hookStruct.pt.x, hookStruct.pt.y, 0));
                    }
                    else if (wParam == (IntPtr)WM_MOUSEMOVE)
                    {
                        MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                        MouseMove?.Invoke(this, new MouseEventArgs(MouseButtons.None, 0, hookStruct.pt.x, hookStruct.pt.y, 0));
                    }
                }
                else if (_hookType == HookType.Keyboard)
                {
                    KBDLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                    Keys keyCode = (Keys)hookStruct.vkCode;
                    
                    // Track modifier key states
                    if (wParam == (IntPtr)WM_KEYDOWN)
                    {
                        // Update modifier key states
                        if (keyCode == Keys.ControlKey || keyCode == Keys.LControlKey || keyCode == Keys.RControlKey)
                            _isCtrlDown = true;
                        else if (keyCode == Keys.ShiftKey || keyCode == Keys.LShiftKey || keyCode == Keys.RShiftKey)
                            _isShiftDown = true;
                        else if (keyCode == Keys.Menu || keyCode == Keys.LMenu || keyCode == Keys.RMenu) // Alt key
                            _isAltDown = true;
                        
                        // Trigger KeyDown event for all keys
                        KeyDown?.Invoke(this, new KeyEventArgs(keyCode));
                        
                        // If any modifier is pressed, also trigger KeyCombination event
                        if (_isCtrlDown || _isShiftDown || _isAltDown)
                        {
                            if (!(keyCode == Keys.ControlKey || keyCode == Keys.LControlKey || keyCode == Keys.RControlKey ||
                                keyCode == Keys.ShiftKey || keyCode == Keys.LShiftKey || keyCode == Keys.RShiftKey ||
                                keyCode == Keys.Menu || keyCode == Keys.LMenu || keyCode == Keys.RMenu))
                            {
                                KeyCombination?.Invoke(this, new KeyCombinationEventArgs
                                {
                                    KeyCode = keyCode,
                                    CtrlKey = _isCtrlDown,
                                    ShiftKey = _isShiftDown,
                                    AltKey = _isAltDown
                                });
                            }
                        }
                        else
                        {
                            // Regular key press (for backward compatibility)
                            KeyPress?.Invoke(this, new KeyPressEventArgs((char)hookStruct.vkCode));
                        }
                    }
                    else if (wParam == (IntPtr)WM_KEYUP)
                    {
                        // Trigger KeyUp event for all keys
                        KeyUp?.Invoke(this, new KeyEventArgs(keyCode));
                        
                        // Reset modifier key states
                        if (keyCode == Keys.ControlKey || keyCode == Keys.LControlKey || keyCode == Keys.RControlKey)
                            _isCtrlDown = false;
                        else if (keyCode == Keys.ShiftKey || keyCode == Keys.LShiftKey || keyCode == Keys.RShiftKey)
                            _isShiftDown = false;
                        else if (keyCode == Keys.Menu || keyCode == Keys.LMenu || keyCode == Keys.RMenu) // Alt key
                            _isAltDown = false;
                    }
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            Uninstall();
        }

        #region Native Methods

        private const int WH_MOUSE_LL = 14;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        #endregion
    }

    // Helper classes for simulating input
    public static class SimulateMouse
    {
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const uint MOUSEEVENTF_VIRTUALDESK = 0x4000;
        private const uint INPUT_MOUSE = 0;

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        }

        public static void Click(int x, int y)
        {
            // Convert screen coordinates to normalized coordinates (0-65535)
            int screenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            int normalizedX = (x * 65536) / screenWidth;
            int normalizedY = (y * 65536) / screenHeight;

            INPUT[] inputs = new INPUT[3];

            // Move mouse (absolute positioning)
            inputs[0] = new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT
                {
                    dx = normalizedX,
                    dy = normalizedY,
                    mouseData = 0,
                    dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            };

            // Mouse down
            inputs[1] = new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT
                {
                    dx = normalizedX,
                    dy = normalizedY,
                    mouseData = 0,
                    dwFlags = MOUSEEVENTF_LEFTDOWN,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            };

            // Mouse up
            inputs[2] = new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT
                {
                    dx = normalizedX,
                    dy = normalizedY,
                    mouseData = 0,
                    dwFlags = MOUSEEVENTF_LEFTUP,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            };

            // Send all inputs at once
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(50); // Small delay to ensure input is processed
        }
        
        public static void MoveTo(int x, int y)
        {
            // Convert to normalized coordinates
            int screenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            int normalizedX = (x * 65536) / screenWidth;
            int normalizedY = (y * 65536) / screenHeight;

            INPUT[] inputs = new INPUT[1];
            inputs[0] = new INPUT
            {
                type = INPUT_MOUSE,
                mi = new MOUSEINPUT
                {
                    dx = normalizedX,
                    dy = normalizedY,
                    mouseData = 0,
                    dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_VIRTUALDESK,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            };

            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
    }

    public static class SimulateKeyboard
    {
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public KEYBDINPUT ki;
        }

        private static INPUT[] CreateKeyboardInput(Keys key, bool isKeyUp)
        {
            return new INPUT[]
            {
                new INPUT
                {
                    type = INPUT_KEYBOARD,
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)key,
                        wScan = 0,
                        dwFlags = isKeyUp ? KEYEVENTF_KEYUP : KEYEVENTF_KEYDOWN | KEYEVENTF_EXTENDEDKEY,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
        }

        public static void KeyPress(Keys key)
        {
            try
            {
                // Send key down
                SendInput(1, CreateKeyboardInput(key, false), Marshal.SizeOf(typeof(INPUT)));
                Thread.Sleep(50); // Small delay between press and release
                // Send key up
                SendInput(1, CreateKeyboardInput(key, true), Marshal.SizeOf(typeof(INPUT)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error simulating key press: {ex.Message}");
            }
        }

        public static void KeyDown(Keys key)
        {
            try
            {
                SendInput(1, CreateKeyboardInput(key, false), Marshal.SizeOf(typeof(INPUT)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error simulating key down: {ex.Message}");
            }
        }

        public static void KeyUp(Keys key)
        {
            try
            {
                SendInput(1, CreateKeyboardInput(key, true), Marshal.SizeOf(typeof(INPUT)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error simulating key up: {ex.Message}");
            }
        }

        public static void KeyCombination(Keys key, bool ctrl, bool shift, bool alt)
        {
            try
            {
                var inputs = new List<INPUT>();

                // Press modifier keys
                if (ctrl)
                    inputs.AddRange(CreateKeyboardInput(Keys.ControlKey, false));
                if (shift)
                    inputs.AddRange(CreateKeyboardInput(Keys.ShiftKey, false));
                if (alt)
                    inputs.AddRange(CreateKeyboardInput(Keys.Menu, false));

                // Press and release the main key
                inputs.AddRange(CreateKeyboardInput(key, false));
                inputs.AddRange(CreateKeyboardInput(key, true));

                // Release modifier keys in reverse order
                if (alt)
                    inputs.AddRange(CreateKeyboardInput(Keys.Menu, true));
                if (shift)
                    inputs.AddRange(CreateKeyboardInput(Keys.ShiftKey, true));
                if (ctrl)
                    inputs.AddRange(CreateKeyboardInput(Keys.ControlKey, true));

                // Send all inputs at once
                SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf(typeof(INPUT)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error simulating key combination: {ex.Message}");
            }
        }
    }
}
