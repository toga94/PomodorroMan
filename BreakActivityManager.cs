using System;
using System.Collections.Generic;
using System.Linq;

namespace PomodorroMan
{
    public class BreakActivity
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public BreakActivityType Type { get; set; }
        public int DurationMinutes { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 1;
    }

    public enum BreakActivityType
    {
        Physical,
        Mental,
        Relaxation,
        Social,
        Creative,
        Learning,
        Mindfulness
    }

    public class BreakActivityManager
    {
        private readonly List<BreakActivity> _activities = new();
        private readonly Random _random = new();

        public BreakActivityManager()
        {
            InitializeDefaultActivities();
        }

        private void InitializeDefaultActivities()
        {
            // Physical Activities
            AddActivity(new BreakActivity
            {
                Id = "stretch",
                Title = "Stretch & Move",
                Description = "Stand up and do some light stretching or walking around",
                Type = BreakActivityType.Physical,
                DurationMinutes = 5,
                Icon = "🤸",
                Category = "Physical",
                Priority = 1
            });

            AddActivity(new BreakActivity
            {
                Id = "walk",
                Title = "Take a Walk",
                Description = "Go for a short walk around your office or outside",
                Type = BreakActivityType.Physical,
                DurationMinutes = 10,
                Icon = "🚶",
                Category = "Physical",
                Priority = 1
            });

            AddActivity(new BreakActivity
            {
                Id = "exercise",
                Title = "Quick Exercise",
                Description = "Do some push-ups, jumping jacks, or other quick exercises",
                Type = BreakActivityType.Physical,
                DurationMinutes = 5,
                Icon = "💪",
                Category = "Physical",
                Priority = 2
            });

            // Mental Activities
            AddActivity(new BreakActivity
            {
                Id = "puzzle",
                Title = "Solve a Puzzle",
                Description = "Do a crossword, Sudoku, or brain teaser",
                Type = BreakActivityType.Mental,
                DurationMinutes = 10,
                Icon = "🧩",
                Category = "Mental",
                Priority = 1
            });

            AddActivity(new BreakActivity
            {
                Id = "read",
                Title = "Read Something",
                Description = "Read a few pages of a book or article",
                Type = BreakActivityType.Mental,
                DurationMinutes = 15,
                Icon = "📚",
                Category = "Mental",
                Priority = 1
            });

            AddActivity(new BreakActivity
            {
                Id = "learn",
                Title = "Learn Something New",
                Description = "Watch a short educational video or read about a new topic",
                Type = BreakActivityType.Learning,
                DurationMinutes = 10,
                Icon = "🎓",
                Category = "Learning",
                Priority = 2
            });

            // Relaxation Activities
            AddActivity(new BreakActivity
            {
                Id = "meditate",
                Title = "Meditate",
                Description = "Take a few minutes to meditate or practice mindfulness",
                Type = BreakActivityType.Mindfulness,
                DurationMinutes = 10,
                Icon = "🧘",
                Category = "Relaxation",
                Priority = 1
            });

            AddActivity(new BreakActivity
            {
                Id = "breathe",
                Title = "Deep Breathing",
                Description = "Practice deep breathing exercises to relax",
                Type = BreakActivityType.Mindfulness,
                DurationMinutes = 5,
                Icon = "🫁",
                Category = "Relaxation",
                Priority = 1
            });

            AddActivity(new BreakActivity
            {
                Id = "music",
                Title = "Listen to Music",
                Description = "Listen to your favorite music or discover new songs",
                Type = BreakActivityType.Relaxation,
                DurationMinutes = 10,
                Icon = "🎵",
                Category = "Relaxation",
                Priority = 1
            });

            // Social Activities
            AddActivity(new BreakActivity
            {
                Id = "chat",
                Title = "Chat with Colleagues",
                Description = "Have a friendly conversation with coworkers",
                Type = BreakActivityType.Social,
                DurationMinutes = 10,
                Icon = "💬",
                Category = "Social",
                Priority = 2
            });

            AddActivity(new BreakActivity
            {
                Id = "call",
                Title = "Make a Call",
                Description = "Call a friend or family member for a quick chat",
                Type = BreakActivityType.Social,
                DurationMinutes = 15,
                Icon = "📞",
                Category = "Social",
                Priority = 2
            });

            // Creative Activities
            AddActivity(new BreakActivity
            {
                Id = "draw",
                Title = "Doodle or Draw",
                Description = "Let your creativity flow with some drawing or doodling",
                Type = BreakActivityType.Creative,
                DurationMinutes = 10,
                Icon = "🎨",
                Category = "Creative",
                Priority = 2
            });

            AddActivity(new BreakActivity
            {
                Id = "write",
                Title = "Write in Journal",
                Description = "Write down your thoughts or ideas in a journal",
                Type = BreakActivityType.Creative,
                DurationMinutes = 10,
                Icon = "📝",
                Category = "Creative",
                Priority = 2
            });

            // Quick Activities
            AddActivity(new BreakActivity
            {
                Id = "water",
                Title = "Drink Water",
                Description = "Stay hydrated by drinking a glass of water",
                Type = BreakActivityType.Physical,
                DurationMinutes = 2,
                Icon = "💧",
                Category = "Health",
                Priority = 1
            });

            AddActivity(new BreakActivity
            {
                Id = "snack",
                Title = "Healthy Snack",
                Description = "Have a healthy snack to refuel your energy",
                Type = BreakActivityType.Physical,
                DurationMinutes = 5,
                Icon = "🍎",
                Category = "Health",
                Priority = 1
            });

            AddActivity(new BreakActivity
            {
                Id = "rest",
                Title = "Just Rest",
                Description = "Simply rest your eyes and mind for a few minutes",
                Type = BreakActivityType.Relaxation,
                DurationMinutes = 5,
                Icon = "😴",
                Category = "Rest",
                Priority = 1
            });
        }

        public void AddActivity(BreakActivity activity)
        {
            _activities.Add(activity);
        }

        public BreakActivity GetRandomActivity()
        {
            var activeActivities = _activities.Where(a => a.IsActive).ToList();
            if (activeActivities.Count == 0) return new BreakActivity();

            var randomIndex = _random.Next(activeActivities.Count);
            return activeActivities[randomIndex];
        }

        public BreakActivity GetRandomActivityByType(BreakActivityType type)
        {
            var activitiesOfType = _activities.Where(a => a.IsActive && a.Type == type).ToList();
            if (activitiesOfType.Count == 0) return GetRandomActivity();

            var randomIndex = _random.Next(activitiesOfType.Count);
            return activitiesOfType[randomIndex];
        }

        public BreakActivity GetRandomActivityByCategory(string category)
        {
            var activitiesInCategory = _activities.Where(a => a.IsActive && a.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            if (activitiesInCategory.Count == 0) return GetRandomActivity();

            var randomIndex = _random.Next(activitiesInCategory.Count);
            return activitiesInCategory[randomIndex];
        }

        public BreakActivity GetRandomActivityByDuration(int maxDurationMinutes)
        {
            var activitiesInDuration = _activities.Where(a => a.IsActive && a.DurationMinutes <= maxDurationMinutes).ToList();
            if (activitiesInDuration.Count == 0) return GetRandomActivity();

            var randomIndex = _random.Next(activitiesInDuration.Count);
            return activitiesInDuration[randomIndex];
        }

        public List<BreakActivity> GetActivitiesByType(BreakActivityType type)
        {
            return _activities.Where(a => a.IsActive && a.Type == type).ToList();
        }

        public List<BreakActivity> GetActivitiesByCategory(string category)
        {
            return _activities.Where(a => a.IsActive && a.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<string> GetCategories()
        {
            return _activities.Select(a => a.Category).Distinct().OrderBy(c => c).ToList();
        }

        public List<BreakActivity> GetAllActivities()
        {
            return new List<BreakActivity>(_activities);
        }

        public void SetActivityActive(string activityId, bool isActive)
        {
            var activity = _activities.FirstOrDefault(a => a.Id == activityId);
            if (activity != null)
            {
                activity.IsActive = isActive;
            }
        }

        public BreakActivity? GetActivity(string activityId)
        {
            return _activities.FirstOrDefault(a => a.Id == activityId);
        }
    }
}


