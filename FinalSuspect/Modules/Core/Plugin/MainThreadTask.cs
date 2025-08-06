using System;

namespace FinalSuspect.Modules.Core.Plugin;

public class MainThreadTask
{
    private static readonly List<MainThreadTask> Tasks = [];
    private readonly Action action;
    private readonly string name;

    /// <summary>
    ///     用于异步线程的类型
    /// </summary>
    /// <param name="action">需要转到主线程执行的行为</param>
    /// <param name="name">本次行为名称，会输出日志</param>
    public MainThreadTask(Action action, string name = "No Name Task")
    {
        this.action = action;
        this.name = name;

        if (name != "")
            Info("\"" + name + "\" is created", "Main Thread Task");
        Tasks.Add(this);
    }

    public static void Update()
    {
        var TasksToRemove = new List<MainThreadTask>();
        foreach (var task in Tasks.ToList())
        {
            try
            {
                task.action();
                if (task.name != "")
                    Info($"\"{task.name}\" is finished", "Main Thread Task");
            }
            catch (Exception ex)
            {
                Error($"{ex.GetType()}: {ex.Message}  in \"{task.name}\"\n{ex.StackTrace}",
                    "Main Thread Task.Error",
                    false);
            }

            TasksToRemove.Add(task);
        }

        TasksToRemove.ForEach(task => Tasks.Remove(task));
    }
}