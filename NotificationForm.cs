using System;
using System.Drawing;
using System.Windows.Forms;

namespace PomodorroMan
{
    internal class NotificationForm : Form
    {
        private readonly System.Windows.Forms.Timer _autoCloseTimer;

        public NotificationForm(string title, string message, ToolTipIcon icon)
        {
            InitializeComponent(title, message, icon);
            
            _autoCloseTimer = new System.Windows.Forms.Timer
            {
                Interval = 4000,
                Enabled = true
            };
            _autoCloseTimer.Tick += (_, _) => Close();
        }

        private void InitializeComponent(string title, string message, ToolTipIcon icon)
        {
            Text = title;
            Size = new Size(350, 150);
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            
            // Position in top-right corner
            Screen? screen = Screen.PrimaryScreen;
            if (screen != null)
            {
                Location = new Point(screen.WorkingArea.Right - Width - 10, 10);
            }
            else
            {
                // Fallback to center if PrimaryScreen is null
                StartPosition = FormStartPosition.CenterScreen;
            }

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(15)
            };

            var iconLabel = new Label
            {
                Text = GetIconText(icon),
                Font = new Font("Segoe UI Emoji", 24, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 5)
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 5)
            };

            var messageLabel = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true,
                Margin = new Padding(0, 5, 0, 5)
            };

            mainPanel.Controls.Add(iconLabel, 0, 0);
            mainPanel.Controls.Add(titleLabel, 0, 1);
            mainPanel.Controls.Add(messageLabel, 0, 2);

            Controls.Add(mainPanel);

            // Add click to close
            Click += (_, _) => Close();
            iconLabel.Click += (_, _) => Close();
            titleLabel.Click += (_, _) => Close();
            messageLabel.Click += (_, _) => Close();

            // Set background color based on icon type
            BackColor = GetBackgroundColor(icon);
        }

        private string GetIconText(ToolTipIcon icon)
        {
            return icon switch
            {
                ToolTipIcon.Info => "🍅",
                ToolTipIcon.Warning => "⚠️",
                ToolTipIcon.Error => "❌",
                _ => "🔔"
            };
        }

        private Color GetBackgroundColor(ToolTipIcon icon)
        {
            return icon switch
            {
                ToolTipIcon.Info => Color.FromArgb(220, 53, 69),
                ToolTipIcon.Warning => Color.FromArgb(255, 193, 7),
                ToolTipIcon.Error => Color.FromArgb(220, 53, 69),
                _ => Color.FromArgb(108, 117, 125)
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _autoCloseTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
