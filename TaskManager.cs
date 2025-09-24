using System;
using System.Collections.Generic;
using System.Linq;

namespace PomodorroMan
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public int EstimatedPomodoros { get; set; }
        public int CompletedPomodoros { get; set; }
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }

    public enum TaskStatus
    {
        Todo,
        InProgress,
        Completed,
        Cancelled
    }

    public class TaskManager
    {
        private readonly List<Task> _tasks = new();
        private int _nextId = 1;
        private readonly object _lockObject = new();

        public event EventHandler<TaskEventArgs>? TaskAdded;
        public event EventHandler<TaskEventArgs>? TaskUpdated;
        public event EventHandler<TaskEventArgs>? TaskDeleted;

        public void AddTask(Task task)
        {
            lock (_lockObject)
            {
                task.Id = _nextId++;
                task.CreatedAt = DateTime.Now;
                _tasks.Add(task);
                TaskAdded?.Invoke(this, new TaskEventArgs(task));
            }
        }

        public void UpdateTask(Task task)
        {
            lock (_lockObject)
            {
                var existingTask = _tasks.FirstOrDefault(t => t.Id == task.Id);
                if (existingTask != null)
                {
                    var index = _tasks.IndexOf(existingTask);
                    _tasks[index] = task;
                    TaskUpdated?.Invoke(this, new TaskEventArgs(task));
                }
            }
        }

        public void DeleteTask(int taskId)
        {
            lock (_lockObject)
            {
                var task = _tasks.FirstOrDefault(t => t.Id == taskId);
                if (task != null)
                {
                    _tasks.Remove(task);
                    TaskDeleted?.Invoke(this, new TaskEventArgs(task));
                }
            }
        }

        public Task? GetTask(int taskId)
        {
            lock (_lockObject)
            {
                return _tasks.FirstOrDefault(t => t.Id == taskId);
            }
        }

        public List<Task> GetAllTasks()
        {
            lock (_lockObject)
            {
                return new List<Task>(_tasks);
            }
        }

        public List<Task> GetTasksByStatus(TaskStatus status)
        {
            lock (_lockObject)
            {
                return _tasks.Where(t => t.Status == status).ToList();
            }
        }

        public List<Task> GetTasksByPriority(TaskPriority priority)
        {
            lock (_lockObject)
            {
                return _tasks.Where(t => t.Priority == priority).ToList();
            }
        }

        public List<Task> GetTasksByCategory(string category)
        {
            lock (_lockObject)
            {
                return _tasks.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        public void CompletePomodoro(int taskId)
        {
            lock (_lockObject)
            {
                var task = _tasks.FirstOrDefault(t => t.Id == taskId);
                if (task != null)
                {
                    task.CompletedPomodoros++;
                    if (task.CompletedPomodoros >= task.EstimatedPomodoros)
                    {
                        task.Status = TaskStatus.Completed;
                    }
                    TaskUpdated?.Invoke(this, new TaskEventArgs(task));
                }
            }
        }

        public List<string> GetCategories()
        {
            lock (_lockObject)
            {
                return _tasks.Select(t => t.Category)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
            }
        }

        public List<string> GetAllTags()
        {
            lock (_lockObject)
            {
                return _tasks.SelectMany(t => t.Tags)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();
            }
        }

        public TaskStatistics GetStatistics()
        {
            lock (_lockObject)
            {
                return new TaskStatistics
                {
                    TotalTasks = _tasks.Count,
                    CompletedTasks = _tasks.Count(t => t.Status == TaskStatus.Completed),
                    InProgressTasks = _tasks.Count(t => t.Status == TaskStatus.InProgress),
                    TodoTasks = _tasks.Count(t => t.Status == TaskStatus.Todo),
                    TotalPomodoros = _tasks.Sum(t => t.CompletedPomodoros),
                    EstimatedPomodoros = _tasks.Sum(t => t.EstimatedPomodoros)
                };
            }
        }
    }

    public class TaskEventArgs : EventArgs
    {
        public Task Task { get; }

        public TaskEventArgs(Task task)
        {
            Task = task;
        }
    }

    public class TaskStatistics
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int TodoTasks { get; set; }
        public int TotalPomodoros { get; set; }
        public int EstimatedPomodoros { get; set; }
        public double CompletionRate => TotalTasks > 0 ? (double)CompletedTasks / TotalTasks * 100 : 0;
        public double PomodoroCompletionRate => EstimatedPomodoros > 0 ? (double)TotalPomodoros / EstimatedPomodoros * 100 : 0;
    }
}


