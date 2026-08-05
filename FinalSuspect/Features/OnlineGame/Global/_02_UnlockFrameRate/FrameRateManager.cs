namespace FinalSuspect.Features.OnlineGame.Global._02_UnlockFrameRate;

public static class FrameRateManager
{
    public static void AdjustFrameRate()
    {
        var refreshRate = Screen.currentResolution.refreshRate;
        Application.targetFrameRate = ConfigManager.UnlockFPS.Value ? refreshRate : 60;
        //SendInGame(string.Format(GetString("Notification.FPSSetTo"), Application.targetFrameRate));
    }
}