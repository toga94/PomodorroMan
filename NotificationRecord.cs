using System;

namespace PomodorroMan
{
    public class NotificationRecord
    {
        public DateTime Timestamp { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string IconType { get; set; } = string.Empty;
        public string SessionType { get; set; } = string.Empty;
        public int CompletedSessions { get; set; }

        public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        public string FormattedMessage => Message.Replace("\n", " | ");
    }
}
