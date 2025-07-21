namespace FinalSuspect.Modules.Core.Game.PlayerControlExtension;

public static class _StatesBoolean
{
    public static bool IsLocalPlayer(this PlayerControl player)
    {
        return PlayerControl.LocalPlayer == player;
    }

    public static bool IsAlive(this PlayerControl pc)
    {
        return pc?.GetFinalData()?.IsDead == false || !IsInGame;
    }

    public static bool OtherModClient(this PlayerControl player)
    {
        return Utils.OtherModClient(player.GetClientId()) ||
               (player.Data.OwnerId == -2
                && !Utils.IsFinalSuspect(player.GetClientId())
                && !IsFreePlay
                && !IsLocalGame);
    }

    public static bool ModClient(this PlayerControl player)
    {
        return Utils.ModClient(player.GetClientId());
    }

    public static bool IsFinalSuspect(this PlayerControl player)
    {
        return Utils.IsFinalSuspect(player.GetClientId());
    }

    public static bool IsDev(this PlayerControl pc)
    {
        return Utils.IsDev(pc.FriendCode);
    }
}