using System;
using System.Collections.Generic;
using System.Linq;
using FinalSuspect.Helpers;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FinalSuspect.Modules.Features;

#nullable enable
public static class CustomPopup
{
    public static GameObject? Fill;
    public static GameObject? InfoScreen;

    public static TextMeshPro? TitleTMP;
    public static TextMeshPro? InfoTMP;

    public static PassiveButton? ActionButtonPrefab;

    public static GameObject? FillTemp;
    public static GameObject? InfoScreenTemp;

    public static TextMeshPro? TitleTMPTemp;
    public static TextMeshPro? InfoTMPTemp;

    public static PassiveButton? ActionButtonPrefabTemp;

    public static List<PassiveButton>? ActionButtons;

    private static bool busy;

    /// <summary>
    /// 显示一个全屏信息显示界面
    /// </summary>
    /// <param name="title">标题</param>
    /// <param name="info">内容</param>
    /// <param name="buttons">按钮（文字，点击事件）</param>
    public static void Show(string title, string info, List<(string, Action)>? buttons)
    {
        if (busy || Fill == null || InfoScreen == null || ActionButtonPrefab == null || TitleTMP == null || InfoTMP == null)
        {
            Init();
            if (Fill == null || InfoScreen == null || ActionButtonPrefab == null || TitleTMP == null || InfoTMP == null)
            {
                Debug.LogError("Failed to initialize CustomPopup.");
                return;
            }
        }

        busy = true;

        TitleTMP.text = title;
        InfoTMP.text = info;

        ActionButtons?.Do(b => Object.Destroy(b.gameObject));
        ActionButtons = [];

        if (buttons != null)
        {
            foreach (var buttonInfo in buttons.Where(b => b.Item1?.Trim() is not null and not ""))
            {
                var (text, action) = buttonInfo;
                var button = Object.Instantiate(ActionButtonPrefab, InfoScreen.transform);
                if (button == null) continue;

                var tmp = button.transform.FindChild("Text_TMP")?.GetComponent<TextMeshPro>();
                if (tmp == null) continue;

                tmp.text = text;
                button.OnClick = new();
                button.OnClick.AddListener((Action)(() =>
                {
                    InfoScreen?.SetActive(false);
                    Fill?.SetActive(false);
                }));
                if (action != null)
                {
                    button.OnClick.AddListener(action);
                }
                button.transform.SetLocalX(0);
                button.gameObject.SetActive(true);
                ActionButtons?.Add(button);
            }
        }

        if (ActionButtons?.Count > 1)
        {
            var widthSum = ActionButtons.Count * (ActionButtonPrefab?.gameObject.GetComponent<BoxCollider2D>()?.size.x ?? 0);
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

        Fill?.SetActive(true);
        InfoScreen?.SetActive(true);

        busy = false;
    }

    private static (string title, string info, List<(string, Action)>? buttons)? waitToShow;
    public static void ShowLater(string title, string info, List<(string, Action)>? buttons) => waitToShow = (title, info, buttons);

    private static string waitToUpdateText = string.Empty;
    public static void UpdateTextLater(string info) => waitToUpdateText = info;

    public static void Update()
    {
        if (waitToShow != null)
        {
            Show(waitToShow.Value.title, waitToShow.Value.info, waitToShow.Value.buttons);
            waitToShow = null;
        }
        if (!string.IsNullOrEmpty(waitToUpdateText))
        {
            InfoTMP?.SetText(waitToUpdateText);
            waitToUpdateText = string.Empty;
        }
    }

    public static void Init()
    {
        var DOBScreen = AccountManager.Instance?.transform.FindChild("DOBEnterScreen");
        if (DOBScreen == null)
        {
            Debug.LogError("DOBEnterScreen not found!");
            return;
        }

        if (Fill == null)
        {
            Fill = Object.Instantiate(DOBScreen.FindChild("Fill")?.gameObject);
            if (Fill == null)
            {
                Debug.LogError("Failed to instantiate Fill.");
                return;
            }
            FillTemp = Fill;
            Fill.transform.SetLocalZ(-100f);
            Fill.name = "FinalSuspect Info Popup Fill";
            Fill.SetActive(false);
        }

        if (InfoScreen == null)
        {
            InfoScreen = Object.Instantiate(DOBScreen.FindChild("InfoPage")?.gameObject);
            if (InfoScreen == null)
            {
                Debug.LogError("Failed to instantiate InfoScreen.");
                return;
            }
            InfoScreen.transform.SetLocalZ(-110f);
            InfoScreen.name = "FinalSuspect Info Popup Page";
            InfoScreen.SetActive(false);
        }

        if (TitleTMP == null)
        {
            TitleTMP = InfoScreen.transform.FindChild("Title Text")?.GetComponent<TextMeshPro>();
            if (TitleTMP == null)
            {
                Debug.LogError("Failed to find TitleTMP.");
                return;
            }
            TitleTMP.transform.localPosition = new(0f, 2.3f, 3f);
            TitleTMP.DestroyTranslator();
            TitleTMP.text = "";
        }

        if (InfoTMP == null)
        {
            InfoTMP = InfoScreen.transform.FindChild("InfoText_TMP")?.GetComponent<TextMeshPro>();
            if (InfoTMP == null)
            {
                Debug.LogError("Failed to find InfoTMP.");
                return;
            }
            InfoTMP.GetComponent<RectTransform>().sizeDelta = new(7f, 1.3f);
            InfoTMP.transform.localScale = new(1f, 1f, 1f);
            InfoTMP.DestroyTranslator();
            InfoTMP.text = "";
        }

        if (ActionButtonPrefab == null)
        {
            ActionButtonPrefab = InfoScreen.transform.FindChild("BackButton")?.GetComponent<PassiveButton>();
            if (ActionButtonPrefab == null)
            {
                Debug.LogError("Failed to find ActionButtonPrefab.");
                return;
            }
            ActionButtonPrefab.gameObject.name = "ActionButtonPrefab";
            ActionButtonPrefab.transform.localScale = new(0.66f, 0.66f, 0.66f);
            ActionButtonPrefab.transform.localPosition = new(0f, -0.65f, 3f);
            ActionButtonPrefab.transform.FindChild("Text_TMP")?.GetComponent<TextMeshPro>()?.DestroyTranslator();
            ActionButtonPrefab.gameObject.SetActive(false);
        }
    }
}