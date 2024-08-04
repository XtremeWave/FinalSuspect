using HarmonyLib;
using Il2CppSystem;
using System.Collections.Generic;
using InnerNet;
using Il2CppSystem.Linq;
using UnityEngine;

namespace FinalSuspect_Xtreme;

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
}//*/

[HarmonyPatch(typeof(MatchMakerGameButton), nameof(MatchMakerGameButton.SetGame))]
public static class MatchMakerGameButtonSetGamePatch
{
    public static List<GameListing> allGames = new();
    public static bool Prefix(MatchMakerGameButton __instance, [HarmonyArgument(0)] GameListing game)
    {
        var nameList = TranslationController.Instance.currentLanguage.languageID is SupportedLangs.SChinese or SupportedLangs.TChinese ? Main.TName_Snacks_CN : Main.TName_Snacks_EN;

        allGames.Remove(game);
        allGames.Add(game); 
        if (game.Language.ToString().Length > 9) return true;
        var color = game.Platform switch
        {
            Platforms.StandaloneItch => "#737373",
            Platforms.StandaloneWin10 => "#FFF88D",
            Platforms.StandaloneEpicPC => "#905CDA",
            Platforms.StandaloneSteamPC => "#3A78A8",

            Platforms.Xbox => "#07ff00",
            Platforms.Switch => "",
            Platforms.Playstation => "#001090",

            Platforms.StandaloneMac => "#e3e3e3",
            Platforms.IPhone => "#e3e3e3",
            Platforms.Android => "#1EA21A",

            Platforms.Unknown or
            _ => "#ffffff"
        };
        var platforms = game.Platform switch
        {
            Platforms.StandaloneItch => "Itch",
            Platforms.StandaloneWin10 => "Win10",
            Platforms.StandaloneEpicPC => "Epic",
            Platforms.StandaloneSteamPC => "Steam",

            Platforms.Xbox => "Xbox",
            Platforms.Switch => "",
            Platforms.Playstation => "PlayStation",

            Platforms.StandaloneMac => "Mac.",
            Platforms.IPhone => Translator.GetString("IPhone"),
            Platforms.Android => Translator.GetString("Android"),

            Platforms.Unknown or
            _ => "???"
        };
        string str = Math.Abs(game.GameId).ToString();
        int id = Math.Min(Math.Max(int.Parse(str.Substring(str.Length - 2, 2)), 1) * nameList.Count / 100, nameList.Count);
        if (game.Platform == Platforms.Switch)
        {
            int halfLength = nameList[id].Length / 2;

            string firstHalf = nameList[id].Substring(0, halfLength);
            string secondHalf = nameList[id].Substring(halfLength);
            game.HostName = $"" +
                $"<size=80%>" +

                $"<color=#00B2FF>" +
                $"{firstHalf}" +
                $"</color>" +

                $"<color=#ff0000>" +
                $"{secondHalf}" +
                $"</color>" +
                $"</size>" +

                $"<size=40%>" +

                "<color=#ffff00>----</color>" +
                $"<color=#00B2FF>" +
                $"Nintendo" +
                $"</color>" +

                $"<color=#ff0000>" +
                $"Switch" +
                $"</color>" +

                $"</size>"
                ;
        }
        else
            game.HostName = $"" +
                $"<size=80%>" +
                $"<color={color}>" +
                $"{nameList[id]}" +
                $"</color>" +
                $"</size>" +
                $"<size=40%>" +
                "<color=#ffff00>----</color>" +
                $"<color={color}>" +
                $"{platforms}" +
                $"</color>" +
                $"</size>"
                ;
        game.HostName += $"<size=30%> ({Math.Max(0, 100 - game.Age / 100)}%)</size>";
        return true;
    }



}
