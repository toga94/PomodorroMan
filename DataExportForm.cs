using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace PomodorroMan
{
    public partial class DataExportForm : Form
    {
        private readonly DataExportManager _exportManager;
        private readonly TimeSpan _timeRange;
        
        private ComboBox _formatComboBox = null!;
        private Button _exportButton = null!;
        private Button _cancelButton = null!;
        private Label _infoLabel = null!;
        private TextBox _filePathTextBox = null!;
        private Button _browseButton = null!;

        public DataExportForm(DataExportManager exportManager, TimeSpan timeRange)
        {
            _exportManager = exportManager;
            _timeRange = timeRange;
            
            InitializeComponent();
            SetupEventHandlers();
        }

        private void InitializeComponent()
        {
            Text = "Export Data - PomodorroMan v1.8.1";
            Size = new Size(500, 300);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.FromArgb(248, 249, 250);

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var titleLabel = new Label
            {
                Text = "📤 Export Efficiency Data",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 53, 69),
                AutoSize = true,
                Location = new Point(0, 0)
            };

            _infoLabel = new Label
            {
                Text = $"Exporting data for the selected time range: {FormatTimeRange(_timeRange)}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(73, 80, 87),
                AutoSize = true,
                Location = new Point(0, 40)
            };

            var formatLabel = new Label
            {
                Text = "Export Format:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Location = new Point(0, 80)
            };

            _formatComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Size = new Size(200, 25),
                Location = new Point(0, 105),
                Items = { }
            };

            _formatComboBox.Items.AddRange(_exportManager.GetSupportedFormats().ToArray());
            _formatComboBox.SelectedIndex = 0;

            var filePathLabel = new Label
            {
                Text = "Save to:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Location = new Point(0, 150)
            };

            _filePathTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Size = new Size(300, 25),
                Location = new Point(0, 175),
                ReadOnly = true
            };

            _browseButton = new Button
            {
                Text = "Browse...",
                Size = new Size(80, 25),
                Location = new Point(310, 175),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };

            _browseButton.FlatAppearance.BorderSize = 0;
            _browseButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 98, 104);

            var buttonPanel = new Panel
            {
                Size = new Size(460, 50),
                Location = new Point(0, 220),
                Dock = DockStyle.Bottom
            };

            _exportButton = new Button
            {
                Text = "Export",
                Size = new Size(100, 35),
                Location = new Point(270, 10),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            _cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(100, 35),
                Location = new Point(380, 10),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            _exportButton.FlatAppearance.BorderSize = 0;
            _exportButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 139, 34);
            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 98, 104);

            buttonPanel.Controls.Add(_exportButton);
            buttonPanel.Controls.Add(_cancelButton);

            mainPanel.Controls.Add(titleLabel);
            mainPanel.Controls.Add(_infoLabel);
            mainPanel.Controls.Add(formatLabel);
            mainPanel.Controls.Add(_formatComboBox);
            mainPanel.Controls.Add(filePathLabel);
            mainPanel.Controls.Add(_filePathTextBox);
            mainPanel.Controls.Add(_browseButton);
            mainPanel.Controls.Add(buttonPanel);

            Controls.Add(mainPanel);
        }

        private void SetupEventHandlers()
        {
            _browseButton.Click += OnBrowseClicked;
            _exportButton.Click += OnExportClicked;
            _cancelButton.Click += OnCancelClicked;
            _formatComboBox.SelectedIndexChanged += OnFormatChanged;
        }

        private void OnBrowseClicked(object? sender, EventArgs e)
        {
            var format = _formatComboBox.SelectedItem?.ToString() ?? "";
            var extension = GetFileExtension(format);
            var filter = $"{format} files (*{extension})|*{extension}|All files (*.*)|*.*";

            using var saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                DefaultExt = extension,
                FileName = $"PomodorroMan_Export_{DateTime.Now:yyyyMMdd_HHmmss}{extension}"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                _filePathTextBox.Text = saveFileDialog.FileName;
            }
        }

        private void OnExportClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_filePathTextBox.Text))
            {
                MessageBox.Show("Please select a file path for export.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var format = _formatComboBox.SelectedItem?.ToString() ?? "";
                
                switch (format)
                {
                    case "JSON (.json)":
                        _exportManager.ExportToJson(_filePathTextBox.Text, _timeRange);
                        break;
                    case "CSV (.csv)":
                        _exportManager.ExportToCsv(_filePathTextBox.Text, _timeRange);
                        break;
                    case "Text (.txt)":
                        _exportManager.ExportToTxt(_filePathTextBox.Text, _timeRange);
                        break;
                    case "HTML (.html)":
                        _exportManager.ExportToHtml(_filePathTextBox.Text, _timeRange);
                        break;
                    case "Notion (.json)":
                        _exportManager.ExportToNotion(_filePathTextBox.Text, _timeRange);
                        break;
                    case "Todoist (.json)":
                        _exportManager.ExportToTodoist(_filePathTextBox.Text, _timeRange);
                        break;
                    default:
                        MessageBox.Show("Unsupported export format.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                }

                MessageBox.Show($"Data exported successfully to:\n{_filePathTextBox.Text}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnCancelClicked(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OnFormatChanged(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_filePathTextBox.Text))
            {
                var format = _formatComboBox.SelectedItem?.ToString() ?? "";
                var extension = GetFileExtension(format);
                var currentPath = _filePathTextBox.Text;
                var newPath = Path.ChangeExtension(currentPath, extension);
                _filePathTextBox.Text = newPath;
            }
        }

        private static string GetFileExtension(string format)
        {
            return format switch
            {
                "JSON (.json)" => ".json",
                "CSV (.csv)" => ".csv",
                "Text (.txt)" => ".txt",
                "HTML (.html)" => ".html",
                "Notion (.json)" => ".json",
                "Todoist (.json)" => ".json",
                _ => ".txt"
            };
        }

        private static string FormatTimeRange(TimeSpan timeRange)
        {
            return timeRange.TotalDays >= 1 
                ? $"{timeRange.Days} day{(timeRange.Days == 1 ? "" : "s")}"
                : timeRange.TotalHours >= 1 
                    ? $"{timeRange.Hours} hour{(timeRange.Hours == 1 ? "" : "s")}"
                    : $"{timeRange.Minutes} minute{(timeRange.Minutes == 1 ? "" : "s")}";
        }
    }
}
