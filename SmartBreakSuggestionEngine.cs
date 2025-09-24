using System;
using System.Collections.Generic;
using System.Linq;

namespace PomodorroMan
{
    public class SmartBreakSuggestionEngine
    {
        private readonly List<WorkPattern> _workPatterns = new();
        private readonly List<BreakSuggestion> _breakSuggestions = new();
        private DateTime _lastBreakTime = DateTime.MinValue;
        private int _consecutiveWorkSessions = 0;
        private double _currentStressLevel = 0.0;

        public event EventHandler<BreakSuggestionEventArgs>? BreakSuggestionGenerated;

        public void AnalyzeWorkPattern(EfficiencyMetrics metrics, TimeSpan sessionDuration)
        {
            var pattern = new WorkPattern
            {
                Timestamp = DateTime.Now,
                SessionDuration = sessionDuration,
                FocusScore = metrics.FocusScore,
                EfficiencyScore = metrics.EfficiencyScore,
                ActivityCount = metrics.ActivityCount,
                DistractionCount = metrics.DistractionCount,
                StressLevel = CalculateStressLevel(metrics)
            };

            _workPatterns.Add(pattern);
            _consecutiveWorkSessions++;

            // Keep only last 50 patterns for analysis
            if (_workPatterns.Count > 50)
            {
                _workPatterns.RemoveAt(0);
            }

            AnalyzeAndSuggestBreak(pattern);
        }

        public void RecordBreakTaken()
        {
            _lastBreakTime = DateTime.Now;
            _consecutiveWorkSessions = 0;
            _currentStressLevel = Math.Max(0, _currentStressLevel - 0.3);
        }

        private double CalculateStressLevel(EfficiencyMetrics metrics)
        {
            var stressFactors = new List<double>();

            // High distraction count increases stress
            if (metrics.DistractionCount > 5)
            {
                stressFactors.Add(metrics.DistractionCount * 0.1);
            }

            // Low focus score increases stress
            if (metrics.FocusScore < 60)
            {
                stressFactors.Add((60 - metrics.FocusScore) * 0.01);
            }

            // High activity count might indicate stress
            if (metrics.ActivityCount > 100)
            {
                stressFactors.Add((metrics.ActivityCount - 100) * 0.005);
            }

            // Time since last break
            var timeSinceLastBreak = DateTime.Now - _lastBreakTime;
            if (timeSinceLastBreak.TotalMinutes > 60)
            {
                stressFactors.Add(timeSinceLastBreak.TotalMinutes * 0.01);
            }

            return Math.Min(1.0, stressFactors.Sum());
        }

        private void AnalyzeAndSuggestBreak(WorkPattern currentPattern)
        {
            var suggestions = new List<BreakSuggestion>();

            // Check if break is needed based on consecutive sessions
            if (_consecutiveWorkSessions >= 4)
            {
                suggestions.Add(new BreakSuggestion
                {
                    Type = BreakType.LongBreak,
                    Priority = BreakPriority.High,
                    Duration = TimeSpan.FromMinutes(15),
                    Reason = "You've completed 4+ consecutive work sessions. Time for a longer break!",
                    Benefits = new[] { "Prevent burnout", "Improve focus", "Reduce stress" },
                    GeneratedAt = DateTime.Now
                });
            }

            // Check stress level
            if (_currentStressLevel > 0.7)
            {
                suggestions.Add(new BreakSuggestion
                {
                    Type = BreakType.StressRelief,
                    Priority = BreakPriority.High,
                    Duration = TimeSpan.FromMinutes(10),
                    Reason = "High stress detected. Take a calming break.",
                    Benefits = new[] { "Reduce stress", "Clear your mind", "Improve mood" },
                    GeneratedAt = DateTime.Now
                });
            }

            // Check focus degradation
            if (currentPattern.FocusScore < 50 && _workPatterns.Count >= 3)
            {
                var recentPatterns = _workPatterns.TakeLast(3).ToList();
                var averageFocus = recentPatterns.Average(p => p.FocusScore);
                
                if (averageFocus < 60)
                {
                    suggestions.Add(new BreakSuggestion
                    {
                        Type = BreakType.FocusRecovery,
                        Priority = BreakPriority.Medium,
                        Duration = TimeSpan.FromMinutes(5),
                        Reason = "Focus levels are declining. A short break can help reset your attention.",
                        Benefits = new[] { "Restore focus", "Clear mental fatigue", "Boost productivity" },
                        GeneratedAt = DateTime.Now
                    });
                }
            }

            // Check for high distraction periods
            if (currentPattern.DistractionCount > 10)
            {
                suggestions.Add(new BreakSuggestion
                {
                    Type = BreakType.DistractionReset,
                    Priority = BreakPriority.Medium,
                    Duration = TimeSpan.FromMinutes(3),
                    Reason = "High distraction level detected. A quick break can help refocus.",
                    Benefits = new[] { "Reset attention", "Reduce distractions", "Improve focus" },
                    GeneratedAt = DateTime.Now
                });
            }

            // Time-based suggestions
            var timeSinceLastBreak = DateTime.Now - _lastBreakTime;
            if (timeSinceLastBreak.TotalMinutes > 90)
            {
                suggestions.Add(new BreakSuggestion
                {
                    Type = BreakType.Regular,
                    Priority = BreakPriority.Low,
                    Duration = TimeSpan.FromMinutes(5),
                    Reason = "It's been over 90 minutes since your last break. Time for a refresh!",
                    Benefits = new[] { "Prevent fatigue", "Maintain energy", "Sustain productivity" },
                    GeneratedAt = DateTime.Now
                });
            }

            // Generate personalized suggestions based on patterns
            var personalizedSuggestions = GeneratePersonalizedSuggestions(currentPattern);
            suggestions.AddRange(personalizedSuggestions);

            // Select the best suggestion
            var bestSuggestion = suggestions
                .OrderByDescending(s => (int)s.Priority)
                .ThenByDescending(s => s.Duration.TotalMinutes)
                .FirstOrDefault();

            if (bestSuggestion != null)
            {
                _breakSuggestions.Add(bestSuggestion);
                OnBreakSuggestionGenerated(bestSuggestion);
            }
        }

        private List<BreakSuggestion> GeneratePersonalizedSuggestions(WorkPattern currentPattern)
        {
            var suggestions = new List<BreakSuggestion>();

            if (_workPatterns.Count < 5) return suggestions;

            // Analyze productivity patterns
            var recentPatterns = _workPatterns.TakeLast(10).ToList();
            var averageEfficiency = recentPatterns.Average(p => p.EfficiencyScore);
            var averageFocus = recentPatterns.Average(p => p.FocusScore);

            // If productivity is consistently high, suggest shorter breaks
            if (averageEfficiency > 80 && averageFocus > 70)
            {
                suggestions.Add(new BreakSuggestion
                {
                    Type = BreakType.MicroBreak,
                    Priority = BreakPriority.Low,
                    Duration = TimeSpan.FromMinutes(2),
                    Reason = "You're in a productive flow! A quick micro-break can help maintain this momentum.",
                    Benefits = new[] { "Maintain flow state", "Prevent eye strain", "Sustain energy" },
                    GeneratedAt = DateTime.Now
                });
            }

            // If productivity is declining, suggest longer breaks
            if (averageEfficiency < 60)
            {
                suggestions.Add(new BreakSuggestion
                {
                    Type = BreakType.ProductivityBoost,
                    Priority = BreakPriority.High,
                    Duration = TimeSpan.FromMinutes(12),
                    Reason = "Productivity levels are below average. A longer break can help you recharge and refocus.",
                    Benefits = new[] { "Recharge energy", "Reset mindset", "Boost creativity" },
                    GeneratedAt = DateTime.Now
                });
            }

            return suggestions;
        }

        public List<BreakSuggestion> GetRecentSuggestions(int count = 10)
        {
            return _breakSuggestions.TakeLast(count).ToList();
        }

        public BreakSuggestion? GetCurrentSuggestion()
        {
            return _breakSuggestions.LastOrDefault();
        }

        public void ClearSuggestions()
        {
            _breakSuggestions.Clear();
        }

        private void OnBreakSuggestionGenerated(BreakSuggestion suggestion)
        {
            BreakSuggestionGenerated?.Invoke(this, new BreakSuggestionEventArgs(suggestion));
        }

        public WorkPatternAnalysis GetWorkPatternAnalysis()
        {
            if (!_workPatterns.Any())
            {
                return new WorkPatternAnalysis();
            }

            var recentPatterns = _workPatterns.TakeLast(20).ToList();
            
            return new WorkPatternAnalysis
            {
                AverageFocusScore = recentPatterns.Average(p => p.FocusScore),
                AverageEfficiencyScore = recentPatterns.Average(p => p.EfficiencyScore),
                AverageStressLevel = recentPatterns.Average(p => p.StressLevel),
                TotalSessions = _workPatterns.Count,
                ConsecutiveSessions = _consecutiveWorkSessions,
                TimeSinceLastBreak = DateTime.Now - _lastBreakTime,
                ProductivityTrend = CalculateProductivityTrend(recentPatterns),
                RecommendedBreakFrequency = CalculateOptimalBreakFrequency(recentPatterns)
            };
        }

        private ProductivityTrend CalculateProductivityTrend(List<WorkPattern> patterns)
        {
            if (patterns.Count < 3) return ProductivityTrend.Stable;

            var firstHalf = patterns.Take(patterns.Count / 2).Average(p => p.EfficiencyScore);
            var secondHalf = patterns.Skip(patterns.Count / 2).Average(p => p.EfficiencyScore);

            var difference = secondHalf - firstHalf;

            return difference switch
            {
                > 10 => ProductivityTrend.Improving,
                < -10 => ProductivityTrend.Declining,
                _ => ProductivityTrend.Stable
            };
        }

        private TimeSpan CalculateOptimalBreakFrequency(List<WorkPattern> patterns)
        {
            if (patterns.Count < 5) return TimeSpan.FromMinutes(25);

            var highProductivityPeriods = patterns
                .Where(p => p.EfficiencyScore > 80)
                .Select(p => p.SessionDuration)
                .ToList();

            if (highProductivityPeriods.Any())
            {
                var averageHighProductivityDuration = TimeSpan.FromSeconds(
                    highProductivityPeriods.Average(d => d.TotalSeconds));
                return averageHighProductivityDuration;
            }

            return TimeSpan.FromMinutes(25);
        }
    }

    public class WorkPattern
    {
        public DateTime Timestamp { get; set; }
        public TimeSpan SessionDuration { get; set; }
        public double FocusScore { get; set; }
        public double EfficiencyScore { get; set; }
        public int ActivityCount { get; set; }
        public int DistractionCount { get; set; }
        public double StressLevel { get; set; }
    }

    public class BreakSuggestion
    {
        public BreakType Type { get; set; }
        public BreakPriority Priority { get; set; }
        public TimeSpan Duration { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string[] Benefits { get; set; } = Array.Empty<string>();
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }

    public class WorkPatternAnalysis
    {
        public double AverageFocusScore { get; set; }
        public double AverageEfficiencyScore { get; set; }
        public double AverageStressLevel { get; set; }
        public int TotalSessions { get; set; }
        public int ConsecutiveSessions { get; set; }
        public TimeSpan TimeSinceLastBreak { get; set; }
        public ProductivityTrend ProductivityTrend { get; set; }
        public TimeSpan RecommendedBreakFrequency { get; set; }
    }

    public enum BreakType
    {
        Regular,
        LongBreak,
        MicroBreak,
        StressRelief,
        FocusRecovery,
        DistractionReset,
        ProductivityBoost
    }

    public enum BreakPriority
    {
        Low,
        Medium,
        High
    }

    public enum ProductivityTrend
    {
        Improving,
        Stable,
        Declining
    }

    public class BreakSuggestionEventArgs : EventArgs
    {
        public BreakSuggestion Suggestion { get; }

        public BreakSuggestionEventArgs(BreakSuggestion suggestion)
        {
            Suggestion = suggestion;
        }
    }
}
