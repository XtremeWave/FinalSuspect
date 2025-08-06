using System;
using FinalSuspect.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static FinalSuspect.ClientActions.FeatureItems.NameTag.NameTagManager;
using Object = UnityEngine.Object;

namespace FinalSuspect.ClientActions.FeatureItems.NameTag;

public static class NameTagPanel
{
    private static int numItems;
    private static ToggleButtonBehaviour ButtonTemplate;
    public static SpriteRenderer CustomBackground { get; set; }
    public static List<GameObject> Items { get; private set; } = [];
    public static int CurrentPage { get; private set; } = 1;
    public static int ItemsPerPage => 8;

    public static int TotalPageCount =>
        (AllNameTags.Count(kv => !kv.Value.Isinternal) + ItemsPerPage - 1) / ItemsPerPage;

    public static void Hide()
    {
        CustomBackground?.gameObject.SetActive(false);
    }

    public static void Init(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        if (CustomBackground != null) return;

        var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
        CustomBackground = CreateBackground(optionsMenuBehaviour);
        ButtonTemplate = mouseMoveToggle;

        CreateCloseButton(mouseMoveToggle);
        CreateNewButton(mouseMoveToggle);
        CreateHelpText(optionsMenuBehaviour);
        CreatePageNavigationButtons(mouseMoveToggle);

        ReloadTag(null);
        RefreshTagList();
    }

    private static SpriteRenderer CreateBackground(OptionsMenuBehaviour options)
    {
        var bg = Object.Instantiate(options.Background, options.transform);
        bg.name = "Name Tag Panel Background";
        bg.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
        bg.transform.localPosition += Vector3.back * 18;
        bg.gameObject.SetActive(false);
        return bg;
    }

    private static void CreateCloseButton(ToggleButtonBehaviour template)
    {
        var closeButton = Object.Instantiate(template, CustomBackground.transform);
        closeButton.transform.localPosition = new Vector3(1.3f, -2.43f, -6f);
        closeButton.name = "Close";
        closeButton.Text.text = GetString("Close");
        closeButton.Background.color = Color.red;

        var closePassiveButton = closeButton.GetComponent<PassiveButton>();
        closePassiveButton.OnClick = new Button.ButtonClickedEvent();
        closePassiveButton.OnClick.AddListener(new Action(() => CustomBackground.gameObject.SetActive(false)));
    }

    private static void CreateNewButton(ToggleButtonBehaviour template)
    {
        var newButton = Object.Instantiate(template, CustomBackground.transform);
        newButton.transform.localPosition = new Vector3(1.3f, -1.88f, -6f);
        newButton.name = "New Tag";
        newButton.Text.text = GetString("NameTag.NewNameTag");
        newButton.Background.color = IsNotJoined ? Palette.White : Palette.DisabledGrey;

        var newPassiveButton = newButton.GetComponent<PassiveButton>();
        newPassiveButton.OnClick = new Button.ButtonClickedEvent();
        newPassiveButton.OnClick.AddListener(new Action(NameTagNewWindow.Open));
        newPassiveButton.enabled = IsNotJoined;
    }

    private static void CreateHelpText(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        var helpText = Object.Instantiate(
            optionsMenuBehaviour.DisableMouseMovement.Text,
            CustomBackground.transform
        );

        helpText.name = "Help Text";
        helpText.transform.localPosition = new Vector3(-1.25f, -2.15f, -5f);
        helpText.transform.localScale = new Vector3(1f, 1f, 1f);

        var helpTextTMP = helpText.GetComponent<TextMeshPro>();
        helpTextTMP.text = GetString("Tip.CustomNameTagHelp");
        helpText.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(2.45f, 1f);
    }

    private static void CreatePageNavigationButtons(ToggleButtonBehaviour template)
    {
        var prevButton = Object.Instantiate(template, CustomBackground.transform);
        prevButton.transform.localPosition = new Vector3(-1.3f, -1.33f, -6f);
        prevButton.name = "PreviousPageButton";
        prevButton.Text.text = GetString("PreviousPage");
        prevButton.Background.color = Color.white;

        var prevPassiveButton = prevButton.GetComponent<PassiveButton>();
        prevPassiveButton.OnClick = new Button.ButtonClickedEvent();
        prevPassiveButton.OnClick.AddListener(new Action(() =>
        {
            CurrentPage = CurrentPage - 1 <= 0 ? TotalPageCount : CurrentPage - 1;
            RefreshTagList();
        }));

        // 下一页按钮
        var nextButton = Object.Instantiate(template, CustomBackground.transform);
        nextButton.transform.localPosition = new Vector3(1.3f, -1.33f, -6f);
        nextButton.name = "NextPageButton";
        nextButton.Text.text = GetString("NextPage");
        nextButton.Background.color = Color.white;

        var nextPassiveButton = nextButton.GetComponent<PassiveButton>();
        nextPassiveButton.OnClick = new Button.ButtonClickedEvent();
        nextPassiveButton.OnClick.AddListener(new Action(() =>
        {
            CurrentPage = CurrentPage % TotalPageCount + 1;
            RefreshTagList();
        }));
    }

    public static void RefreshTagList()
    {
        try
        {
            Items?.Do(Object.Destroy);
            Items = [];
            numItems = 0;

            var startIndex = (CurrentPage - 1) * ItemsPerPage;
            var count = 0;

            foreach (var nameTag in AllNameTags)
            {
                if (nameTag.Value.Isinternal) continue;
                if (count++ < startIndex) continue;
                if (numItems >= ItemsPerPage) break;

                CreateTagItem(nameTag.Key, nameTag.Value);
                numItems++;
            }
        }
        catch (Exception ex)
        {
            Error($"RefreshTagList failed: {ex}", "NameTagPanel");
        }
    }

    private static void CreateTagItem(string key, NameTagManager.NameTag value)
    {
        if (ButtonTemplate == null || CustomBackground == null) return;

        var posY = 2.2f - 0.5f * numItems;

        var button = Object.Instantiate(ButtonTemplate, CustomBackground.transform);
        button.transform.localPosition = new Vector3(-1.3f, posY, -4f);
        button.name = "Btn-" + key;
        button.Text.text = key;
        button.Background.color =
            IsNotJoined
                ? ColorHelper.FSClientFeatureColor
                : ColorHelper.FSClientFeatureColor_CanNotUse;

        var passiveButton = button.GetComponent<PassiveButton>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener(new Action(() => NameTagEditMenu.Toggle(key, true)));
        passiveButton.enabled = IsNotJoined;

        var previewText = Object.Instantiate(
            button.Text.gameObject,
            CustomBackground.transform
        );

        previewText.name = "PreText-" + key;
        previewText.transform.localPosition = new Vector3(1.3f, posY, -6f);

        var previewTMP = previewText.GetComponent<TextMeshPro>();
        previewTMP.text = value.Apply(null, true).title ?? GetString("PreviewNotAvailable");
        previewTMP.fontSize = 1.2f;
        previewTMP.alignment = TextAlignmentOptions.Center;

        Items.Add(button.gameObject);
        Items.Add(previewText);
    }
}