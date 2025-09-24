using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PomodorroMan
{
    public partial class ThemesForm : Form
    {
        private readonly ThemeManager _themeManager;
        private ListBox _themesListBox = null!;
        private Panel _previewPanel = null!;
        private Label _previewLabel = null!;
        private Button _applyButton = null!;
        private Button _addCustomButton = null!;
        private Label _currentThemeLabel = null!;

        public ThemesForm(ThemeManager themeManager)
        {
            _themeManager = themeManager;
            InitializeComponent();
            LoadThemes();
            UpdatePreview();
        }

        private void InitializeComponent()
        {
            Text = "Themes - PomodorroMan v1.8.0";
            Size = new Size(700, 500);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.FromArgb(248, 249, 250);

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };

            // Left panel - Theme list
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var themesLabel = new Label
            {
                Text = "Available Themes",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            _themesListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                SelectionMode = SelectionMode.One,
                DisplayMember = "Name"
            };
            _themesListBox.SelectedIndexChanged += OnThemeSelected;

            leftPanel.Controls.Add(_themesListBox);
            leftPanel.Controls.Add(themesLabel);

            // Right panel - Preview and controls
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var previewLabel = new Label
            {
                Text = "Theme Preview",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            var controlsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Current theme
            _currentThemeLabel = new Label
            {
                Text = $"Current Theme: {_themeManager.GetCurrentTheme().Name}",
                Location = new Point(10, 20),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            // Preview panel
            _previewPanel = new Panel
            {
                Location = new Point(10, 60),
                Size = new Size(300, 200),
                BorderStyle = BorderStyle.FixedSingle
            };

            _previewLabel = new Label
            {
                Text = "Sample Text",
                Location = new Point(20, 20),
                Size = new Size(100, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var previewButton = new Button
            {
                Text = "Sample Button",
                Location = new Point(20, 60),
                Size = new Size(100, 30)
            };

            var previewProgressBar = new ProgressBar
            {
                Location = new Point(20, 100),
                Size = new Size(200, 20),
                Value = 60
            };

            _previewPanel.Controls.AddRange(new Control[] { _previewLabel, previewButton, previewProgressBar });

            // Apply button
            _applyButton = new Button
            {
                Text = "Apply Theme",
                Location = new Point(10, 280),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _applyButton.Click += OnApplyTheme;

            // Add custom theme button
            _addCustomButton = new Button
            {
                Text = "Add Custom Theme",
                Location = new Point(120, 280),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _addCustomButton.Click += OnAddCustomTheme;

            // Theme info
            var infoLabel = new Label
            {
                Text = "Theme Information",
                Location = new Point(10, 320),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var infoTextBox = new TextBox
            {
                Location = new Point(10, 340),
                Size = new Size(300, 80),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            controlsPanel.Controls.AddRange(new Control[]
            {
                _currentThemeLabel,
                _previewPanel,
                _applyButton, _addCustomButton,
                infoLabel, infoTextBox
            });

            rightPanel.Controls.Add(controlsPanel);
            rightPanel.Controls.Add(previewLabel);

            mainPanel.Controls.Add(leftPanel, 0, 0);
            mainPanel.Controls.Add(rightPanel, 1, 0);

            Controls.Add(mainPanel);
        }

        private void LoadThemes()
        {
            _themesListBox.DataSource = null;
            _themesListBox.DataSource = _themeManager.GetAllThemes();
            _themesListBox.DisplayMember = "Name";

            // Select current theme
            var currentTheme = _themeManager.GetCurrentTheme();
            for (int i = 0; i < _themesListBox.Items.Count; i++)
            {
                if (_themesListBox.Items[i] is Theme theme && theme.Name == currentTheme.Name)
                {
                    _themesListBox.SelectedIndex = i;
                    break;
                }
            }
        }

        private void OnThemeSelected(object? sender, EventArgs e)
        {
            UpdatePreview();
            _applyButton.Enabled = _themesListBox.SelectedItem != null;
        }

        private void OnApplyTheme(object? sender, EventArgs e)
        {
            if (_themesListBox.SelectedItem is Theme selectedTheme)
            {
                _themeManager.SetTheme(selectedTheme.Name);
                _currentThemeLabel.Text = $"Current Theme: {selectedTheme.Name}";
                MessageBox.Show($"Theme '{selectedTheme.Name}' has been applied!", "Theme Applied", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnAddCustomTheme(object? sender, EventArgs e)
        {
            using var form = new CustomThemeForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                var customTheme = form.GetCustomTheme();
                _themeManager.AddCustomTheme(customTheme);
                LoadThemes();
                MessageBox.Show($"Custom theme '{customTheme.Name}' has been added!", "Theme Added", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdatePreview()
        {
            if (_themesListBox.SelectedItem is Theme selectedTheme)
            {
                _previewPanel.BackColor = selectedTheme.BackgroundColor;
                _previewLabel.ForeColor = selectedTheme.TextColor;
                _previewLabel.BackColor = selectedTheme.BackgroundColor;

                var button = _previewPanel.Controls.OfType<Button>().FirstOrDefault();
                if (button != null)
                {
                    button.BackColor = selectedTheme.ButtonColor;
                    button.ForeColor = selectedTheme.TextColor;
                }

                var progressBar = _previewPanel.Controls.OfType<ProgressBar>().FirstOrDefault();
                if (progressBar != null)
                {
                    progressBar.BackColor = selectedTheme.ProgressBarBackgroundColor;
                    // Note: ProgressBar color changes require more complex handling
                }
            }
        }
    }

    public partial class CustomThemeForm : Form
    {
        private TextBox _nameTextBox = null!;
        private Button _backgroundColorButton = null!;
        private Button _textColorButton = null!;
        private Button _accentColorButton = null!;
        private Button _buttonColorButton = null!;
        private ColorDialog _colorDialog = null!;
        private Theme _customTheme;

        public CustomThemeForm()
        {
            InitializeComponent();
            _customTheme = new Theme
            {
                Name = "Custom Theme",
                BackgroundColor = Color.White,
                TextColor = Color.Black,
                AccentColor = Color.Blue,
                ButtonColor = Color.Blue,
                ButtonHoverColor = Color.DarkBlue,
                ForegroundColor = Color.Black,
                BorderColor = Color.Gray,
                ProgressBarColor = Color.Green,
                ProgressBarBackgroundColor = Color.LightGray,
                IsDarkMode = false
            };
        }

        private void InitializeComponent()
        {
            Text = "Add Custom Theme";
            Size = new Size(400, 300);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.FromArgb(248, 249, 250);

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var nameLabel = new Label
            {
                Text = "Theme Name:",
                Location = new Point(10, 20),
                Size = new Size(80, 20)
            };

            _nameTextBox = new TextBox
            {
                Location = new Point(100, 18),
                Size = new Size(200, 20),
                Text = "Custom Theme"
            };

            var backgroundColorLabel = new Label
            {
                Text = "Background:",
                Location = new Point(10, 50),
                Size = new Size(80, 20)
            };

            _backgroundColorButton = new Button
            {
                Location = new Point(100, 48),
                Size = new Size(100, 25),
                BackColor = Color.White,
                Text = "Choose Color"
            };
            _backgroundColorButton.Click += OnBackgroundColorClicked;

            var textColorLabel = new Label
            {
                Text = "Text Color:",
                Location = new Point(10, 80),
                Size = new Size(80, 20)
            };

            _textColorButton = new Button
            {
                Location = new Point(100, 78),
                Size = new Size(100, 25),
                BackColor = Color.Black,
                ForeColor = Color.White,
                Text = "Choose Color"
            };
            _textColorButton.Click += OnTextColorClicked;

            var accentColorLabel = new Label
            {
                Text = "Accent Color:",
                Location = new Point(10, 110),
                Size = new Size(80, 20)
            };

            _accentColorButton = new Button
            {
                Location = new Point(100, 108),
                Size = new Size(100, 25),
                BackColor = Color.Blue,
                ForeColor = Color.White,
                Text = "Choose Color"
            };
            _accentColorButton.Click += OnAccentColorClicked;

            var buttonColorLabel = new Label
            {
                Text = "Button Color:",
                Location = new Point(10, 140),
                Size = new Size(80, 20)
            };

            _buttonColorButton = new Button
            {
                Location = new Point(100, 138),
                Size = new Size(100, 25),
                BackColor = Color.Blue,
                ForeColor = Color.White,
                Text = "Choose Color"
            };
            _buttonColorButton.Click += OnButtonColorClicked;

            var okButton = new Button
            {
                Text = "OK",
                Location = new Point(200, 180),
                Size = new Size(75, 30),
                DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(285, 180),
                Size = new Size(75, 30),
                DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            mainPanel.Controls.AddRange(new Control[]
            {
                nameLabel, _nameTextBox,
                backgroundColorLabel, _backgroundColorButton,
                textColorLabel, _textColorButton,
                accentColorLabel, _accentColorButton,
                buttonColorLabel, _buttonColorButton,
                okButton, cancelButton
            });

            Controls.Add(mainPanel);

            _colorDialog = new ColorDialog();
        }

        private void OnBackgroundColorClicked(object? sender, EventArgs e)
        {
            _colorDialog.Color = _customTheme.BackgroundColor;
            if (_colorDialog.ShowDialog() == DialogResult.OK)
            {
                _customTheme.BackgroundColor = _colorDialog.Color;
                _backgroundColorButton.BackColor = _colorDialog.Color;
            }
        }

        private void OnTextColorClicked(object? sender, EventArgs e)
        {
            _colorDialog.Color = _customTheme.TextColor;
            if (_colorDialog.ShowDialog() == DialogResult.OK)
            {
                _customTheme.TextColor = _colorDialog.Color;
                _textColorButton.BackColor = _colorDialog.Color;
            }
        }

        private void OnAccentColorClicked(object? sender, EventArgs e)
        {
            _colorDialog.Color = _customTheme.AccentColor;
            if (_colorDialog.ShowDialog() == DialogResult.OK)
            {
                _customTheme.AccentColor = _colorDialog.Color;
                _accentColorButton.BackColor = _colorDialog.Color;
            }
        }

        private void OnButtonColorClicked(object? sender, EventArgs e)
        {
            _colorDialog.Color = _customTheme.ButtonColor;
            if (_colorDialog.ShowDialog() == DialogResult.OK)
            {
                _customTheme.ButtonColor = _colorDialog.Color;
                _buttonColorButton.BackColor = _colorDialog.Color;
            }
        }

        public Theme GetCustomTheme()
        {
            _customTheme.Name = _nameTextBox.Text.Trim();
            return _customTheme;
        }
    }
}
