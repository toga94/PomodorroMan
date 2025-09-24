using System;
using System.Collections.Generic;
using System.Linq;

namespace PomodorroMan
{
    public class ProductivityGoalsManager
    {
        private readonly List<ProductivityGoal> _goals = new();
        private readonly List<GoalProgress> _progressHistory = new();
        private readonly EfficiencyDataManager _dataManager;

        public event EventHandler<GoalAchievedEventArgs>? GoalAchieved;
        public event EventHandler<GoalProgressEventArgs>? GoalProgressUpdated;

        public ProductivityGoalsManager(EfficiencyDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public void CreateGoal(ProductivityGoal goal)
        {
            goal.Id = Guid.NewGuid();
            goal.CreatedAt = DateTime.Now;
            goal.Status = GoalStatus.Active;
            _goals.Add(goal);
        }

        public void UpdateGoalProgress(Guid goalId, double progress)
        {
            var goal = _goals.FirstOrDefault(g => g.Id == goalId);
            if (goal == null) return;

            var progressRecord = new GoalProgress
            {
                GoalId = goalId,
                Timestamp = DateTime.Now,
                Progress = progress,
                IsCompleted = progress >= 100.0
            };

            _progressHistory.Add(progressRecord);

            if (progressRecord.IsCompleted && goal.Status != GoalStatus.Completed)
            {
                goal.Status = GoalStatus.Completed;
                goal.CompletedAt = DateTime.Now;
                OnGoalAchieved(goal);
            }

            OnGoalProgressUpdated(goal, progressRecord);
        }

        public void UpdateDailyGoals()
        {
            var today = DateTime.Today;
            var todaySessions = _dataManager.GetSessions(TimeSpan.FromDays(1));
            var workSessions = todaySessions.Where(s => s.SessionType == EfficiencySessionType.Work).ToList();

            foreach (var goal in _goals.Where(g => g.Status == GoalStatus.Active && g.Type == GoalType.Daily))
            {
                var progress = CalculateGoalProgress(goal, workSessions, today);
                UpdateGoalProgress(goal.Id, progress);
            }
        }

        public void UpdateWeeklyGoals()
        {
            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var weekSessions = _dataManager.GetSessions(weekStart, DateTime.Today);
            var workSessions = weekSessions.Where(s => s.SessionType == EfficiencySessionType.Work).ToList();

            foreach (var goal in _goals.Where(g => g.Status == GoalStatus.Active && g.Type == GoalType.Weekly))
            {
                var progress = CalculateGoalProgress(goal, workSessions, weekStart);
                UpdateGoalProgress(goal.Id, progress);
            }
        }

        private double CalculateGoalProgress(ProductivityGoal goal, List<EfficiencySession> sessions, DateTime periodStart)
        {
            return goal.Metric switch
            {
                GoalMetric.WorkSessions => CalculateWorkSessionsProgress(goal, sessions),
                GoalMetric.TotalWorkTime => CalculateTotalWorkTimeProgress(goal, sessions),
                GoalMetric.AverageFocusScore => CalculateAverageFocusProgress(goal, sessions),
                GoalMetric.AverageEfficiencyScore => CalculateAverageEfficiencyProgress(goal, sessions),
                GoalMetric.DistractionReduction => CalculateDistractionReductionProgress(goal, sessions),
                GoalMetric.ProductivityIndex => CalculateProductivityIndexProgress(goal, sessions),
                _ => 0.0
            };
        }

        private double CalculateWorkSessionsProgress(ProductivityGoal goal, List<EfficiencySession> sessions)
        {
            var currentCount = sessions.Count;
            var targetCount = (int)goal.TargetValue;
            return Math.Min(100.0, (currentCount / targetCount) * 100.0);
        }

        private double CalculateTotalWorkTimeProgress(ProductivityGoal goal, List<EfficiencySession> sessions)
        {
            var currentTime = sessions.Sum(s => s.Metrics.SessionDuration.TotalMinutes);
            var targetTime = goal.TargetValue;
            return Math.Min(100.0, (currentTime / targetTime) * 100.0);
        }

        private double CalculateAverageFocusProgress(ProductivityGoal goal, List<EfficiencySession> sessions)
        {
            if (!sessions.Any()) return 0.0;

            var currentAverage = sessions.Average(s => s.Metrics.FocusScore);
            var targetScore = goal.TargetValue;
            return Math.Min(100.0, (currentAverage / targetScore) * 100.0);
        }

        private double CalculateAverageEfficiencyProgress(ProductivityGoal goal, List<EfficiencySession> sessions)
        {
            if (!sessions.Any()) return 0.0;

            var currentAverage = sessions.Average(s => s.Metrics.EfficiencyScore);
            var targetScore = goal.TargetValue;
            return Math.Min(100.0, (currentAverage / targetScore) * 100.0);
        }

        private double CalculateDistractionReductionProgress(ProductivityGoal goal, List<EfficiencySession> sessions)
        {
            if (!sessions.Any()) return 0.0;

            var currentAverage = sessions.Average(s => s.Metrics.DistractionCount);
            var targetCount = goal.TargetValue;
            
            // For reduction goals, we want lower values to be better
            if (currentAverage <= targetCount)
                return 100.0;
            
            var reductionNeeded = currentAverage - targetCount;
            var maxReduction = currentAverage;
            var progress = Math.Max(0.0, 100.0 - (reductionNeeded / maxReduction) * 100.0);
            
            return progress;
        }

        private double CalculateProductivityIndexProgress(ProductivityGoal goal, List<EfficiencySession> sessions)
        {
            if (!sessions.Any()) return 0.0;

            var currentAverage = sessions.Average(s => s.Metrics.ProductivityIndex);
            var targetIndex = goal.TargetValue;
            return Math.Min(100.0, (currentAverage / targetIndex) * 100.0);
        }

        public List<ProductivityGoal> GetActiveGoals()
        {
            return _goals.Where(g => g.Status == GoalStatus.Active).ToList();
        }

        public List<ProductivityGoal> GetCompletedGoals()
        {
            return _goals.Where(g => g.Status == GoalStatus.Completed).ToList();
        }

        public List<GoalProgress> GetGoalProgress(Guid goalId)
        {
            return _progressHistory.Where(p => p.GoalId == goalId).ToList();
        }

        public GoalStatistics GetGoalStatistics()
        {
            var totalGoals = _goals.Count;
            var completedGoals = _goals.Count(g => g.Status == GoalStatus.Completed);
            var activeGoals = _goals.Count(g => g.Status == GoalStatus.Active);
            var completionRate = totalGoals > 0 ? (double)completedGoals / totalGoals * 100.0 : 0.0;

            var recentProgress = _progressHistory
                .Where(p => p.Timestamp >= DateTime.Today.AddDays(-7))
                .ToList();

            var averageProgress = recentProgress.Any() 
                ? recentProgress.Average(p => p.Progress) 
                : 0.0;

            return new GoalStatistics
            {
                TotalGoals = totalGoals,
                CompletedGoals = completedGoals,
                ActiveGoals = activeGoals,
                CompletionRate = completionRate,
                AverageProgress = averageProgress,
                GoalsThisWeek = _goals.Count(g => g.CreatedAt >= DateTime.Today.AddDays(-7)),
                GoalsCompletedThisWeek = _goals.Count(g => g.CompletedAt >= DateTime.Today.AddDays(-7))
            };
        }

        public void DeleteGoal(Guid goalId)
        {
            var goal = _goals.FirstOrDefault(g => g.Id == goalId);
            if (goal != null)
            {
                goal.Status = GoalStatus.Deleted;
            }
        }

        public void ArchiveGoal(Guid goalId)
        {
            var goal = _goals.FirstOrDefault(g => g.Id == goalId);
            if (goal != null)
            {
                goal.Status = GoalStatus.Archived;
            }
        }

        private void OnGoalAchieved(ProductivityGoal goal)
        {
            GoalAchieved?.Invoke(this, new GoalAchievedEventArgs(goal));
        }

        private void OnGoalProgressUpdated(ProductivityGoal goal, GoalProgress progress)
        {
            GoalProgressUpdated?.Invoke(this, new GoalProgressEventArgs(goal, progress));
        }
    }

    public class ProductivityGoal
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public GoalType Type { get; set; }
        public GoalMetric Metric { get; set; }
        public double TargetValue { get; set; }
        public GoalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
    }

    public class GoalProgress
    {
        public Guid GoalId { get; set; }
        public DateTime Timestamp { get; set; }
        public double Progress { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class GoalStatistics
    {
        public int TotalGoals { get; set; }
        public int CompletedGoals { get; set; }
        public int ActiveGoals { get; set; }
        public double CompletionRate { get; set; }
        public double AverageProgress { get; set; }
        public int GoalsThisWeek { get; set; }
        public int GoalsCompletedThisWeek { get; set; }
    }

    public enum GoalType
    {
        Daily,
        Weekly,
        Monthly,
        Custom
    }

    public enum GoalMetric
    {
        WorkSessions,
        TotalWorkTime,
        AverageFocusScore,
        AverageEfficiencyScore,
        DistractionReduction,
        ProductivityIndex
    }

    public enum GoalStatus
    {
        Active,
        Completed,
        Paused,
        Archived,
        Deleted
    }

    public class GoalAchievedEventArgs : EventArgs
    {
        public ProductivityGoal Goal { get; }

        public GoalAchievedEventArgs(ProductivityGoal goal)
        {
            Goal = goal;
        }
    }

    public class GoalProgressEventArgs : EventArgs
    {
        public ProductivityGoal Goal { get; }
        public GoalProgress Progress { get; }

        public GoalProgressEventArgs(ProductivityGoal goal, GoalProgress progress)
        {
            Goal = goal;
            Progress = progress;
        }
    }
}


