using HarmonyLib;
using System;
using InnerNet;
using TMPro;
using UnityEngine;

namespace FinalSuspect;

[HarmonyPatch(typeof(FindAGameManager), nameof(FindAGameManager.Update))]
public static class FindAGameManagerUpdatePatch
{
    private static int buffer = 80;
    private static GameObject RefreshButton;
    private static GameObject InputDisplayGlyph;
    public static void Postfix(FindAGameManager __instance)
    {
        if ((RefreshButton = GameObject.Find("RefreshButton")) != null)
            RefreshButton.transform.localPosition = new Vector3(100f, 100f, 100f);
        if ((InputDisplayGlyph = GameObject.Find("InputDisplayGlyph")) != null)
            InputDisplayGlyph.transform.localPosition = new Vector3(100f, 100f, 100f);

        buffer--; if (buffer > 0) return; buffer = 80;
        __instance.RefreshList();
    }
}

[HarmonyPatch(typeof(MatchMakerGameButton), nameof(MatchMakerGameButton.SetGame))]
public static class MatchMakerGameButtonSetGamePatch
{
    public static bool Prefix([HarmonyArgument(0)] GameListing game)
    {

        var nameList = TranslationController.Instance.currentLanguage.languageID is SupportedLangs.SChinese or SupportedLangs.TChinese ? Main.TName_Snacks_CN : Main.TName_Snacks_EN;

        if (game.Language.ToString().Length > 9) return true;

        string str = Math.Abs(game.GameId).ToString();
        int id = Math.Min(Math.Max(int.Parse(str.Substring(str.Length - 2, 2)), 1) * nameList.Count / 100, nameList.Count);

        var color = "#ffffff";
        string RoomName = null;
        var name = "?";

        switch (game.Platform)
        {
            case Platforms.StandaloneEpicPC:
                color = "#905CDA";
                name = "Itch";
                break;
            case Platforms.StandaloneSteamPC:
                color = "#3A78A8";
                name = "Steam";
                break;
            case Platforms.StandaloneMac:
                color = "#e3e3e3";
                name = "Mac.";
                break;
            case Platforms.StandaloneWin10:
                color = "#FFF88D";
                name = "Win-10";
                break;
            case Platforms.StandaloneItch:
                color = "#737373";
                name = "Itch";
                break;
            case Platforms.IPhone:
                color = "#e3e3e3";
                name = Translator.GetString("IPhone");
                break;
            case Platforms.Android:
                color = "#1EA21A";
                name = Translator.GetString("Android");
                break;
            case Platforms.Switch:
                string totalname = nameList[id];
                int halfLength = totalname.Length / 2;
                string firstHalf = totalname.AsSpan(0, halfLength).ToString();
                string secondHalf = totalname.AsSpan(halfLength).ToString();

                RoomName = $"<color=#00B2FF>{firstHalf}</color><color=#ff0000>{secondHalf}</color>";
                name = $"<color=#00B2FF>Nintendo</color><color=#ff0000>Switch</color>";
                break;
            case Platforms.Xbox:
                color = "#07ff00";
                name = "Xbox";
                break;
            case Platforms.Playstation:
                color = "#001090";
                name = "PlayStation";
                break;
        }
        RoomName ??= $"<color={color}>{nameList[id]}</color>";
        var platforms = $"<color={color}>{name}</color>";

        
        game.HostName = $"<size=60%>{RoomName}</size>" +
                $"<size=30%> ({Math.Max(0, 100 - game.Age / 100)}%)</size>" +
                $"\n<size=40%><color={ColorHelper.ModColor}>{GameCode.IntToGameName(game.GameId)}</color></size>" +
                $"<size=40%><color=#ffff00>----</color>{platforms}</size>";
        return true;
    }

    public static void Postfix(MatchMakerGameButton __instance)
    {
        __instance.NameText.fontStyle = FontStyles.Bold;
    }
}
