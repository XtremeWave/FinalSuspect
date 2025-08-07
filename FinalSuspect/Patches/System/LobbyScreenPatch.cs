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
    private static int _gameId;
    private static Color _color = ColorHelper.CompleteGreen;
    private static GameObject _lobbyText;
    private static GameObject _leftShiftSprite;
    private static GameObject _rightShiftSprite;
    private static GameObject _keyBindBackground;
    private static GameObject _keyBindBackgroundClone;

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.JoinGame))]
    [HarmonyPostfix]
    public static void Postfix(InnerNetClient __instance)
    {
        _gameId = __instance.GameId;
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    [HarmonyPostfix]
    public static void Postfix()
    {
        var code2 = GUIUtility.systemCopyBuffer;

        if (code2.Length != 6 || !Regex.IsMatch(code2, "^[a-zA-Z]+$"))
            code2 = "";

        if (_lobbyText) return;
        _lobbyText = new GameObject("lobbycode");
        _lobbyText.transform.SetParent(GameObject.Find("RightPanel").transform, false);
        var comp = _lobbyText.AddComponent<TextMeshPro>();
        comp.fontSize = 2.5f;
        comp.outlineWidth = -2f;
        var lastY = code2 == "" ? -0.15f : 0.1f;
        _lobbyText.transform.localPosition = new Vector3(8.3f, lastY, 0);
        _lobbyText.SetActive(true);
        //LeftShift Sprite
        _leftShiftSprite = new GameObject("LeftShiftSprite");
        _leftShiftSprite.transform.SetParent(GameObject.Find("RightPanel").transform, false);
        var LS_SP = _leftShiftSprite.AddComponent<SpriteRenderer>();
        LS_SP.sprite = LoadSprite("KeyLeftShift.png", 115f);
        if (_lobbyText != null) _leftShiftSprite.SetActive(true);
        //RightShift Sprite
        _rightShiftSprite = new GameObject("RightShiftSprite");
        _rightShiftSprite.transform.SetParent(GameObject.Find("RightPanel").transform, false);
        var RS_SP = _rightShiftSprite.AddComponent<SpriteRenderer>();
        RS_SP.sprite = LoadSprite("KeyRightShift.png", 115f);
        if (_lobbyText != null) _rightShiftSprite.SetActive(true);
        //KeyBindBackGround Belong to Left Shift
        _keyBindBackground = new GameObject("KeyBindBackground");
        _keyBindBackground.transform.SetParent(GameObject.Find("RightPanel").transform, false);
        var keyBindSp = _keyBindBackground.AddComponent<SpriteRenderer>();
        keyBindSp.GetComponent<SpriteRenderer>().sprite = LoadSprite("KeyBackground.png", 100f);
        if (_leftShiftSprite != null) _keyBindBackground.SetActive(true);
        //KeyBindBackGround Belong to Right Shift
        _keyBindBackgroundClone = new GameObject("KeyBindBackground_Clone");
        _keyBindBackgroundClone.transform.SetParent(GameObject.Find("RightPanel").transform, false);
        var keyBindSpClone = _keyBindBackgroundClone.AddComponent<SpriteRenderer>();
        keyBindSpClone.GetComponent<SpriteRenderer>().sprite = LoadSprite("KeyBackground.png", 100f);
        if (_rightShiftSprite != null) _keyBindBackgroundClone.SetActive(true);
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
    [HarmonyPostfix]
    public static void Postfix(MainMenuManager __instance)
    {
        var code2 = GUIUtility.systemCopyBuffer;

        if (code2.Length != 6 || !Regex.IsMatch(code2, "^[a-zA-Z]+$"))
            code2 = "";
        var code2Disp = DataManager.Settings.Gameplay.StreamerMode ? new string('*', code2.Length) : code2.ToUpper();
        if (_gameId != 0 && Input.GetKeyDown(KeyCode.LeftShift))
        {
            __instance.StartCoroutine(AmongUsClient.Instance.CoJoinOnlineGameFromCode(_gameId));
            _lobbyText.GetComponent<TextMeshPro>().color = _color.AlphaMultiplied(0.75f);
        }

        else if (Input.GetKeyDown(KeyCode.RightShift) && code2 != "")
        {
            __instance.StartCoroutine(AmongUsClient.Instance.CoJoinOnlineGameFromCode(GameCode.GameNameToInt(code2)));
            _lobbyText.GetComponent<TextMeshPro>().color = _color.AlphaMultiplied(0.75f);
        }

        if (_lobbyText)
        {
            _lobbyText.GetComponent<TextMeshPro>().text = "";
            _leftShiftSprite.SetActive(false);
            _rightShiftSprite.SetActive(false);
            _keyBindBackground.SetActive(false);
            _keyBindBackgroundClone.SetActive(false);
            if (_gameId != 0 && _gameId != 32)
            {
                var code = GameCode.IntToGameName(_gameId);

                if (code != "")
                {
                    code = DataManager.Settings.Gameplay.StreamerMode ? new string('*', code.Length) : code;
                    _leftShiftSprite.transform.localPosition = new Vector3(-1.9f, 2.1f, -1);
                    _keyBindBackground.transform.localPosition = new Vector3(_leftShiftSprite.transform.localPosition.x,
                        _leftShiftSprite.transform.localPosition.y, -0.5f);
                    _keyBindBackground.SetActive(true);
                    _leftShiftSprite.SetActive(true);
                    if (code != "" && code2 != "")
                    {
                        _leftShiftSprite.transform.localPosition = new Vector3(-1.9f, 2.4f, -1);
                        _rightShiftSprite.transform.localPosition = new Vector3(-1.9f, 2.15f, -1);
                        _keyBindBackground.transform.localPosition = new Vector3(
                            _leftShiftSprite.transform.localPosition.x, _leftShiftSprite.transform.localPosition.y,
                            -0.5f);
                        _keyBindBackgroundClone.transform.localPosition = new Vector3(
                            _rightShiftSprite.transform.localPosition.x, _rightShiftSprite.transform.localPosition.y,
                            -0.5f);
                        _leftShiftSprite.SetActive(true);
                        _rightShiftSprite.SetActive(true);
                        _keyBindBackground.SetActive(true);
                        _keyBindBackgroundClone.SetActive(true);
                    }

                    _lobbyText.GetComponent<TextMeshPro>().text =
                        string.Format($"{GetString("LShift")}：<color={ColorHelper.FSColorHex}>{code}</color>");
                }
            }

            if (code2 != "")
            {
                _rightShiftSprite.transform.localPosition = new Vector3(-1.9f, 2.1f, -1);
                _keyBindBackgroundClone.transform.localPosition = new Vector3(
                    _rightShiftSprite.transform.localPosition.x, _rightShiftSprite.transform.localPosition.y, -0.5f);
                _rightShiftSprite.SetActive(true);
                _keyBindBackgroundClone.SetActive(true);
                _lobbyText.GetComponent<TextMeshPro>().text +=
                    string.Format($"\n{GetString("RShift")}：<color={ColorHelper.FSColorHex}>{code2Disp}</color>");
            }
        }
    }
}