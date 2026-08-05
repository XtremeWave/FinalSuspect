using System;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch(typeof(DisconnectPopup))]
internal class DisconnectPopupPatch
{
    public static DisconnectReasons Reason;
    public static string StringReason;

    [HarmonyPatch(nameof(DisconnectPopup.DoShow))]
    [HarmonyPostfix]
    public static void DoShow_Postfix(DisconnectPopup __instance)
    {
        _ = new MainThreadTask(() =>
        {
            if (!__instance) return;
            try
            {
                void SetText(string text)
                {
                    if (__instance._textArea?.text != null)
                        __instance._textArea.text = text;
                }

                switch (Reason)
                {
                    case DisconnectReasons.Hacking:
                        SetText(GetString("DCNotify.Hacking"));
                        break;
                    case DisconnectReasons.Banned:
                        SetText(GetString("DCNotify.Banned"));
                        break;
                    case DisconnectReasons.Kicked:
                        SetText(GetString("DCNotify.Kicked"));
                        break;
                    case DisconnectReasons.GameNotFound:
                        SetText(GetString("DCNotify.GameNotFound"));
                        break;
                    case DisconnectReasons.GameStarted:
                        SetText(GetString("DCNotify.GameStarted"));
                        break;
                    case DisconnectReasons.GameFull:
                        SetText(GetString("DCNotify.GameFull"));
                        break;
                    case DisconnectReasons.IncorrectVersion:
                        SetText(GetString("DCNotify.IncorrectVersion"));
                        break;
                    case DisconnectReasons.Error:
                        if (StringReason.Contains("Failed to send message"))
                            SetText(GetString("DCNotify.DCFromServer"));
                        break;
                    case DisconnectReasons.Custom:
                        if (StringReason.Contains("Reliable packet") ||
                            StringReason.Contains("remote has not responded to"))
                            SetText(GetString("DCNotify.DCFromServer"));
                        break;
                    case DisconnectReasons.ExitGame:
                    case DisconnectReasons.InvalidName:
                    case DisconnectReasons.NotAuthorized:
                    case DisconnectReasons.ConnectionLimit:
                    case DisconnectReasons.Destroy:
                    case DisconnectReasons.IncorrectGame:
                    case DisconnectReasons.ServerRequest:
                    case DisconnectReasons.ServerFull:
                    case DisconnectReasons.MismatchedVersion:
                    case DisconnectReasons.InternalPlayerMissing:
                    case DisconnectReasons.InternalNonceFailure:
                    case DisconnectReasons.InternalConnectionToken:
                    case DisconnectReasons.PlatformLock:
                    case DisconnectReasons.LobbyInactivity:
                    case DisconnectReasons.MatchmakerInactivity:
                    case DisconnectReasons.InvalidGameOptions:
                    case DisconnectReasons.NoServersAvailable:
                    case DisconnectReasons.QuickmatchDisabled:
                    case DisconnectReasons.TooManyGames:
                    case DisconnectReasons.QuickchatLock:
                    case DisconnectReasons.MatchmakerFull:
                    case DisconnectReasons.Sanctions:
                    case DisconnectReasons.ServerError:
                    case DisconnectReasons.SelfPlatformLock:
                    case DisconnectReasons.DuplicateConnectionDetected:
                    case DisconnectReasons.TooManyRequests:
                    case DisconnectReasons.IntentionalLeaving:
                    case DisconnectReasons.FocusLostBackground:
                    case DisconnectReasons.FocusLost:
                    case DisconnectReasons.NewConnection:
                    case DisconnectReasons.PlatformParentalControlsBlock:
                    case DisconnectReasons.PlatformUserBlock:
                    case DisconnectReasons.PlatformFailedToGetUserBlock:
                    case DisconnectReasons.ServerNotFound:
                    case DisconnectReasons.ClientTimeout:
                    case DisconnectReasons.ErrorAuthNonceFailure:
                    case DisconnectReasons.Unknown:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch
            {
                /* ignored */
            }
        }, "Override Disconnect Text");
    }
}