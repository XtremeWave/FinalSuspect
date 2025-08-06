using System;

namespace FinalSuspect.Modules.Core.Plugin;

internal class LateTask
{
    private static readonly List<LateTask> Tasks = [];
    private readonly Action action;
    private readonly string name;
    private float timer;

    public LateTask(Action action, float time, string name = "No Name Task")
    {
        this.action = action;
        timer = time;
        this.name = name;
        Tasks.Add(this);
        if (name != "")
            Info("\"" + name + "\" is created", "LateTask");
    }

    private bool Run(float deltaTime)
    {
        timer -= deltaTime;
        if (!(timer <= 0)) return false;
        action();
        return true;
    }

    public static void Update(float deltaTime)
    {
        var TasksToRemove = new List<LateTask>();
        foreach (var task in Tasks)
            try
            {
                if (!task.Run(deltaTime)) continue;
                if (task.name != "")
                    Info($"\"{task.name}\" is finished", "LateTask");
                TasksToRemove.Add(task);
            }
            catch (Exception ex)
            {
                Error($"{ex.GetType()}: {ex.Message}  in \"{task.name}\"\n{ex.StackTrace}", "LateTask.Error", false);
                TasksToRemove.Add(task);
            }

        TasksToRemove.ForEach(task => Tasks.Remove(task));
    }
}