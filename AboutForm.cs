using System;
using System.Drawing;
using System.Windows.Forms;

namespace PomodorroMan
{
    internal class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "About PomodorroMan";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = Color.FromArgb(248, 249, 250);
            MinimumSize = new Size(700, 500);

            var mainTableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };

            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));

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
                Text = "🍅",
                Font = new Font("Segoe UI Emoji", 48, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            var appNameLabel = new Label
            {
                Text = "PomodorroMan",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            var versionLabel = new Label
            {
                Text = "Version 1.0.0",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(255, 255, 255, 200),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };

            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(40, 30, 40, 20)
            };

            var contentTableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 6,
                ColumnCount = 1,
                BackColor = Color.Transparent,
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 10)
            };

            contentTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            contentTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            contentTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 15));
            contentTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            contentTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            contentTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var devNameLabel = new Label
            {
                Text = "Developer",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            var devNameValueLabel = new Label
            {
                Text = "Toghrul Huseynzade",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            var spacer1 = new Panel
            {
                Height = 10,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            var contactLabel = new Label
            {
                Text = "Contact Information",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 58, 64),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };

            var contactTableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };

            contactTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
            contactTableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));

            var emailLabel = new Label
            {
                Text = "📧 twota.games@gmail.com",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(0, 123, 255),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand
            };

            var githubLabel = new Label
            {
                Text = "🌐 toga94.github.io",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(0, 123, 255),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand
            };

            var descriptionLabel = new Label
            {
                Text = "A modern Pomodoro timer application for Windows with advanced notification system, visual progress tracking, and comprehensive session management. Built with productivity in mind to help you focus and achieve your goals.\n\nFeatures:\n• Customizable timer intervals\n• Multiple notification types\n• Progress tracking and analytics\n• Dark mode support\n• Auto-start with Windows\n• Productivity insights",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(108, 117, 125),
                TextAlign = ContentAlignment.TopLeft,
                Dock = DockStyle.Fill,
                AutoSize = false
            };

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(40, 20, 40, 20),
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            var closeButton = new Button
            {
                Text = "Close",
                Size = new Size(120, 40),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 0, 0)
            };

            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 35, 51);

            headerTableLayout.Controls.Add(iconLabel, 0, 0);
            headerTableLayout.Controls.Add(appNameLabel, 0, 1);
            headerTableLayout.Controls.Add(versionLabel, 0, 2);

            contactTableLayout.Controls.Add(emailLabel, 0, 0);
            contactTableLayout.Controls.Add(githubLabel, 0, 1);

            contentTableLayout.Controls.Add(devNameLabel, 0, 0);
            contentTableLayout.Controls.Add(devNameValueLabel, 0, 1);
            contentTableLayout.Controls.Add(spacer1, 0, 2);
            contentTableLayout.Controls.Add(contactLabel, 0, 3);
            contentTableLayout.Controls.Add(contactTableLayout, 0, 4);
            contentTableLayout.Controls.Add(descriptionLabel, 0, 5);

            headerPanel.Controls.Add(headerTableLayout);
            contentPanel.Controls.Add(contentTableLayout);
            buttonPanel.Controls.Add(closeButton);

            mainTableLayout.Controls.Add(headerPanel, 0, 0);
            mainTableLayout.Controls.Add(contentPanel, 0, 1);
            mainTableLayout.Controls.Add(buttonPanel, 0, 2);

            Controls.Add(mainTableLayout);

            AcceptButton = closeButton;
            CancelButton = closeButton;

            emailLabel.Click += (s, e) => HandleEmailClick();
            githubLabel.Click += (s, e) => HandleGitHubClick();
        }

        private void HandleEmailClick()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "mailto:twota.games@gmail.com",
                    UseShellExecute = true
                });
            }
            catch
            {
                Clipboard.SetText("twota.games@gmail.com");
                MessageBox.Show("Email address copied to clipboard!", "Email", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void HandleGitHubClick()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://toga94.github.io",
                    UseShellExecute = true
                });
            }
            catch
            {
                Clipboard.SetText("https://toga94.github.io");
                MessageBox.Show("GitHub URL copied to clipboard!", "GitHub", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
