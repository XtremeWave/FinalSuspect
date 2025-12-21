using FinalSuspect.Helpers;

namespace FinalSuspect.Modules.Core.Game.PlayerControlExtension;

public static class _GamePlayer
{
    extension(PlayerControl player)
    {
        public bool IsSelf()
        {
            return PlayerControl.LocalPlayer == player;
        }

        public bool IsAlive()
        {
            return player?.GetData()?.IsDead == false || !IsInGame;
        }

        public bool OtherModClient()
        {
            return Utils.OtherModClient(player.GetClientId()) ||
                   (player.Data.OwnerId == -2
                    && !Utils.IsFinalSuspect(player.GetClientId())
                    && !IsFreePlay
                    && !IsLocalGame);
        }

        public bool ModClient()
        {
            return Utils.ModClient(player.GetClientId());
        }

        public bool IsFinalSuspect()
        {
            return Utils.IsFinalSuspect(player.GetClientId());
        }

        public bool IsDev()
        {
            return Utils.IsDev(player.FriendCode);
        }

        public PlainShipRoom GetPlainShipRoom()
        {
            var rooms = ShipStatus.Instance.AllRooms;
            return rooms?.Where(room => room.roomArea)
                .FirstOrDefault(room => player.Collider.IsTouching(room.roomArea));
        }

        public string GetPlainShipRoomName()
        {
            try
            {
                if (IsInMeeting)
                    return player.GetData().PreMeetingRoomName;
                var roomStr = player.GetPlainShipRoom().RoomId.ToString();
                var roomName = StringHelper.ColorString(ColorHelper.ClientlessColor, $"({GetString(roomStr)})");
                return roomName;
            }
            catch
            {
                return "";
            }
        }
    }
}