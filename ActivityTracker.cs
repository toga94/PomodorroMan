using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace PomodorroMan
{
    public class ActivityTracker : IDisposable
    {
        private readonly WorkEfficiencyCalculator _efficiencyCalculator;
        private readonly System.Windows.Forms.Timer _activityTimer;
        private readonly System.Windows.Forms.Timer _afkTimer;
        private readonly System.Threading.Timer _backgroundTimer;
        private bool _isTracking;
        private bool _disposed;
        private Point _lastMousePosition;
        private DateTime _lastActivityTime;
        private readonly object _lockObject = new();
        private readonly Dictionary<ActivityType, int> _activityCounts = new();
        private DateTime _lastActivityReset = DateTime.Now;
        private CancellationTokenSource? _cancellationTokenSource;

        private const int ActivityCheckInterval = 1000;
        private const int AfkThresholdSeconds = 120; // 2 minutes
        private const int MouseMovementThreshold = 5;
        private const int KeyboardActivityThreshold = 1;

        public event EventHandler<ActivityDetectedEventArgs>? ActivityDetected;
        public event EventHandler<AfkDetectedEventArgs>? AfkDetected;
        public event EventHandler<FocusLostEventArgs>? FocusLost;

        public bool IsTracking => _isTracking;
        public bool IsAfk { get; private set; }
        public TimeSpan AfkDuration { get; private set; }
        public DateTime LastActivityTime => _lastActivityTime;

        public ActivityTracker(WorkEfficiencyCalculator efficiencyCalculator)
        {
            _efficiencyCalculator = efficiencyCalculator;
            _activityTimer = new System.Windows.Forms.Timer { Interval = ActivityCheckInterval };
            _activityTimer.Tick += OnActivityCheck;
            
            _afkTimer = new System.Windows.Forms.Timer { Interval = AfkThresholdSeconds * 1000 };
            _afkTimer.Tick += OnAfkCheck;

            // Background timer for reliable operation in tray mode
            _backgroundTimer = new System.Threading.Timer(OnBackgroundCheck, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));

            _lastMousePosition = Cursor.Position;
            _lastActivityTime = DateTime.Now;
            
            // Start tracking immediately
            StartTracking();
        }

        public void StartTracking()
        {
            if (_isTracking) return;

            lock (_lockObject)
            {
                _isTracking = true;
                _lastActivityTime = DateTime.Now;
                _lastMousePosition = Cursor.Position;
                IsAfk = false;
                AfkDuration = TimeSpan.Zero;

                _activityTimer.Start();
                _afkTimer.Start();

                System.Diagnostics.Debug.WriteLine($"ActivityTracker started - ActivityTimer: {_activityTimer.Enabled}, AfkTimer: {_afkTimer.Enabled}");

                _cancellationTokenSource = new CancellationTokenSource();
                _ = System.Threading.Tasks.Task.Run(() => MonitorKeyboardActivity(_cancellationTokenSource.Token));
                _ = System.Threading.Tasks.Task.Run(() => MonitorMouseActivity(_cancellationTokenSource.Token));
            }
        }

        public void StopTracking()
        {
            if (!_isTracking) return;

            lock (_lockObject)
            {
                _isTracking = false;
                _activityTimer.Stop();
                _afkTimer.Stop();
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void OnActivityCheck(object? sender, EventArgs e)
        {
            if (!_isTracking) return;

            var currentMousePosition = Cursor.Position;
            var mouseMoved = Math.Abs(currentMousePosition.X - _lastMousePosition.X) > MouseMovementThreshold ||
                           Math.Abs(currentMousePosition.Y - _lastMousePosition.Y) > MouseMovementThreshold;

            if (mouseMoved)
            {
                RecordActivity(ActivityType.MouseMovement, CalculateMouseIntensity(currentMousePosition));
                _lastMousePosition = currentMousePosition;
            }
            
            // Also check for any key presses during the timer interval
            if (IsKeyPressed())
            {
                RecordActivity(ActivityType.KeyboardTyping, 1.0);
            }
        }

        private void OnBackgroundCheck(object? state)
        {
            if (!_isTracking || _disposed) return;

            try
            {
                // Check mouse movement
                var currentMousePosition = Cursor.Position;
                var mouseMoved = Math.Abs(currentMousePosition.X - _lastMousePosition.X) > MouseMovementThreshold ||
                               Math.Abs(currentMousePosition.Y - _lastMousePosition.Y) > MouseMovementThreshold;

                if (mouseMoved)
                {
                    RecordActivity(ActivityType.MouseMovement, CalculateMouseIntensity(currentMousePosition));
                    _lastMousePosition = currentMousePosition;
                }

                // Check keyboard activity
                if (IsKeyPressed())
                {
                    RecordActivity(ActivityType.KeyboardTyping, 1.0);
                }

                // Check AFK status
                var timeSinceLastActivity = DateTime.Now - _lastActivityTime;
                if (timeSinceLastActivity.TotalSeconds >= AfkThresholdSeconds)
                {
                    if (!IsAfk)
                    {
                        IsAfk = true;
                        AfkDuration = timeSinceLastActivity;
                        AfkDetected?.Invoke(this, new AfkDetectedEventArgs(timeSinceLastActivity));
                    }
                    else
                    {
                        AfkDuration = timeSinceLastActivity;
                    }
                }
                else
                {
                    if (IsAfk)
                    {
                        IsAfk = false;
                        AfkDuration = TimeSpan.Zero;
                    }
                }
            }
            catch (Exception ex)
            {
                // Improved error handling for stability
                System.Diagnostics.Debug.WriteLine($"Error in background activity check: {ex.Message}");
                
                // Reset state on critical errors to prevent stuck states
                if (ex is OutOfMemoryException || ex is InvalidOperationException)
                {
                    try
                    {
                        _lastActivityTime = DateTime.Now;
                        IsAfk = false;
                        AfkDuration = TimeSpan.Zero;
                    }
                    catch
                    {
                        // Ignore secondary errors during recovery
                    }
                }
            }
        }

        private async void MonitorKeyboardActivity(CancellationToken cancellationToken)
        {
            var lastKeyPressTime = DateTime.MinValue;
            var errorCount = 0;
            
            while (!cancellationToken.IsCancellationRequested && _isTracking)
            {
                try
                {
                    var now = DateTime.Now;
                    if (IsKeyPressed() && (now - lastKeyPressTime).TotalMilliseconds >= EfficiencyConfig.MinKeyPressInterval)
                    {
                        RecordActivity(ActivityType.KeyboardTyping, 1.0);
                        lastKeyPressTime = now;
                    }

                    // Reset error count on successful operation
                    errorCount = 0;
                    await System.Threading.Tasks.Task.Delay(EfficiencyConfig.MonitoringDelay, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    System.Diagnostics.Debug.WriteLine($"Error in keyboard monitoring (attempt {errorCount}): {ex.Message}");
                    
                    // If too many errors, stop monitoring
                    if (errorCount >= EfficiencyConfig.MaxErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("Too many keyboard monitoring errors, stopping monitoring");
                        break;
                    }
                    
                    await System.Threading.Tasks.Task.Delay(1000, cancellationToken); // Wait longer on error
                }
            }
        }

        private async void MonitorMouseActivity(CancellationToken cancellationToken)
        {
            var lastScrollTime = DateTime.MinValue;
            var errorCount = 0;
            
            while (!cancellationToken.IsCancellationRequested && _isTracking)
            {
                try
                {
                    var now = DateTime.Now;
                    var scrollDelta = GetScrollDelta();
                    if (scrollDelta != 0 && (now - lastScrollTime).TotalMilliseconds >= EfficiencyConfig.MinScrollInterval)
                    {
                        RecordActivity(ActivityType.Scrolling, Math.Abs(scrollDelta) / 120.0);
                        lastScrollTime = now;
                    }

                    // Reset error count on successful operation
                    errorCount = 0;
                    await System.Threading.Tasks.Task.Delay(EfficiencyConfig.MonitoringDelay, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    System.Diagnostics.Debug.WriteLine($"Error in mouse monitoring (attempt {errorCount}): {ex.Message}");
                    
                    // If too many errors, stop monitoring
                    if (errorCount >= EfficiencyConfig.MaxErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("Too many mouse monitoring errors, stopping monitoring");
                        break;
                    }
                    
                    await System.Threading.Tasks.Task.Delay(1000, cancellationToken); // Wait longer on error
                }
            }
        }

        private void OnAfkCheck(object? sender, EventArgs e)
        {
            if (!_isTracking) return;

            var timeSinceLastActivity = DateTime.Now - _lastActivityTime;
            System.Diagnostics.Debug.WriteLine($"AFK Check - Time since last activity: {timeSinceLastActivity.TotalSeconds:F1}s, IsAFK: {IsAfk}");
            
            if (timeSinceLastActivity.TotalSeconds >= AfkThresholdSeconds)
            {
                if (!IsAfk)
                {
                    IsAfk = true;
                    AfkDuration = timeSinceLastActivity;
                    AfkDetected?.Invoke(this, new AfkDetectedEventArgs(timeSinceLastActivity));
                    System.Diagnostics.Debug.WriteLine($"AFK detected: {timeSinceLastActivity.TotalSeconds:F1} seconds");
                }
                else
                {
                    AfkDuration = timeSinceLastActivity;
                }
            }
            else
            {
                if (IsAfk)
                {
                    IsAfk = false;
                    AfkDuration = TimeSpan.Zero;
                    System.Diagnostics.Debug.WriteLine("AFK status cleared - activity detected");
                }
            }
        }

        private void RecordActivity(ActivityType activityType, double intensity)
        {
            if (!_isTracking) return;

            lock (_lockObject)
            {
                if (!_isTracking) return; // Double-check after acquiring lock

                _lastActivityTime = DateTime.Now;
                IsAfk = false;
                AfkDuration = TimeSpan.Zero;

                // Always record activity to efficiency calculator
                _efficiencyCalculator.RecordActivity(activityType, intensity);
                
                // Track activity counts
                if (!_activityCounts.ContainsKey(activityType))
                    _activityCounts[activityType] = 0;
                _activityCounts[activityType]++;
                
                ActivityDetected?.Invoke(this, new ActivityDetectedEventArgs(activityType, intensity, _lastActivityTime));
            }
        }

        private double CalculateMouseIntensity(Point currentPosition)
        {
            var distance = Math.Sqrt(
                Math.Pow(currentPosition.X - _lastMousePosition.X, 2) +
                Math.Pow(currentPosition.Y - _lastMousePosition.Y, 2));

            return Math.Min(2.0, distance / 10.0);
        }

        private bool IsKeyPressed()
        {
            for (int i = 0; i < 256; i++)
            {
                if ((GetAsyncKeyState(i) & 0x8000) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        private int GetScrollDelta()
        {
            return 0;
        }

        public void RecordWindowSwitch()
        {
            if (!_isTracking) return;

            RecordActivity(ActivityType.WindowSwitching, 1.0);
            _efficiencyCalculator.RecordDistraction();
            FocusLost?.Invoke(this, new FocusLostEventArgs(DateTime.Now));
        }

        public void RecordDistraction()
        {
            if (!_isTracking) return;

            _efficiencyCalculator.RecordDistraction();
            FocusLost?.Invoke(this, new FocusLostEventArgs(DateTime.Now));
        }

        public Dictionary<ActivityType, int> GetActivityCounts()
        {
            lock (_lockObject)
            {
                return new Dictionary<ActivityType, int>(_activityCounts);
            }
        }

        public void ResetActivityCounts()
        {
            lock (_lockObject)
            {
                _activityCounts.Clear();
                _lastActivityReset = DateTime.Now;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                StopTracking();
                _activityTimer?.Dispose();
                _afkTimer?.Dispose();
                _backgroundTimer?.Dispose();
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during ActivityTracker disposal: {ex.Message}");
            }
            finally
            {
                _disposed = true;
            }
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentProcessId();
    }

    public class ActivityDetectedEventArgs : EventArgs
    {
        public ActivityType ActivityType { get; }
        public double Intensity { get; }
        public DateTime Timestamp { get; }

        public ActivityDetectedEventArgs(ActivityType activityType, double intensity, DateTime timestamp)
        {
            ActivityType = activityType;
            Intensity = intensity;
            Timestamp = timestamp;
        }
    }

    public class AfkDetectedEventArgs : EventArgs
    {
        public TimeSpan AfkDuration { get; }

        public AfkDetectedEventArgs(TimeSpan afkDuration)
        {
            AfkDuration = afkDuration;
        }
    }

    public class FocusLostEventArgs : EventArgs
    {
        public DateTime Timestamp { get; }

        public FocusLostEventArgs(DateTime timestamp)
        {
            Timestamp = timestamp;
        }
    }
}
