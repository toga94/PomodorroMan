using System;
using System.Drawing;
using System.Windows.Forms;

namespace PomodorroMan
{
    public partial class NotificationSettingsForm : Form
    {
        private readonly TrayPomodoroContext _parent;
        
        private CheckBox _soundEnabledCheckBox = null!;
        private CheckBox _trayNotificationsCheckBox = null!;
        private CheckBox _toastNotificationsCheckBox = null!;
        private CheckBox _autoStartCheckBox = null!;
        private CheckBox _themeCheckBox = null!;
        private CheckBox _productivityCheckBox = null!;
        private CheckBox _breakReminderCheckBox = null!;
        private CheckBox _focusModeCheckBox = null!;
        private CheckBox _statisticsCheckBox = null!;
        private CheckBox _customSoundsCheckBox = null!;
        private CheckBox _keyboardShortcutsCheckBox = null!;
        private NumericUpDown _notificationDurationNumeric = null!;
        private NumericUpDown _pomodoroDurationNumeric = null!;
        private NumericUpDown _shortBreakNumeric = null!;
        private NumericUpDown _longBreakNumeric = null!;
        private ComboBox _soundTypeComboBox = null!;
        private ComboBox _themeComboBox = null!;
        private Button _okButton = null!;
        private Button _cancelButton = null!;

        internal NotificationSettingsForm(TrayPomodoroContext parent)
        {
            _parent = parent;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            Text = "Settings - PomodorroMan v1.8.1";
            Size = new Size(800, 650);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(248, 249, 250);
            MinimumSize = new Size(750, 600);

            var mainTableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };

            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

            var headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(220, 53, 69),
                Padding = new Padding(0)
            };

            var headerTableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };

            headerTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            headerTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            headerTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));

            var iconLabel = new Label
            {
                Text = "⚙️",
                Font = new Font("Segoe UI Emoji", 48, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            var appNameLabel = new Label
            {
                Text = "Settings",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            var versionLabel = new Label
            {
                Text = "PomodorroMan v1.8.1",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(255, 255, 255, 200),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(25, 20, 25, 15),
                AutoScroll = true
            };

            var contentTableLayout = new TableLayoutPanel
            {
                AutoSize = true,
                RowCount = 9,
                ColumnCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 5, 0, 5),
                Location = new Point(0, 0)
            };

            contentTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            contentTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            contentTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));
            contentTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            contentTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
            contentTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 8));
            contentTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            contentTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));
            contentTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var notificationSectionLabel = new Label
            {
                Text = "🔔 Notification Settings",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            var notificationPanel = new Panel
            {
                AutoSize = true,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(15, 10, 15, 10)
            };

            var notificationTableLayout = new TableLayoutPanel
            {
                AutoSize = true,
                RowCount = 6,
                ColumnCount = 2,
                BackColor = Color.Transparent,
                Padding = new Padding(5)
            };

            notificationTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            notificationTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            notificationTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            notificationTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            notificationTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            notificationTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            notificationTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            notificationTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            _soundEnabledCheckBox = new CheckBox
            {
                Text = "🔊 Enable Sound Notifications",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                CheckAlign = ContentAlignment.MiddleLeft
            };

            _trayNotificationsCheckBox = new CheckBox
            {
                Text = "🔔 Enable Tray Balloon Notifications",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                CheckAlign = ContentAlignment.MiddleLeft
            };

            _toastNotificationsCheckBox = new CheckBox
            {
                Text = "📢 Enable Windows Toast Notifications",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                CheckAlign = ContentAlignment.MiddleLeft
            };

            var durationLabel = new Label
            {
                Text = "Notification Duration (ms):",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            _notificationDurationNumeric = new NumericUpDown
            {
                Minimum = 1000,
                Maximum = 10000,
                Increment = 500,
                Value = 4000,
                Width = 100,
                Height = 25,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Anchor = AnchorStyles.Left
            };

            var soundTypeLabel = new Label
            {
                Text = "Sound Type:",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            _soundTypeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 110,
                Height = 25,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                BackColor = Color.White,
                Anchor = AnchorStyles.Left
            };
            _soundTypeComboBox.Items.AddRange(new[] { "Classic", "Modern", "Nature", "Electronic" });
            _soundTypeComboBox.SelectedIndex = 0;

            _customSoundsCheckBox = new CheckBox
            {
                Text = "🎵 Custom Sound Files",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                CheckAlign = ContentAlignment.MiddleLeft
            };

            var spacer1 = new Panel
            {
                Height = 10,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            var timerSectionLabel = new Label
            {
                Text = "⏱️ Timer Settings",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            var timerPanel = new Panel
            {
                AutoSize = true,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(15, 10, 15, 10)
            };

            var timerTableLayout = new TableLayoutPanel
            {
                AutoSize = true,
                RowCount = 4,
                ColumnCount = 2,
                BackColor = Color.Transparent,
                Padding = new Padding(5)
            };

            timerTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            timerTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            timerTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            timerTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            timerTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            timerTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            var pomodoroLabel = new Label
            {
                Text = "Pomodoro Duration (minutes):",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            _pomodoroDurationNumeric = new NumericUpDown
            {
                Minimum = 5,
                Maximum = 60,
                Increment = 5,
                Value = 25,
                Width = 80,
                Height = 25,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Anchor = AnchorStyles.Left
            };

            var shortBreakLabel = new Label
            {
                Text = "Short Break (minutes):",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            _shortBreakNumeric = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 30,
                Increment = 1,
                Value = 5,
                Width = 80,
                Height = 25,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Anchor = AnchorStyles.Left
            };

            var longBreakLabel = new Label
            {
                Text = "Long Break (minutes):",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            _longBreakNumeric = new NumericUpDown
            {
                Minimum = 5,
                Maximum = 60,
                Increment = 5,
                Value = 15,
                Width = 80,
                Height = 25,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Anchor = AnchorStyles.Left
            };

            var systemSectionLabel = new Label
            {
                Text = "🚀 Advanced Settings",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            var systemPanel = new Panel
            {
                AutoSize = true,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(15, 10, 15, 10)
            };

            var systemTableLayout = new TableLayoutPanel
            {
                AutoSize = true,
                RowCount = 6,
                ColumnCount = 2,
                BackColor = Color.Transparent,
                Padding = new Padding(5)
            };

            systemTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            systemTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            systemTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            systemTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            systemTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            systemTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            systemTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            systemTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            _autoStartCheckBox = new CheckBox
            {
                Text = "🚀 Auto Start with Windows",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                CheckAlign = ContentAlignment.MiddleLeft
            };

            _themeCheckBox = new CheckBox
            {
                Text = "🌙 Enable Dark Mode",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                CheckAlign = ContentAlignment.MiddleLeft
            };

            _productivityCheckBox = new CheckBox
            {
                Text = "📊 Enable Productivity Tracking",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                CheckAlign = ContentAlignment.MiddleLeft
            };

            _breakReminderCheckBox = new CheckBox
            {
                Text = "⏰ Smart Break Reminders",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                CheckAlign = ContentAlignment.MiddleLeft
            };

            _focusModeCheckBox = new CheckBox
            {
                Text = "🎯 Focus Mode (Block Distractions)",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                CheckAlign = ContentAlignment.MiddleLeft
            };

            _statisticsCheckBox = new CheckBox
            {
                Text = "📈 Detailed Statistics",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                CheckAlign = ContentAlignment.MiddleLeft
            };

            _keyboardShortcutsCheckBox = new CheckBox
            {
                Text = "⌨️ Keyboard Shortcuts",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                CheckAlign = ContentAlignment.MiddleLeft
            };

            var themeLabel = new Label
            {
                Text = "Theme:",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            _themeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 110,
                Height = 25,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                BackColor = Color.White,
                Anchor = AnchorStyles.Left
            };
            _themeComboBox.Items.AddRange(new[] { "Light", "Dark", "Auto", "High Contrast" });
            _themeComboBox.SelectedIndex = 0;

            var infoLabel = new Label
            {
                Text = "💡 Advanced features help you customize your Pomodoro experience for maximum productivity and focus.",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.FromArgb(108, 117, 125),
                TextAlign = ContentAlignment.TopLeft,
                Dock = DockStyle.Fill
            };

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(30, 15, 30, 15),
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            _cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(8, 0, 0, 0)
            };

            _okButton = new Button
            {
                Text = "Save Settings",
                DialogResult = DialogResult.OK,
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 0)
            };

            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 98, 104);
            _okButton.FlatAppearance.BorderSize = 0;
            _okButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 35, 51);

            notificationTableLayout.Controls.Add(_soundEnabledCheckBox, 0, 0);
            notificationTableLayout.Controls.Add(_trayNotificationsCheckBox, 0, 1);
            notificationTableLayout.Controls.Add(_toastNotificationsCheckBox, 0, 2);
            notificationTableLayout.Controls.Add(_customSoundsCheckBox, 0, 3);
            notificationTableLayout.Controls.Add(durationLabel, 0, 4);
            notificationTableLayout.Controls.Add(soundTypeLabel, 0, 5);
            notificationTableLayout.Controls.Add(_notificationDurationNumeric, 1, 4);
            notificationTableLayout.Controls.Add(_soundTypeComboBox, 1, 5);

            timerTableLayout.Controls.Add(pomodoroLabel, 0, 0);
            timerTableLayout.Controls.Add(shortBreakLabel, 0, 1);
            timerTableLayout.Controls.Add(longBreakLabel, 0, 2);
            timerTableLayout.Controls.Add(_pomodoroDurationNumeric, 1, 0);
            timerTableLayout.Controls.Add(_shortBreakNumeric, 1, 1);
            timerTableLayout.Controls.Add(_longBreakNumeric, 1, 2);

            systemTableLayout.Controls.Add(_autoStartCheckBox, 0, 0);
            systemTableLayout.Controls.Add(_themeCheckBox, 0, 1);
            systemTableLayout.Controls.Add(_productivityCheckBox, 0, 2);
            systemTableLayout.Controls.Add(_breakReminderCheckBox, 0, 3);
            systemTableLayout.Controls.Add(_focusModeCheckBox, 0, 4);
            systemTableLayout.Controls.Add(_statisticsCheckBox, 0, 5);
            systemTableLayout.Controls.Add(_keyboardShortcutsCheckBox, 1, 0);
            systemTableLayout.Controls.Add(themeLabel, 1, 1);
            systemTableLayout.Controls.Add(_themeComboBox, 1, 2);

            notificationPanel.Controls.Add(notificationTableLayout);
            timerPanel.Controls.Add(timerTableLayout);
            systemPanel.Controls.Add(systemTableLayout);

            headerTableLayout.Controls.Add(iconLabel, 0, 0);
            headerTableLayout.Controls.Add(appNameLabel, 0, 1);
            headerTableLayout.Controls.Add(versionLabel, 0, 2);

            contentTableLayout.Controls.Add(notificationSectionLabel, 0, 0);
            contentTableLayout.Controls.Add(notificationPanel, 0, 1);
            contentTableLayout.Controls.Add(spacer1, 0, 2);
            contentTableLayout.Controls.Add(timerSectionLabel, 0, 3);
            contentTableLayout.Controls.Add(timerPanel, 0, 4);
            contentTableLayout.Controls.Add(new Panel { Height = 10, Dock = DockStyle.Fill, BackColor = Color.Transparent }, 0, 5);
            contentTableLayout.Controls.Add(systemSectionLabel, 0, 6);
            contentTableLayout.Controls.Add(systemPanel, 0, 7);
            contentTableLayout.Controls.Add(infoLabel, 0, 8);

            headerPanel.Controls.Add(headerTableLayout);
            contentPanel.Controls.Add(contentTableLayout);
            buttonPanel.Controls.Add(_cancelButton);
            buttonPanel.Controls.Add(_okButton);

            mainTableLayout.Controls.Add(headerPanel, 0, 0);
            mainTableLayout.Controls.Add(contentPanel, 0, 1);
            mainTableLayout.Controls.Add(buttonPanel, 0, 2);

            Controls.Add(mainTableLayout);

            AcceptButton = _okButton;
            CancelButton = _cancelButton;
        }

        private void LoadSettings()
        {
            // Basic settings
            _soundEnabledCheckBox.Checked = _parent.GetSoundEnabled();
            _trayNotificationsCheckBox.Checked = _parent.GetTrayNotificationsEnabled();
            _toastNotificationsCheckBox.Checked = _parent.GetToastNotificationsEnabled();
            _autoStartCheckBox.Checked = _parent.GetAutoStartEnabled();
            _notificationDurationNumeric.Value = _parent.GetNotificationDuration();
            
            // Advanced settings
            _themeCheckBox.Checked = _parent.GetDarkModeEnabled();
            _productivityCheckBox.Checked = _parent.GetProductivityTrackingEnabled();
            _breakReminderCheckBox.Checked = _parent.GetBreakReminderEnabled();
            _focusModeCheckBox.Checked = _parent.GetFocusModeEnabled();
            _statisticsCheckBox.Checked = _parent.GetStatisticsEnabled();
            _customSoundsCheckBox.Checked = _parent.GetCustomSoundsEnabled();
            _keyboardShortcutsCheckBox.Checked = _parent.GetKeyboardShortcutsEnabled();
            
            // Sound and theme settings
            var soundType = _parent.GetSoundType();
            var theme = _parent.GetTheme();
            _soundTypeComboBox.SelectedItem = soundType;
            _themeComboBox.SelectedItem = theme;
            
            // Timer settings
            _pomodoroDurationNumeric.Value = _parent.GetPomodoroDuration();
            _shortBreakNumeric.Value = _parent.GetShortBreakDuration();
            _longBreakNumeric.Value = _parent.GetLongBreakDuration();
        }

        public bool SoundEnabled => _soundEnabledCheckBox.Checked;
        public bool TrayNotificationsEnabled => _trayNotificationsCheckBox.Checked;
        public bool ToastNotificationsEnabled => _toastNotificationsCheckBox.Checked;
        public bool AutoStartEnabled => _autoStartCheckBox.Checked;
        public bool DarkModeEnabled => _themeCheckBox.Checked;
        public bool ProductivityTrackingEnabled => _productivityCheckBox.Checked;
        public bool BreakReminderEnabled => _breakReminderCheckBox.Checked;
        public bool FocusModeEnabled => _focusModeCheckBox.Checked;
        public bool StatisticsEnabled => _statisticsCheckBox.Checked;
        public bool CustomSoundsEnabled => _customSoundsCheckBox.Checked;
        public bool KeyboardShortcutsEnabled => _keyboardShortcutsCheckBox.Checked;
        public int NotificationDuration => (int)_notificationDurationNumeric.Value;
        public int PomodoroDuration => (int)_pomodoroDurationNumeric.Value;
        public int ShortBreakDuration => (int)_shortBreakNumeric.Value;
        public int LongBreakDuration => (int)_longBreakNumeric.Value;
        public string SoundType => _soundTypeComboBox.SelectedItem?.ToString() ?? "Classic";
        public string Theme => _themeComboBox.SelectedItem?.ToString() ?? "Light";
    }
}
