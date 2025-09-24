using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace PomodorroMan
{
    public partial class EfficiencyReportForm : Form
    {
        private readonly EfficiencyReport _report;

        public EfficiencyReportForm(EfficiencyReport report)
        {
            _report = report;
            InitializeComponent();
            PopulateReport();
        }

        private void InitializeComponent()
        {
            Text = "Efficiency Report - PomodorroMan v1.8.1";
            Size = new Size(700, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.FromArgb(248, 249, 250);

            var mainTableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(20)
            };

            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));

            var headerPanel = CreateHeaderPanel();
            var metricsPanel = CreateMetricsPanel();
            var recommendationsPanel = CreateRecommendationsPanel();

            mainTableLayout.Controls.Add(headerPanel, 0, 0);
            mainTableLayout.Controls.Add(metricsPanel, 0, 1);
            mainTableLayout.Controls.Add(recommendationsPanel, 0, 2);

            Controls.Add(mainTableLayout);
        }

        private Panel CreateHeaderPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 167, 69),
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
                Text = "📊 Efficiency Report",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            var timeRangeLabel = new Label
            {
                Text = $"Generated: {_report.GeneratedAt:yyyy-MM-dd HH:mm:ss}",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(255, 255, 255, 200),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            headerTableLayout.Controls.Add(titleLabel, 0, 0);
            headerTableLayout.Controls.Add(timeRangeLabel, 0, 1);

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
                RowCount = 6,
                ColumnCount = 2,
                BackColor = Color.Transparent
            };

            for (int i = 0; i < 6; i++)
            {
                tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 16.67F));
            }

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            var metrics = _report.Metrics;

            var focusScoreLabel = CreateMetricLabel("Focus Score", $"{metrics.FocusScore:F1}%", GetScoreColor(metrics.FocusScore));
            var efficiencyScoreLabel = CreateMetricLabel("Efficiency Score", $"{metrics.EfficiencyScore:F1}%", GetScoreColor(metrics.EfficiencyScore));
            var productivityIndexLabel = CreateMetricLabel("Productivity Index", $"{metrics.ProductivityIndex:F1}", GetScoreColor(metrics.ProductivityIndex));
            var activeTimeLabel = CreateMetricLabel("Active Time", FormatTimeSpan(metrics.ActiveTime), Color.FromArgb(40, 167, 69));
            var idleTimeLabel = CreateMetricLabel("Idle Time", FormatTimeSpan(metrics.IdleTime), Color.FromArgb(220, 53, 69));
            var activePercentageLabel = CreateMetricLabel("Active Percentage", $"{metrics.ActivePercentage:F1}%", GetScoreColor(metrics.ActivePercentage));
            var activityCountLabel = CreateMetricLabel("Total Activities", metrics.ActivityCount.ToString(), Color.FromArgb(52, 58, 64));
            var distractionCountLabel = CreateMetricLabel("Distractions", metrics.DistractionCount.ToString(), Color.FromArgb(220, 53, 69));
            var sessionDurationLabel = CreateMetricLabel("Session Duration", FormatTimeSpan(metrics.SessionDuration), Color.FromArgb(52, 58, 64));
            var efficiencyGradeLabel = CreateMetricLabel("Overall Grade", GetEfficiencyGrade(metrics.EfficiencyScore), GetGradeColor(metrics.EfficiencyScore));

            tableLayout.Controls.Add(focusScoreLabel, 0, 0);
            tableLayout.Controls.Add(efficiencyScoreLabel, 1, 0);
            tableLayout.Controls.Add(productivityIndexLabel, 0, 1);
            tableLayout.Controls.Add(activeTimeLabel, 1, 1);
            tableLayout.Controls.Add(idleTimeLabel, 0, 2);
            tableLayout.Controls.Add(activePercentageLabel, 1, 2);
            tableLayout.Controls.Add(activityCountLabel, 0, 3);
            tableLayout.Controls.Add(distractionCountLabel, 1, 3);
            tableLayout.Controls.Add(sessionDurationLabel, 0, 4);
            tableLayout.Controls.Add(efficiencyGradeLabel, 1, 4);

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
                Text = "💡 Recommendations & Insights",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64),
                Dock = DockStyle.Top,
                Height = 25
            };

            var recommendationsListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            foreach (var recommendation in _report.Recommendations)
            {
                recommendationsListBox.Items.Add($"• {recommendation}");
            }

            if (recommendationsListBox.Items.Count == 0)
            {
                recommendationsListBox.Items.Add("• No specific recommendations available");
            }

            panel.Controls.Add(recommendationsListBox);
            panel.Controls.Add(recommendationsLabel);
            return panel;
        }

        private Label CreateMetricLabel(string title, string value, Color valueColor)
        {
            var label = new Label
            {
                Text = $"{title}: {value}",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            var valueStartIndex = label.Text.IndexOf(": ") + 2;
            if (valueStartIndex > 1)
            {
                var titleText = label.Text.Substring(0, valueStartIndex);
                var valueText = label.Text.Substring(valueStartIndex);

                label.Text = titleText;
                label.ForeColor = Color.FromArgb(73, 80, 87);

                var valueLabel = new Label
                {
                    Text = valueText,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = valueColor,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Dock = DockStyle.Fill
                };

                var container = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Transparent
                };

                container.Controls.Add(valueLabel);
                container.Controls.Add(label);

                return label;
            }

            return label;
        }

        private void PopulateReport()
        {
            // Report is populated in the constructor
        }

        private Color GetScoreColor(double score)
        {
            return score switch
            {
                >= 80 => Color.FromArgb(40, 167, 69),
                >= 60 => Color.FromArgb(255, 193, 7),
                >= 40 => Color.FromArgb(255, 152, 0),
                _ => Color.FromArgb(220, 53, 69)
            };
        }

        private Color GetGradeColor(double score)
        {
            return score switch
            {
                >= 90 => Color.FromArgb(40, 167, 69),
                >= 80 => Color.FromArgb(40, 167, 69),
                >= 70 => Color.FromArgb(255, 193, 7),
                >= 60 => Color.FromArgb(255, 152, 0),
                _ => Color.FromArgb(220, 53, 69)
            };
        }

        private string GetEfficiencyGrade(double score)
        {
            return score switch
            {
                >= 90 => "A+",
                >= 80 => "A",
                >= 70 => "B",
                >= 60 => "C",
                >= 50 => "D",
                _ => "F"
            };
        }

        private static string FormatTimeSpan(TimeSpan timeSpan)
        {
            return timeSpan.TotalHours >= 1 
                ? $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}"
                : $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
}
