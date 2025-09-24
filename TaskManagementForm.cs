using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PomodorroMan
{
    public partial class TaskManagementForm : Form
    {
        private readonly TaskManager _taskManager;
        private ListBox _taskListBox = null!;
        private TextBox _titleTextBox = null!;
        private TextBox _descriptionTextBox = null!;
        private ComboBox _priorityComboBox = null!;
        private ComboBox _statusComboBox = null!;
        private ComboBox _categoryComboBox = null!;
        private NumericUpDown _estimatedPomodorosNumeric = null!;
        private Button _addButton = null!;
        private Button _updateButton = null!;
        private Button _deleteButton = null!;
        private Button _completePomodoroButton = null!;
        private Label _statsLabel = null!;

        public TaskManagementForm(TaskManager taskManager)
        {
            _taskManager = taskManager;
            InitializeComponent();
            LoadTasks();
            UpdateStats();
        }

        private void InitializeComponent()
        {
            Text = "Task Management - PomodorroMan v1.8.0";
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

            // Left panel - Task list
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var taskListLabel = new Label
            {
                Text = "Tasks",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            _taskListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                SelectionMode = SelectionMode.One,
                DisplayMember = "Title"
            };
            _taskListBox.SelectedIndexChanged += OnTaskSelected;

            leftPanel.Controls.Add(_taskListBox);
            leftPanel.Controls.Add(taskListLabel);

            // Right panel - Task details
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var detailsLabel = new Label
            {
                Text = "Task Details",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            var detailsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Title
            var titleLabel = new Label { Text = "Title:", Location = new Point(10, 20), Size = new Size(80, 20) };
            _titleTextBox = new TextBox { Location = new Point(100, 18), Size = new Size(300, 20) };

            // Description
            var descriptionLabel = new Label { Text = "Description:", Location = new Point(10, 50), Size = new Size(80, 20) };
            _descriptionTextBox = new TextBox 
            { 
                Location = new Point(100, 48), 
                Size = new Size(300, 60), 
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };

            // Priority
            var priorityLabel = new Label { Text = "Priority:", Location = new Point(10, 120), Size = new Size(80, 20) };
            _priorityComboBox = new ComboBox 
            { 
                Location = new Point(100, 118), 
                Size = new Size(150, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _priorityComboBox.Items.AddRange(Enum.GetNames<TaskPriority>());

            // Status
            var statusLabel = new Label { Text = "Status:", Location = new Point(10, 150), Size = new Size(80, 20) };
            _statusComboBox = new ComboBox 
            { 
                Location = new Point(100, 148), 
                Size = new Size(150, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _statusComboBox.Items.AddRange(Enum.GetNames<TaskStatus>());

            // Category
            var categoryLabel = new Label { Text = "Category:", Location = new Point(10, 180), Size = new Size(80, 20) };
            _categoryComboBox = new ComboBox 
            { 
                Location = new Point(100, 178), 
                Size = new Size(150, 20)
            };

            // Estimated Pomodoros
            var estimatedLabel = new Label { Text = "Est. Pomodoros:", Location = new Point(10, 210), Size = new Size(80, 20) };
            _estimatedPomodorosNumeric = new NumericUpDown 
            { 
                Location = new Point(100, 208), 
                Size = new Size(80, 20),
                Minimum = 1,
                Maximum = 20,
                Value = 1
            };

            // Buttons
            _addButton = new Button 
            { 
                Text = "Add Task", 
                Location = new Point(10, 250), 
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _addButton.Click += OnAddTask;

            _updateButton = new Button 
            { 
                Text = "Update", 
                Location = new Point(100, 250), 
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _updateButton.Click += OnUpdateTask;

            _deleteButton = new Button 
            { 
                Text = "Delete", 
                Location = new Point(190, 250), 
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _deleteButton.Click += OnDeleteTask;

            _completePomodoroButton = new Button 
            { 
                Text = "Complete Pomodoro", 
                Location = new Point(10, 290), 
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _completePomodoroButton.Click += OnCompletePomodoro;

            // Stats
            _statsLabel = new Label 
            { 
                Text = "Statistics will appear here",
                Location = new Point(10, 330), 
                Size = new Size(380, 100),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            detailsPanel.Controls.AddRange(new Control[] 
            {
                titleLabel, _titleTextBox,
                descriptionLabel, _descriptionTextBox,
                priorityLabel, _priorityComboBox,
                statusLabel, _statusComboBox,
                categoryLabel, _categoryComboBox,
                estimatedLabel, _estimatedPomodorosNumeric,
                _addButton, _updateButton, _deleteButton, _completePomodoroButton,
                _statsLabel
            });

            rightPanel.Controls.Add(detailsPanel);
            rightPanel.Controls.Add(detailsLabel);

            mainPanel.Controls.Add(leftPanel, 0, 0);
            mainPanel.Controls.Add(rightPanel, 1, 0);

            Controls.Add(mainPanel);

            LoadCategories();
        }

        private void LoadTasks()
        {
            _taskListBox.DataSource = null;
            _taskListBox.DataSource = _taskManager.GetAllTasks();
            _taskListBox.DisplayMember = "Title";
        }

        private void LoadCategories()
        {
            _categoryComboBox.Items.Clear();
            _categoryComboBox.Items.AddRange(_taskManager.GetCategories().ToArray());
        }

        private void OnTaskSelected(object? sender, EventArgs e)
        {
            if (_taskListBox.SelectedItem is Task selectedTask)
            {
                _titleTextBox.Text = selectedTask.Title;
                _descriptionTextBox.Text = selectedTask.Description;
                _priorityComboBox.SelectedItem = selectedTask.Priority.ToString();
                _statusComboBox.SelectedItem = selectedTask.Status.ToString();
                _categoryComboBox.Text = selectedTask.Category;
                _estimatedPomodorosNumeric.Value = selectedTask.EstimatedPomodoros;

                _updateButton.Enabled = true;
                _deleteButton.Enabled = true;
                _completePomodoroButton.Enabled = selectedTask.Status == TaskStatus.InProgress;
            }
            else
            {
                ClearForm();
            }
        }

        private void ClearForm()
        {
            _titleTextBox.Clear();
            _descriptionTextBox.Clear();
            _priorityComboBox.SelectedIndex = -1;
            _statusComboBox.SelectedIndex = -1;
            _categoryComboBox.Text = "";
            _estimatedPomodorosNumeric.Value = 1;

            _updateButton.Enabled = false;
            _deleteButton.Enabled = false;
            _completePomodoroButton.Enabled = false;
        }

        private void OnAddTask(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_titleTextBox.Text))
            {
                MessageBox.Show("Please enter a task title.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var task = new Task
            {
                Title = _titleTextBox.Text.Trim(),
                Description = _descriptionTextBox.Text.Trim(),
                Priority = Enum.Parse<TaskPriority>(_priorityComboBox.Text),
                Status = TaskStatus.Todo,
                Category = _categoryComboBox.Text.Trim(),
                EstimatedPomodoros = (int)_estimatedPomodorosNumeric.Value
            };

            _taskManager.AddTask(task);
            LoadTasks();
            LoadCategories();
            ClearForm();
            UpdateStats();
        }

        private void OnUpdateTask(object? sender, EventArgs e)
        {
            if (_taskListBox.SelectedItem is Task selectedTask)
            {
                selectedTask.Title = _titleTextBox.Text.Trim();
                selectedTask.Description = _descriptionTextBox.Text.Trim();
                selectedTask.Priority = Enum.Parse<TaskPriority>(_priorityComboBox.Text);
                selectedTask.Status = Enum.Parse<TaskStatus>(_statusComboBox.Text);
                selectedTask.Category = _categoryComboBox.Text.Trim();
                selectedTask.EstimatedPomodoros = (int)_estimatedPomodorosNumeric.Value;

                _taskManager.UpdateTask(selectedTask);
                LoadTasks();
                LoadCategories();
                UpdateStats();
            }
        }

        private void OnDeleteTask(object? sender, EventArgs e)
        {
            if (_taskListBox.SelectedItem is Task selectedTask)
            {
                var result = MessageBox.Show($"Are you sure you want to delete '{selectedTask.Title}'?", 
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    _taskManager.DeleteTask(selectedTask.Id);
                    LoadTasks();
                    LoadCategories();
                    ClearForm();
                    UpdateStats();
                }
            }
        }

        private void OnCompletePomodoro(object? sender, EventArgs e)
        {
            if (_taskListBox.SelectedItem is Task selectedTask)
            {
                _taskManager.CompletePomodoro(selectedTask.Id);
                LoadTasks();
                UpdateStats();
            }
        }

        private void UpdateStats()
        {
            var stats = _taskManager.GetStatistics();
            _statsLabel.Text = $"Total Tasks: {stats.TotalTasks}\n" +
                             $"Completed: {stats.CompletedTasks}\n" +
                             $"In Progress: {stats.InProgressTasks}\n" +
                             $"Todo: {stats.TodoTasks}\n" +
                             $"Completion Rate: {stats.CompletionRate:F1}%\n" +
                             $"Pomodoros: {stats.TotalPomodoros}/{stats.EstimatedPomodoros}";
        }
    }
}
