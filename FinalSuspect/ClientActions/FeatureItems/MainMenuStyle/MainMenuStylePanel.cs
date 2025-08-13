using System;
using FinalSuspect.ClientActions.FeatureItems.MyMusic;
using FinalSuspect.Helpers;
using FinalSuspect.Patches.System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static FinalSuspect.ClientActions.FeatureItems.MainMenuStyle.MainMenuStyleManager;
using Object = UnityEngine.Object;

namespace FinalSuspect.ClientActions.FeatureItems.MainMenuStyle;

public static class MainMenuStylePanel
{
    private static ToggleButtonBehaviour _applyButton;
    private static TextMeshPro _titleText;
    private static TextMeshPro _authorText;
    private static TextMeshPro _descriptionText;
    private static SpriteRenderer _previewImage;
    public static SpriteRenderer CustomBackground { get; set; }
    public static List<GameObject> Items { get; private set; } = [];
    private static int CurrentPage { get; set; } = Main.CurrentStyleId.Value + 1;
    private static int TotalPageCount => MainMenuStyles.Count;

    public static void Hide() => CustomBackground?.gameObject.SetActive(false);

    public static void Init(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        if (CustomBackground != null) return;

        var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
        CustomBackground = CreateBackground(optionsMenuBehaviour);

        CreateAuthorText(optionsMenuBehaviour);
        CreatePreviewImage();
        CreateTitleText(optionsMenuBehaviour);
        CreateCloseButton(mouseMoveToggle);
        CreateApplyButton(mouseMoveToggle);
        CreateHelpText(optionsMenuBehaviour);
        CreateDescriptionText(optionsMenuBehaviour);
        CreatePageNavigationButtons(mouseMoveToggle);
        var currentBackground = MainMenuStyles[Main.CurrentStyleId.Value];
        currentBackground.CurrentState = CurrentState.Applied;
        Refresh(currentBackground);
    }

    private static SpriteRenderer CreateBackground(OptionsMenuBehaviour options)
    {
        var bg = Object.Instantiate(options.Background, options.transform);
        bg.name = "Main Menu Style Panel Background";
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

    private static void CreateApplyButton(ToggleButtonBehaviour template)
    {
        _applyButton = Object.Instantiate(template, CustomBackground.transform);
        _applyButton.transform.localPosition = new Vector3(1.3f, -1.88f, -6f);
        _applyButton.name = "ApplyButton";
        _applyButton.Text.text = GetString("MainMenuStyle.Apply");
        _applyButton.Background.color = IsNotJoined ? Palette.White : Palette.DisabledGrey;

        var button = _applyButton.GetComponent<PassiveButton>();
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener(new Action(() =>
        {
            var id = CurrentPage - 1;
            Main.CurrentStyleId.Value = id;
            var style = MainMenuStyles[id];
            MainMenuStyles.Where(x => x.Applied).Do(x => x.CurrentState = CurrentState.NotApply);
            style.CurrentState = CurrentState.Applied;
            Refresh(style);
            var sr = ModMainMenuManager.FinalSuspect_Background.GetComponent<SpriteRenderer>();

            sr.sprite = style.Sprite;
            if (id == 3)
            {
                var rd = HashRandom.Next(0, 100);
                if (rd < 5)
                    sr.sprite = LoadSprite("FinalSuspect-BG-MiraStudioNewYear.png", 179f);
            }

            ModMainMenuManager.Starfield.SetActive(style.StarFieldActive);
            var starGen = ModMainMenuManager.Starfield.GetComponent<StarGen>();
            starGen.SetDirection(new Vector2(0, style.StarGenDire));

            var instance = DestroyableSingleton<MainMenuManager>.Instance;
            Color shade = new(0f, 0f, 0f, 0f);
            var standardActiveSprite = instance.newsButton.activeSprites.GetComponent<SpriteRenderer>().sprite;
            var minorActiveSprite = instance.quitButton.activeSprites.GetComponent<SpriteRenderer>().sprite;
            AwakeFriendCodeUIPatch.Prefix();
            var friendsButton = ModMainMenuManager.FriendsButton.GetComponent<PassiveButton>();
            Dictionary<List<PassiveButton>, (Sprite, Color, Color, Color, Color)> mainButtons = new()
            {
                {
                    [
                        instance.playButton,
                        instance.inventoryButton,
                        instance.shopButton
                    ],
                    (standardActiveSprite, style.MainUIColors[0], shade, Color.white, Color.white)
                },
                {
                    [
                        instance.newsButton,
                        instance.myAccountButton,
                        instance.settingsButton
                    ],
                    (minorActiveSprite, style.MainUIColors[1], shade, Color.white, Color.white)
                },
                {
                    [
                        instance.creditsButton,
                        instance.quitButton,
                        ModMainMenuManager.InviteButton.GetComponent<PassiveButton>(),
                        ModMainMenuManager.GithubButton.GetComponent<PassiveButton>()
                    ],
                    (minorActiveSprite, style.MainUIColors[2], shade, Color.white, Color.white)
                },
                {
                    [friendsButton],
                    (minorActiveSprite, style.MainUIColors[3], shade, Color.white, Color.white)
                }
            };
            foreach (var kvp in mainButtons)
                kvp.Key.Do(passiveButton =>
                {
                    FormatButtonColor(instance, passiveButton, kvp.Value.Item2, kvp.Value.Item3, kvp.Value.Item4,
                        kvp.Value.Item5);
                });

            if (ModMainMenuManager.BackgroundTexture)
                ModMainMenuManager.BackgroundTexture.GetComponent<SpriteRenderer>().color =
                    style.MainUIColors[0].SetAlpha(1);

            var lastAudio = FinalMusic.Musics.FirstOrDefault(x => x.PlayAsMainMenuMusic);
            var audio = FinalMusic.Musics.FirstOrDefault(x => x.CurrentAudio == style.MainMenuMusic);

            if (lastAudio == null || audio == null) return;
            if (lastAudio != audio)
                SoundManager.Instance.StopAllSound();
            AudioPlayer.Play(audio, true);
        }));
        button.enabled = IsNotJoined;
    }

    private static void CreateHelpText(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        var helpText = Object.Instantiate(
            optionsMenuBehaviour.DisableMouseMovement.Text,
            CustomBackground.transform
        );
        helpText.name = "HelpText";
        helpText.transform.localPosition = new Vector3(-1.25f, -2.15f, -5f);
        helpText.transform.localScale = Vector3.one;

        var tmp = helpText.GetComponent<TextMeshPro>();
        tmp.text = GetString("Tip.MainMenuStyleHelp");
        helpText.GetComponent<RectTransform>().sizeDelta = new Vector2(2.45f, 1f);
    }

    private static void CreateTitleText(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        _titleText = Object.Instantiate(
            optionsMenuBehaviour.DisableMouseMovement.Text,
            CustomBackground.transform
        );
        _titleText.name = "TitleText";
        _titleText.transform.localPosition = new Vector3(-0.05f, 2.55f, -5f);
        _titleText.transform.localScale = Vector3.one;

        var tmp = _titleText.GetComponent<TextMeshPro>();
        tmp.text = "Title";
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.fontStyle = FontStyles.Bold;
        //tmp.autoSizeTextContainer = true;
        _titleText.GetComponent<RectTransform>().sizeDelta = new Vector2(5f, 0.3f);
    }

    private static void CreateAuthorText(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        _authorText = Object.Instantiate(
            optionsMenuBehaviour.DisableMouseMovement.Text,
            CustomBackground.transform
        );
        _authorText.name = "AuthorText";
        _authorText.transform.localPosition = new Vector3(0.75f, -1.25f, -5f);
        _authorText.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

        var tmp = _authorText.GetComponent<TextMeshPro>();
        tmp.text = "Author";
        tmp.alignment = TextAlignmentOptions.BottomRight;
        tmp.fontStyle = FontStyles.Bold;
        _authorText.GetComponent<RectTransform>().sizeDelta = new Vector2(4f, 1f);
    }

    private static void CreateDescriptionText(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        _descriptionText = Object.Instantiate(
            optionsMenuBehaviour.DisableMouseMovement.Text,
            CustomBackground.transform
        );
        _descriptionText.name = "DescriptionText";
        _descriptionText.transform.localPosition = new Vector3(-0.05f, -0.2f, -5f);
        _descriptionText.transform.localScale = Vector3.one;

        var tmp = _descriptionText.GetComponent<TextMeshPro>();
        tmp.text =
            "111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111";
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.fontStyle = FontStyles.Italic;
        tmp.fontSizeMin = tmp.fontSizeMax = tmp.fontSize = 1.25f;

        _descriptionText.GetComponent<RectTransform>().sizeDelta = new Vector2(4.2f, 0.2f);
    }

    private static void CreatePreviewImage()
    {
        _previewImage = ObjectHelper.CreateSpriteRenderer(
            "FinalSuspect-BG-Preview",
            "FinalSuspect-BG-MiraHQ.png",
            450f,
            new Vector3(0, 1.1f, -5f),
            CustomBackground.transform
        );
        _previewImage.color = Color.white;
        _previewImage.gameObject.layer = 5;
        _previewImage.transform.localScale = Vector3.one;
    }

    private static void CreatePageNavigationButtons(ToggleButtonBehaviour template)
    {
        var prevButton = Object.Instantiate(template, CustomBackground.transform);
        prevButton.transform.localPosition = new Vector3(-2.4f, 1.1f, -6f);
        prevButton.name = "PreviousPageButton";
        prevButton.Text.text = "←";
        prevButton.Background.color = Color.white;
        prevButton.Background.size = new Vector2(0.4f, 0.4f);
        prevButton.transform.FindChild("ButtonHighlight").gameObject.GetComponent<SpriteRenderer>().size =
            new Vector2(0.55f, 0.55f);
        prevButton.GetComponent<BoxCollider2D>().size = new Vector2(0.39f, 0.39f);

        var prevPassiveButton = prevButton.GetComponent<PassiveButton>();
        prevPassiveButton.OnClick = new Button.ButtonClickedEvent();
        prevPassiveButton.OnClick.AddListener(new Action(() =>
        {
            CurrentPage = CurrentPage - 1 <= 0 ? TotalPageCount : CurrentPage - 1;
            Refresh(MainMenuStyles[CurrentPage - 1]);
        }));

        // 下一页按钮
        var nextButton = Object.Instantiate(template, CustomBackground.transform);
        nextButton.transform.localPosition = new Vector3(2.4f, 1.1f, -6f);
        nextButton.name = "NextPageButton";
        nextButton.Text.text = "→";
        nextButton.Background.color = Color.white;
        nextButton.Background.size = new Vector2(0.4f, 0.4f);
        nextButton.transform.FindChild("ButtonHighlight").gameObject.GetComponent<SpriteRenderer>().size =
            new Vector2(0.55f, 0.55f);
        nextButton.GetComponent<BoxCollider2D>().size = new Vector2(0.39f, 0.39f);

        var nextPassiveButton = nextButton.GetComponent<PassiveButton>();
        nextPassiveButton.OnClick = new Button.ButtonClickedEvent();
        nextPassiveButton.OnClick.AddListener(new Action(() =>
        {
            CurrentPage = CurrentPage % TotalPageCount + 1;
            Refresh(MainMenuStyles[CurrentPage - 1]);
        }));
    }

    private static void Refresh(MainMenuStyleManager.MainMenuStyle style)
    {
        _titleText.text = style.Title;
        _authorText.text = $"{GetString("Author")}:{style.Author}";
        _descriptionText.text = style.Description;
        _previewImage.sprite = style.PreviewSprite;
        _applyButton.Background.color = style.CurrentState switch
        {
            CurrentState.NotFound => _applyButton.Text.color = Palette.DisabledGrey,
            CurrentState.NotApply => _applyButton.Text.color = ColorHelper.FSClientFeatureColor,
            CurrentState.Applied => _applyButton.Text.color = ColorHelper.FSColor,
            _ => _applyButton.Background.color
        };
        _applyButton.GetComponent<PassiveButton>().enabled = style.CurrentState != CurrentState.NotFound;
        _applyButton.Text.text = GetString($"MainMenuStyle.{style.CurrentState}");
    }
}