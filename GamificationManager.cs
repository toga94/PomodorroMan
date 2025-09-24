using System;
using System.Collections.Generic;
using System.Linq;

namespace PomodorroMan
{
    public class Achievement
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public AchievementType Type { get; set; }
        public int RequiredValue { get; set; }
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockedAt { get; set; }
        public int Points { get; set; }
    }

    public enum AchievementType
    {
        SessionsCompleted,
        ConsecutiveDays,
        TotalTime,
        FocusScore,
        Streak,
        TasksCompleted,
        PerfectDay,
        Marathon
    }

    public class UserStats
    {
        public int TotalSessions { get; set; }
        public int ConsecutiveDays { get; set; }
        public int TotalMinutes { get; set; }
        public double AverageFocusScore { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TasksCompleted { get; set; }
        public int PerfectDays { get; set; }
        public int TotalPoints { get; set; }
        public DateTime LastActivity { get; set; }
    }

    public class GamificationManager
    {
        private readonly List<Achievement> _achievements = new();
        private UserStats _userStats = new();
        private readonly object _lockObject = new();

        public event EventHandler<AchievementUnlockedEventArgs>? AchievementUnlocked;
        public event EventHandler<StatsUpdatedEventArgs>? StatsUpdated;

        public GamificationManager()
        {
            InitializeAchievements();
        }

        private void InitializeAchievements()
        {
            // Session-based achievements
            AddAchievement(new Achievement
            {
                Id = "first_session",
                Name = "Getting Started",
                Description = "Complete your first Pomodoro session",
                Icon = "🎯",
                Type = AchievementType.SessionsCompleted,
                RequiredValue = 1,
                Points = 10
            });

            AddAchievement(new Achievement
            {
                Id = "ten_sessions",
                Name = "Dedicated Worker",
                Description = "Complete 10 Pomodoro sessions",
                Icon = "🔥",
                Type = AchievementType.SessionsCompleted,
                RequiredValue = 10,
                Points = 50
            });

            AddAchievement(new Achievement
            {
                Id = "fifty_sessions",
                Name = "Productivity Master",
                Description = "Complete 50 Pomodoro sessions",
                Icon = "👑",
                Type = AchievementType.SessionsCompleted,
                RequiredValue = 50,
                Points = 200
            });

            AddAchievement(new Achievement
            {
                Id = "hundred_sessions",
                Name = "Pomodoro Legend",
                Description = "Complete 100 Pomodoro sessions",
                Icon = "🏆",
                Type = AchievementType.SessionsCompleted,
                RequiredValue = 100,
                Points = 500
            });

            // Streak achievements
            AddAchievement(new Achievement
            {
                Id = "three_day_streak",
                Name = "Consistent",
                Description = "Maintain a 3-day streak",
                Icon = "⚡",
                Type = AchievementType.Streak,
                RequiredValue = 3,
                Points = 30
            });

            AddAchievement(new Achievement
            {
                Id = "week_streak",
                Name = "Week Warrior",
                Description = "Maintain a 7-day streak",
                Icon = "💪",
                Type = AchievementType.Streak,
                RequiredValue = 7,
                Points = 100
            });

            AddAchievement(new Achievement
            {
                Id = "month_streak",
                Name = "Month Master",
                Description = "Maintain a 30-day streak",
                Icon = "🌟",
                Type = AchievementType.Streak,
                RequiredValue = 30,
                Points = 500
            });

            // Time-based achievements
            AddAchievement(new Achievement
            {
                Id = "hour_total",
                Name = "Hour Power",
                Description = "Complete 1 hour of focused work",
                Icon = "⏰",
                Type = AchievementType.TotalTime,
                RequiredValue = 60,
                Points = 25
            });

            AddAchievement(new Achievement
            {
                Id = "day_total",
                Name = "Full Day",
                Description = "Complete 8 hours of focused work",
                Icon = "🌅",
                Type = AchievementType.TotalTime,
                RequiredValue = 480,
                Points = 100
            });

            // Focus achievements
            AddAchievement(new Achievement
            {
                Id = "high_focus",
                Name = "Focused Mind",
                Description = "Achieve 90% focus score",
                Icon = "🧠",
                Type = AchievementType.FocusScore,
                RequiredValue = 90,
                Points = 50
            });

            AddAchievement(new Achievement
            {
                Id = "perfect_focus",
                Name = "Zen Master",
                Description = "Achieve 100% focus score",
                Icon = "🎯",
                Type = AchievementType.FocusScore,
                RequiredValue = 100,
                Points = 100
            });

            // Task achievements
            AddAchievement(new Achievement
            {
                Id = "first_task",
                Name = "Task Starter",
                Description = "Complete your first task",
                Icon = "✅",
                Type = AchievementType.TasksCompleted,
                RequiredValue = 1,
                Points = 15
            });

            AddAchievement(new Achievement
            {
                Id = "ten_tasks",
                Name = "Task Master",
                Description = "Complete 10 tasks",
                Icon = "📋",
                Type = AchievementType.TasksCompleted,
                RequiredValue = 10,
                Points = 75
            });

            // Special achievements
            AddAchievement(new Achievement
            {
                Id = "perfect_day",
                Name = "Perfect Day",
                Description = "Complete all planned sessions in a day",
                Icon = "✨",
                Type = AchievementType.PerfectDay,
                RequiredValue = 1,
                Points = 200
            });

            AddAchievement(new Achievement
            {
                Id = "marathon",
                Name = "Marathon Runner",
                Description = "Complete 10 sessions in a single day",
                Icon = "🏃",
                Type = AchievementType.Marathon,
                RequiredValue = 10,
                Points = 300
            });
        }

        public void AddAchievement(Achievement achievement)
        {
            lock (_lockObject)
            {
                _achievements.Add(achievement);
            }
        }

        public void UpdateStats(UserStats stats)
        {
            lock (_lockObject)
            {
                _userStats = stats;
                CheckAchievements();
                StatsUpdated?.Invoke(this, new StatsUpdatedEventArgs(_userStats));
            }
        }

        public void RecordSession(int minutes, double focusScore)
        {
            lock (_lockObject)
            {
                _userStats.TotalSessions++;
                _userStats.TotalMinutes += minutes;
                _userStats.LastActivity = DateTime.Now;

                // Update average focus score
                var totalFocus = _userStats.AverageFocusScore * (_userStats.TotalSessions - 1) + focusScore;
                _userStats.AverageFocusScore = totalFocus / _userStats.TotalSessions;

                CheckAchievements();
            }
        }

        public void RecordTaskCompletion()
        {
            lock (_lockObject)
            {
                _userStats.TasksCompleted++;
                CheckAchievements();
            }
        }

        public void RecordPerfectDay()
        {
            lock (_lockObject)
            {
                _userStats.PerfectDays++;
                CheckAchievements();
            }
        }

        private void CheckAchievements()
        {
            foreach (var achievement in _achievements)
            {
                if (!achievement.IsUnlocked && IsAchievementUnlocked(achievement))
                {
                    achievement.IsUnlocked = true;
                    achievement.UnlockedAt = DateTime.Now;
                    _userStats.TotalPoints += achievement.Points;
                    AchievementUnlocked?.Invoke(this, new AchievementUnlockedEventArgs(achievement));
                }
            }
        }

        private bool IsAchievementUnlocked(Achievement achievement)
        {
            return achievement.Type switch
            {
                AchievementType.SessionsCompleted => _userStats.TotalSessions >= achievement.RequiredValue,
                AchievementType.ConsecutiveDays => _userStats.ConsecutiveDays >= achievement.RequiredValue,
                AchievementType.TotalTime => _userStats.TotalMinutes >= achievement.RequiredValue,
                AchievementType.FocusScore => _userStats.AverageFocusScore >= achievement.RequiredValue,
                AchievementType.Streak => _userStats.CurrentStreak >= achievement.RequiredValue,
                AchievementType.TasksCompleted => _userStats.TasksCompleted >= achievement.RequiredValue,
                AchievementType.PerfectDay => _userStats.PerfectDays >= achievement.RequiredValue,
                AchievementType.Marathon => _userStats.TotalSessions >= achievement.RequiredValue,
                _ => false
            };
        }

        public List<Achievement> GetUnlockedAchievements()
        {
            lock (_lockObject)
            {
                return _achievements.Where(a => a.IsUnlocked).ToList();
            }
        }

        public List<Achievement> GetLockedAchievements()
        {
            lock (_lockObject)
            {
                return _achievements.Where(a => !a.IsUnlocked).ToList();
            }
        }

        public List<Achievement> GetAllAchievements()
        {
            lock (_lockObject)
            {
                return new List<Achievement>(_achievements);
            }
        }

        public UserStats GetUserStats()
        {
            lock (_lockObject)
            {
                return _userStats;
            }
        }

        public int GetTotalPoints()
        {
            lock (_lockObject)
            {
                return _userStats.TotalPoints;
            }
        }

        public int GetUnlockedAchievementCount()
        {
            lock (_lockObject)
            {
                return _achievements.Count(a => a.IsUnlocked);
            }
        }
    }

    public class AchievementUnlockedEventArgs : EventArgs
    {
        public Achievement Achievement { get; }

        public AchievementUnlockedEventArgs(Achievement achievement)
        {
            Achievement = achievement;
        }
    }

    public class StatsUpdatedEventArgs : EventArgs
    {
        public UserStats Stats { get; }

        public StatsUpdatedEventArgs(UserStats stats)
        {
            Stats = stats;
        }
    }
}


