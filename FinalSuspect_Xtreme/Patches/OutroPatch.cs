using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using FinalSuspect_Xtreme.Modules;

using FinalSuspect_Xtreme.Patches;
using FinalSuspect_Xtreme.Templates;
using UnityEngine;
using static FinalSuspect_Xtreme.Translator;
using static Il2CppSystem.Globalization.CultureInfo;

namespace FinalSuspect_Xtreme;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
class EndGamePatch
{
    public static Dictionary<byte, string> SummaryText = new();
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ref EndGameResult endGameResult)
    {
        SummaryText = new();
        foreach (var id in GamePlayerData.AllGamePlayerData.Keys)
            SummaryText[id] = Utils.SummaryTexts(id);
    }
}
[HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
class SetEverythingUpPatch
{
    private static TextMeshPro roleSummary;
    private static SimpleButton showHideButton;
    static bool DidHumansWin;
    public static void Prefix()
    {
        DidHumansWin = GameManager.Instance.DidHumansWin(EndGameResult.CachedGameOverReason);
    }
    public static void Postfix(EndGameManager __instance)
    {

        var showInitially = Main.ShowResults.Value;

        //#######################################
        //          ==勝利陣営表示==
        //#######################################
        Logger.Info("胜利阵营显示", "SetEverythingUpPatch");
        string CustomWinnerColor = "#ffffff";

        //if (Main.AssistivePluginMode.Value) goto ShowResult;
        //if (!Main.playerVersion.ContainsKey(0)) return;

        //var WinnerTextObject = UnityEngine.Object.Instantiate(__instance.WinText.gameObject);
        //WinnerTextObject.transform.position = new(__instance.WinText.transform.position.x, __instance.WinText.transform.position.y - 0.5f, __instance.WinText.transform.position.z);
        //WinnerTextObject.transform.localScale = new(0.6f, 0.6f, 0.6f);
        //var WinnerText = WinnerTextObject.GetComponent<TMPro.TextMeshPro>(); //WinTextと同じ型のコンポーネントを取得
        //WinnerText.fontSizeMin = 3f;
        //WinnerText.text = "";
        //var InEndWinnerText = "";


        //var winnerRole = (CustomRoles)CustomWinnerHolder.WinnerTeam;
        //if (winnerRole >= 0)
        //{
        //    CustomWinnerText = GetWinnerRoleName(winnerRole, 0);
        //    EndWinnerText = GetWinnerRoleName(winnerRole, 1);
        //    CustomWinnerColor = Utils.GetRoleColorCode(winnerRole);
        //    EndWinnerColor = Utils.GetRoleColorCode(winnerRole);
        //    if (winnerRole.IsNeutral())
        //    {
        //        __instance.BackgroundBar.material.color = Utils.GetRoleColor(winnerRole);
        //    }
        //}
        //if (AmongUsClient.Instance.AmHost && PlayerState.GetByPlayerId(0).MainRole == CustomRoles.GM)
        //{
        //    __instance.WinText.text = GetString("GameOver");
        //    __instance.WinText.color = Utils.GetRoleColor(CustomRoles.GM);
        //    __instance.BackgroundBar.material.color = Utils.GetRoleColor(CustomRoles.GM);
        //}
        //switch (CustomWinnerHolder.WinnerTeam)
        //{
        //    //通常勝利
        //    case CustomWinner.Crewmate:
        //        CustomWinnerColor = "#8cffff";
        //        EndWinnerColor = "#8cffff";
        //        break;
        //    //特殊勝利
        //    case CustomWinner.Terrorist:
        //        __instance.Foreground.material.color = Color.red;
        //        break;
        //    case CustomWinner.Lovers:
        //        __instance.BackgroundBar.material.color = Utils.GetRoleColor(CustomRoles.Lovers);
        //        break;
        //    case CustomWinner.AdmirerLovers:
        //        __instance.BackgroundBar.material.color = Utils.GetRoleColor(CustomRoles.AdmirerLovers);
        //        break;
        //    case CustomWinner.AkujoLovers:
        //        __instance.BackgroundBar.material.color = Utils.GetRoleColor(CustomRoles.AkujoLovers);
        //        break;
        //    case CustomWinner.CupidLovers:
        //        __instance.BackgroundBar.material.color = Utils.GetRoleColor(CustomRoles.CupidLovers);
        //        break;
        //    //引き分け処理
        //    case CustomWinner.Draw:
        //        __instance.WinText.text = GetString("ForceEnd");
        //        __instance.WinText.color = Color.white;
        //        __instance.BackgroundBar.material.color = Color.gray;
        //        WinnerText.text = GetString("ForceEndText");
        //        WinnerText.color = Color.gray;
        //        break;
        //    //全滅
        //    case CustomWinner.None:
        //        __instance.WinText.text = "";
        //        __instance.WinText.color = Color.black;
        //        __instance.BackgroundBar.material.color = Color.gray;
        //        WinnerText.text = GetString("EveryoneDied");
        //        WinnerText.color = Color.gray;
        //        break;
        //    case CustomWinner.Error:
        //        __instance.WinText.text = GetString("ErrorEndText");
        //        __instance.WinText.color = Color.red;
        //        __instance.BackgroundBar.material.color = Color.red;
        //        WinnerText.text = GetString("ErrorEndTextDescription");
        //        WinnerText.color = Color.white;
        //        break;
        //}


        //foreach (var role in CustomWinnerHolder.AdditionalWinnerRoles)
        //{
        //    var addWinnerRole = (CustomRoles)role;
        //    AdditionalWinnerText.Append('＆').Append(Utils.ColorString(Utils.GetRoleColor(role), GetWinnerRoleName(addWinnerRole, 0)));
        //    EndAdditionalWinnerText.Append('＆').Append(Utils.ColorString(Utils.GetRoleColor(role), GetWinnerRoleName(addWinnerRole, 1) + GetString("Win")));
        //}
        //if (CustomWinnerHolder.WinnerTeam is not CustomWinner.Draw and not CustomWinner.None and not CustomWinner.Error)
        //{
        //    if (AdditionalWinnerText.Length < 1) WinnerText.text = $"<color={CustomWinnerColor}>{CustomWinnerText}</color>";
        //    else WinnerText.text = $"<color={CustomWinnerColor}>{CustomWinnerText}</color>{AdditionalWinnerText}";
        //    if (EndAdditionalWinnerText.Length < 1) InEndWinnerText = $"<color={EndWinnerColor}>{EndWinnerText}{GetString("Win")}</color>";
        //    else InEndWinnerText = $"<color={CustomWinnerColor}>{CustomWinnerText}</color>{AdditionalWinnerText}{GetString("Win")}";
        //}

        //static string GetWinnerRoleName(CustomRoles role, int a)
        //{
        //    if (a == 0
        //        )
        //    {
        //        var name = GetString($"WinnerRoleText.{Enum.GetName(typeof(CustomRoles), role)}");
        //        if (name == "" || name.StartsWith("*") || name.StartsWith("<INVALID")) name = Utils.GetRoleName(role);
        //        return name;
        //    }
        //    else
        //    {
        //        var name = GetString($"WinnerRoleText.InEnd.{Enum.GetName(typeof(CustomRoles), role)}");
        //        if (name == "" || name.StartsWith("*") || name.StartsWith("<INVALID")) name = Utils.GetRoleName(role);
        //        return name;
        //    }

        //}

        //LastWinsText = InEndWinnerText;


        //ShowResult:
        Logger.Info("最终结果显示", "SetEverythingUpPatch");
        showHideButton = new SimpleButton(
           __instance.transform,
           "ShowHideResultsButton",
           new(-4.5f, 2.6f, -14f),  // BackgroundLayer(z=-13)より手前
           new(209, 190, 0, byte.MaxValue),
           new(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue),
           () =>
           {
               var setToActive = !roleSummary.gameObject.activeSelf;
               roleSummary.gameObject.SetActive(setToActive);
               Main.ShowResults.Value = setToActive;
               showHideButton.Label.text = GetString(setToActive ? "HideResults" : "ShowResults");
           },
           GetString(showInitially ? "HideResults" : "ShowResults"))
        {
            Scale = new(1.5f, 0.5f),
            FontSize = 2f,
        };

        StringBuilder sb = new($"{GetString("RoleSummaryText")}");




        CustomWinnerColor = !DidHumansWin ? "#FF1919" : "#8CFFFF";
        __instance.WinText.color.SetAlpha(0.35f);



        foreach (var kvp in GamePlayerData.AllGamePlayerData.Where(x => x.Value.IsImpostor != DidHumansWin))
        {
            var id = kvp.Key;
            var data = kvp.Value;
            sb.Append($"\n<color={CustomWinnerColor}>★</color> ").Append(EndGamePatch.SummaryText[id]);
        }
        foreach (var kvp in GamePlayerData.AllGamePlayerData.Where(x => x.Value.IsImpostor == DidHumansWin))
        {
            var id = kvp.Key;
            var data = kvp.Value;
            sb.Append($"\n　 ").Append(EndGamePatch.SummaryText[id]);

        }
        Logger.Info("判断胜利结束", "SetEverythingUpPatch");



        roleSummary = TMPTemplate.Create(
                "RoleSummaryText",
                sb.ToString(),
                Color.white,
                1.25f,
                TextAlignmentOptions.TopLeft,
                setActive: showInitially,
                parent: showHideButton.Button.transform);
        roleSummary.transform.localPosition = new(1.7f, -0.4f, 0f);
        roleSummary.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////Utils.ApplySuffix();
    }
}
