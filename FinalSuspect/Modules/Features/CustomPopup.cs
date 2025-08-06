using System;
using FinalSuspect.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#pragma warning disable CS8602 // 解引用可能出现空引用。

namespace FinalSuspect.Modules.Features;

#nullable enable
public static class CustomPopup
{
    private static GameObject? Fill;
    private static GameObject? InfoScreen;

    private static TextMeshPro? TitleTMP;
    public static TextMeshPro? InfoTMP;

    private static PassiveButton? ActionButtonPrefab;

    public static GameObject? FillTemp;
    public static GameObject? InfoScreenTemp;

    public static TextMeshPro? TitleTMPTemp;
    public static TextMeshPro? InfoTMPTemp;

    public static PassiveButton? ActionButtonPrefabTemp;

    private static List<PassiveButton>? ActionButtons;

    private static bool busy;

    private static (string title, string info, List<(string, Action)>? buttons)? waitToShow;

    private static string waitToUpdateText = string.Empty;

    /// <summary>
    ///     显示一个全屏信息显示界面
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="info">内容</param>
    /// <param name="buttons">按钮（文字，点击事件）</param>
    public static void Show(string title, string info, List<(string, Action)>? buttons)
    {
        if (busy || !Fill || !InfoScreen || !ActionButtonPrefab || !TitleTMP || !InfoTMP)
        {
            Init();
            if (!Fill || !InfoScreen || !ActionButtonPrefab || !TitleTMP || !InfoTMP) return;
        }

        busy = true;

        TitleTMP.text = title;
        InfoTMP.text = info;

        ActionButtons?.Do(b => Object.Destroy(b.gameObject));
        ActionButtons = [];

        if (buttons != null)
            foreach (var buttonInfo in buttons.Where(b => !b.Item1.Trim().IsNullOrWhiteSpace()))
            {
                var (text, action) = buttonInfo;
                var button = Object.Instantiate(ActionButtonPrefab, InfoScreen.transform);
                if (!button) continue;

                var tmp = button.transform.FindChild("Text_TMP")?.GetComponent<TextMeshPro>();
                if (!tmp) continue;

                tmp.text = text;
                button.OnClick = new Button.ButtonClickedEvent();
                button.OnClick.AddListener((Action)(() =>
                {
                    InfoScreen.SetActive(false);
                    Fill.SetActive(false);
                }));
                button.OnClick.AddListener(action);
                button.transform.SetLocalX(0);
                button.gameObject.SetActive(true);
                ActionButtons?.Add(button);
            }

        if (ActionButtons?.Count > 1)
        {
            var widthSum = ActionButtons.Count *
                           (ActionButtonPrefab.gameObject.GetComponent<BoxCollider2D>()?.size.x ?? 0);
            widthSum += (ActionButtons.Count - 1) * 0.1f;
            var start = -Math.Abs(widthSum / 2);
            var each = widthSum / ActionButtons.Count;
            var index = 0;
            foreach (var button in ActionButtons)
            {
                button.transform.SetLocalX(start + each * (index + 0.5f));
                index++;
            }
        }

        Fill.SetActive(true);
        InfoScreen.SetActive(true);

        busy = false;
    }

    public static void ShowLater(string title, string info, List<(string, Action)>? buttons)
    {
        waitToShow = (title, info, buttons);
    }

    public static void UpdateTextLater(string info)
    {
        waitToUpdateText = info;
    }

    public static void Update()
    {
        if (waitToShow != null)
        {
            Show(waitToShow.Value.title, waitToShow.Value.info, waitToShow.Value.buttons);
            waitToShow = null;
        }

        if (string.IsNullOrEmpty(waitToUpdateText)) return;
        InfoTMP?.SetText(waitToUpdateText);
        waitToUpdateText = string.Empty;
    }

    public static void Init()
    {
        var DOBScreen = AccountManager.Instance?.transform.FindChild("DOBEnterScreen");
        if (!DOBScreen) return;

        if (!Fill)
        {
            Fill = Object.Instantiate(DOBScreen.FindChild("Fill")?.gameObject);
            if (!Fill) return;

            FillTemp = Fill;
            Fill.transform.SetLocalZ(-100f);
            Fill.name = "FinalSuspect Info Popup Fill";
            Fill.SetActive(false);
        }

        if (!InfoScreen)
        {
            InfoScreen = Object.Instantiate(DOBScreen.FindChild("InfoPage")?.gameObject);
            if (!InfoScreen) return;

            InfoScreen.transform.SetLocalZ(-110f);
            InfoScreen.name = "FinalSuspect Info Popup Page";
            InfoScreen.SetActive(false);
        }

        if (!TitleTMP)
        {
            TitleTMP = InfoScreen.transform.FindChild("Title Text")?.GetComponent<TextMeshPro>();
            if (!TitleTMP) return;

            TitleTMP.transform.localPosition = new Vector3(0f, 2.3f, 3f);
            TitleTMP.DestroyTranslator();
            TitleTMP.text = "";
        }

        if (!InfoTMP)
        {
            InfoTMP = InfoScreen.transform.FindChild("InfoText_TMP")?.GetComponent<TextMeshPro>();
            if (!InfoTMP) return;

            InfoTMP.GetComponent<RectTransform>().sizeDelta = new Vector2(7f, 1.3f);
            InfoTMP.transform.localScale = new Vector3(1f, 1f, 1f);
            InfoTMP.DestroyTranslator();
            InfoTMP.text = "";
        }

        if (!ActionButtonPrefab)
        {
            ActionButtonPrefab = InfoScreen.transform.FindChild("BackButton")?.GetComponent<PassiveButton>();
            if (!ActionButtonPrefab) return;

            ActionButtonPrefab.gameObject.name = "ActionButtonPrefab";
            ActionButtonPrefab.transform.localScale = new Vector3(0.66f, 0.66f, 0.66f);
            ActionButtonPrefab.transform.localPosition = new Vector3(0f, -0.65f, 3f);
            ActionButtonPrefab.transform.FindChild("Text_TMP")?.GetComponent<TextMeshPro>()?.DestroyTranslator();
            ActionButtonPrefab.gameObject.SetActive(false);
        }
    }
}