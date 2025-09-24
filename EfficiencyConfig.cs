using System;

namespace PomodorroMan
{
    public static class EfficiencyConfig
    {
        // Focus Score Configuration
        public const double MinFocusScore = 0.0;
        public const double MaxFocusScore = 100.0;
        
        // Data Points Configuration
        public const int MaxDataPoints = 1000;
        public const int CleanupThreshold = 800;
        
        // Activity Tracking Configuration
        public const int MinKeyPressInterval = 50; // ms
        public const int MinScrollInterval = 30; // ms
        public const int MaxErrors = 5;
        public const int MonitoringDelay = 100; // ms
        
        // Focus Score Multipliers
        public const double MouseMovementMultiplier = 0.3;
        public const double KeyboardTypingMultiplier = 0.8;
        public const double ScrollingMultiplier = 0.4;
        public const double WindowSwitchingMultiplier = -0.5;
        public const double IdleMultiplier = -0.1;
        
        // Time Multipliers (seconds)
        public const double VeryRecentTimeMultiplier = 1.0; // < 1s
        public const double RecentTimeMultiplier = 0.8; // < 5s
        public const double MediumTimeMultiplier = 0.6; // < 10s
        public const double LongTimeMultiplier = 0.3; // < 30s
        public const double VeryLongTimeMultiplier = 0.1; // >= 30s
        
        // Intensity Multipliers
        public const double MinIntensityMultiplier = 0.1;
        public const double MaxIntensityMultiplier = 2.0;
        
        // AFK Configuration
        public const int AfkDetectionDelay = 5000; // ms
        public const int IconFlashInterval = 600; // ms
        public const int MaxIconFlashCount = 8;
        
        // UI Configuration
        public const int RecentFocusScoresCount = 10;
        public const int ProgressBarMaxValue = 100;
        public const int ProgressBarMinValue = 0;
        
        // Export Configuration
        public const int FileStreamBufferSize = 4096;
        
        // Performance Configuration
        public const int MaxConcurrentOperations = 5;
        public const int CacheExpirationMinutes = 30;
        public const int MaxRetryAttempts = 3;
        
        // UI Configuration
        public const int TooltipDelay = 500; // ms
        public const int AnimationDuration = 200; // ms
        public const int MaxTooltipLength = 200;
        
        // Session Configuration
        public const int MaxSessionHistory = 50;
        public const int SessionDataRetentionDays = 30;
        public const int MaxConcurrentSessions = 1;
        
        // Notification Configuration
        public const int MaxNotificationHistory = 100;
        public const int NotificationCooldownMs = 1000;
        public const int MaxConcurrentNotifications = 3;
        
        // Focus Configuration
        public const double HighFocusThreshold = 80.0;
        public const double MediumFocusThreshold = 60.0;
        public const double LowFocusThreshold = 40.0;
        public const int FocusStabilityWindow = 5; // minutes
        
        // Productivity Configuration
        public const int ProductivityGoalMinutes = 480; // 8 hours
        public const int WeeklyGoalDays = 5;
        public const double ProductivityTargetPercentage = 85.0;
        
        // Task Management Configuration
        public const int MaxTasksPerCategory = 100;
        public const int MaxTaskDescriptionLength = 500;
        public const int MaxTaskTitleLength = 100;
        
        // Ambient Sound Configuration
        public const float DefaultSoundVolume = 0.5f;
        public const int MaxSoundVolume = 1;
        public const int MinSoundVolume = 0;
        
        // Gamification Configuration
        public const int PointsPerSession = 10;
        public const int PointsPerTask = 5;
        public const int PointsPerPerfectDay = 50;
        public const int StreakBonusMultiplier = 2;
        
        // Break Activity Configuration
        public const int MaxBreakActivityDuration = 30; // minutes
        public const int MinBreakActivityDuration = 1; // minutes
        public const int DefaultBreakActivityCount = 3;
        
        // Theme Configuration
        public const string DefaultTheme = "Light";
        public const int MaxCustomThemes = 10;
        
        // Export Configuration
        public const string ExportVersion = "1.8.0";
    }
}
