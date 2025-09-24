using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace PomodorroMan
{
    public partial class EfficiencyTrackingForm : Form
    {
        private readonly WorkEfficiencyCalculator _efficiencyCalculator;
        private readonly ActivityTracker _activityTracker;
        private readonly EfficiencyDataManager _dataManager;
        private readonly DataExportManager _exportManager;
        
        private Label _focusScoreLabel = null!;
        private Label _efficiencyScoreLabel = null!;
        private Label _productivityIndexLabel = null!;
        private Label _activeTimeLabel = null!;
        private Label _idleTimeLabel = null!;
        private Label _activityCountLabel = null!;
        private Label _distractionCountLabel = null!;
        private Label _afkStatusLabel = null!;
        private ProgressBar _focusProgressBar = null!;
        private ProgressBar _efficiencyProgressBar = null!;
        private ListBox _recommendationsListBox = null!;
        private Button _generateReportButton = null!;
        private Button _clearDataButton = null!;
        private Button _exportDataButton = null!;
        private ComboBox _timeRangeComboBox = null!;
        private System.Windows.Forms.Timer _updateTimer = null!;

        public EfficiencyTrackingForm(WorkEfficiencyCalculator efficiencyCalculator, ActivityTracker activityTracker, EfficiencyDataManager dataManager)
        {
            _efficiencyCalculator = efficiencyCalculator;
            _activityTracker = activityTracker;
            _dataManager = dataManager;
            _exportManager = new DataExportManager(dataManager);
            
            InitializeComponent();
            SetupEventHandlers();
            StartRealTimeUpdates();
        }

        private void InitializeComponent()
        {
            Text = "Work Efficiency Tracker - PomodorroMan v1.8.1";
            Size = new Size(600, 500);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.FromArgb(248, 249, 250);

            var mainTableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(20)
            };

            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            var headerPanel = CreateHeaderPanel();
            var metricsPanel = CreateMetricsPanel();
            var recommendationsPanel = CreateRecommendationsPanel();
            var buttonPanel = CreateButtonPanel();

            mainTableLayout.Controls.Add(headerPanel, 0, 0);
            mainTableLayout.Controls.Add(metricsPanel, 0, 1);
            mainTableLayout.Controls.Add(recommendationsPanel, 0, 2);
            mainTableLayout.Controls.Add(buttonPanel, 0, 3);

            Controls.Add(mainTableLayout);
        }

        private Panel CreateHeaderPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(220, 53, 69),
                Padding = new Padding(0)
            };

            var headerTableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };

            var titleLabel = new Label
            {
                Text = "📊 Work Efficiency Tracker",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            var statusLabel = new Label
            {
                Text = "Real-time productivity monitoring",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(255, 255, 255, 200),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            headerTableLayout.Controls.Add(titleLabel, 0, 0);
            headerTableLayout.Controls.Add(statusLabel, 0, 1);

            panel.Controls.Add(headerTableLayout);
            return panel;
        }

        private Panel CreateMetricsPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 2,
                BackColor = Color.Transparent
            };

            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            _focusScoreLabel = CreateMetricLabel("Focus Score", "0%");
            _efficiencyScoreLabel = CreateMetricLabel("Efficiency Score", "0%");
            _productivityIndexLabel = CreateMetricLabel("Productivity Index", "0");
            _activeTimeLabel = CreateMetricLabel("Active Time", "00:00:00");
            _idleTimeLabel = CreateMetricLabel("Idle Time", "00:00:00");
            _activityCountLabel = CreateMetricLabel("Activities", "0");
            _distractionCountLabel = CreateMetricLabel("Distractions", "0");
            _afkStatusLabel = CreateMetricLabel("AFK Status", "Active");

            _focusProgressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Style = ProgressBarStyle.Continuous,
                Maximum = 100,
                Value = 0
            };

            _efficiencyProgressBar = new ProgressBar
            {
                Dock = DockStyle.Fill,
                Style = ProgressBarStyle.Continuous,
                Maximum = 100,
                Value = 0
            };

            tableLayout.Controls.Add(_focusScoreLabel, 0, 0);
            tableLayout.Controls.Add(_focusProgressBar, 1, 0);
            tableLayout.Controls.Add(_efficiencyScoreLabel, 0, 1);
            tableLayout.Controls.Add(_efficiencyProgressBar, 1, 1);
            tableLayout.Controls.Add(_productivityIndexLabel, 0, 2);
            tableLayout.Controls.Add(_activeTimeLabel, 1, 2);
            tableLayout.Controls.Add(_idleTimeLabel, 0, 3);
            tableLayout.Controls.Add(_activityCountLabel, 1, 3);

            panel.Controls.Add(tableLayout);
            return panel;
        }

        private Panel CreateRecommendationsPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(15)
            };

            var recommendationsLabel = new Label
            {
                Text = "💡 Recommendations",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64),
                Dock = DockStyle.Top,
                Height = 25
            };

            _recommendationsListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            panel.Controls.Add(_recommendationsListBox);
            panel.Controls.Add(recommendationsLabel);
            return panel;
        }

        private Panel CreateButtonPanel()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 249, 250),
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Padding = new Padding(0, 10, 0, 0)
            };

            _timeRangeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120,
                Height = 25,
                Font = new Font("Segoe UI", 9)
            };
            _timeRangeComboBox.Items.AddRange(new[] { "Last Hour", "Last 4 Hours", "Today", "This Week" });

            _generateReportButton = new Button
            {
                Text = "📊 Generate Report",
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 0, 0, 0)
            };

            _clearDataButton = new Button
            {
                Text = "🗑️ Clear Data",
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 0, 0, 0)
            };

            _exportDataButton = new Button
            {
                Text = "📤 Export Data",
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(5, 0, 0, 0)
            };

            _generateReportButton.FlatAppearance.BorderSize = 0;
            _generateReportButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 139, 34);
            _clearDataButton.FlatAppearance.BorderSize = 0;
            _clearDataButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 35, 51);
            _exportDataButton.FlatAppearance.BorderSize = 0;
            _exportDataButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 86, 179);

            panel.Controls.Add(_clearDataButton);
            panel.Controls.Add(_generateReportButton);
            panel.Controls.Add(_exportDataButton);
            panel.Controls.Add(_timeRangeComboBox);

            return panel;
        }

        private Label CreateMetricLabel(string title, string value)
        {
            return new Label
            {
                Text = $"{title}: {value}",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };
        }

        private void SetupEventHandlers()
        {
            _efficiencyCalculator.EfficiencyUpdated += OnEfficiencyUpdated;
            _activityTracker.AfkDetected += OnAfkDetected;
            _activityTracker.FocusLost += OnFocusLost;
            _generateReportButton.Click += OnGenerateReportClicked;
            _clearDataButton.Click += OnClearDataClicked;
            _exportDataButton.Click += OnExportDataClicked;
        }

        private void StartRealTimeUpdates()
        {
            _updateTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _updateTimer.Tick += OnUpdateTimerTick;
            _updateTimer.Start();
        }

        private void OnEfficiencyUpdated(object? sender, EfficiencyUpdatedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateMetrics(e.Metrics)));
            }
            else
            {
                UpdateMetrics(e.Metrics);
            }
        }

        private void OnAfkDetected(object? sender, AfkDetectedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateAfkStatus(true, e.AfkDuration)));
            }
            else
            {
                UpdateAfkStatus(true, e.AfkDuration);
            }
        }

        private void OnFocusLost(object? sender, FocusLostEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateDistractionCount()));
            }
            else
            {
                UpdateDistractionCount();
            }
        }

        private void OnUpdateTimerTick(object? sender, EventArgs e)
        {
            UpdateAfkStatus(_activityTracker.IsAfk, _activityTracker.AfkDuration);
        }

        private void UpdateMetrics(EfficiencyMetrics metrics)
        {
            if (metrics == null) return;

            _focusScoreLabel.Text = $"🎯 Focus Score: {metrics.FocusScore:F1}%";
            _efficiencyScoreLabel.Text = $"⚡ Efficiency Score: {metrics.EfficiencyScore:F1}%";
            _productivityIndexLabel.Text = $"📈 Productivity Index: {metrics.ProductivityIndex:F1}";
            _activeTimeLabel.Text = $"⏱️ Active Time: {FormatTimeSpan(metrics.ActiveTime)}";
            _idleTimeLabel.Text = $"😴 Idle Time: {FormatTimeSpan(metrics.IdleTime)}";
            _activityCountLabel.Text = $"🔄 Activities: {metrics.ActivityCount}";
            _distractionCountLabel.Text = $"⚠️ Distractions: {metrics.DistractionCount}";

            _focusProgressBar.Value = (int)Math.Max(0, Math.Min(100, metrics.FocusScore));
            _efficiencyProgressBar.Value = (int)Math.Max(0, Math.Min(100, metrics.EfficiencyScore));

            UpdateRecommendations(metrics);
        }

        private void UpdateAfkStatus(bool isAfk, TimeSpan afkDuration)
        {
            if (isAfk)
            {
                _afkStatusLabel.Text = $"AFK Status: Away ({FormatTimeSpan(afkDuration)})";
                _afkStatusLabel.ForeColor = Color.FromArgb(220, 53, 69);
            }
            else
            {
                _afkStatusLabel.Text = "AFK Status: Active";
                _afkStatusLabel.ForeColor = Color.FromArgb(40, 167, 69);
            }
        }

        private void UpdateDistractionCount()
        {
            var metrics = _efficiencyCalculator.CurrentMetrics;
            _distractionCountLabel.Text = $"Distractions: {metrics.DistractionCount}";
        }

        private void UpdateRecommendations(EfficiencyMetrics metrics)
        {
            _recommendationsListBox.Items.Clear();

            if (metrics.ActivePercentage < 70)
            {
                _recommendationsListBox.Items.Add("• Consider reducing idle time to improve productivity");
            }

            if (metrics.FocusScore < 60)
            {
                _recommendationsListBox.Items.Add("• Try to minimize distractions and maintain focus");
            }

            if (metrics.DistractionCount > 10)
            {
                _recommendationsListBox.Items.Add("• High distraction count - consider using focus mode");
            }

            if (metrics.EfficiencyScore > 80)
            {
                _recommendationsListBox.Items.Add("• Excellent work efficiency! Keep up the great work!");
            }
            else if (metrics.EfficiencyScore > 60)
            {
                _recommendationsListBox.Items.Add("• Good efficiency - room for improvement in focus areas");
            }
            else
            {
                _recommendationsListBox.Items.Add("• Consider taking breaks and reorganizing your work approach");
            }

            if (_recommendationsListBox.Items.Count == 0)
            {
                _recommendationsListBox.Items.Add("• No specific recommendations at this time");
            }
        }

        private void OnGenerateReportClicked(object? sender, EventArgs e)
        {
            var timeRange = GetSelectedTimeRange();
            var report = _efficiencyCalculator.GenerateReport(timeRange);
            
            using var reportForm = new EfficiencyReportForm(report);
            reportForm.ShowDialog();
        }

        private void OnClearDataClicked(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all efficiency data? This action cannot be undone.",
                "Clear Data",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                _efficiencyCalculator.ClearData();
                _dataManager.ClearData();
                UpdateMetrics(new EfficiencyMetrics());
                MessageBox.Show("Efficiency data cleared successfully.", "Data Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnExportDataClicked(object? sender, EventArgs e)
        {
            using var exportForm = new DataExportForm(_exportManager, GetSelectedTimeRange());
            exportForm.ShowDialog();
        }

        private TimeSpan GetSelectedTimeRange()
        {
            return _timeRangeComboBox.SelectedItem?.ToString() switch
            {
                "Last Hour" => TimeSpan.FromHours(1),
                "Last 4 Hours" => TimeSpan.FromHours(4),
                "Today" => TimeSpan.FromDays(1),
                "This Week" => TimeSpan.FromDays(7),
                _ => TimeSpan.FromHours(1)
            };
        }

        private static string FormatTimeSpan(TimeSpan timeSpan)
        {
            return timeSpan.TotalHours >= 1 
                ? $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}"
                : $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _updateTimer?.Stop();
            _updateTimer?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
