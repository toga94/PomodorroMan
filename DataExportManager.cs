using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace PomodorroMan
{
    public class DataExportManager
    {
        private readonly EfficiencyDataManager _dataManager;

        public DataExportManager(EfficiencyDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public void ExportToJson(string filePath, TimeSpan timeRange)
        {
            try
            {
                var sessions = _dataManager.GetSessions(timeRange);
                var statistics = _dataManager.GetStatistics(timeRange);
                
                var exportData = new
                {
                    ExportInfo = new
                    {
                        GeneratedAt = DateTime.Now,
                        TimeRange = timeRange,
                        Version = EfficiencyConfig.ExportVersion,
                        TotalSessions = sessions.Count
                    },
                    Statistics = statistics,
                    Sessions = sessions.Select(s => new
                    {
                        s.Id,
                        s.StartTime,
                        s.EndTime,
                        s.SessionType,
                        s.Notes,
                        Metrics = new
                        {
                            s.Metrics.FocusScore,
                            s.Metrics.EfficiencyScore,
                            s.Metrics.ProductivityIndex,
                            s.Metrics.ActiveTime,
                            s.Metrics.IdleTime,
                            s.Metrics.ActivePercentage,
                            s.Metrics.ActivityCount,
                            s.Metrics.DistractionCount,
                            s.Metrics.SessionDuration
                        }
                    })
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, EfficiencyConfig.FileStreamBufferSize, FileOptions.SequentialScan);
        using var writer = new StreamWriter(fileStream, Encoding.UTF8, EfficiencyConfig.FileStreamBufferSize);
                var json = JsonSerializer.Serialize(exportData, options);
                writer.Write(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to export JSON data: {ex.Message}", ex);
            }
        }

        public void ExportToCsv(string filePath, TimeSpan timeRange)
        {
            var sessions = _dataManager.GetSessions(timeRange);
            var csv = new StringBuilder();
            
            // Header
            csv.AppendLine("SessionId,StartTime,EndTime,SessionType,FocusScore,EfficiencyScore,ProductivityIndex,ActiveTime,IdleTime,ActivePercentage,ActivityCount,DistractionCount,SessionDuration,Notes");
            
            // Data rows
            foreach (var session in sessions)
            {
                csv.AppendLine($"{session.Id},{session.StartTime:yyyy-MM-dd HH:mm:ss},{session.EndTime:yyyy-MM-dd HH:mm:ss},{session.SessionType},{session.Metrics.FocusScore:F2},{session.Metrics.EfficiencyScore:F2},{session.Metrics.ProductivityIndex:F2},{session.Metrics.ActiveTime.TotalMinutes:F2},{session.Metrics.IdleTime.TotalMinutes:F2},{session.Metrics.ActivePercentage:F2},{session.Metrics.ActivityCount},{session.Metrics.DistractionCount},{session.Metrics.SessionDuration.TotalMinutes:F2},\"{session.Notes ?? ""}\"");
            }
            
            File.WriteAllText(filePath, csv.ToString());
        }

        public void ExportToTxt(string filePath, TimeSpan timeRange)
        {
            var sessions = _dataManager.GetSessions(timeRange);
            var statistics = _dataManager.GetStatistics(timeRange);
            var report = new StringBuilder();
            
            report.AppendLine("PomodorroMan Efficiency Report");
            report.AppendLine("==============================");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Version: 1.8.1");
            report.AppendLine($"Time Range: {timeRange}");
            report.AppendLine();
            
            report.AppendLine("Summary Statistics");
            report.AppendLine("-----------------");
            report.AppendLine($"Total Sessions: {statistics.TotalSessions}");
            report.AppendLine($"Total Duration: {FormatTimeSpan(statistics.TotalDuration)}");
            report.AppendLine($"Average Efficiency Score: {statistics.AverageEfficiencyScore:F2}%");
            report.AppendLine($"Average Focus Score: {statistics.AverageFocusScore:F2}%");
            report.AppendLine($"Average Productivity Index: {statistics.AverageProductivityIndex:F2}");
            report.AppendLine($"Total Activities: {statistics.TotalActivities}");
            report.AppendLine($"Total Distractions: {statistics.TotalDistractions}");
            report.AppendLine($"Total Active Time: {FormatTimeSpan(statistics.TotalActiveTime)}");
            report.AppendLine($"Total Idle Time: {FormatTimeSpan(statistics.TotalIdleTime)}");
            report.AppendLine($"Average Active Percentage: {statistics.AverageActivePercentage:F2}%");
            report.AppendLine();
            
            report.AppendLine("Session Details");
            report.AppendLine("--------------");
            foreach (var session in sessions.OrderBy(s => s.StartTime))
            {
                report.AppendLine($"Session: {session.SessionType}");
                report.AppendLine($"  Start: {session.StartTime:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine($"  End: {session.EndTime:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine($"  Duration: {FormatTimeSpan(session.Metrics.SessionDuration)}");
                report.AppendLine($"  Focus Score: {session.Metrics.FocusScore:F2}%");
                report.AppendLine($"  Efficiency Score: {session.Metrics.EfficiencyScore:F2}%");
                report.AppendLine($"  Productivity Index: {session.Metrics.ProductivityIndex:F2}");
                report.AppendLine($"  Active Time: {FormatTimeSpan(session.Metrics.ActiveTime)}");
                report.AppendLine($"  Idle Time: {FormatTimeSpan(session.Metrics.IdleTime)}");
                report.AppendLine($"  Activities: {session.Metrics.ActivityCount}");
                report.AppendLine($"  Distractions: {session.Metrics.DistractionCount}");
                if (!string.IsNullOrEmpty(session.Notes))
                {
                    report.AppendLine($"  Notes: {session.Notes}");
                }
                report.AppendLine();
            }
            
            File.WriteAllText(filePath, report.ToString());
        }

        public void ExportToHtml(string filePath, TimeSpan timeRange)
        {
            var sessions = _dataManager.GetSessions(timeRange);
            var statistics = _dataManager.GetStatistics(timeRange);
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang=\"en\">");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset=\"UTF-8\">");
            html.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.AppendLine("    <title>PomodorroMan Efficiency Report</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }");
            html.AppendLine("        .container { background-color: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
            html.AppendLine("        h1 { color: #dc3545; text-align: center; }");
            html.AppendLine("        h2 { color: #495057; border-bottom: 2px solid #dc3545; padding-bottom: 5px; }");
            html.AppendLine("        .stats { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px; margin: 20px 0; }");
            html.AppendLine("        .stat-card { background-color: #f8f9fa; padding: 15px; border-radius: 5px; border-left: 4px solid #dc3545; }");
            html.AppendLine("        .stat-value { font-size: 24px; font-weight: bold; color: #dc3545; }");
            html.AppendLine("        .stat-label { color: #6c757d; font-size: 14px; }");
            html.AppendLine("        table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            html.AppendLine("        th, td { padding: 10px; text-align: left; border-bottom: 1px solid #dee2e6; }");
            html.AppendLine("        th { background-color: #dc3545; color: white; }");
            html.AppendLine("        tr:nth-child(even) { background-color: #f8f9fa; }");
            html.AppendLine("        .progress-bar { background-color: #e9ecef; height: 20px; border-radius: 10px; overflow: hidden; }");
            html.AppendLine("        .progress-fill { height: 100%; background-color: #dc3545; transition: width 0.3s ease; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div class=\"container\">");
            html.AppendLine("        <h1>🍅 PomodorroMan Efficiency Report</h1>");
            html.AppendLine($"        <p><strong>Generated:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss} | <strong>Version:</strong> 1.8.1 | <strong>Time Range:</strong> {timeRange}</p>");
            html.AppendLine();
            
            html.AppendLine("        <h2>📊 Summary Statistics</h2>");
            html.AppendLine("        <div class=\"stats\">");
            html.AppendLine($"            <div class=\"stat-card\"><div class=\"stat-value\">{statistics.TotalSessions}</div><div class=\"stat-label\">Total Sessions</div></div>");
            html.AppendLine($"            <div class=\"stat-card\"><div class=\"stat-value\">{FormatTimeSpan(statistics.TotalDuration)}</div><div class=\"stat-label\">Total Duration</div></div>");
            html.AppendLine($"            <div class=\"stat-card\"><div class=\"stat-value\">{statistics.AverageEfficiencyScore:F1}%</div><div class=\"stat-label\">Avg Efficiency</div></div>");
            html.AppendLine($"            <div class=\"stat-card\"><div class=\"stat-value\">{statistics.AverageFocusScore:F1}%</div><div class=\"stat-label\">Avg Focus</div></div>");
            html.AppendLine($"            <div class=\"stat-card\"><div class=\"stat-value\">{statistics.TotalActivities}</div><div class=\"stat-label\">Total Activities</div></div>");
            html.AppendLine($"            <div class=\"stat-card\"><div class=\"stat-value\">{statistics.TotalDistractions}</div><div class=\"stat-label\">Total Distractions</div></div>");
            html.AppendLine("        </div>");
            html.AppendLine();
            
            html.AppendLine("        <h2>📈 Session Details</h2>");
            html.AppendLine("        <table>");
            html.AppendLine("            <thead>");
            html.AppendLine("                <tr>");
            html.AppendLine("                    <th>Session Type</th>");
            html.AppendLine("                    <th>Start Time</th>");
            html.AppendLine("                    <th>Duration</th>");
            html.AppendLine("                    <th>Focus Score</th>");
            html.AppendLine("                    <th>Efficiency Score</th>");
            html.AppendLine("                    <th>Activities</th>");
            html.AppendLine("                    <th>Distractions</th>");
            html.AppendLine("                </tr>");
            html.AppendLine("            </thead>");
            html.AppendLine("            <tbody>");
            
            foreach (var session in sessions.OrderBy(s => s.StartTime))
            {
                html.AppendLine("                <tr>");
                html.AppendLine($"                    <td>{session.SessionType}</td>");
                html.AppendLine($"                    <td>{session.StartTime:yyyy-MM-dd HH:mm}</td>");
                html.AppendLine($"                    <td>{FormatTimeSpan(session.Metrics.SessionDuration)}</td>");
                html.AppendLine($"                    <td>{session.Metrics.FocusScore:F1}%</td>");
                html.AppendLine($"                    <td>{session.Metrics.EfficiencyScore:F1}%</td>");
                html.AppendLine($"                    <td>{session.Metrics.ActivityCount}</td>");
                html.AppendLine($"                    <td>{session.Metrics.DistractionCount}</td>");
                html.AppendLine("                </tr>");
            }
            
            html.AppendLine("            </tbody>");
            html.AppendLine("        </table>");
            html.AppendLine("    </div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            File.WriteAllText(filePath, html.ToString());
        }

        public void ExportToNotion(string filePath, TimeSpan timeRange)
        {
            var sessions = _dataManager.GetSessions(timeRange);
            var statistics = _dataManager.GetStatistics(timeRange);
            var notionData = new
            {
                Title = "PomodorroMan Efficiency Report",
                Properties = new
                {
                    GeneratedAt = DateTime.Now.ToString("yyyy-MM-dd"),
                    Version = "1.8.1",
                    TimeRange = timeRange.ToString(),
                    TotalSessions = statistics.TotalSessions,
                    AverageEfficiency = statistics.AverageEfficiencyScore,
                    AverageFocus = statistics.AverageFocusScore,
                    TotalActivities = statistics.TotalActivities,
                    TotalDistractions = statistics.TotalDistractions
                },
                Sessions = sessions.Select(s => new
                {
                    SessionType = s.SessionType.ToString(),
                    StartTime = s.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Duration = s.Metrics.SessionDuration.TotalMinutes,
                    FocusScore = s.Metrics.FocusScore,
                    EfficiencyScore = s.Metrics.EfficiencyScore,
                    ActivityCount = s.Metrics.ActivityCount,
                    DistractionCount = s.Metrics.DistractionCount,
                    Notes = s.Notes ?? ""
                })
            };

            var json = JsonSerializer.Serialize(notionData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public void ExportToTodoist(string filePath, TimeSpan timeRange)
        {
            var sessions = _dataManager.GetSessions(timeRange);
            var statistics = _dataManager.GetStatistics(timeRange);
            var todoistData = new
            {
                Project = "PomodorroMan Efficiency Tracking",
                Tasks = sessions.Select(s => new
                {
                    Content = $"{s.SessionType} Session - {s.Metrics.EfficiencyScore:F1}% Efficiency",
                    Due = s.StartTime.ToString("yyyy-MM-dd"),
                    Priority = s.Metrics.EfficiencyScore > 80 ? 4 : s.Metrics.EfficiencyScore > 60 ? 3 : 2,
                    Labels = new[] { "pomodoro", s.SessionType.ToString().ToLower() },
                    Description = $"Focus: {s.Metrics.FocusScore:F1}%, Activities: {s.Metrics.ActivityCount}, Distractions: {s.Metrics.DistractionCount}"
                }).ToList(),
                Summary = new
                {
                    TotalSessions = statistics.TotalSessions,
                    AverageEfficiency = statistics.AverageEfficiencyScore,
                    AverageFocus = statistics.AverageFocusScore,
                    TotalActivities = statistics.TotalActivities,
                    TotalDistractions = statistics.TotalDistractions
                }
            };

            var json = JsonSerializer.Serialize(todoistData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public List<string> GetSupportedFormats()
        {
            return new List<string>
            {
                "JSON (.json)",
                "CSV (.csv)",
                "Text (.txt)",
                "HTML (.html)",
                "Notion (.json)",
                "Todoist (.json)"
            };
        }

        private static string FormatTimeSpan(TimeSpan timeSpan)
        {
            return timeSpan.TotalHours >= 1 
                ? $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}"
                : $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
}
