using AmongUs.GameOptions;
using FinalSuspect.DataHandling.FinalAntiCheat.Core;
using FinalSuspect.DataHandling.FinalGameData;
using FinalSuspect.Helpers;

namespace FinalSuspect.Modules.Core.Game.PlayerControlExtension;

public static class _Data
{
    extension(PlayerControl player)
    {
        public string GetRealName(bool isMeeting = false)
        {
            if (player == null) return null;

            string dataName = null;
            try
            {
                var data = player.GetData();
                if (data != null)
                    dataName = player.GetDataName();
            }
            catch
            {
                /* ignored */
            }

            var realName = isMeeting ? player.Data?.PlayerName : player.name;
            return realName ?? dataName;
        }

        public string CheckAndGetNameWithDetails(out Color topcolor,
            out Color bottomcolor,
            out string toptext,
            out string bottomtext,
            bool topswap = false)
        {
            return FinalLocalHandling.CheckAndGetNameWithDetails(player.PlayerId, out topcolor, out bottomcolor,
                out toptext, out bottomtext,
                topswap);
        }

        public FinalPlayerData GetData()
        {
            try
            {
                return GetFinalDataById(player.PlayerId);
            }
            catch
            {
                try
                {
                    return FinalPlayerData.AllPlayerData.FirstOrDefault(data => data.Player == player);
                }
                catch
                {
                    return null;
                }
            }
        }

        public PlayerCheatData GetCheatData()
        {
            try
            {
                return GetCheatDataById(player.PlayerId);
            }
            catch
            {
                try
                {
                    return player.GetData().CheatData;
                }
                catch
                {
                    return null;
                }
            }
        }

        public string GetDataName()
        {
            try
            {
                return GetPlayerNameById(player.PlayerId);
            }
            catch
            {
                return null;
            }
        }

        public string GetColoredName()
        {
            try
            {
                var data = GetFinalDataById(player.PlayerId);
                return StringHelper.ColorString(data.PlayerColor, data.PlayerName);
            }
            catch
            {
                return null;
            }
        }

        public void SetDead()
        {
            player.GetData().SetDead();
        }

        public void SetDisconnected()
        {
            player.GetData().SetDisconnected();
            FinalPlayerData.AllPlayerData.Do(_data => _data.AdjustPlayerId());
        }

        public void SetRole(RoleTypes role)
        {
            player.GetData().SetRole(role);
        }

        public void SetDeathReason(VanillaDeathReason deathReason, bool focus = false)
        {
            player.GetData().SetDeathReason(deathReason, focus);
        }

        public void SetRealKiller(PlayerControl killer)
        {
            if (player.GetData().RealKiller != null || !player.Data.IsDead) return;
            player.GetData().SetRealKiller(killer.GetData());
        }

        public void SetTaskTotalCount(int TaskTotalCount)
        {
            player.GetData().SetTaskTotalCount(TaskTotalCount);
        }
    }
}