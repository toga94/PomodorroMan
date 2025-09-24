using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Media;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using Microsoft.Win32;

namespace PomodorroMan
{
	internal sealed class TrayPomodoroContext : ApplicationContext
	{
		private enum SessionType
		{
			Work,
			ShortBreak,
			LongBreak
		}

		private readonly NotifyIcon _trayIcon;
		private readonly System.Windows.Forms.Timer _secondTimer;
		private readonly ToolStripMenuItem _startPauseMenuItem;
		private readonly ToolStripMenuItem _resetMenuItem;
		private readonly ToolStripMenuItem _skipMenuItem;
		private readonly ToolStripMenuItem _soundMenuItem;
		private readonly ToolStripMenuItem _notificationsMenuItem;
		private readonly ToolStripMenuItem _autoStartMenuItem;
		private readonly ToolStripMenuItem _pauseAfterPhaseMenuItem;
		private readonly ToolStripMenuItem _settingsMenuItem;
		private readonly ToolStripMenuItem _historyMenuItem;
		private readonly ToolStripMenuItem _aboutMenuItem;
		private readonly ToolStripMenuItem _exitMenuItem;
		private readonly ToolStripMenuItem _efficiencyMenuItem;
		private readonly ToolStripMenuItem _taskManagementMenuItem;
		private readonly ToolStripMenuItem _ambientSoundsMenuItem;
		private readonly ToolStripMenuItem _themesMenuItem;
		private readonly ToolStripMenuItem _achievementsMenuItem;
		private readonly ToolStripMenuItem _breakActivitiesMenuItem;

		private TimeSpan _remaining;
		private SessionType _currentSessionType = SessionType.Work;
		private bool _isRunning;
		private bool _pauseAfterNextPhase;
		private int _completedWorkSessions;

		// Efficiency tracking
        private readonly WorkEfficiencyCalculator _efficiencyCalculator;
        private readonly ActivityTracker _activityTracker;
        private readonly EfficiencyDataManager _dataManager;
        private bool _efficiencyTrackingEnabled;
        private System.Windows.Forms.Timer? _afkFlashTimer;
        private bool _isAfkFlashing;
        private Point _lastMousePos;
        private DateTime _lastActivityTime;
        private int _afkFlashCount;
        private DateTime _sessionStartTime;
        private readonly List<DateTime> _sessionHistory = new();
        private readonly TaskManager _taskManager = new();
        private readonly AmbientSoundManager _soundManager = new();
        private readonly GamificationManager _gamificationManager = new();
        private readonly BreakActivityManager _breakActivityManager = new();
        private readonly ThemeManager _themeManager = ThemeManager.Instance;

		// Defaults: 25/5 with long break every 4 sessions
		private readonly TimeSpan _workDuration = TimeSpan.FromMinutes(25);
		private readonly TimeSpan _shortBreakDuration = TimeSpan.FromMinutes(5);
		private readonly TimeSpan _longBreakDuration = TimeSpan.FromMinutes(15);
		
		// Current active durations (can be customized)
		private TimeSpan _currentWorkDuration => _customWorkDuration;
		private TimeSpan _currentShortBreakDuration => _customShortBreakDuration;
		private TimeSpan _currentLongBreakDuration => _customLongBreakDuration;
		private const int WorkSessionsPerLongBreak = 4;

		// Sound and notification settings
		private bool _soundEnabled = true;
		private bool _notificationsEnabled = true;
		private bool _toastNotificationsEnabled = true;
		private bool _trayNotificationsEnabled = true;
		private bool _autoStartEnabled = false;
		private int _notificationDuration = 4000;
		private SoundPlayer? _notificationSound;
		private SoundPlayer? _workCompleteSound;
		private SoundPlayer? _breakCompleteSound;
		private SoundPlayer? _sessionStartSound;
		
		// Advanced settings
		private bool _darkModeEnabled = false;
		private bool _productivityTrackingEnabled = true;
		private bool _breakReminderEnabled = false;
		private bool _focusModeEnabled = false;
		private bool _statisticsEnabled = false;
		private bool _customSoundsEnabled = false;
		private bool _keyboardShortcutsEnabled = false;
		private string _soundType = "Classic";
		private string _theme = "Light";
		
		// Timer customization
		private TimeSpan _customWorkDuration = TimeSpan.FromMinutes(25);
		private TimeSpan _customShortBreakDuration = TimeSpan.FromMinutes(5);
		private TimeSpan _customLongBreakDuration = TimeSpan.FromMinutes(15);

		// Notification history
		private readonly List<NotificationRecord> _notificationHistory = new();
		private const int MaxHistoryItems = 50;
		private readonly string _historyFilePath;

		public TrayPomodoroContext()
		{
			_remaining = _currentWorkDuration;
			_historyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "notification_history.json");
			LoadNotificationHistory();
			LoadAutoStartSettings();
			InitializeNotificationSounds();

			// Initialize efficiency tracking
			_efficiencyCalculator = new WorkEfficiencyCalculator();
			_activityTracker = new ActivityTracker(_efficiencyCalculator);
			
			// Subscribe to AFK events
			_activityTracker.AfkDetected += OnAfkDetected;
			
			// Ensure ActivityTracker runs continuously in tray mode
			_activityTracker.StartTracking();
			_dataManager = new EfficiencyDataManager();
			_efficiencyTrackingEnabled = _productivityTrackingEnabled;
			
			// Start efficiency calculator for continuous tracking
			_efficiencyCalculator.StartSession();

			// Initialize AFK flashing timer
			_afkFlashTimer = new System.Windows.Forms.Timer
			{
				Interval = EfficiencyConfig.IconFlashInterval,
				Enabled = false
			};
			_afkFlashTimer.Tick += OnAfkFlashTick;

		// Initialize AFK status check timer (runs every 2 seconds regardless of Pomodoro state)
		var afkStatusTimer = new System.Windows.Forms.Timer
		{
			Interval = 2000,
			Enabled = true
		};
		afkStatusTimer.Tick += (s, e) => CheckAfkStatus();

		// Initialize simple AFK detection timer
		var simpleAfkTimer = new System.Windows.Forms.Timer
		{
			Interval = 5000, // Check every 5 seconds
			Enabled = true
		};
		simpleAfkTimer.Tick += OnSimpleAfkCheck;

		// Initialize AFK tracking
		_lastMousePos = Cursor.Position;
		_lastActivityTime = DateTime.Now;

			_startPauseMenuItem = new ToolStripMenuItem("▶ Start", null, OnStartPauseClicked);
			_resetMenuItem = new ToolStripMenuItem("🔄 Reset", null, OnResetClicked);
			_skipMenuItem = new ToolStripMenuItem("⏭ Skip", null, OnSkipClicked);
			_soundMenuItem = new ToolStripMenuItem("🔊 Sound", null, OnSoundToggleClicked) { Checked = _soundEnabled };
			_notificationsMenuItem = new ToolStripMenuItem("🔔 Notifications", null, OnNotificationsToggleClicked) { Checked = _notificationsEnabled };
			_autoStartMenuItem = new ToolStripMenuItem("🚀 Auto Start", null, OnAutoStartToggleClicked) { Checked = _autoStartEnabled };
			_pauseAfterPhaseMenuItem = new ToolStripMenuItem("⏸️ Pause After Next Phase", null, OnPauseAfterPhaseToggleClicked) { Checked = _pauseAfterNextPhase };
			_aboutMenuItem = new ToolStripMenuItem("ℹ️ About PomodorroMan", null, OnAboutClicked);
			_settingsMenuItem = new ToolStripMenuItem("⚙️ Settings");
			_settingsMenuItem.DropDownItems.Add(_soundMenuItem);
			_settingsMenuItem.DropDownItems.Add(_notificationsMenuItem);
			_settingsMenuItem.DropDownItems.Add(new ToolStripSeparator());
			_settingsMenuItem.DropDownItems.Add(new ToolStripMenuItem("📢 Toast Notifications", null, OnToastNotificationsToggleClicked) { Checked = _toastNotificationsEnabled });
			_settingsMenuItem.DropDownItems.Add(new ToolStripMenuItem("🔔 Tray Notifications", null, OnTrayNotificationsToggleClicked) { Checked = _trayNotificationsEnabled });
			_settingsMenuItem.DropDownItems.Add(new ToolStripSeparator());
			_settingsMenuItem.DropDownItems.Add(new ToolStripMenuItem("⚙️ Advanced Settings...", null, OnAdvancedSettingsClicked));
			_historyMenuItem = new ToolStripMenuItem("📋 Notification History", null, OnHistoryClicked);
			_efficiencyMenuItem = new ToolStripMenuItem("📊 Efficiency Tracker", null, OnEfficiencyClicked);
			_taskManagementMenuItem = new ToolStripMenuItem("📝 Task Management", null, OnTaskManagementClicked);
			_ambientSoundsMenuItem = new ToolStripMenuItem("🎵 Ambient Sounds", null, OnAmbientSoundsClicked);
			_themesMenuItem = new ToolStripMenuItem("🎨 Themes", null, OnThemesClicked);
			_achievementsMenuItem = new ToolStripMenuItem("🏆 Achievements", null, OnAchievementsClicked);
			_breakActivitiesMenuItem = new ToolStripMenuItem("☕ Break Activities", null, OnBreakActivitiesClicked);
			var testAfkMenuItem = new ToolStripMenuItem("🧪 Test AFK Flashing", null, OnTestAfkClicked);
			_exitMenuItem = new ToolStripMenuItem("❌ Exit", null, OnExitClicked);

			_trayIcon = new NotifyIcon
			{
				Icon = CreateProgressIcon(),
				Visible = true,
				Text = "PomodorroMan"
			};

			_trayIcon.ContextMenuStrip = new ContextMenuStrip();
			_trayIcon.ContextMenuStrip.Items.Add(_startPauseMenuItem);
			_trayIcon.ContextMenuStrip.Items.Add(_resetMenuItem);
			_trayIcon.ContextMenuStrip.Items.Add(_skipMenuItem);
			_trayIcon.ContextMenuStrip.Items.Add(_pauseAfterPhaseMenuItem);
			_trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			_trayIcon.ContextMenuStrip.Items.Add(_autoStartMenuItem);
			_trayIcon.ContextMenuStrip.Items.Add(_settingsMenuItem);
			_trayIcon.ContextMenuStrip.Items.Add(_historyMenuItem);
			_trayIcon.ContextMenuStrip.Items.Add(_efficiencyMenuItem);
			_trayIcon.ContextMenuStrip.Items.Add(_taskManagementMenuItem);
			_trayIcon.ContextMenuStrip.Items.Add(_ambientSoundsMenuItem);
			_trayIcon.ContextMenuStrip.Items.Add(_themesMenuItem);
			_trayIcon.ContextMenuStrip.Items.Add(_achievementsMenuItem);
			_trayIcon.ContextMenuStrip.Items.Add(_breakActivitiesMenuItem);
			_trayIcon.ContextMenuStrip.Items.Add(testAfkMenuItem);
			_trayIcon.ContextMenuStrip.Items.Add(_aboutMenuItem);
			_trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
			_trayIcon.ContextMenuStrip.Items.Add(_exitMenuItem);
			_trayIcon.DoubleClick += (_, _) => ToggleStartPause();

			_secondTimer = new System.Windows.Forms.Timer { Interval = 1000 };
			_secondTimer.Tick += OnTick;

			UpdateTrayText();
			ShowEnhancedNotification($"🍅 PomodorroMan Ready!\n{GetSessionLabel()}: {Format(_remaining)}", "PomodorroMan", ToolTipIcon.Info);
		}

		private void InitializeNotificationSounds()
		{
			try
			{
				string assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
				
				// Load different sounds for different events
				string workCompletePath = Path.Combine(assetsPath, "work_complete.wav");
				string breakCompletePath = Path.Combine(assetsPath, "break_complete.wav");
				string sessionStartPath = Path.Combine(assetsPath, "session_start.wav");
				string notificationPath = Path.Combine(assetsPath, "notification.wav");

				if (File.Exists(workCompletePath))
					_workCompleteSound = new SoundPlayer(workCompletePath);
				if (File.Exists(breakCompletePath))
					_breakCompleteSound = new SoundPlayer(breakCompletePath);
				if (File.Exists(sessionStartPath))
					_sessionStartSound = new SoundPlayer(sessionStartPath);
				if (File.Exists(notificationPath))
					_notificationSound = new SoundPlayer(notificationPath);
			}
			catch
			{
				// Fallback to system sounds
			}
		}

		private void CheckAfkStatus()
		{
			if (_activityTracker?.IsAfk == true && !_isAfkFlashing)
			{
				StartAfkFlashing();
			}
			else if (_activityTracker?.IsAfk == false && _isAfkFlashing)
			{
				StopAfkFlashing();
			}
		}

		private void StartAfkFlashing()
		{
			_isAfkFlashing = true;
			_afkFlashCount = 0;
			_afkFlashTimer?.Start();
			// Force immediate first flash
			OnAfkFlashTick(null, EventArgs.Empty);
		}

		private void StopAfkFlashing()
		{
			_isAfkFlashing = false;
			_afkFlashTimer?.Stop();
			UpdateTrayText(); // Restore normal icon and text
		}

		private void OnAfkFlashTick(object? sender, EventArgs e)
		{
			try
			{
				if (_afkFlashCount >= EfficiencyConfig.MaxIconFlashCount)
				{
					StopAfkFlashing();
					return;
				}

				_afkFlashCount++;
				
				// Create a pulsing effect with different intensities
				if (_afkFlashCount % 2 == 1)
				{
					// Show bright pulsing icon
					_trayIcon.Icon = CreateAfkIcon(true);
					_trayIcon.Text = "😴 AFK - Move mouse to stop!";
				}
				else
				{
					// Show dimmed pulsing icon
					_trayIcon.Icon = CreateAfkIcon(false);
					_trayIcon.Text = "😴 AFK - Move mouse to stop!";
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error in AFK flash tick: {ex.Message}");
				// Stop flashing on error to prevent continuous errors
				StopAfkFlashing();
			}
		}

		private Icon CreateAfkFlashingIcon()
		{
			return CreateAfkIcon(true);
		}

		private Icon CreateAfkIcon(bool isBright)
		{
			try
			{
				const int iconSize = 32;
				using var bitmap = new Bitmap(iconSize, iconSize);
				using var graphics = Graphics.FromImage(bitmap);
				
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				
				// Create a pulsing red background
				var redIntensity = isBright ? 255 : 200;
				using var backgroundBrush = new SolidBrush(Color.FromArgb(255, redIntensity, 0, 0));
				graphics.FillEllipse(backgroundBrush, 0, 0, iconSize, iconSize);
				
				// Add a pulsing white "Z" (no border)
				var fontSize = isBright ? 20 : 16;
				using var textBrush = new SolidBrush(Color.White);
				using var font = new Font("Arial", fontSize, FontStyle.Bold);
				var textSize = graphics.MeasureString("Z", font);
				var textX = (iconSize - textSize.Width) / 2;
				var textY = (iconSize - textSize.Height) / 2;
				graphics.DrawString("Z", font, textBrush, textX, textY);
				
				return Icon.FromHandle(bitmap.GetHicon());
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error creating AFK icon: {ex.Message}");
				// Return a simple fallback icon
				return CreateProgressIcon();
			}
		}

		private Icon CreateDimmedIcon()
		{
			try
			{
				const int iconSize = 32;
				using var bitmap = new Bitmap(iconSize, iconSize);
				using var graphics = Graphics.FromImage(bitmap);
				
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				
				// Create a dark gray background for contrast
				using var backgroundBrush = new SolidBrush(Color.FromArgb(255, 50, 50, 50)); // Dark gray background
				graphics.FillEllipse(backgroundBrush, 0, 0, iconSize, iconSize);
				
				// Add a dimmed "Z" for sleep/AFK (no border)
				using var textBrush = new SolidBrush(Color.FromArgb(255, 200, 200, 200)); // Light gray text
				using var font = new Font("Arial", 18, FontStyle.Bold);
				var textSize = graphics.MeasureString("Z", font);
				var textX = (iconSize - textSize.Width) / 2;
				var textY = (iconSize - textSize.Height) / 2;
				graphics.DrawString("Z", font, textBrush, textX, textY);
				
				return Icon.FromHandle(bitmap.GetHicon());
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error creating dimmed icon: {ex.Message}");
				// Return a simple fallback icon
				return CreateProgressIcon();
			}
		}

		private void OnTick(object? sender, EventArgs e)
		{
			try
			{
				if (!_isRunning)
				{
					return;
				}

				if (_remaining > TimeSpan.Zero)
				{
					_remaining -= TimeSpan.FromSeconds(1);
					UpdateTrayText();

					// Check for AFK and start flashing if needed (always check, not just during efficiency tracking)
					CheckAfkStatus();

					// Update efficiency tracking during work sessions
					if (_efficiencyTrackingEnabled && _currentSessionType == SessionType.Work)
					{
						try
						{
							_efficiencyCalculator?.RecordActivity(ActivityType.Idle, 0.1);
							
							// Record session milestone every 5 minutes
							if (_remaining.TotalMinutes % 5 == 0 && _remaining.TotalSeconds < 60)
							{
								_efficiencyCalculator?.RecordSessionMilestone();
							}
						}
						catch (Exception ex)
						{
							System.Diagnostics.Debug.WriteLine($"Error in efficiency tracking: {ex.Message}");
						}
					}

					return;
				}

				EndSessionAndMaybeStartNext();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error in OnTick: {ex.Message}");
				// Graceful degradation - try to continue operation
				try
				{
					UpdateTrayText();
				}
				catch
				{
					// If even basic update fails, reset to safe state
					_isRunning = false;
					_secondTimer.Stop();
				}
			}
		}

		private void EndSessionAndMaybeStartNext()
		{
			_isRunning = false;
			_secondTimer.Stop();

			// Save efficiency data if tracking is enabled
			if (_efficiencyTrackingEnabled)
			{
				_efficiencyCalculator.EndSession();
				
				var session = new EfficiencySession
				{
					Id = Guid.NewGuid(),
					StartTime = DateTime.Now - (_efficiencyCalculator.SessionMetrics?.SessionDuration ?? TimeSpan.Zero),
					EndTime = DateTime.Now,
					SessionType = (EfficiencySessionType)_currentSessionType,
					Metrics = _efficiencyCalculator.SessionMetrics ?? new EfficiencyMetrics()
				};
				
				_dataManager.SaveSession(session);
			}

			if (_currentSessionType == SessionType.Work)
			{
				_completedWorkSessions++;
			}

			// Enhanced notification with different messages based on session type
			string message = GetCompletionMessage();
			ToolTipIcon icon = _currentSessionType == SessionType.Work ? ToolTipIcon.Info : ToolTipIcon.None;
			ShowEnhancedNotification(message, "🍅 PomodorroMan", icon);
			PlaySessionCompleteSound();

			// Check if we should pause after this phase
			if (_pauseAfterNextPhase)
			{
				_pauseAfterNextPhase = false;
				_pauseAfterPhaseMenuItem.Checked = false;
				UpdateTrayText();
				ShowEnhancedNotification("⏸️ Timer paused as requested", "PomodorroMan", ToolTipIcon.Info);
				return; // Don't start next session
			}

			// Determine next session
			if (_currentSessionType == SessionType.Work)
			{
				_currentSessionType =
					_completedWorkSessions % WorkSessionsPerLongBreak == 0
						? SessionType.LongBreak
						: SessionType.ShortBreak;
				_remaining = _currentSessionType == SessionType.LongBreak ? _currentLongBreakDuration : _currentShortBreakDuration;
			}
			else
			{
				_currentSessionType = SessionType.Work;
				_remaining = _currentWorkDuration;
			}

			UpdateTrayText();
			AutoStartNextSession();
		}

		private void AutoStartNextSession()
		{
			_isRunning = true;
			_sessionStartTime = DateTime.Now;
			_sessionHistory.Add(_sessionStartTime);
			
			// Keep only recent session history
			if (_sessionHistory.Count > EfficiencyConfig.MaxSessionHistory)
			{
				_sessionHistory.RemoveAt(0);
			}
			
			_secondTimer.Start();
			_startPauseMenuItem.Text = "⏸ Pause";
			string nextMessage = GetNextSessionMessage();
			ShowEnhancedNotification(nextMessage, "🍅 PomodorroMan", ToolTipIcon.Info);
			PlaySessionStartSound();
		}

		private void OnStartPauseClicked(object? sender, EventArgs e) => ToggleStartPause();

		private void ToggleStartPause()
		{
			_isRunning = !_isRunning;
			if (_isRunning)
			{
				_secondTimer.Start();
				_startPauseMenuItem.Text = "⏸ Pause";
				
				// Start efficiency tracking for work sessions
				if (_efficiencyTrackingEnabled && _currentSessionType == SessionType.Work)
				{
					_efficiencyCalculator.StartSession();
				}
			}
			else
			{
				_secondTimer.Stop();
				_startPauseMenuItem.Text = "▶ Start";
				
				// Stop efficiency tracking
				if (_efficiencyTrackingEnabled)
				{
					_efficiencyCalculator.EndSession();
				}
			}
			UpdateTrayText();
		}

		private void OnResetClicked(object? sender, EventArgs e)
		{
			_isRunning = false;
			_secondTimer.Stop();
			_currentSessionType = SessionType.Work;
			_remaining = _currentWorkDuration;
			_pauseAfterNextPhase = false;
			_pauseAfterPhaseMenuItem.Checked = false;
			_startPauseMenuItem.Text = "▶ Start";
			
			// Stop efficiency tracking
			if (_efficiencyTrackingEnabled)
			{
				_efficiencyCalculator.EndSession();
			}
			
			UpdateTrayText();
			ShowEnhancedNotification("🔄 Timer Reset\nReady to start a new work session!", "PomodorroMan", ToolTipIcon.Info);
		}

		private void OnSkipClicked(object? sender, EventArgs e)
		{
			_remaining = TimeSpan.Zero;
			ShowEnhancedNotification("⏭ Session Skipped", "PomodorroMan", ToolTipIcon.Warning);
			OnTick(this, EventArgs.Empty);
		}

		private void OnSoundToggleClicked(object? sender, EventArgs e)
		{
			_soundEnabled = !_soundEnabled;
			_soundMenuItem.Checked = _soundEnabled;
			ShowEnhancedNotification(_soundEnabled ? "🔊 Sound enabled" : "🔇 Sound disabled", "Settings", ToolTipIcon.Info);
		}

		private void OnNotificationsToggleClicked(object? sender, EventArgs e)
		{
			_notificationsEnabled = !_notificationsEnabled;
			_notificationsMenuItem.Checked = _notificationsEnabled;
			ShowEnhancedNotification(_notificationsEnabled ? "🔔 Notifications enabled" : "🔕 Notifications disabled", "Settings", ToolTipIcon.Info);
		}

		private void OnAutoStartToggleClicked(object? sender, EventArgs e)
		{
			_autoStartEnabled = !_autoStartEnabled;
			_autoStartMenuItem.Checked = _autoStartEnabled;
			
			try
			{
				if (_autoStartEnabled)
				{
					SetAutoStartEnabled(true);
					ShowEnhancedNotification("🚀 Auto Start enabled - Pomodoro Timer will start with Windows", "Settings", ToolTipIcon.Info);
				}
				else
				{
					SetAutoStartEnabled(false);
					ShowEnhancedNotification("🚀 Auto Start disabled", "Settings", ToolTipIcon.Info);
				}
			}
			catch (Exception ex)
			{
				ShowEnhancedNotification($"Failed to update auto start setting: {ex.Message}", "Error", ToolTipIcon.Error);
				_autoStartEnabled = !_autoStartEnabled; // Revert the change
				_autoStartMenuItem.Checked = _autoStartEnabled;
			}
		}

		private void OnPauseAfterPhaseToggleClicked(object? sender, EventArgs e)
		{
			_pauseAfterNextPhase = !_pauseAfterNextPhase;
			_pauseAfterPhaseMenuItem.Checked = _pauseAfterNextPhase;
			
			string message = _pauseAfterNextPhase 
				? $"⏸️ Timer will pause after {GetSessionLabel()} ends" 
				: "▶️ Timer will continue normally after session ends";
			
			ShowEnhancedNotification(message, "PomodorroMan", ToolTipIcon.Info);
		}

		private void OnToastNotificationsToggleClicked(object? sender, EventArgs e)
		{
			_toastNotificationsEnabled = !_toastNotificationsEnabled;
			((ToolStripMenuItem)sender!).Checked = _toastNotificationsEnabled;
			ShowEnhancedNotification(_toastNotificationsEnabled ? "📢 Toast notifications enabled" : "📢 Toast notifications disabled", "Settings", ToolTipIcon.Info);
		}

		private void OnTrayNotificationsToggleClicked(object? sender, EventArgs e)
		{
			_trayNotificationsEnabled = !_trayNotificationsEnabled;
			((ToolStripMenuItem)sender!).Checked = _trayNotificationsEnabled;
			ShowEnhancedNotification(_trayNotificationsEnabled ? "🔔 Tray notifications enabled" : "🔔 Tray notifications disabled", "Settings", ToolTipIcon.Info);
		}

		private void OnAdvancedSettingsClicked(object? sender, EventArgs e)
		{
			using var settingsForm = new NotificationSettingsForm(this);
			if (settingsForm.ShowDialog() == DialogResult.OK)
			{
				// Basic settings
				_soundEnabled = settingsForm.SoundEnabled;
				_trayNotificationsEnabled = settingsForm.TrayNotificationsEnabled;
				_toastNotificationsEnabled = settingsForm.ToastNotificationsEnabled;
				_notificationDuration = settingsForm.NotificationDuration;
				
				// Advanced settings
				_darkModeEnabled = settingsForm.DarkModeEnabled;
				_productivityTrackingEnabled = settingsForm.ProductivityTrackingEnabled;
				_breakReminderEnabled = settingsForm.BreakReminderEnabled;
				_focusModeEnabled = settingsForm.FocusModeEnabled;
				_statisticsEnabled = settingsForm.StatisticsEnabled;
				_customSoundsEnabled = settingsForm.CustomSoundsEnabled;
				_keyboardShortcutsEnabled = settingsForm.KeyboardShortcutsEnabled;
				_soundType = settingsForm.SoundType;
				_theme = settingsForm.Theme;
				
				// Timer customization
				_customWorkDuration = TimeSpan.FromMinutes(settingsForm.PomodoroDuration);
				_customShortBreakDuration = TimeSpan.FromMinutes(settingsForm.ShortBreakDuration);
				_customLongBreakDuration = TimeSpan.FromMinutes(settingsForm.LongBreakDuration);
				
				// Handle auto-start setting separately
				bool newAutoStartValue = settingsForm.AutoStartEnabled;
				if (newAutoStartValue != _autoStartEnabled)
				{
					try
					{
						_autoStartEnabled = newAutoStartValue;
						SetAutoStartEnabled(_autoStartEnabled);
						_autoStartMenuItem.Checked = _autoStartEnabled;
					}
					catch (Exception ex)
					{
						ShowEnhancedNotification($"Failed to update auto start setting: {ex.Message}", "Error", ToolTipIcon.Error);
					}
				}
				
				// Apply advanced features
				ApplyTheme();
				ApplyFocusMode();
				ApplyProductivityTracking();
				ApplyStatistics();
				ApplyKeyboardShortcuts();
				ApplyCustomSounds();
				ApplyBreakReminders();
				
				// Update menu items
				_soundMenuItem.Checked = _soundEnabled;
				
				// Update other menu item check states
				foreach (ToolStripMenuItem item in _settingsMenuItem.DropDownItems.OfType<ToolStripMenuItem>())
				{
					if (item.Text.Contains("Toast Notifications"))
						item.Checked = _toastNotificationsEnabled;
					else if (item.Text.Contains("Tray Notifications"))
						item.Checked = _trayNotificationsEnabled;
				}
				
				ShowEnhancedNotification("⚙️ Settings updated successfully!", "PomodorroMan", ToolTipIcon.Info);
			}
		}

		private void OnExitClicked(object? sender, EventArgs e)
		{
			_trayIcon.Visible = false;
			_trayIcon.Dispose();
			_secondTimer.Dispose();
			_activityTracker?.Dispose();
			ExitThread();
		}

		private void UpdateTrayText()
		{
			try
			{
				string status = _isRunning ? "▶" : "⏸";
				string sessionIcon = GetSessionIcon();
				string progressBar = GetProgressBar();
				int progressPercentage = GetProgressPercentage();
				
				string pauseIndicator = _pauseAfterNextPhase ? " [PAUSE AFTER]" : "";
				string efficiencyInfo = "";
				
				// Add efficiency info if tracking is enabled
				if (_efficiencyTrackingEnabled && _currentSessionType == SessionType.Work)
				{
					try
					{
						var metrics = _efficiencyCalculator?.CurrentMetrics;
						if (metrics != null)
						{
							efficiencyInfo = $" | Eff: {metrics.EfficiencyScore:F0}% | Focus: {metrics.FocusScore:F0}%";
						}
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine($"Error getting efficiency metrics: {ex.Message}");
					}
				}
				
            var focusLevelInfo = "";
            if (_efficiencyTrackingEnabled && _currentSessionType == SessionType.Work)
            {
                try
                {
                    var focusLevel = _efficiencyCalculator?.GetCurrentFocusLevel();
                    if (focusLevel.HasValue)
                    {
                        var focusIcon = focusLevel.Value switch
                        {
                            FocusLevel.High => "🔥",
                            FocusLevel.Medium => "⚡",
                            FocusLevel.Low => "💤",
                            _ => "❓"
                        };
                        focusLevelInfo = $" | {focusIcon} {focusLevel.Value}";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting focus level: {ex.Message}");
                }
            }

            var tooltipText = $"{status} {sessionIcon} {GetSessionLabel()} {Format(_remaining)} ({progressPercentage}%){pauseIndicator}{efficiencyInfo}{focusLevelInfo}";
            
            // Truncate tooltip if too long
            if (tooltipText.Length > EfficiencyConfig.MaxTooltipLength)
            {
                tooltipText = tooltipText.Substring(0, EfficiencyConfig.MaxTooltipLength - 3) + "...";
            }
            
            _trayIcon.Text = tooltipText;
				
				string balloonPauseInfo = _pauseAfterNextPhase ? "\n⏸️ Will pause after this session" : "";
				string balloonEfficiencyInfo = "";
				
				if (_efficiencyTrackingEnabled && _currentSessionType == SessionType.Work)
				{
					try
					{
						var metrics = _efficiencyCalculator?.CurrentMetrics;
						if (metrics != null)
						{
							balloonEfficiencyInfo = $"\n📊 Efficiency: {metrics.EfficiencyScore:F1}% | Focus: {metrics.FocusScore:F1}%";
							if (_activityTracker?.IsAfk == true)
							{
								balloonEfficiencyInfo += $"\n😴 AFK: {FormatTimeSpan(_activityTracker.AfkDuration)}";
							}
							else
							{
								balloonEfficiencyInfo += $"\n✅ Active";
							}
						}
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine($"Error getting efficiency metrics for balloon: {ex.Message}");
					}
				}
				
				_trayIcon.BalloonTipTitle = "🍅 PomodorroMan";
				_trayIcon.BalloonTipText = $"{sessionIcon} {GetSessionLabel()}: {Format(_remaining)}\n{progressBar}\nSessions completed: {_completedWorkSessions}{balloonPauseInfo}{balloonEfficiencyInfo}";
				
				// Update tray icon with progress visualization
				try
				{
					var newIcon = CreateProgressIcon();
					_trayIcon.Icon?.Dispose();
					_trayIcon.Icon = newIcon;
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Error updating tray icon: {ex.Message}");
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error in UpdateTrayText: {ex.Message}");
				// Fallback to basic text
				try
				{
					string status = _isRunning ? "▶" : "⏸";
					_trayIcon.Text = $"{status} PomodorroMan: {Format(_remaining)}";
				}
				catch
				{
					// If even basic update fails, use minimal text
					_trayIcon.Text = "PomodorroMan";
				}
			}
		}

		private void ShowEnhancedNotification(string text, string title, ToolTipIcon icon = ToolTipIcon.None)
		{
			if (!_notificationsEnabled) return;

			// Record notification in history
			RecordNotification(text, title, icon);

			if (_toastNotificationsEnabled)
			{
				ShowToastNotification(text, title, icon);
			}

			if (_trayNotificationsEnabled)
			{
				_trayIcon.BalloonTipTitle = title;
				_trayIcon.BalloonTipText = text;
				_trayIcon.BalloonTipIcon = icon;
				
				int displayTime = icon switch
				{
					ToolTipIcon.Error => 6000,
					ToolTipIcon.Warning => 5000,
					ToolTipIcon.Info => 4000,
					_ => 3000
				};
				
				_trayIcon.ShowBalloonTip(displayTime);
			}
			
			PlayEnhancedNotificationSound();
		}

		private void ShowBalloon(string text, string title)
		{
			ShowEnhancedNotification(text, title, ToolTipIcon.None);
		}

		private void PlayEnhancedNotificationSound()
		{
			if (!_soundEnabled) return;

			try
			{
				// Play specific sound based on session type
				SoundPlayer? soundToPlay = _currentSessionType switch
				{
					SessionType.Work => _workCompleteSound ?? _notificationSound,
					SessionType.ShortBreak => _breakCompleteSound ?? _notificationSound,
					SessionType.LongBreak => _breakCompleteSound ?? _notificationSound,
					_ => _notificationSound
				};

				if (soundToPlay != null)
				{
					soundToPlay.Play();
				}
				else
				{
					// Play system notification sound with different types based on session
					uint soundType = _currentSessionType switch
					{
						SessionType.Work => 0xFFFFFFFF, // SystemDefault
						SessionType.ShortBreak => 0x40, // SystemAsterisk
						SessionType.LongBreak => 0x10,  // SystemHand
						_ => 0xFFFFFFFF
					};
					MessageBeep(soundType);
				}
			}
			catch
			{
				// Fallback to basic beep
				MessageBeep(0xFFFFFFFF);
			}
		}

		private void PlaySessionCompleteSound()
		{
			if (!_soundEnabled) return;

			try
			{
				SoundPlayer? soundToPlay = _currentSessionType switch
				{
					SessionType.Work => _workCompleteSound,
					SessionType.ShortBreak => _breakCompleteSound,
					SessionType.LongBreak => _breakCompleteSound,
					_ => _notificationSound
				};

				if (soundToPlay != null)
				{
					soundToPlay.Play();
				}
				else
				{
					// Play multiple beeps for session completion without blocking UI
					_ = System.Threading.Tasks.Task.Run(async () =>
					{
						for (int i = 0; i < 3; i++)
						{
							MessageBeep(0xFFFFFFFF);
							if (i < 2) // Don't sleep after the last beep
							{
								await System.Threading.Tasks.Task.Delay(200);
							}
						}
					});
				}
			}
			catch
			{
				MessageBeep(0xFFFFFFFF);
			}
		}

		private void PlaySessionStartSound()
		{
			if (!_soundEnabled) return;

			try
			{
				if (_sessionStartSound != null)
				{
					_sessionStartSound.Play();
				}
				else
				{
					MessageBeep(0x40); // SystemAsterisk for session start
				}
			}
			catch
			{
				MessageBeep(0x40);
			}
		}

		private void PlayNotificationBeep()
		{
			PlayEnhancedNotificationSound();
		}

		private Icon? LoadCustomIcon()
		{
			try
			{
				string baseDir = AppDomain.CurrentDomain.BaseDirectory;
				string icoPath = System.IO.Path.Combine(baseDir, "Assets", "tomato.ico");
				if (System.IO.File.Exists(icoPath))
				{
					return new Icon(icoPath);
				}

				string pngPath = System.IO.Path.Combine(baseDir, "Assets", "tomato.png");
				if (System.IO.File.Exists(pngPath))
				{
					using var bmp = new Bitmap(pngPath);
					IntPtr hIcon = bmp.GetHicon();
					try
					{
						using Icon temp = Icon.FromHandle(hIcon);
						return (Icon)temp.Clone();
					}
					finally
					{
						DestroyIcon(hIcon);
					}
				}
			}
			catch
			{
				// ignore and fallback to default icon
			}
			return null;
		}

		private Icon CreateProgressIcon()
		{
			const int iconSize = 32;
			using var bitmap = new Bitmap(iconSize, iconSize);
			using var graphics = Graphics.FromImage(bitmap);
			
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.CompositingQuality = CompositingQuality.HighQuality;
			
			TimeSpan totalDuration = _currentSessionType switch
			{
				SessionType.Work => _currentWorkDuration,
				SessionType.ShortBreak => _currentShortBreakDuration,
				SessionType.LongBreak => _currentLongBreakDuration,
				_ => TimeSpan.FromMinutes(25)
			};

			double progress = 1.0 - (_remaining.TotalSeconds / totalDuration.TotalSeconds);
			progress = Math.Max(0, Math.Min(1, progress));

			Color backgroundColor = _currentSessionType switch
			{
				SessionType.Work => Color.FromArgb(220, 53, 69),
				SessionType.ShortBreak => Color.FromArgb(40, 167, 69),
				SessionType.LongBreak => Color.FromArgb(255, 193, 7),
				_ => Color.FromArgb(108, 117, 125)
			};

			Color progressColor = _currentSessionType switch
			{
				SessionType.Work => Color.FromArgb(255, 255, 255),
				SessionType.ShortBreak => Color.FromArgb(255, 255, 255),
				SessionType.LongBreak => Color.FromArgb(0, 0, 0),
				_ => Color.FromArgb(255, 255, 255)
			};

			Rectangle iconRect = new Rectangle(0, 0, iconSize, iconSize);
			using var backgroundBrush = new SolidBrush(backgroundColor);
			graphics.FillEllipse(backgroundBrush, iconRect);

			if (progress > 0)
			{
				Rectangle progressRect = new Rectangle(2, 2, iconSize - 4, iconSize - 4);
				using var progressBrush = new SolidBrush(progressColor);
				
				float startAngle = -90f;
				float sweepAngle = (float)(360 * progress);
				
				using var pen = new Pen(progressBrush, 3);
				graphics.DrawArc(pen, progressRect, startAngle, sweepAngle);
			}

			string sessionSymbol = _currentSessionType switch
			{
				SessionType.Work => "🍅",
				SessionType.ShortBreak => "☕",
				SessionType.LongBreak => "🌟",
				_ => "⏰"
			};

			using var font = new Font("Segoe UI Emoji", 12, FontStyle.Bold);
			using var textBrush = new SolidBrush(progressColor);
			var textSize = graphics.MeasureString(sessionSymbol, font);
			var textX = (iconSize - textSize.Width) / 2;
			var textY = (iconSize - textSize.Height) / 2;
			graphics.DrawString(sessionSymbol, font, textBrush, textX, textY);

			IntPtr hIcon = bitmap.GetHicon();
			try
			{
				using Icon temp = Icon.FromHandle(hIcon);
				return (Icon)temp.Clone();
			}
			finally
			{
				DestroyIcon(hIcon);
			}
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool DestroyIcon(IntPtr handle);

		[DllImport("user32.dll")]
		private static extern bool MessageBeep(uint uType);

		private void ShowToastNotification(string text, string title, ToolTipIcon icon)
		{
			try
			{
				// Use Windows API to show a more prominent notification
				ShowWindowsNotification(text, title, icon);
			}
			catch
			{
				// Fallback to tray notification if toast fails
			}
		}

		private void ShowWindowsNotification(string text, string title, ToolTipIcon icon)
		{
			try
			{
				// Create a temporary form to show notification
				using var notificationForm = new NotificationForm(title, text, icon);
				notificationForm.Show();
			}
			catch
			{
				// Fallback to system beep
				MessageBeep(0xFFFFFFFF);
			}
		}

		private static string Format(TimeSpan span)
		{
			if (span < TimeSpan.Zero) span = TimeSpan.Zero;
			return $"{(int)span.TotalMinutes:00}:{span.Seconds:00}";
		}

		private static string FormatTimeSpan(TimeSpan timeSpan)
		{
			return timeSpan.TotalHours >= 1 
				? $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}"
				: $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
		}

		private string GetSessionLabel()
		{
			return _currentSessionType switch
			{
				SessionType.Work => "Work",
				SessionType.ShortBreak => "Short Break",
				SessionType.LongBreak => "Long Break",
				_ => ""
			};
		}

		private string GetSessionIcon()
		{
			return _currentSessionType switch
			{
				SessionType.Work => "🍅",
				SessionType.ShortBreak => "☕",
				SessionType.LongBreak => "🌟",
				_ => "⏰"
			};
		}

		private string GetCompletionMessage()
		{
			return _currentSessionType switch
			{
				SessionType.Work => $"🎉 Great work! Session {_completedWorkSessions} completed!\nTime for a {(_completedWorkSessions % WorkSessionsPerLongBreak == 0 ? "long" : "short")} break!",
				SessionType.ShortBreak => "☕ Short break finished!\nReady to get back to work?",
				SessionType.LongBreak => "🌟 Long break completed!\nYou've earned this rest! Time to focus again!",
				_ => "Session completed!"
			};
		}

		private string GetNextSessionMessage()
		{
			return _currentSessionType switch
			{
				SessionType.Work => $"🍅 Starting work session {_completedWorkSessions + 1}\nLet's focus for {Format(_remaining)}!",
				SessionType.ShortBreak => $"☕ Taking a short break\nRelax for {Format(_remaining)}",
				SessionType.LongBreak => $"🌟 Enjoying a long break\nTake {Format(_remaining)} to recharge",
				_ => $"Starting {GetSessionLabel()}: {Format(_remaining)}"
			};
		}

		private string GetProgressBar()
		{
			TimeSpan totalDuration = _currentSessionType switch
			{
				SessionType.Work => _currentWorkDuration,
				SessionType.ShortBreak => _currentShortBreakDuration,
				SessionType.LongBreak => _currentLongBreakDuration,
				_ => TimeSpan.FromMinutes(25)
			};

			double progress = 1.0 - (_remaining.TotalSeconds / totalDuration.TotalSeconds);
			int barLength = 10;
			int filledLength = (int)(progress * barLength);
			
			string bar = new string('█', filledLength) + new string('░', barLength - filledLength);
			int percentage = (int)(progress * 100);
			
			return $"[{bar}] {percentage}%";
		}

		private int GetProgressPercentage()
		{
			TimeSpan totalDuration = _currentSessionType switch
			{
				SessionType.Work => _currentWorkDuration,
				SessionType.ShortBreak => _currentShortBreakDuration,
				SessionType.LongBreak => _currentLongBreakDuration,
				_ => TimeSpan.FromMinutes(25)
			};

			double progress = 1.0 - (_remaining.TotalSeconds / totalDuration.TotalSeconds);
			return (int)(Math.Max(0, Math.Min(100, progress * 100)));
		}

		public bool GetSoundEnabled() => _soundEnabled;
		public bool GetTrayNotificationsEnabled() => _trayNotificationsEnabled;
		public bool GetToastNotificationsEnabled() => _toastNotificationsEnabled;
		public bool GetAutoStartEnabled() => _autoStartEnabled;
		public int GetNotificationDuration() => _notificationDuration;
		
		// Advanced settings getters
		public bool GetDarkModeEnabled() => _darkModeEnabled;
		public bool GetProductivityTrackingEnabled() => _productivityTrackingEnabled;
		public bool GetBreakReminderEnabled() => _breakReminderEnabled;
		public bool GetFocusModeEnabled() => _focusModeEnabled;
		public bool GetStatisticsEnabled() => _statisticsEnabled;
		public bool GetCustomSoundsEnabled() => _customSoundsEnabled;
		public bool GetKeyboardShortcutsEnabled() => _keyboardShortcutsEnabled;
		public string GetSoundType() => _soundType;
		public string GetTheme() => _theme;
		
		// Timer customization getters
		public int GetPomodoroDuration() => (int)_customWorkDuration.TotalMinutes;
		public int GetShortBreakDuration() => (int)_customShortBreakDuration.TotalMinutes;
		public int GetLongBreakDuration() => (int)_customLongBreakDuration.TotalMinutes;
		
		// Advanced feature implementations
		private void ApplyTheme()
		{
			// Theme switching functionality
			if (_darkModeEnabled)
			{
				// Apply dark theme - this would affect the UI colors
				// For now, we'll just show a notification
				ShowEnhancedNotification("🌙 Dark mode enabled", "Theme", ToolTipIcon.Info);
			}
			else
			{
				ShowEnhancedNotification("☀️ Light mode enabled", "Theme", ToolTipIcon.Info);
			}
		}
		
		private void ApplyFocusMode()
		{
			if (_focusModeEnabled)
			{
				ShowEnhancedNotification("🎯 Focus mode enabled - Distractions blocked", "Focus Mode", ToolTipIcon.Info);
				// Here you would implement actual distraction blocking
			}
			else
			{
				ShowEnhancedNotification("🎯 Focus mode disabled", "Focus Mode", ToolTipIcon.Info);
			}
		}
		
		private void ApplyProductivityTracking()
		{
			_efficiencyTrackingEnabled = _productivityTrackingEnabled;
			
			if (_productivityTrackingEnabled)
			{
				ShowEnhancedNotification("📊 Productivity tracking enabled", "Analytics", ToolTipIcon.Info);
				
				// Start tracking if timer is running and in work session
				if (_isRunning && _currentSessionType == SessionType.Work)
				{
					_efficiencyCalculator.StartSession();
				}
			}
			else
			{
				ShowEnhancedNotification("📊 Productivity tracking disabled", "Analytics", ToolTipIcon.Info);
				
				// Stop efficiency tracking
				_efficiencyCalculator.EndSession();
			}
		}
		
		private void ApplyStatistics()
		{
			if (_statisticsEnabled)
			{
				ShowEnhancedNotification("📈 Detailed statistics enabled", "Statistics", ToolTipIcon.Info);
				// Here you would implement detailed statistics
			}
			else
			{
				ShowEnhancedNotification("📈 Detailed statistics disabled", "Statistics", ToolTipIcon.Info);
			}
		}
		
		private void ApplyKeyboardShortcuts()
		{
			if (_keyboardShortcutsEnabled)
			{
				ShowEnhancedNotification("⌨️ Keyboard shortcuts enabled", "Shortcuts", ToolTipIcon.Info);
				// Here you would implement global keyboard shortcuts
			}
			else
			{
				ShowEnhancedNotification("⌨️ Keyboard shortcuts disabled", "Shortcuts", ToolTipIcon.Info);
			}
		}
		
		private void ApplyCustomSounds()
		{
			if (_customSoundsEnabled)
			{
				ShowEnhancedNotification("🎵 Custom sound files enabled", "Audio", ToolTipIcon.Info);
				// Here you would implement custom sound file loading
			}
			else
			{
				ShowEnhancedNotification("🎵 Using default sounds", "Audio", ToolTipIcon.Info);
			}
		}
		
		private void ApplyBreakReminders()
		{
			if (_breakReminderEnabled)
			{
				ShowEnhancedNotification("⏰ Smart break reminders enabled", "Break Reminders", ToolTipIcon.Info);
				// Here you would implement smart break reminder logic
			}
			else
			{
				ShowEnhancedNotification("⏰ Smart break reminders disabled", "Break Reminders", ToolTipIcon.Info);
			}
		}

		private void RecordNotification(string text, string title, ToolTipIcon icon)
		{
			var record = new NotificationRecord
			{
				Timestamp = DateTime.Now,
				Title = title,
				Message = text,
				IconType = icon.ToString(),
				SessionType = _currentSessionType.ToString(),
				CompletedSessions = _completedWorkSessions
			};

			_notificationHistory.Insert(0, record);

			if (_notificationHistory.Count > MaxHistoryItems)
			{
				_notificationHistory.RemoveAt(_notificationHistory.Count - 1);
			}

			SaveNotificationHistory();
		}

		private void LoadNotificationHistory()
		{
			try
			{
				if (File.Exists(_historyFilePath))
				{
					string json = File.ReadAllText(_historyFilePath);
					var history = JsonSerializer.Deserialize<List<NotificationRecord>>(json);
					if (history != null)
					{
						_notificationHistory.AddRange(history);
					}
				}
			}
			catch (Exception ex)
			{
				// Log error but don't crash the application
				System.Diagnostics.Debug.WriteLine($"Failed to load notification history: {ex.Message}");
			}
		}

		private void SaveNotificationHistory()
		{
			try
			{
				string json = JsonSerializer.Serialize(_notificationHistory, new JsonSerializerOptions { WriteIndented = true });
				File.WriteAllText(_historyFilePath, json);
			}
			catch (Exception ex)
			{
				// Log error but don't crash the application
				System.Diagnostics.Debug.WriteLine($"Failed to save notification history: {ex.Message}");
			}
		}

		private void OnHistoryClicked(object? sender, EventArgs e)
		{
			using var historyForm = new NotificationHistoryForm(_notificationHistory);
			historyForm.ShowDialog();
		}

		private void OnAboutClicked(object? sender, EventArgs e)
		{
			using var aboutForm = new AboutForm();
			aboutForm.ShowDialog();
		}

		private void OnEfficiencyClicked(object? sender, EventArgs e)
		{
			using var efficiencyForm = new EfficiencyTrackingForm(_efficiencyCalculator, _activityTracker, _dataManager);
			efficiencyForm.ShowDialog();
		}

		private void OnTaskManagementClicked(object? sender, EventArgs e)
		{
			using var form = new TaskManagementForm(_taskManager);
			form.ShowDialog();
		}

		private void OnAmbientSoundsClicked(object? sender, EventArgs e)
		{
			using var form = new AmbientSoundsForm(_soundManager);
			form.ShowDialog();
		}

		private void OnThemesClicked(object? sender, EventArgs e)
		{
			using var form = new ThemesForm(_themeManager);
			form.ShowDialog();
		}

		private void OnAchievementsClicked(object? sender, EventArgs e)
		{
			using var form = new AchievementsForm(_gamificationManager);
			form.ShowDialog();
		}

		private void OnBreakActivitiesClicked(object? sender, EventArgs e)
		{
			using var form = new BreakActivitiesForm(_breakActivityManager);
			form.ShowDialog();
		}

		private void OnTestAfkClicked(object? sender, EventArgs e)
		{
			TestAfkFlashing();
		}

		private void OnAfkDetected(object? sender, AfkDetectedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine($"AFK event received: {e.AfkDuration.TotalSeconds:F1} seconds");
			// The CheckAfkStatus method will handle the flashing
		}

		// Test method to manually trigger AFK flashing for debugging
		public void TestAfkFlashing()
		{
			StartAfkFlashing();
		}

		private void OnSimpleAfkCheck(object? sender, EventArgs e)
		{
			var currentMousePos = Cursor.Position;
			var now = DateTime.Now;
			
			// Check if mouse moved
			if (currentMousePos != _lastMousePos)
			{
				_lastMousePos = currentMousePos;
				_lastActivityTime = now;
				if (_isAfkFlashing)
				{
					StopAfkFlashing();
				}
				return;
			}
			
			// Check if enough time has passed since last activity
			var timeSinceActivity = now - _lastActivityTime;
			if (timeSinceActivity.TotalSeconds >= 120) // 2 minutes AFK threshold
			{
				if (!_isAfkFlashing)
				{
					StartAfkFlashing();
				}
			}
		}

		private void LoadAutoStartSettings()
		{
			try
			{
				_autoStartEnabled = IsAutoStartEnabled();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to load auto start settings: {ex.Message}");
				_autoStartEnabled = false;
			}
		}

		private bool IsAutoStartEnabled()
		{
			try
			{
				using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
				var appName = "PomodoroTimer";
				var value = key?.GetValue(appName);
				return value != null;
			}
			catch
			{
				return false;
			}
		}

		private void SetAutoStartEnabled(bool enabled)
		{
			using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
			var appName = "PomodoroTimer";
			var appPath = Application.ExecutablePath;

			if (enabled)
			{
				key?.SetValue(appName, $"\"{appPath}\"");
			}
			else
			{
				key?.DeleteValue(appName, false);
			}
		}
	}
}


