using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PomodorroMan
{
    public partial class BreakActivitiesForm : Form
    {
        private readonly BreakActivityManager _breakActivityManager;
        private ListBox _activitiesListBox = null!;
        private ComboBox _categoryComboBox = null!;
        private ComboBox _typeComboBox = null!;
        private ComboBox _durationComboBox = null!;
        private Button _getRandomButton = null!;
        private Button _getRandomByCategoryButton = null!;
        private Button _getRandomByTypeButton = null!;
        private Button _getRandomByDurationButton = null!;
        private Label _selectedActivityLabel = null!;
        private TextBox _activityDetailsTextBox = null!;

        public BreakActivitiesForm(BreakActivityManager breakActivityManager)
        {
            _breakActivityManager = breakActivityManager;
            InitializeComponent();
            LoadActivities();
        }

        private void InitializeComponent()
        {
            Text = "Break Activities - PomodorroMan v1.8.0";
            Size = new Size(800, 600);
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

            // Left panel - Filters and controls
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var filtersLabel = new Label
            {
                Text = "Filters & Controls",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            var filtersPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Category filter
            var categoryLabel = new Label
            {
                Text = "Category:",
                Location = new Point(10, 20),
                Size = new Size(80, 20),
                Font = new Font("Segoe UI", 9)
            };

            _categoryComboBox = new ComboBox
            {
                Location = new Point(100, 18),
                Size = new Size(150, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _categoryComboBox.SelectedIndexChanged += OnFilterChanged;

            // Type filter
            var typeLabel = new Label
            {
                Text = "Type:",
                Location = new Point(10, 50),
                Size = new Size(80, 20),
                Font = new Font("Segoe UI", 9)
            };

            _typeComboBox = new ComboBox
            {
                Location = new Point(100, 48),
                Size = new Size(150, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _typeComboBox.SelectedIndexChanged += OnFilterChanged;

            // Duration filter
            var durationLabel = new Label
            {
                Text = "Max Duration:",
                Location = new Point(10, 80),
                Size = new Size(80, 20),
                Font = new Font("Segoe UI", 9)
            };

            _durationComboBox = new ComboBox
            {
                Location = new Point(100, 78),
                Size = new Size(150, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _durationComboBox.SelectedIndexChanged += OnFilterChanged;

            // Random buttons
            var randomLabel = new Label
            {
                Text = "Get Random Activity:",
                Location = new Point(10, 120),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            _getRandomButton = new Button
            {
                Text = "🎲 Any Activity",
                Location = new Point(10, 150),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _getRandomButton.Click += OnGetRandomClicked;

            _getRandomByCategoryButton = new Button
            {
                Text = "📂 By Category",
                Location = new Point(140, 150),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _getRandomByCategoryButton.Click += OnGetRandomByCategoryClicked;

            _getRandomByTypeButton = new Button
            {
                Text = "🏷️ By Type",
                Location = new Point(10, 190),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            _getRandomByTypeButton.Click += OnGetRandomByTypeClicked;

            _getRandomByDurationButton = new Button
            {
                Text = "⏱️ By Duration",
                Location = new Point(140, 190),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(111, 66, 193),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _getRandomByDurationButton.Click += OnGetRandomByDurationClicked;

            // Selected activity
            _selectedActivityLabel = new Label
            {
                Text = "No activity selected",
                Location = new Point(10, 240),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            filtersPanel.Controls.AddRange(new Control[]
            {
                categoryLabel, _categoryComboBox,
                typeLabel, _typeComboBox,
                durationLabel, _durationComboBox,
                randomLabel,
                _getRandomButton, _getRandomByCategoryButton,
                _getRandomByTypeButton, _getRandomByDurationButton,
                _selectedActivityLabel
            });

            leftPanel.Controls.Add(filtersPanel);
            leftPanel.Controls.Add(filtersLabel);

            // Right panel - Activity list and details
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var activitiesLabel = new Label
            {
                Text = "Available Activities",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            var activitiesPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            _activitiesListBox = new ListBox
            {
                Location = new Point(10, 10),
                Size = new Size(300, 200),
                Font = new Font("Segoe UI", 9),
                SelectionMode = SelectionMode.One,
                DisplayMember = "Title"
            };
            _activitiesListBox.SelectedIndexChanged += OnActivitySelected;

            var detailsLabel = new Label
            {
                Text = "Activity Details:",
                Location = new Point(10, 220),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            _activityDetailsTextBox = new TextBox
            {
                Location = new Point(10, 240),
                Size = new Size(300, 100),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(248, 249, 250)
            };

            activitiesPanel.Controls.AddRange(new Control[]
            {
                _activitiesListBox,
                detailsLabel,
                _activityDetailsTextBox
            });

            rightPanel.Controls.Add(activitiesPanel);
            rightPanel.Controls.Add(activitiesLabel);

            mainPanel.Controls.Add(leftPanel, 0, 0);
            mainPanel.Controls.Add(rightPanel, 1, 0);

            Controls.Add(mainPanel);

            LoadFilters();
        }

        private void LoadFilters()
        {
            // Load categories
            _categoryComboBox.Items.Clear();
            _categoryComboBox.Items.Add("All Categories");
            _categoryComboBox.Items.AddRange(_breakActivityManager.GetCategories().ToArray());
            _categoryComboBox.SelectedIndex = 0;

            // Load types
            _typeComboBox.Items.Clear();
            _typeComboBox.Items.Add("All Types");
            _typeComboBox.Items.AddRange(Enum.GetNames<BreakActivityType>());
            _typeComboBox.SelectedIndex = 0;

            // Load durations
            _durationComboBox.Items.Clear();
            _durationComboBox.Items.Add("Any Duration");
            _durationComboBox.Items.Add("5 minutes or less");
            _durationComboBox.Items.Add("10 minutes or less");
            _durationComboBox.Items.Add("15 minutes or less");
            _durationComboBox.Items.Add("30 minutes or less");
            _durationComboBox.SelectedIndex = 0;
        }

        private void LoadActivities()
        {
            _activitiesListBox.DataSource = null;
            _activitiesListBox.DataSource = _breakActivityManager.GetAllActivities();
            _activitiesListBox.DisplayMember = "Title";
        }

        private void OnFilterChanged(object? sender, EventArgs e)
        {
            LoadActivities();
        }

        private void OnActivitySelected(object? sender, EventArgs e)
        {
            if (_activitiesListBox.SelectedItem is BreakActivity selectedActivity)
            {
                _selectedActivityLabel.Text = $"Selected: {selectedActivity.Title}";
                _activityDetailsTextBox.Text = $"Title: {selectedActivity.Title}\n" +
                                             $"Description: {selectedActivity.Description}\n" +
                                             $"Type: {selectedActivity.Type}\n" +
                                             $"Category: {selectedActivity.Category}\n" +
                                             $"Duration: {selectedActivity.DurationMinutes} minutes\n" +
                                             $"Icon: {selectedActivity.Icon}";
            }
        }

        private void OnGetRandomClicked(object? sender, EventArgs e)
        {
            var randomActivity = _breakActivityManager.GetRandomActivity();
            ShowRandomActivity(randomActivity);
        }

        private void OnGetRandomByCategoryClicked(object? sender, EventArgs e)
        {
            if (_categoryComboBox.SelectedIndex > 0)
            {
                var category = _categoryComboBox.Text;
                var randomActivity = _breakActivityManager.GetRandomActivityByCategory(category);
                ShowRandomActivity(randomActivity);
            }
            else
            {
                MessageBox.Show("Please select a category first.", "No Category Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnGetRandomByTypeClicked(object? sender, EventArgs e)
        {
            if (_typeComboBox.SelectedIndex > 0)
            {
                var type = Enum.Parse<BreakActivityType>(_typeComboBox.Text);
                var randomActivity = _breakActivityManager.GetRandomActivityByType(type);
                ShowRandomActivity(randomActivity);
            }
            else
            {
                MessageBox.Show("Please select a type first.", "No Type Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OnGetRandomByDurationClicked(object? sender, EventArgs e)
        {
            if (_durationComboBox.SelectedIndex > 0)
            {
                var maxDuration = _durationComboBox.SelectedIndex switch
                {
                    1 => 5,
                    2 => 10,
                    3 => 15,
                    4 => 30,
                    _ => 30
                };
                var randomActivity = _breakActivityManager.GetRandomActivityByDuration(maxDuration);
                ShowRandomActivity(randomActivity);
            }
            else
            {
                MessageBox.Show("Please select a duration first.", "No Duration Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ShowRandomActivity(BreakActivity activity)
        {
            _selectedActivityLabel.Text = $"Random Activity: {activity.Title}";
            _activityDetailsTextBox.Text = $"Title: {activity.Title}\n" +
                                         $"Description: {activity.Description}\n" +
                                         $"Type: {activity.Type}\n" +
                                         $"Category: {activity.Category}\n" +
                                         $"Duration: {activity.DurationMinutes} minutes\n" +
                                         $"Icon: {activity.Icon}";

            MessageBox.Show($"Random Activity: {activity.Title}\n\n{activity.Description}\n\nDuration: {activity.DurationMinutes} minutes", 
                "Break Activity Suggestion", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
