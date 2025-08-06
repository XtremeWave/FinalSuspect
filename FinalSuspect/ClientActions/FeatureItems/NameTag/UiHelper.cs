using System;
using FinalSuspect.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FinalSuspect.ClientActions.FeatureItems.NameTag;

public static class UiHelper
{
    private static Transform TempCB;

    public static GameObject CreateBaseWindow(string name, Transform parent, float zOffset, float scale)
    {
        var template = AccountManager.Instance.transform.Find("InfoTextBox").gameObject;
        var window = Object.Instantiate(template, parent);
        window.name = name;
        window.transform.localPosition += Vector3.forward * zOffset;

        var offset = GetResolutionOffset();
        var background = window.transform.Find("Background");
        if (background != null)
            background.localScale = background.localScale * scale * offset;

        // 移除不需要的按钮
        var button2 = window.transform.Find("Button2");
        if (button2 != null) Object.Destroy(button2.gameObject);

        // 设置预制体的名字
        var title = window.transform.Find("TitleText_TMP");
        if (title != null) title.gameObject.name = "Title Prefab";
        var info = window.transform.Find("InfoText_TMP");
        if (info != null) info.gameObject.name = "Info Prefab";
        var button1 = window.transform.Find("Button1");
        if (button1 != null) button1.gameObject.name = "Button Prefab";

        return window;
    }

    public static GameObject CreateCloseButton(Transform parent, Action onClick)
    {
        var template = parent.parent.Find("CloseButton");
        if (template != null)
            TempCB = template;
        template ??= TempCB;
        if (template == null) return null;

        var button = Object.Instantiate(template, parent).gameObject;
        button.name = "CloseButton";
        button.transform.localScale = Vector3.one;

        var passiveButton = button.GetComponent<PassiveButton>();
        if (passiveButton != null)
        {
            passiveButton.OnClick.RemoveAllListeners();
            passiveButton.OnClick.AddListener(new Action(() => onClick?.Invoke()));
        }

        return button;
    }

    public static GameObject CreateButton(GameObject template, Transform parent, Vector3 position, string text,
        float scale, bool getString = true)
    {
        var button = Object.Instantiate(template, parent);
        button.name = $"Button_{text}";

        button.transform.localPosition = position;
        button.transform.localScale = new Vector3(scale, scale, 1f);

        var passiveButton = button.GetComponent<PassiveButton>();
        if (passiveButton != null)
            passiveButton.OnClick = new Button.ButtonClickedEvent();

        var textComp = button.transform.Find("Text_TMP")?.GetComponent<TextMeshPro>();
        if (textComp == null) return button;
        if (getString)
            text = "NameTag." + text;
        textComp.text = getString ? GetString(text) : text;
        if (EnumHelper.GetAllNames<NameTagEditMenu.ComponentType>().Contains(text.Replace("NameTag.", "")) &&
            text.Replace("NameTag.", "") != "DisplayName")
        {
            textComp.text += $"({GetString("Disable")})";
            passiveButton.enabled = false;
        }

        return button;
    }

    public static GameObject CreateText(GameObject template, Transform parent, Vector3 position, string text,
        float fontSize)
    {
        var textObj = Object.Instantiate(template, parent);
        textObj.name = $"Text_{text.Substring(0, Mathf.Min(10, text.Length))}";

        textObj.transform.localPosition = position;

        var textComp = textObj.GetComponent<TextMeshPro>();
        if (textComp != null)
        {
            textComp.text = text;
            textComp.fontSize = fontSize;
        }

        return textObj;
    }

    public static GameObject CreateInputField(Transform parent, Vector3 position, bool allowSymbols)
    {
        var template = AccountManager.Instance.transform
            .Find("PremissionRequestWindow/GuardianEmailConfirm").gameObject;
        if (template == null) return null;

        var input = Object.Instantiate(template, parent);
        input.name = "InputField";

        input.transform.localPosition = position;
        input.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

        var textBox = input.GetComponent<TextBoxTMP>();
        if (textBox != null)
        {
            textBox.AllowPaste = true;
            textBox.allowAllCharacters = true;
            textBox.AllowEmail = true;
            textBox.AllowSymbols = allowSymbols;
        }

        var emailBehaviour = input.GetComponent<EmailTextBehaviour>();
        if (emailBehaviour != null)
            Object.Destroy(emailBehaviour);

        return input;
    }

    public static void HideTemplateObjects(Transform parent)
    {
        var templates = new[] { "Title Prefab", "Info Prefab", "Button Prefab", "Enter Box Prefab" };
        foreach (var name in templates)
        {
            var obj = parent.Find(name);
            if (obj != null) obj.gameObject.SetActive(false);
        }
    }
}