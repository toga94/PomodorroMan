using System;
using System.Collections.Generic;
using System.Linq;

namespace PomodorroMan
{
    public enum FocusLevel
    {
        Low,
        Medium,
        High
    }

    public class WorkEfficiencyCalculator
    {
        private readonly List<EfficiencyDataPoint> _dataPoints = new();
        private readonly object _lockObject = new();
        private DateTime _sessionStartTime;
        private DateTime _lastActivityTime;
        private TimeSpan _totalActiveTime;
        private TimeSpan _totalIdleTime;
        private int _activityCount;
        private int _distractionCount;
        private double _currentFocusScore;
        private double _averageFocusScore;
        private readonly Queue<double> _recentFocusScores = new(EfficiencyConfig.RecentFocusScoresCount);
        private readonly Dictionary<ActivityType, int> _activityTypeCounts = new();
        private DateTime _lastMetricsUpdate = DateTime.MinValue;
        private readonly List<DateTime> _sessionMilestones = new();
        private int _consecutiveHighFocusCount = 0;
        private FocusLevel _currentFocusLevel = FocusLevel.Low;
        private readonly Queue<FocusLevel> _focusLevelHistory = new(10);
        // Configuration moved to EfficiencyConfig class

        public event EventHandler<EfficiencyUpdatedEventArgs>? EfficiencyUpdated;

        public EfficiencyMetrics CurrentMetrics { get; private set; } = new();
        public EfficiencyMetrics SessionMetrics { get; private set; } = new();
        public EfficiencyMetrics DailyMetrics { get; private set; } = new();

        public void StartSession()
        {
            lock (_lockObject)
            {
                _sessionStartTime = DateTime.Now;
                _lastActivityTime = _sessionStartTime;
                _totalActiveTime = TimeSpan.Zero;
                _totalIdleTime = TimeSpan.Zero;
                _activityCount = 0;
                _distractionCount = 0;
                _currentFocusScore = 100.0;
                _recentFocusScores.Clear();
            }
        }

        public void RecordActivity(ActivityType activityType, double intensity = 1.0)
        {
            lock (_lockObject)
            {
                var now = DateTime.Now;
                var timeSinceLastActivity = now - _lastActivityTime;

                // Optimize by skipping very frequent similar activities
                if (timeSinceLastActivity.TotalMilliseconds < 50 && 
                    _dataPoints.Count > 0 && 
                    _dataPoints.Last().ActivityType == activityType)
                {
                    // Update the last data point instead of creating a new one
                    var lastPoint = _dataPoints.Last();
                    lastPoint.Intensity = Math.Max(lastPoint.Intensity, intensity);
                    lastPoint.Timestamp = now;
                    _lastActivityTime = now;
                    UpdateMetrics();
                    OnEfficiencyUpdated();
                    return;
                }

                if (timeSinceLastActivity.TotalSeconds > 5)
                {
                    _totalIdleTime += timeSinceLastActivity;
                }
                else
                {
                    _totalActiveTime += timeSinceLastActivity;
                }

            _lastActivityTime = now;
            _activityCount++;

            // Track activity type counts
            if (!_activityTypeCounts.ContainsKey(activityType))
                _activityTypeCounts[activityType] = 0;
            _activityTypeCounts[activityType]++;

            var dataPoint = new EfficiencyDataPoint
            {
                Timestamp = now,
                ActivityType = activityType,
                Intensity = intensity,
                TimeSinceLastActivity = timeSinceLastActivity,
                FocusScore = CalculateFocusScore(activityType, intensity, timeSinceLastActivity)
            };

                _dataPoints.Add(dataPoint);
                
                // Clean up old data points to prevent memory issues
                if (_dataPoints.Count > EfficiencyConfig.MaxDataPoints)
                {
                    var excessCount = _dataPoints.Count - EfficiencyConfig.MaxDataPoints;
                    _dataPoints.RemoveRange(0, excessCount);
                }
                else if (_dataPoints.Count > EfficiencyConfig.CleanupThreshold)
                {
                    // Proactive cleanup when approaching the limit
                    var cleanupCount = _dataPoints.Count - EfficiencyConfig.CleanupThreshold;
                    _dataPoints.RemoveRange(0, cleanupCount);
                }
                
                UpdateMetrics();
                OnEfficiencyUpdated();
            }
        }

        public void RecordDistraction()
        {
            lock (_lockObject)
            {
                _distractionCount++;
                _currentFocusScore = Math.Max(0, _currentFocusScore - 5);
                _recentFocusScores.Enqueue(_currentFocusScore);
                
                if (_recentFocusScores.Count > 10)
                {
                    _recentFocusScores.Dequeue();
                }

                _averageFocusScore = _recentFocusScores.Average();
                UpdateMetrics();
                OnEfficiencyUpdated();
            }
        }

        public void EndSession()
        {
            lock (_lockObject)
            {
                var sessionDuration = DateTime.Now - _sessionStartTime;
                var finalIdleTime = DateTime.Now - _lastActivityTime;
                _totalIdleTime += finalIdleTime;

                SessionMetrics = CalculateSessionMetrics(sessionDuration);
                UpdateDailyMetrics();
                OnEfficiencyUpdated();
            }
        }

        private double CalculateFocusScore(ActivityType activityType, double intensity, TimeSpan timeSinceLastActivity)
        {
            var activityMultiplier = activityType switch
            {
                ActivityType.MouseMovement => EfficiencyConfig.MouseMovementMultiplier,
                ActivityType.KeyboardTyping => EfficiencyConfig.KeyboardTypingMultiplier,
                ActivityType.Scrolling => EfficiencyConfig.ScrollingMultiplier,
                ActivityType.WindowSwitching => EfficiencyConfig.WindowSwitchingMultiplier,
                ActivityType.Idle => EfficiencyConfig.IdleMultiplier,
                _ => 0.0
            };

            var timeMultiplier = timeSinceLastActivity.TotalSeconds switch
            {
                < 1 => EfficiencyConfig.VeryRecentTimeMultiplier,
                < 5 => EfficiencyConfig.RecentTimeMultiplier,
                < 10 => EfficiencyConfig.MediumTimeMultiplier,
                < 30 => EfficiencyConfig.LongTimeMultiplier,
                _ => EfficiencyConfig.VeryLongTimeMultiplier
            };

            var intensityMultiplier = Math.Max(EfficiencyConfig.MinIntensityMultiplier, Math.Min(EfficiencyConfig.MaxIntensityMultiplier, intensity));

            var scoreChange = activityMultiplier * timeMultiplier * intensityMultiplier;
            _currentFocusScore = Math.Max(EfficiencyConfig.MinFocusScore, Math.Min(EfficiencyConfig.MaxFocusScore, _currentFocusScore + scoreChange));

            // Update focus level based on new score
            UpdateFocusLevel(_currentFocusScore);

            return _currentFocusScore;
        }

        private double ValidateFocusScore(double score)
        {
            return Math.Max(EfficiencyConfig.MinFocusScore, Math.Min(EfficiencyConfig.MaxFocusScore, score));
        }

        public Dictionary<ActivityType, int> GetActivityTypeCounts()
        {
            lock (_lockObject)
            {
                return new Dictionary<ActivityType, int>(_activityTypeCounts);
            }
        }

        public void ResetActivityTypeCounts()
        {
            lock (_lockObject)
            {
                _activityTypeCounts.Clear();
            }
        }

        public void RecordSessionMilestone()
        {
            lock (_lockObject)
            {
                _sessionMilestones.Add(DateTime.Now);
                
                // Keep only recent milestones
                if (_sessionMilestones.Count > EfficiencyConfig.MaxSessionHistory)
                {
                    _sessionMilestones.RemoveAt(0);
                }
            }
        }

        public List<DateTime> GetSessionMilestones()
        {
            lock (_lockObject)
            {
                return new List<DateTime>(_sessionMilestones);
            }
        }

        public int GetConsecutiveHighFocusCount()
        {
            lock (_lockObject)
            {
                return _consecutiveHighFocusCount;
            }
        }

        public FocusLevel GetCurrentFocusLevel()
        {
            lock (_lockObject)
            {
                return _currentFocusLevel;
            }
        }

        public FocusLevel DetermineFocusLevel(double focusScore)
        {
            if (focusScore >= EfficiencyConfig.HighFocusThreshold)
                return FocusLevel.High;
            else if (focusScore >= EfficiencyConfig.MediumFocusThreshold)
                return FocusLevel.Medium;
            else
                return FocusLevel.Low;
        }

        public void UpdateFocusLevel(double focusScore)
        {
            lock (_lockObject)
            {
                var newFocusLevel = DetermineFocusLevel(focusScore);
                if (newFocusLevel != _currentFocusLevel)
                {
                    _currentFocusLevel = newFocusLevel;
                    _focusLevelHistory.Enqueue(newFocusLevel);
                    
                    // Keep only recent focus levels
                    if (_focusLevelHistory.Count > EfficiencyConfig.RecentFocusScoresCount)
                    {
                        _focusLevelHistory.Dequeue();
                    }
                }
            }
        }

        public Queue<FocusLevel> GetFocusLevelHistory()
        {
            lock (_lockObject)
            {
                return new Queue<FocusLevel>(_focusLevelHistory);
            }
        }

        private void UpdateMetrics()
        {
            var sessionDuration = DateTime.Now - _sessionStartTime;
            var activePercentage = sessionDuration.TotalSeconds > 0 
                ? (_totalActiveTime.TotalSeconds / sessionDuration.TotalSeconds) * 100 
                : 0;

            var efficiencyScore = CalculateEfficiencyScore(activePercentage, _currentFocusScore, _distractionCount);
            var productivityIndex = CalculateProductivityIndex(_activityCount, _distractionCount, sessionDuration);

            CurrentMetrics = new EfficiencyMetrics
            {
                FocusScore = _currentFocusScore,
                AverageFocusScore = _averageFocusScore,
                ActiveTime = _totalActiveTime,
                IdleTime = _totalIdleTime,
                ActivePercentage = activePercentage,
                EfficiencyScore = efficiencyScore,
                ProductivityIndex = productivityIndex,
                ActivityCount = _activityCount,
                DistractionCount = _distractionCount,
                SessionDuration = sessionDuration
            };
        }

        private EfficiencyMetrics CalculateSessionMetrics(TimeSpan sessionDuration)
        {
            var activePercentage = sessionDuration.TotalSeconds > 0 
                ? (_totalActiveTime.TotalSeconds / sessionDuration.TotalSeconds) * 100 
                : 0;

            var efficiencyScore = CalculateEfficiencyScore(activePercentage, _averageFocusScore, _distractionCount);
            var productivityIndex = CalculateProductivityIndex(_activityCount, _distractionCount, sessionDuration);

            return new EfficiencyMetrics
            {
                FocusScore = _averageFocusScore,
                AverageFocusScore = _averageFocusScore,
                ActiveTime = _totalActiveTime,
                IdleTime = _totalIdleTime,
                ActivePercentage = activePercentage,
                EfficiencyScore = efficiencyScore,
                ProductivityIndex = productivityIndex,
                ActivityCount = _activityCount,
                DistractionCount = _distractionCount,
                SessionDuration = sessionDuration
            };
        }

        private void UpdateDailyMetrics()
        {
            var today = DateTime.Today;
            var todaySessions = _dataPoints
                .Where(dp => dp.Timestamp.Date == today)
                .ToList();

            if (!todaySessions.Any()) return;

            var totalActiveTime = TimeSpan.FromSeconds(
                todaySessions.Where(dp => dp.ActivityType != ActivityType.Idle)
                    .Sum(dp => dp.TimeSinceLastActivity.TotalSeconds));

            var totalSessionTime = todaySessions.Any() 
                ? todaySessions.Max(dp => dp.Timestamp) - todaySessions.Min(dp => dp.Timestamp)
                : TimeSpan.Zero;

            var activePercentage = totalSessionTime.TotalSeconds > 0 
                ? (totalActiveTime.TotalSeconds / totalSessionTime.TotalSeconds) * 100 
                : 0;

            var averageFocusScore = todaySessions.Any() ? todaySessions.Average(dp => dp.FocusScore) : 0.0;
            var totalDistractions = todaySessions.Count(dp => dp.ActivityType == ActivityType.WindowSwitching);
            var totalActivities = todaySessions.Count;

            var efficiencyScore = CalculateEfficiencyScore(activePercentage, averageFocusScore, totalDistractions);
            var productivityIndex = CalculateProductivityIndex(totalActivities, totalDistractions, totalSessionTime);

            DailyMetrics = new EfficiencyMetrics
            {
                FocusScore = averageFocusScore,
                AverageFocusScore = averageFocusScore,
                ActiveTime = totalActiveTime,
                IdleTime = totalSessionTime - totalActiveTime,
                ActivePercentage = activePercentage,
                EfficiencyScore = efficiencyScore,
                ProductivityIndex = productivityIndex,
                ActivityCount = totalActivities,
                DistractionCount = totalDistractions,
                SessionDuration = totalSessionTime
            };
        }

        private double CalculateEfficiencyScore(double activePercentage, double focusScore, int distractionCount)
        {
            var baseScore = (activePercentage * 0.4) + (focusScore * 0.4);
            var distractionPenalty = Math.Min(20, distractionCount * 2);
            return Math.Max(0, Math.Min(100, baseScore - distractionPenalty));
        }

        private double CalculateProductivityIndex(int activityCount, int distractionCount, TimeSpan duration)
        {
            if (duration.TotalMinutes == 0) return 0;

            var activityRate = activityCount / duration.TotalMinutes;
            var distractionRate = distractionCount / duration.TotalMinutes;
            var productivityRatio = activityRate / Math.Max(1, distractionRate + 1);
            
            return Math.Max(0, Math.Min(100, productivityRatio * 10));
        }

        public EfficiencyReport GenerateReport(TimeSpan timeRange)
        {
            lock (_lockObject)
            {
                var cutoffTime = DateTime.Now - timeRange;
                var relevantData = _dataPoints.Where(dp => dp.Timestamp >= cutoffTime).ToList();

                if (!relevantData.Any())
                {
                    return new EfficiencyReport
                    {
                        TimeRange = timeRange,
                        GeneratedAt = DateTime.Now,
                        Metrics = new EfficiencyMetrics(),
                        Recommendations = new List<string> { "No data available for the specified time range." }
                    };
                }

                var totalActiveTime = TimeSpan.FromSeconds(
                    relevantData.Where(dp => dp.ActivityType != ActivityType.Idle)
                        .Sum(dp => dp.TimeSinceLastActivity.TotalSeconds));

                var totalTime = relevantData.Any() 
                    ? relevantData.Max(dp => dp.Timestamp) - relevantData.Min(dp => dp.Timestamp)
                    : TimeSpan.Zero;

                var activePercentage = totalTime.TotalSeconds > 0 
                    ? (totalActiveTime.TotalSeconds / totalTime.TotalSeconds) * 100 
                    : 0;

                var averageFocusScore = relevantData.Average(dp => dp.FocusScore);
                var totalDistractions = relevantData.Count(dp => dp.ActivityType == ActivityType.WindowSwitching);
                var totalActivities = relevantData.Count;

                var efficiencyScore = CalculateEfficiencyScore(activePercentage, averageFocusScore, totalDistractions);
                var productivityIndex = CalculateProductivityIndex(totalActivities, totalDistractions, totalTime);

                var metrics = new EfficiencyMetrics
                {
                    FocusScore = averageFocusScore,
                    AverageFocusScore = averageFocusScore,
                    ActiveTime = totalActiveTime,
                    IdleTime = totalTime - totalActiveTime,
                    ActivePercentage = activePercentage,
                    EfficiencyScore = efficiencyScore,
                    ProductivityIndex = productivityIndex,
                    ActivityCount = totalActivities,
                    DistractionCount = totalDistractions,
                    SessionDuration = totalTime
                };

                var recommendations = GenerateRecommendations(metrics);

                return new EfficiencyReport
                {
                    TimeRange = timeRange,
                    GeneratedAt = DateTime.Now,
                    Metrics = metrics,
                    Recommendations = recommendations
                };
            }
        }

        private List<string> GenerateRecommendations(EfficiencyMetrics metrics)
        {
            var recommendations = new List<string>();

            if (metrics.ActivePercentage < 70)
            {
                recommendations.Add("Consider reducing idle time to improve productivity");
            }

            if (metrics.FocusScore < 60)
            {
                recommendations.Add("Try to minimize distractions and maintain focus");
            }

            if (metrics.DistractionCount > 10)
            {
                recommendations.Add("High distraction count detected - consider using focus mode");
            }

            if (metrics.EfficiencyScore > 80)
            {
                recommendations.Add("Excellent work efficiency! Keep up the great work!");
            }
            else if (metrics.EfficiencyScore > 60)
            {
                recommendations.Add("Good efficiency - room for improvement in focus areas");
            }
            else
            {
                recommendations.Add("Consider taking breaks and reorganizing your work approach");
            }

            return recommendations;
        }

        private void OnEfficiencyUpdated()
        {
            EfficiencyUpdated?.Invoke(this, new EfficiencyUpdatedEventArgs(CurrentMetrics));
        }

        public void ClearData()
        {
            lock (_lockObject)
            {
                _dataPoints.Clear();
                _recentFocusScores.Clear();
                CurrentMetrics = new EfficiencyMetrics();
                SessionMetrics = new EfficiencyMetrics();
                DailyMetrics = new EfficiencyMetrics();
            }
        }
    }

    public enum ActivityType
    {
        MouseMovement,
        KeyboardTyping,
        Scrolling,
        WindowSwitching,
        Idle
    }

    public class EfficiencyDataPoint
    {
        public DateTime Timestamp { get; set; }
        public ActivityType ActivityType { get; set; }
        public double Intensity { get; set; }
        public TimeSpan TimeSinceLastActivity { get; set; }
        public double FocusScore { get; set; }
    }

    public class EfficiencyMetrics
    {
        public double FocusScore { get; set; }
        public double AverageFocusScore { get; set; }
        public TimeSpan ActiveTime { get; set; }
        public TimeSpan IdleTime { get; set; }
        public double ActivePercentage { get; set; }
        public double EfficiencyScore { get; set; }
        public double ProductivityIndex { get; set; }
        public int ActivityCount { get; set; }
        public int DistractionCount { get; set; }
        public TimeSpan SessionDuration { get; set; }
    }

    public class EfficiencyReport
    {
        public TimeSpan TimeRange { get; set; }
        public DateTime GeneratedAt { get; set; }
        public EfficiencyMetrics Metrics { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    public class EfficiencyUpdatedEventArgs : EventArgs
    {
        public EfficiencyMetrics Metrics { get; }

        public EfficiencyUpdatedEventArgs(EfficiencyMetrics metrics)
        {
            Metrics = metrics;
        }
    }
}
