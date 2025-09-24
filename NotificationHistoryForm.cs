using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PomodorroMan
{
    public partial class NotificationHistoryForm : Form
    {
        private readonly List<NotificationRecord> _history;

        internal NotificationHistoryForm(List<NotificationRecord> history)
        {
            _history = history;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Notification History - PomodorroMan v1.8.1";
            Size = new System.Drawing.Size(800, 600);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MinimumSize = new System.Drawing.Size(600, 400);

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };

            var headerLabel = new Label
            {
                Text = "📋 Notification History",
                Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };

            var listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Margin = new Padding(0, 0, 0, 10)
            };

            listView.Columns.Add("Time", 150);
            listView.Columns.Add("Type", 80);
            listView.Columns.Add("Session", 100);
            listView.Columns.Add("Title", 150);
            listView.Columns.Add("Message", 300);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 50,
                Padding = new Padding(10)
            };

            var clearButton = new Button
            {
                Text = "Clear History",
                Size = new System.Drawing.Size(75, 23),
                Margin = new Padding(5, 0, 0, 0)
            };
            clearButton.Click += OnClearHistoryClicked;

            var exportButton = new Button
            {
                Text = "Export...",
                Size = new System.Drawing.Size(75, 23),
                Margin = new Padding(5, 0, 0, 0)
            };
            exportButton.Click += OnExportClicked;

            var closeButton = new Button
            {
                Text = "Close",
                Size = new System.Drawing.Size(75, 23),
                Margin = new Padding(5, 0, 0, 0),
                DialogResult = DialogResult.OK
            };

            buttonPanel.Controls.Add(closeButton);
            buttonPanel.Controls.Add(exportButton);
            buttonPanel.Controls.Add(clearButton);

            mainPanel.Controls.Add(headerLabel, 0, 0);
            mainPanel.Controls.Add(listView, 0, 1);

            Controls.Add(mainPanel);
            Controls.Add(buttonPanel);

            AcceptButton = closeButton;
            CancelButton = closeButton;

            LoadHistory();
        }

        private void LoadHistory()
        {
            var listView = (ListView)Controls[0].Controls[1];
            listView.Items.Clear();

            foreach (var record in _history.OrderByDescending(r => r.Timestamp))
            {
                var item = new ListViewItem(record.FormattedTimestamp);
                item.SubItems.Add(record.IconType);
                item.SubItems.Add(record.SessionType);
                item.SubItems.Add(record.Title);
                item.SubItems.Add(record.FormattedMessage);
                item.Tag = record;

                listView.Items.Add(item);
            }

            if (listView.Items.Count > 0)
            {
                listView.Items[0].Selected = true;
                listView.Items[0].EnsureVisible();
            }
        }

        private void OnClearHistoryClicked(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all notification history?",
                "Clear History",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _history.Clear();
                LoadHistory();
            }
        }

        private void OnExportClicked(object? sender, EventArgs e)
        {
            using var saveDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = "txt",
                FileName = $"Pomodoro_Notifications_{DateTime.Now:yyyy-MM-dd}.txt"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var content = GenerateExportContent(saveDialog.FileName.EndsWith(".csv"));
                    System.IO.File.WriteAllText(saveDialog.FileName, content);
                    MessageBox.Show("Notification history exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export history: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GenerateExportContent(bool isCsv)
        {
            if (isCsv)
            {
                var lines = new List<string> { "Timestamp,Type,Session,Title,Message" };
                foreach (var record in _history.OrderByDescending(r => r.Timestamp))
                {
                    lines.Add($"\"{record.FormattedTimestamp}\",\"{record.IconType}\",\"{record.SessionType}\",\"{record.Title}\",\"{record.FormattedMessage}\"");
                }
                return string.Join("\n", lines);
            }
            else
            {
                var lines = new List<string>
                {
                    "Pomodoro Timer - Notification History",
                    $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    $"Total notifications: {_history.Count}",
                    new string('=', 50)
                };

                foreach (var record in _history.OrderByDescending(r => r.Timestamp))
                {
                    lines.Add($"Time: {record.FormattedTimestamp}");
                    lines.Add($"Type: {record.IconType} | Session: {record.SessionType}");
                    lines.Add($"Title: {record.Title}");
                    lines.Add($"Message: {record.Message}");
                    lines.Add(new string('-', 30));
                }

                return string.Join("\n", lines);
            }
        }
    }
}
