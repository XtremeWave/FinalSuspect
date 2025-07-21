namespace FinalSuspect.Modules.Core.Game.PlayerControlExtension;

public static class _Events
{
    public static void OnCompleteTask(this PlayerControl pc)
    {
        pc.GetFinalData().UpdateProcess();
    }
}