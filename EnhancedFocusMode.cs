using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PomodorroMan
{
    public class EnhancedFocusMode : IDisposable
    {
        private readonly List<BlockedApplication> _blockedApplications = new();
        private readonly List<BlockedWebsite> _blockedWebsites = new();
        private bool _isActive;
        private bool _disposed;
        private readonly object _lockObject = new();

        public event EventHandler<ApplicationBlockedEventArgs>? ApplicationBlocked;
        public event EventHandler<WebsiteBlockedEventArgs>? WebsiteBlocked;
        public event EventHandler<FocusModeEventArgs>? FocusModeChanged;

        public bool IsActive => _isActive;
        public int BlockedApplicationsCount => _blockedApplications.Count;
        public int BlockedWebsitesCount => _blockedWebsites.Count;

        public void AddBlockedApplication(string processName, string displayName, bool isTemporary = false)
        {
            lock (_lockObject)
            {
                var existing = _blockedApplications.FirstOrDefault(a => a.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    existing.IsTemporary = isTemporary;
                    return;
                }

                _blockedApplications.Add(new BlockedApplication
                {
                    ProcessName = processName,
                    DisplayName = displayName,
                    IsTemporary = isTemporary,
                    AddedAt = DateTime.Now
                });
            }
        }

        public void AddBlockedWebsite(string domain, string displayName, bool isTemporary = false)
        {
            lock (_lockObject)
            {
                var existing = _blockedWebsites.FirstOrDefault(w => w.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    existing.IsTemporary = isTemporary;
                    return;
                }

                _blockedWebsites.Add(new BlockedWebsite
                {
                    Domain = domain,
                    DisplayName = displayName,
                    IsTemporary = isTemporary,
                    AddedAt = DateTime.Now
                });
            }
        }

        public void RemoveBlockedApplication(string processName)
        {
            lock (_lockObject)
            {
                _blockedApplications.RemoveAll(a => a.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase));
            }
        }

        public void RemoveBlockedWebsite(string domain)
        {
            lock (_lockObject)
            {
                _blockedWebsites.RemoveAll(w => w.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase));
            }
        }

        public void StartFocusMode()
        {
            if (_isActive) return;

            lock (_lockObject)
            {
                _isActive = true;
                _ = System.Threading.Tasks.Task.Run(MonitorApplications);
                _ = System.Threading.Tasks.Task.Run(MonitorWebsites);
                OnFocusModeChanged(true);
            }
        }

        public void StopFocusMode()
        {
            if (!_isActive) return;

            lock (_lockObject)
            {
                _isActive = false;
                OnFocusModeChanged(false);
            }
        }

        private async void MonitorApplications()
        {
            while (_isActive && !_disposed)
            {
                try
                {
                    var runningProcesses = Process.GetProcesses();
                    var blockedProcesses = runningProcesses.Where(p => 
                        _blockedApplications.Any(ba => 
                            ba.ProcessName.Equals(p.ProcessName, StringComparison.OrdinalIgnoreCase))).ToList();

                    foreach (var process in blockedProcesses)
                    {
                        try
                        {
                            var blockedApp = _blockedApplications.FirstOrDefault(ba => 
                                ba.ProcessName.Equals(process.ProcessName, StringComparison.OrdinalIgnoreCase));

                            if (blockedApp != null)
                            {
                                // Try to minimize the window instead of killing the process
                                var mainWindowHandle = process.MainWindowHandle;
                                if (mainWindowHandle != IntPtr.Zero)
                                {
                                    ShowWindow(mainWindowHandle, SW_MINIMIZE);
                                }

                                OnApplicationBlocked(blockedApp, process);
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue monitoring
                            System.Diagnostics.Debug.WriteLine($"Error handling blocked application {process.ProcessName}: {ex.Message}");
                        }
                    }

                    // Clean up process references
                    foreach (var process in runningProcesses)
                    {
                        process.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in application monitoring: {ex.Message}");
                }

                await System.Threading.Tasks.Task.Delay(2000); // Check every 2 seconds
            }
        }

        private async void MonitorWebsites()
        {
            while (_isActive && !_disposed)
            {
                try
                {
                    // This is a simplified implementation
                    // In a real implementation, you would need to hook into browser processes
                    // or use network monitoring to detect website access
                    await System.Threading.Tasks.Task.Delay(5000); // Check every 5 seconds
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in website monitoring: {ex.Message}");
                }
            }
        }

        public void LoadDefaultBlockedApplications()
        {
            var defaultApps = new[]
            {
                new { ProcessName = "chrome", DisplayName = "Google Chrome" },
                new { ProcessName = "firefox", DisplayName = "Mozilla Firefox" },
                new { ProcessName = "msedge", DisplayName = "Microsoft Edge" },
                new { ProcessName = "discord", DisplayName = "Discord" },
                new { ProcessName = "slack", DisplayName = "Slack" },
                new { ProcessName = "teams", DisplayName = "Microsoft Teams" },
                new { ProcessName = "whatsapp", DisplayName = "WhatsApp" },
                new { ProcessName = "telegram", DisplayName = "Telegram" },
                new { ProcessName = "spotify", DisplayName = "Spotify" },
                new { ProcessName = "youtube", DisplayName = "YouTube" },
                new { ProcessName = "netflix", DisplayName = "Netflix" },
                new { ProcessName = "steam", DisplayName = "Steam" },
                new { ProcessName = "epicgameslauncher", DisplayName = "Epic Games Launcher" }
            };

            foreach (var app in defaultApps)
            {
                AddBlockedApplication(app.ProcessName, app.DisplayName, true);
            }
        }

        public void LoadDefaultBlockedWebsites()
        {
            var defaultWebsites = new[]
            {
                new { Domain = "facebook.com", DisplayName = "Facebook" },
                new { Domain = "twitter.com", DisplayName = "Twitter" },
                new { Domain = "instagram.com", DisplayName = "Instagram" },
                new { Domain = "tiktok.com", DisplayName = "TikTok" },
                new { Domain = "youtube.com", DisplayName = "YouTube" },
                new { Domain = "reddit.com", DisplayName = "Reddit" },
                new { Domain = "netflix.com", DisplayName = "Netflix" },
                new { Domain = "twitch.tv", DisplayName = "Twitch" },
                new { Domain = "discord.com", DisplayName = "Discord" },
                new { Domain = "slack.com", DisplayName = "Slack" }
            };

            foreach (var website in defaultWebsites)
            {
                AddBlockedWebsite(website.Domain, website.DisplayName, true);
            }
        }

        public List<BlockedApplication> GetBlockedApplications()
        {
            lock (_lockObject)
            {
                return _blockedApplications.ToList();
            }
        }

        public List<BlockedWebsite> GetBlockedWebsites()
        {
            lock (_lockObject)
            {
                return _blockedWebsites.ToList();
            }
        }

        public void ClearTemporaryBlocks()
        {
            lock (_lockObject)
            {
                _blockedApplications.RemoveAll(a => a.IsTemporary);
                _blockedWebsites.RemoveAll(w => w.IsTemporary);
            }
        }

        public FocusModeStatistics GetStatistics()
        {
            lock (_lockObject)
            {
                return new FocusModeStatistics
                {
                    TotalBlockedApplications = _blockedApplications.Count,
                    TotalBlockedWebsites = _blockedWebsites.Count,
                    TemporaryBlocks = _blockedApplications.Count(a => a.IsTemporary) + _blockedWebsites.Count(w => w.IsTemporary),
                    PermanentBlocks = _blockedApplications.Count(a => !a.IsTemporary) + _blockedWebsites.Count(w => !w.IsTemporary),
                    IsActive = _isActive
                };
            }
        }

        private void OnApplicationBlocked(BlockedApplication app, Process process)
        {
            ApplicationBlocked?.Invoke(this, new ApplicationBlockedEventArgs(app, process));
        }

        private void OnWebsiteBlocked(BlockedWebsite website)
        {
            WebsiteBlocked?.Invoke(this, new WebsiteBlockedEventArgs(website));
        }

        private void OnFocusModeChanged(bool isActive)
        {
            FocusModeChanged?.Invoke(this, new FocusModeEventArgs(isActive));
        }

        public void Dispose()
        {
            if (_disposed) return;

            StopFocusMode();
            _disposed = true;
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_MINIMIZE = 6;
    }

    public class BlockedApplication
    {
        public string ProcessName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsTemporary { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public class BlockedWebsite
    {
        public string Domain { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsTemporary { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public class FocusModeStatistics
    {
        public int TotalBlockedApplications { get; set; }
        public int TotalBlockedWebsites { get; set; }
        public int TemporaryBlocks { get; set; }
        public int PermanentBlocks { get; set; }
        public bool IsActive { get; set; }
    }

    public class ApplicationBlockedEventArgs : EventArgs
    {
        public BlockedApplication Application { get; }
        public Process Process { get; }

        public ApplicationBlockedEventArgs(BlockedApplication application, Process process)
        {
            Application = application;
            Process = process;
        }
    }

    public class WebsiteBlockedEventArgs : EventArgs
    {
        public BlockedWebsite Website { get; }

        public WebsiteBlockedEventArgs(BlockedWebsite website)
        {
            Website = website;
        }
    }

    public class FocusModeEventArgs : EventArgs
    {
        public bool IsActive { get; }

        public FocusModeEventArgs(bool isActive)
        {
            IsActive = isActive;
        }
    }
}
