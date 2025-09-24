using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace PomodorroMan
{
    public class EfficiencyDataManager
    {
        private readonly string _dataFilePath;
        private readonly List<EfficiencySession> _sessions;
        private readonly object _lockObject = new();

        public EfficiencyDataManager()
        {
            _dataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "efficiency_data.json");
            _sessions = new List<EfficiencySession>();
            LoadData();
        }

        public void SaveSession(EfficiencySession session)
        {
            lock (_lockObject)
            {
                _sessions.Add(session);
                SaveData();
            }
        }

        public void SaveMetrics(EfficiencyMetrics metrics, DateTime timestamp, EfficiencySessionType sessionType)
        {
            lock (_lockObject)
            {
                var session = new EfficiencySession
                {
                    Id = Guid.NewGuid(),
                    StartTime = timestamp,
                    EndTime = timestamp,
                    SessionType = sessionType,
                    Metrics = metrics
                };

                _sessions.Add(session);
                SaveData();
            }
        }

        public List<EfficiencySession> GetSessions(TimeSpan timeRange)
        {
            lock (_lockObject)
            {
                var cutoffTime = DateTime.Now - timeRange;
                return _sessions.Where(s => s.StartTime >= cutoffTime).ToList();
            }
        }

        public List<EfficiencySession> GetSessions(DateTime startDate, DateTime endDate)
        {
            lock (_lockObject)
            {
                return _sessions.Where(s => s.StartTime >= startDate && s.StartTime <= endDate).ToList();
            }
        }

        public EfficiencyStatistics GetStatistics(TimeSpan timeRange)
        {
            lock (_lockObject)
            {
                var sessions = GetSessions(timeRange);
                if (!sessions.Any())
                {
                    return new EfficiencyStatistics();
                }

                var totalSessions = sessions.Count;
                var totalDuration = TimeSpan.FromSeconds(sessions.Sum(s => s.Metrics.SessionDuration.TotalSeconds));
                var averageEfficiency = sessions.Average(s => s.Metrics.EfficiencyScore);
                var averageFocus = sessions.Average(s => s.Metrics.FocusScore);
                var averageProductivity = sessions.Average(s => s.Metrics.ProductivityIndex);
                var totalActivities = sessions.Sum(s => s.Metrics.ActivityCount);
                var totalDistractions = sessions.Sum(s => s.Metrics.DistractionCount);
                var totalActiveTime = TimeSpan.FromSeconds(sessions.Sum(s => s.Metrics.ActiveTime.TotalSeconds));
                var totalIdleTime = TimeSpan.FromSeconds(sessions.Sum(s => s.Metrics.IdleTime.TotalSeconds));

                var averageActivePercentage = sessions.Average(s => s.Metrics.ActivePercentage);

                var bestSession = sessions.OrderByDescending(s => s.Metrics.EfficiencyScore).FirstOrDefault();
                var worstSession = sessions.OrderBy(s => s.Metrics.EfficiencyScore).FirstOrDefault();

                return new EfficiencyStatistics
                {
                    TimeRange = timeRange,
                    TotalSessions = totalSessions,
                    TotalDuration = totalDuration,
                    AverageEfficiencyScore = averageEfficiency,
                    AverageFocusScore = averageFocus,
                    AverageProductivityIndex = averageProductivity,
                    TotalActivities = totalActivities,
                    TotalDistractions = totalDistractions,
                    TotalActiveTime = totalActiveTime,
                    TotalIdleTime = totalIdleTime,
                    AverageActivePercentage = averageActivePercentage,
                    BestSession = bestSession,
                    WorstSession = worstSession,
                    GeneratedAt = DateTime.Now
                };
            }
        }

        public void ClearData()
        {
            lock (_lockObject)
            {
                _sessions.Clear();
                SaveData();
            }
        }

        public void ClearOldData(int daysToKeep)
        {
            lock (_lockObject)
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                _sessions.RemoveAll(s => s.StartTime < cutoffDate);
                SaveData();
            }
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    string json = File.ReadAllText(_dataFilePath);
                    var sessions = JsonSerializer.Deserialize<List<EfficiencySession>>(json);
                    if (sessions != null)
                    {
                        _sessions.AddRange(sessions);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load efficiency data: {ex.Message}");
            }
        }

        private void SaveData()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string json = JsonSerializer.Serialize(_sessions, options);
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save efficiency data: {ex.Message}");
            }
        }
    }

    public class EfficiencySession
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public EfficiencySessionType SessionType { get; set; }
        public EfficiencyMetrics Metrics { get; set; } = new();
        public string? Notes { get; set; }
    }

    public class EfficiencyStatistics
    {
        public TimeSpan TimeRange { get; set; }
        public int TotalSessions { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public double AverageEfficiencyScore { get; set; }
        public double AverageFocusScore { get; set; }
        public double AverageProductivityIndex { get; set; }
        public int TotalActivities { get; set; }
        public int TotalDistractions { get; set; }
        public TimeSpan TotalActiveTime { get; set; }
        public TimeSpan TotalIdleTime { get; set; }
        public double AverageActivePercentage { get; set; }
        public EfficiencySession? BestSession { get; set; }
        public EfficiencySession? WorstSession { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public enum EfficiencySessionType
    {
        Work,
        ShortBreak,
        LongBreak
    }
}
