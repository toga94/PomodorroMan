using System;
using System.Collections.Generic;
using System.Drawing;

namespace PomodorroMan
{
    public class Theme
    {
        public string Name { get; set; } = string.Empty;
        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }
        public Color AccentColor { get; set; }
        public Color ButtonColor { get; set; }
        public Color ButtonHoverColor { get; set; }
        public Color TextColor { get; set; }
        public Color BorderColor { get; set; }
        public Color ProgressBarColor { get; set; }
        public Color ProgressBarBackgroundColor { get; set; }
        public bool IsDarkMode { get; set; }
    }

    public class ThemeManager
    {
        private readonly Dictionary<string, Theme> _themes = new();
        private Theme _currentTheme;
        private static ThemeManager? _instance;
        private static readonly object _lockObject = new();

        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

        private ThemeManager()
        {
            InitializeDefaultThemes();
            _currentTheme = _themes["Light"];
        }

        public static ThemeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new ThemeManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private void InitializeDefaultThemes()
        {
            // Light Theme
            _themes["Light"] = new Theme
            {
                Name = "Light",
                BackgroundColor = Color.FromArgb(248, 249, 250),
                ForegroundColor = Color.FromArgb(33, 37, 41),
                AccentColor = Color.FromArgb(0, 123, 255),
                ButtonColor = Color.FromArgb(0, 123, 255),
                ButtonHoverColor = Color.FromArgb(0, 86, 179),
                TextColor = Color.FromArgb(33, 37, 41),
                BorderColor = Color.FromArgb(206, 212, 218),
                ProgressBarColor = Color.FromArgb(40, 167, 69),
                ProgressBarBackgroundColor = Color.FromArgb(233, 236, 239),
                IsDarkMode = false
            };

            // Dark Theme
            _themes["Dark"] = new Theme
            {
                Name = "Dark",
                BackgroundColor = Color.FromArgb(33, 37, 41),
                ForegroundColor = Color.FromArgb(248, 249, 250),
                AccentColor = Color.FromArgb(0, 123, 255),
                ButtonColor = Color.FromArgb(0, 123, 255),
                ButtonHoverColor = Color.FromArgb(0, 86, 179),
                TextColor = Color.FromArgb(248, 249, 250),
                BorderColor = Color.FromArgb(73, 80, 87),
                ProgressBarColor = Color.FromArgb(40, 167, 69),
                ProgressBarBackgroundColor = Color.FromArgb(52, 58, 64),
                IsDarkMode = true
            };

            // Blue Theme
            _themes["Blue"] = new Theme
            {
                Name = "Blue",
                BackgroundColor = Color.FromArgb(240, 248, 255),
                ForegroundColor = Color.FromArgb(0, 48, 87),
                AccentColor = Color.FromArgb(0, 123, 255),
                ButtonColor = Color.FromArgb(0, 123, 255),
                ButtonHoverColor = Color.FromArgb(0, 86, 179),
                TextColor = Color.FromArgb(0, 48, 87),
                BorderColor = Color.FromArgb(173, 216, 230),
                ProgressBarColor = Color.FromArgb(0, 123, 255),
                ProgressBarBackgroundColor = Color.FromArgb(173, 216, 230),
                IsDarkMode = false
            };

            // Green Theme
            _themes["Green"] = new Theme
            {
                Name = "Green",
                BackgroundColor = Color.FromArgb(240, 255, 240),
                ForegroundColor = Color.FromArgb(0, 64, 0),
                AccentColor = Color.FromArgb(40, 167, 69),
                ButtonColor = Color.FromArgb(40, 167, 69),
                ButtonHoverColor = Color.FromArgb(30, 126, 52),
                TextColor = Color.FromArgb(0, 64, 0),
                BorderColor = Color.FromArgb(144, 238, 144),
                ProgressBarColor = Color.FromArgb(40, 167, 69),
                ProgressBarBackgroundColor = Color.FromArgb(144, 238, 144),
                IsDarkMode = false
            };

            // Purple Theme
            _themes["Purple"] = new Theme
            {
                Name = "Purple",
                BackgroundColor = Color.FromArgb(248, 240, 255),
                ForegroundColor = Color.FromArgb(64, 0, 64),
                AccentColor = Color.FromArgb(111, 66, 193),
                ButtonColor = Color.FromArgb(111, 66, 193),
                ButtonHoverColor = Color.FromArgb(88, 52, 153),
                TextColor = Color.FromArgb(64, 0, 64),
                BorderColor = Color.FromArgb(186, 170, 255),
                ProgressBarColor = Color.FromArgb(111, 66, 193),
                ProgressBarBackgroundColor = Color.FromArgb(186, 170, 255),
                IsDarkMode = false
            };
        }

        public void SetTheme(string themeName)
        {
            if (_themes.TryGetValue(themeName, out var theme))
            {
                var oldTheme = _currentTheme;
                _currentTheme = theme;
                ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(oldTheme, theme));
            }
        }

        public Theme GetCurrentTheme()
        {
            return _currentTheme;
        }

        public List<Theme> GetAllThemes()
        {
            return new List<Theme>(_themes.Values);
        }

        public List<string> GetThemeNames()
        {
            return new List<string>(_themes.Keys);
        }

        public void AddCustomTheme(Theme theme)
        {
            _themes[theme.Name] = theme;
        }

        public void RemoveTheme(string themeName)
        {
            if (_themes.ContainsKey(themeName) && themeName != "Light" && themeName != "Dark")
            {
                _themes.Remove(themeName);
            }
        }

        public bool IsDarkMode => _currentTheme.IsDarkMode;
    }

    public class ThemeChangedEventArgs : EventArgs
    {
        public Theme OldTheme { get; }
        public Theme NewTheme { get; }

        public ThemeChangedEventArgs(Theme oldTheme, Theme newTheme)
        {
            OldTheme = oldTheme;
            NewTheme = newTheme;
        }
    }
}


