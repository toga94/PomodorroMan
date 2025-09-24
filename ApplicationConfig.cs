using System.Reflection;

namespace PomodorroMan
{
    /// <summary>
    /// Centralized configuration for application titles, version information, and branding
    /// </summary>
    public static class ApplicationConfig
    {
        // Application Information
        public static string ApplicationName => "PomodorroMan";
        public static string ApplicationTitle => GetApplicationTitle();
        public static string Version => GetVersion();
        public static string FullVersion => GetFullVersion();
        
        // Form Titles
        public static string EfficiencyTrackerTitle => $"{ApplicationName} - Work Efficiency Tracker v{Version}";
        public static string DataExportTitle => $"{ApplicationName} - Export Data v{Version}";
        public static string SettingsTitle => $"{ApplicationName} - Settings v{Version}";
        public static string NotificationHistoryTitle => $"{ApplicationName} - Notification History v{Version}";
        public static string EfficiencyReportTitle => $"{ApplicationName} - Efficiency Report v{Version}";
        public static string ThemesTitle => $"{ApplicationName} - Themes v{Version}";
        public static string AchievementsTitle => $"{ApplicationName} - Achievements v{Version}";
        public static string BreakActivitiesTitle => $"{ApplicationName} - Break Activities v{Version}";
        public static string AmbientSoundsTitle => $"{ApplicationName} - Ambient Sounds v{Version}";
        public static string TaskManagementTitle => $"{ApplicationName} - Task Management v{Version}";
        public static string AboutTitle => $"{ApplicationName} - About v{Version}";
        
        // Notification Titles
        public static string ReadyNotificationTitle => $"{ApplicationName} Ready!";
        public static string PausedNotificationTitle => $"{ApplicationName}";
        public static string ResetNotificationTitle => $"{ApplicationName}";
        public static string SkippedNotificationTitle => $"{ApplicationName}";
        public static string SessionCompleteNotificationTitle => $"{ApplicationName}";
        public static string SettingsUpdatedNotificationTitle => $"{ApplicationName}";
        
        // Tray Icon
        public static string TrayIconText => ApplicationName;
        
        // Version Information
        public static string VersionLabel => $"{ApplicationName} v{Version}";
        
        // Export Information
        public static string ExportVersion => Version;
        
        // Balloon Tip Titles
        public static string BalloonTipTitle => $"🍅 {ApplicationName}";
        
        private static string GetApplicationTitle()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var titleAttribute = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
            return titleAttribute?.Title ?? ApplicationName;
        }
        
        private static string GetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
        }
        
        private static string GetFullVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version != null ? version.ToString() : "1.0.0.0";
        }
    }
}


