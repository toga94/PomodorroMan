using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PomodorroMan
{
    public partial class AmbientSoundsForm : Form
    {
        private readonly AmbientSoundManager _soundManager;
        private ListBox _soundsListBox = null!;
        private ComboBox _categoryComboBox = null!;
        private TrackBar _volumeTrackBar = null!;
        private Label _volumeLabel = null!;
        private Button _playButton = null!;
        private Button _stopButton = null!;
        private Label _currentSoundLabel = null!;
        private CheckBox _loopCheckBox = null!;

        public AmbientSoundsForm(AmbientSoundManager soundManager)
        {
            _soundManager = soundManager;
            InitializeComponent();
            LoadSounds();
            UpdateUI();
        }

        private void InitializeComponent()
        {
            Text = "Ambient Sounds - PomodorroMan v1.8.0";
            Size = new Size(600, 500);
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

            // Left panel - Sound list
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var soundsLabel = new Label
            {
                Text = "Available Sounds",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            var categoryLabel = new Label
            {
                Text = "Category:",
                Font = new Font("Segoe UI", 9),
                Location = new Point(10, 40),
                Size = new Size(60, 20)
            };

            _categoryComboBox = new ComboBox
            {
                Location = new Point(80, 38),
                Size = new Size(150, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _categoryComboBox.SelectedIndexChanged += OnCategoryChanged;

            _soundsListBox = new ListBox
            {
                Location = new Point(10, 70),
                Size = new Size(280, 300),
                Font = new Font("Segoe UI", 9),
                SelectionMode = SelectionMode.One,
                DisplayMember = "Name"
            };
            _soundsListBox.SelectedIndexChanged += OnSoundSelected;

            leftPanel.Controls.AddRange(new Control[] { soundsLabel, categoryLabel, _categoryComboBox, _soundsListBox });

            // Right panel - Controls
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var controlsLabel = new Label
            {
                Text = "Sound Controls",
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

            // Current sound
            _currentSoundLabel = new Label
            {
                Text = "No sound playing",
                Location = new Point(10, 20),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            // Play/Stop buttons
            _playButton = new Button
            {
                Text = "▶ Play",
                Location = new Point(10, 60),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _playButton.Click += OnPlayClicked;

            _stopButton = new Button
            {
                Text = "⏹ Stop",
                Location = new Point(100, 60),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _stopButton.Click += OnStopClicked;

            // Volume control
            var volumeLabel = new Label
            {
                Text = "Volume:",
                Location = new Point(10, 110),
                Size = new Size(60, 20),
                Font = new Font("Segoe UI", 9)
            };

            _volumeTrackBar = new TrackBar
            {
                Location = new Point(80, 100),
                Size = new Size(200, 40),
                Minimum = 0,
                Maximum = 100,
                Value = 50,
                TickFrequency = 10
            };
            _volumeTrackBar.ValueChanged += OnVolumeChanged;

            _volumeLabel = new Label
            {
                Text = "50%",
                Location = new Point(290, 110),
                Size = new Size(30, 20),
                Font = new Font("Segoe UI", 9)
            };

            // Loop checkbox
            _loopCheckBox = new CheckBox
            {
                Text = "Loop sound",
                Location = new Point(10, 150),
                Size = new Size(120, 20),
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };

            // Sound info
            var infoLabel = new Label
            {
                Text = "Sound Information",
                Location = new Point(10, 180),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var infoTextBox = new TextBox
            {
                Location = new Point(10, 200),
                Size = new Size(300, 100),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            controlsPanel.Controls.AddRange(new Control[]
            {
                _currentSoundLabel,
                _playButton, _stopButton,
                volumeLabel, _volumeTrackBar, _volumeLabel,
                _loopCheckBox,
                infoLabel, infoTextBox
            });

            rightPanel.Controls.Add(controlsPanel);
            rightPanel.Controls.Add(controlsLabel);

            mainPanel.Controls.Add(leftPanel, 0, 0);
            mainPanel.Controls.Add(rightPanel, 1, 0);

            Controls.Add(mainPanel);

            LoadCategories();
        }

        private void LoadCategories()
        {
            _categoryComboBox.Items.Clear();
            _categoryComboBox.Items.Add("All Categories");
            _categoryComboBox.Items.AddRange(Enum.GetNames<SoundCategory>());
            _categoryComboBox.SelectedIndex = 0;
        }

        private void LoadSounds()
        {
            _soundsListBox.DataSource = null;
            
            if (_categoryComboBox.SelectedIndex == 0)
            {
                _soundsListBox.DataSource = _soundManager.GetAllSounds();
            }
            else
            {
                var category = Enum.Parse<SoundCategory>(_categoryComboBox.Text);
                _soundsListBox.DataSource = _soundManager.GetSoundsByCategory(category);
            }
            
            _soundsListBox.DisplayMember = "Name";
        }

        private void OnCategoryChanged(object? sender, EventArgs e)
        {
            LoadSounds();
        }

        private void OnSoundSelected(object? sender, EventArgs e)
        {
            UpdateUI();
        }

        private void OnPlayClicked(object? sender, EventArgs e)
        {
            if (_soundsListBox.SelectedItem is AmbientSound selectedSound)
            {
                _soundManager.PlaySound(selectedSound.Name);
                UpdateUI();
            }
            else
            {
                MessageBox.Show("Please select a sound to play.", "No Sound Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnStopClicked(object? sender, EventArgs e)
        {
            _soundManager.StopCurrentSound();
            UpdateUI();
        }

        private void OnVolumeChanged(object? sender, EventArgs e)
        {
            var volume = _volumeTrackBar.Value / 100f;
            _soundManager.SetVolume(volume);
            _volumeLabel.Text = $"{_volumeTrackBar.Value}%";
        }

        private void UpdateUI()
        {
            var currentSound = _soundManager.GetCurrentSound();
            var isPlaying = _soundManager.IsPlaying;

            if (currentSound != null && isPlaying)
            {
                _currentSoundLabel.Text = $"Now Playing: {currentSound.Name}";
                _currentSoundLabel.ForeColor = Color.FromArgb(40, 167, 69);
                _playButton.Enabled = false;
                _stopButton.Enabled = true;
            }
            else
            {
                _currentSoundLabel.Text = "No sound playing";
                _currentSoundLabel.ForeColor = Color.FromArgb(73, 80, 87);
                _playButton.Enabled = _soundsListBox.SelectedItem != null;
                _stopButton.Enabled = false;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _soundManager.StopCurrentSound();
            base.OnFormClosed(e);
        }
    }
}
