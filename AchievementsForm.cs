using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PomodorroMan
{
    public partial class AchievementsForm : Form
    {
        private readonly GamificationManager _gamificationManager;
        private TabControl _tabControl = null!;
        private ListBox _unlockedAchievementsListBox = null!;
        private ListBox _lockedAchievementsListBox = null!;
        private Label _statsLabel = null!;
        private Label _pointsLabel = null!;

        public AchievementsForm(GamificationManager gamificationManager)
        {
            _gamificationManager = gamificationManager;
            InitializeComponent();
            LoadAchievements();
            UpdateStats();
        }

        private void InitializeComponent()
        {
            Text = "Achievements - PomodorroMan v1.8.0";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Color.FromArgb(248, 249, 250);

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };

            // Stats panel
            var statsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Height = 100
            };

            var statsLabel = new Label
            {
                Text = "Your Progress",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            var statsContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            _pointsLabel = new Label
            {
                Text = "Total Points: 0",
                Location = new Point(10, 10),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 123, 255)
            };

            _statsLabel = new Label
            {
                Text = "Statistics will appear here",
                Location = new Point(10, 40),
                Size = new Size(400, 40),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(73, 80, 87)
            };

            statsContentPanel.Controls.AddRange(new Control[] { _pointsLabel, _statsLabel });
            statsPanel.Controls.Add(statsContentPanel);
            statsPanel.Controls.Add(statsLabel);

            // Achievements panel
            var achievementsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var achievementsLabel = new Label
            {
                Text = "Achievements",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9)
            };

            // Unlocked achievements tab
            var unlockedTab = new TabPage("Unlocked Achievements");
            var unlockedPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var unlockedLabel = new Label
            {
                Text = "Achievements you've unlocked:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            _unlockedAchievementsListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                SelectionMode = SelectionMode.One,
                DisplayMember = "Name"
            };
            _unlockedAchievementsListBox.SelectedIndexChanged += OnAchievementSelected;

            unlockedPanel.Controls.Add(_unlockedAchievementsListBox);
            unlockedPanel.Controls.Add(unlockedLabel);
            unlockedTab.Controls.Add(unlockedPanel);

            // Locked achievements tab
            var lockedTab = new TabPage("Locked Achievements");
            var lockedPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var lockedLabel = new Label
            {
                Text = "Achievements you can still unlock:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            _lockedAchievementsListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                SelectionMode = SelectionMode.One,
                DisplayMember = "Name"
            };
            _lockedAchievementsListBox.SelectedIndexChanged += OnAchievementSelected;

            lockedPanel.Controls.Add(_lockedAchievementsListBox);
            lockedPanel.Controls.Add(lockedLabel);
            lockedTab.Controls.Add(lockedPanel);

            _tabControl.TabPages.AddRange(new TabPage[] { unlockedTab, lockedTab });

            achievementsPanel.Controls.Add(_tabControl);
            achievementsPanel.Controls.Add(achievementsLabel);

            mainPanel.Controls.Add(statsPanel, 0, 0);
            mainPanel.Controls.Add(achievementsPanel, 0, 1);

            Controls.Add(mainPanel);
        }

        private void LoadAchievements()
        {
            _unlockedAchievementsListBox.DataSource = null;
            _unlockedAchievementsListBox.DataSource = _gamificationManager.GetUnlockedAchievements();
            _unlockedAchievementsListBox.DisplayMember = "Name";

            _lockedAchievementsListBox.DataSource = null;
            _lockedAchievementsListBox.DataSource = _gamificationManager.GetLockedAchievements();
            _lockedAchievementsListBox.DisplayMember = "Name";
        }

        private void OnAchievementSelected(object? sender, EventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox?.SelectedItem is Achievement selectedAchievement)
            {
                ShowAchievementDetails(selectedAchievement);
            }
        }

        private void ShowAchievementDetails(Achievement achievement)
        {
            var details = $"Name: {achievement.Name}\n" +
                         $"Description: {achievement.Description}\n" +
                         $"Points: {achievement.Points}\n" +
                         $"Type: {achievement.Type}\n" +
                         $"Required Value: {achievement.RequiredValue}\n" +
                         $"Status: {(achievement.IsUnlocked ? "Unlocked" : "Locked")}";

            if (achievement.IsUnlocked && achievement.UnlockedAt.HasValue)
            {
                details += $"\nUnlocked: {achievement.UnlockedAt.Value:yyyy-MM-dd HH:mm}";
            }

            MessageBox.Show(details, "Achievement Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateStats()
        {
            var stats = _gamificationManager.GetUserStats();
            var totalPoints = _gamificationManager.GetTotalPoints();
            var unlockedCount = _gamificationManager.GetUnlockedAchievementCount();

            _pointsLabel.Text = $"Total Points: {totalPoints}";
            _statsLabel.Text = $"Sessions Completed: {stats.TotalSessions}\n" +
                             $"Total Time: {stats.TotalMinutes} minutes\n" +
                             $"Current Streak: {stats.CurrentStreak} days\n" +
                             $"Longest Streak: {stats.LongestStreak} days\n" +
                             $"Tasks Completed: {stats.TasksCompleted}\n" +
                             $"Perfect Days: {stats.PerfectDays}\n" +
                             $"Achievements Unlocked: {unlockedCount}";
        }
    }
}
