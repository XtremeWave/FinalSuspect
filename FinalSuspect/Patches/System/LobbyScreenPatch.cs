using System.Text.RegularExpressions;
using AmongUs.Data;
using FinalSuspect.Helpers;
using InnerNet;
using TMPro;
using UnityEngine;

namespace FinalSuspect.Patches.System;

[HarmonyPatch]
public sealed class LobbyJoinBind
{
    private static int GameId;
    private static Color Color = ColorHelper.CompleteGreen;
    private static GameObject LobbyText;
    private static GameObject LeftShiftSprite;
    private static GameObject RightShiftSprite;
    private static GameObject KeyBindBackground;
    private static GameObject KeyBindBackground_Clone;

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
    [HarmonyPostfix]
    public static void Postfix(InnerNetClient __instance)
    {
        GameId = __instance.GameId;
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    public static void Postfix()
    {
        var code2 = GUIUtility.systemCopyBuffer;

        if (code2.Length != 6 || !Regex.IsMatch(code2, "^[a-zA-Z]+$"))
            code2 = "";

        if (LobbyText) return;
        LobbyText = new GameObject("lobbycode");
        LobbyText.transform.SetParent(GameObject.Find("RightPanel").transform, false);
        var comp = LobbyText.AddComponent<TextMeshPro>();
        comp.fontSize = 2.5f;
        comp.outlineWidth = -2f;
        var lastY = code2 == "" ? -0.15f : 0.1f;
        LobbyText.transform.localPosition = new Vector3(8.3f, lastY, 0);
        LobbyText.SetActive(true);
        //LeftShift Sprite
        LeftShiftSprite = new GameObject("LeftShiftSprite");
        LeftShiftSprite.transform.SetParent(GameObject.Find("RightPanel").transform, false);
        var LSsp = LeftShiftSprite.AddComponent<SpriteRenderer>();
        LSsp.sprite = LoadSprite("KeyLeftShift.png", 115f);
        if (LobbyText != null) LeftShiftSprite.SetActive(true);
        //RightShift Sprite
        RightShiftSprite = new GameObject("RightShiftSprite");
        RightShiftSprite.transform.SetParent(GameObject.Find("RightPanel").transform, false);
        var RSsp = RightShiftSprite.AddComponent<SpriteRenderer>();
        RSsp.sprite = LoadSprite("KeyRightShift.png", 115f);
        if (LobbyText != null) RightShiftSprite.SetActive(true);
        //KeyBindBackGround Belong to Left Shift
        KeyBindBackground = new GameObject("KeyBindBackground");
        KeyBindBackground.transform.SetParent(GameObject.Find("RightPanel").transform, false);
        var KeyBindSp = KeyBindBackground.AddComponent<SpriteRenderer>();
        KeyBindSp.GetComponent<SpriteRenderer>().sprite = LoadSprite("KeyBackground.png", 100f);
        if (LeftShiftSprite != null) KeyBindBackground.SetActive(true);
        //KeyBindBackGround Belong to Right Shift
        KeyBindBackground_Clone = new GameObject("KeyBindBackground_Clone");
        KeyBindBackground_Clone.transform.SetParent(GameObject.Find("RightPanel").transform, false);
        var KeyBindSp_Clone = KeyBindBackground_Clone.AddComponent<SpriteRenderer>();
        KeyBindSp_Clone.GetComponent<SpriteRenderer>().sprite = LoadSprite("KeyBackground.png", 100f);
        if (RightShiftSprite != null) KeyBindBackground_Clone.SetActive(true);
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
    [HarmonyPostfix]
    public static void Postfix(MainMenuManager __instance)
    {
        var code2 = GUIUtility.systemCopyBuffer;

        if (code2.Length != 6 || !Regex.IsMatch(code2, "^[a-zA-Z]+$"))
            code2 = "";
        var code2Disp = DataManager.Settings.Gameplay.StreamerMode ? new string('*', code2.Length) : code2.ToUpper();
        if (GameId != 0 && Input.GetKeyDown(KeyCode.LeftShift))
        {
            __instance.StartCoroutine(AmongUsClient.Instance.CoJoinOnlineGameFromCode(GameId));
            LobbyText.GetComponent<TextMeshPro>().color = Color.AlphaMultiplied(0.75f);
        }

        else if (Input.GetKeyDown(KeyCode.RightShift) && code2 != "")
        {
            __instance.StartCoroutine(AmongUsClient.Instance.CoJoinOnlineGameFromCode(GameCode.GameNameToInt(code2)));
            LobbyText.GetComponent<TextMeshPro>().color = Color.AlphaMultiplied(0.75f);
        }

        if (LobbyText)
        {
            LobbyText.GetComponent<TextMeshPro>().text = "";
            LeftShiftSprite.SetActive(false);
            RightShiftSprite.SetActive(false);
            KeyBindBackground.SetActive(false);
            KeyBindBackground_Clone.SetActive(false);
            if (GameId != 0 && GameId != 32)
            {
                var code = GameCode.IntToGameName(GameId);

                if (code != "")
                {
                    code = DataManager.Settings.Gameplay.StreamerMode ? new string('*', code.Length) : code;
                    LeftShiftSprite.transform.localPosition = new Vector3(-1.9f, 2.1f, -1);
                    KeyBindBackground.transform.localPosition = new Vector3(LeftShiftSprite.transform.localPosition.x,
                        LeftShiftSprite.transform.localPosition.y, -0.5f);
                    KeyBindBackground.SetActive(true);
                    LeftShiftSprite.SetActive(true);
                    if (code != "" && code2 != "")
                    {
                        LeftShiftSprite.transform.localPosition = new Vector3(-1.9f, 2.4f, -1);
                        RightShiftSprite.transform.localPosition = new Vector3(-1.9f, 2.15f, -1);
                        KeyBindBackground.transform.localPosition = new Vector3(
                            LeftShiftSprite.transform.localPosition.x, LeftShiftSprite.transform.localPosition.y,
                            -0.5f);
                        KeyBindBackground_Clone.transform.localPosition = new Vector3(
                            RightShiftSprite.transform.localPosition.x, RightShiftSprite.transform.localPosition.y,
                            -0.5f);
                        LeftShiftSprite.SetActive(true);
                        RightShiftSprite.SetActive(true);
                        KeyBindBackground.SetActive(true);
                        KeyBindBackground_Clone.SetActive(true);
                    }

                    LobbyText.GetComponent<TextMeshPro>().text =
                        string.Format($"{GetString("LShift")}：<color={ColorHelper.FSColorHex}>{code}</color>");
                }
            }

            if (code2 != "")
            {
                RightShiftSprite.transform.localPosition = new Vector3(-1.9f, 2.1f, -1);
                KeyBindBackground_Clone.transform.localPosition = new Vector3(
                    RightShiftSprite.transform.localPosition.x, RightShiftSprite.transform.localPosition.y, -0.5f);
                RightShiftSprite.SetActive(true);
                KeyBindBackground_Clone.SetActive(true);
                LobbyText.GetComponent<TextMeshPro>().text +=
                    string.Format($"\n{GetString("RShift")}：<color={ColorHelper.FSColorHex}>{code2Disp}</color>");
            }
        }
    }
}