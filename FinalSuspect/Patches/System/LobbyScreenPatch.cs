using AmongUs.Data;
using HarmonyLib;
using InnerNet;
using System.Text.RegularExpressions;
using FinalSuspect.Helpers;
using TMPro;
using UnityEngine;
using static FinalSuspect.Modules.Core.Plugin.Translator;

namespace FinalSuspect.Patches.System;
[HarmonyPatch]
public sealed class LobbyJoinBind
{
    private static int GameId;
    private static GameObject LastRoomText;
    private static GameObject CopiedRoomText;
    private static TextMeshPro lastRoomTextComponent;
    private static TextMeshPro copiedRoomTextComponent;

    private const float TEXT_SIZE = 1.5f;
    private const string LAST_ROOM_TEXT_NAME = "LastLobbyCode";
    private const string COPIED_ROOM_TEXT_NAME = "CopiedLobbyCode";
    private const string MOD_COLOR = ColorHelper.ModColor;

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
    [HarmonyPostfix]
    public static void Postfix(InnerNetClient __instance)
    {
        GameId = __instance.GameId;
    }

    [HarmonyPatch(typeof(MMOnlineManager), nameof(MMOnlineManager.Start))]
    [HarmonyPostfix]
    public static void Postfix()
    {
        InitializeTextObject(ref LastRoomText, ref lastRoomTextComponent, LAST_ROOM_TEXT_NAME, new Vector3(9.8f, -3.6f, 0));
        InitializeTextObject(ref CopiedRoomText, ref copiedRoomTextComponent, COPIED_ROOM_TEXT_NAME, new Vector3(9.8f, -3.8f, 0));
        
    }

    private static void InitializeTextObject(ref GameObject gameObject, ref TextMeshPro textComponent, string name, Vector3 position)
    {
        if (gameObject) return;
        gameObject = new GameObject(name);
        textComponent = gameObject.AddComponent<TextMeshPro>();
        textComponent.fontSize = TEXT_SIZE;
        gameObject.transform.localPosition = position;
        gameObject.SetActive(true);

    }

    [HarmonyPatch(typeof(MMOnlineManager), nameof(MMOnlineManager.Update))]
    [HarmonyPostfix]
    public static void Postfix(MMOnlineManager __instance)
    {
        UpdateGameJoinLogic(__instance);
        UpdateTextDisplay();
    }
    private static void UpdateGameJoinLogic(MMOnlineManager manager)
    {
        if (GameId != 0 && Input.GetKeyDown(KeyCode.LeftShift))
        {
            manager.StartCoroutine(AmongUsClient.Instance.CoJoinOnlineGameFromCode(GameId));
        }
        else if (Input.GetKeyDown(KeyCode.RightShift))
        {
            var copyBuffer = GUIUtility.systemCopyBuffer;
            if (Regex.IsMatch(copyBuffer, @"^[a-zA-Z]+$"))
            {
                manager.StartCoroutine(AmongUsClient.Instance.CoJoinOnlineGameFromCode(GameCode.GameNameToInt(copyBuffer)));
            }
        }
    }

    private static void UpdateTextDisplay()
    {
        if (lastRoomTextComponent == null || copiedRoomTextComponent == null)
        {
            return;
        }
        
        var lastCode = GameId != 0 && GameId != 32 ? GameCode.IntToGameName(GameId) : "";
        var copiedCode = GUIUtility.systemCopyBuffer;
        
        if (!Regex.IsMatch(copiedCode, @"^[a-zA-Z]+$") || copiedCode.Length > 6)
        {
            copiedCode = "";
        }

        if (DataManager.Settings.Gameplay.StreamerMode)
        {
            lastCode = new string('*', lastCode.Length);
            copiedCode = new string('*', copiedCode.Length);
        }
        var lastY = copiedCode == "" ? -3.8f : -3.6f;
        LastRoomText.transform.localPosition = new Vector3(9.8f, lastY, 0);
        lastCode = string.IsNullOrEmpty(lastCode) ? "" : lastCode.ToUpper();
        copiedCode = string.IsNullOrEmpty(copiedCode) ? "" : copiedCode.ToUpper();

        lastRoomTextComponent.text = lastCode != "" ? $"        {GetString("LShift")}: <color={MOD_COLOR}>{lastCode}</color>  " : "";
        copiedRoomTextComponent.text = copiedCode != "" ? $"        {GetString("RShift")}: <color={MOD_COLOR}>{copiedCode}</color>  " : "";
    }
}