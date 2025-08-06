using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace FinalSuspect.ClientActions.FeatureItems.NameTag;

public static class NameTagNewWindow
{
    private static readonly Regex FriendCodeRegex = new("^[a-z]+#[0-9]{4}$", RegexOptions.Compiled);

    public static GameObject Window { get; private set; }
    public static GameObject Info { get; private set; }
    public static GameObject EnterBox { get; private set; }
    public static GameObject ConfirmButton { get; private set; }

    public static void Open()
    {
        try
        {
            if (NameTagEditMenu.Menu?.activeSelf ?? false) return;
        }
        catch
        {
            /* ignored */
        }

        if (Window == null) Init();
        Window?.SetActive(true);
        EnterBox?.GetComponent<TextBoxTMP>()?.Clear();
    }

    public static void Init()
    {
        Window = UiHelper.CreateBaseWindow(
            "New Name Tag Window",
            NameTagPanel.CustomBackground.transform.parent,
            -10,
            0.7f
        );

        CreateCloseButton();
        CreateInfoText();
        CreateInputField();
        CreateConfirmButton();

        UiHelper.HideTemplateObjects(Window.transform);
    }

    private static void CreateCloseButton()
    {
        var closeButton = UiHelper.CreateCloseButton(Window.transform, () => Window.SetActive(false));
        closeButton.transform.localPosition = new Vector3(2.4f, 1.2f, -1f) * GetResolutionOffset();
    }

    private static void CreateInfoText()
    {
        Info = UiHelper.CreateText(
            Window.transform.Find("Info Prefab").gameObject,
            Window.transform,
            new Vector3(0f, 0.1f, 0f) * GetResolutionOffset(),
            GetString("Tip.PleaseEnterFriendCode"),
            1f
        );
        Info.name = "Enter Friend Code Description";
    }

    private static void CreateInputField()
    {
        EnterBox = UiHelper.CreateInputField(
            Window.transform,
            new Vector3(0f, -0.04f, 0f) * GetResolutionOffset(),
            true
        );
        EnterBox.name = "Enter Friend Code Box";

        var enterBoxTBT = EnterBox.GetComponent<TextBoxTMP>();
        enterBoxTBT.AllowEmail = false;
        enterBoxTBT.AllowSymbols = true;
        enterBoxTBT.AllowPaste = true;
    }

    private static void CreateConfirmButton()
    {
        ConfirmButton = UiHelper.CreateButton(
            Window.transform.Find("Button Prefab").gameObject,
            Window.transform,
            new Vector3(0, -0.8f, 0f) * GetResolutionOffset(),
            GetString(StringNames.Confirm),
            GetResolutionOffset(),
            false
        );
        ConfirmButton.name = "Confirm Button";

        ConfirmButton.GetComponent<PassiveButton>().OnClick.AddListener((Action)OnConfirmClicked);
    }

    private static void OnConfirmClicked()
    {
        var input = EnterBox.GetComponent<TextBoxTMP>().text;
        var code = NormalizeFriendCode(input);
        var infoTmp = Info.GetComponent<TextMeshPro>();

        if (!FriendCodeRegex.IsMatch(code))
        {
            ShowError(infoTmp, GetString("Tip.FriendCodeIncorrect"));
            return;
        }

        if (NameTagManager.AllNameTags.ContainsKey(code))
        {
            ShowError(infoTmp, GetString("Tip.FriendCodeAlreadyExist"));
            return;
        }

        Window.SetActive(false);
        NameTagEditMenu.Toggle(code, true);
    }

    private static string NormalizeFriendCode(string input)
    {
        return input.ToLower()
            .Replace("-", "#")
            .Replace("—", "#")
            .Replace(" ", string.Empty)
            .Trim();
    }

    private static void ShowError(TextMeshPro text, string message)
    {
        ConfirmButton.SetActive(false);
        text.text = message;
        text.color = message.Contains("Incorrect") ? Color.red : Color.blue;

        _ = new LateTask(() =>
        {
            text.text = GetString("Tip.PleaseEnterFriendCode");
            text.color = Color.white;
            ConfirmButton.SetActive(true);
        }, 1.2f, "Reactivate Enter Box");
    }
}