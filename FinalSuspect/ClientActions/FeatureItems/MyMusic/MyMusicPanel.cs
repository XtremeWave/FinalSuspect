using System;
using System.Diagnostics.CodeAnalysis;
using FinalSuspect.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FinalSuspect.ClientActions.FeatureItems.MyMusic;

[SuppressMessage("ReSharper", "PossibleLossOfFraction")]
public static class MyMusicPanel
{
    private static int numItems;

    public static int PlayMode;
    public static SpriteRenderer CustomBackground { get; set; }
    public static List<GameObject> Items { get; private set; }
    public static OptionsMenuBehaviour OptionsMenuBehaviourNow { get; private set; }

    public static int CurrentPage { get; private set; } = 1;
    public static int ItemsPerPage => 7;
    public static int TotalPageCount => (FinalMusic.musics.Count + ItemsPerPage - 1) / ItemsPerPage;

    //public static ToggleButtonBehaviour ChangePlayMode { get; private set; }
    public static void Hide()
    {
        if (CustomBackground)
            CustomBackground?.gameObject.SetActive(false);
    }

    public static void Init(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
        OptionsMenuBehaviourNow = optionsMenuBehaviour;
        if (!CustomBackground)
        {
            CurrentPage = 1;
            numItems = 0;
            PlayMode = 0;
            CustomBackground = Object.Instantiate(optionsMenuBehaviour.Background, optionsMenuBehaviour.transform);
            CustomBackground.name = "My Music Panel Background";
            CustomBackground.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            CustomBackground.transform.localPosition += Vector3.back * 18;
            CustomBackground.gameObject.SetActive(false);

            var closeButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
            closeButton.transform.localPosition = new Vector3(1.3f, -2.43f, -16f);
            closeButton.name = "Close";
            closeButton.Text.text = GetString("Close");
            closeButton.Background.color = Color.red;
            var closePassiveButton = closeButton.GetComponent<PassiveButton>();
            closePassiveButton.OnClick = new Button.ButtonClickedEvent();
            closePassiveButton.OnClick.AddListener(new Action(() => { CustomBackground.gameObject.SetActive(false); }));

            var stopButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
            stopButton.transform.localPosition = new Vector3(1.3f, -1.88f, -16f);
            var stopButtonScale = stopButton.transform.localScale;
            stopButton.transform.localScale = stopButtonScale;
            stopButton.name = "stopButton";
            stopButton.Text.text = GetString("MusPlay.Stop");
            stopButton.Background.color = Color.white;

            var stopPassiveButton = stopButton.GetComponent<PassiveButton>();
            stopPassiveButton.OnClick = new Button.ButtonClickedEvent();
            stopPassiveButton.OnClick.AddListener(new Action(() => AudioPlayer.StopPlayMod()));

            AddPageNavigationButton(optionsMenuBehaviour);

            var helpText =
                Object.Instantiate(optionsMenuBehaviour.DisableMouseMovement.Text, CustomBackground.transform);
            helpText.name = "Help Text";
            helpText.transform.localPosition = new Vector3(-1.25f, -2.15f, -15f);
            helpText.transform.localScale = new Vector3(1f, 1f, 1f);
            var helpTextTMP = helpText.GetComponent<TextMeshPro>();
            helpTextTMP.text = GetString("Tip.MyMusic");
            helpText.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(2.45f, 1f);

            //AddChangePlayModeButton(optionsMenuBehaviour);
        }

        RefreshTagList();
    }

    private static void AddPageNavigationButton(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
        var nextPageButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
        nextPageButton.transform.localPosition = new Vector3(1.3f, -1.33f, -16f);
        var nextPageButtonScale = nextPageButton.transform.localScale;
        nextPageButton.transform.localScale = nextPageButtonScale;
        nextPageButton.name = "NextPageButton";
        nextPageButton.Text.text = GetString("NextPage");
        nextPageButton.Background.color = Color.white;

        var nextPagePassiveButton = nextPageButton.GetComponent<PassiveButton>();
        nextPagePassiveButton.OnClick = new Button.ButtonClickedEvent();
        nextPagePassiveButton.OnClick.AddListener(new Action(() =>
        {
            CurrentPage++;

            if (CurrentPage > TotalPageCount)
            {
                CurrentPage = 1;
            }

            RefreshTagList();
        }));
    }

    /*static void AddChangePlayModeButton(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        //var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
        //ChangePlayMode = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
        //ChangePlayMode.transform.localPosition = new Vector3(-1.3f, -1.33f, -16f);
        //ChangePlayMode.name = "ChangePlayMode";
        //ChangePlayMode.Text.text = GetString($"PlayMode{PlayMode}");
        //ChangePlayMode.Background.color = Palette.DisabledGrey;

        //var nextPagePassiveButton = ChangePlayMode.GetComponent<PassiveButton>();
        //nextPagePassiveButton.OnClick = new();
        //nextPagePassiveButton.OnClick.AddListener(new Action(() =>
        //{

        //    PlayMode++;
        //    if (PlayMode > 3)
        //    {
        //        PlayMode = 0;
        //    }

        //    Object.Destroy(ChangePlayMode.gameObject);
        //    AddChangePlayModeButton(OptionsMenuBehaviourNow);
        //}));
    }*/
    public static void RefreshTagList()
    {
        try
        {
            Items?.Do(Object.Destroy);
            Items = [];
            numItems = 0;
            var optionsMenuBehaviour = OptionsMenuBehaviourNow;
            var startIndex = (CurrentPage - 1) * ItemsPerPage;

            var count = 0;
            foreach (var audio in FinalMusic.musics.Skip(startIndex))
            {
                if (count >= ItemsPerPage)
                {
                    break;
                }

                RefreshTags(optionsMenuBehaviour, audio);
                count++;
            }
        }
        catch
        {
            /* ignored */
        }
    }

    public static void RefreshTags(OptionsMenuBehaviour optionsMenuBehaviour, FinalMusic audio)
    {
        try
        {
            var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
            var name = audio.Name;
            var path = audio.Path;
            var filename = audio.FileName;
            var author = audio.Author;

            var offsetX = numItems % 2 == 0 ? -1.3f : 1.3f;
            var offsetY = 2.2f - 0.5f * (numItems / 2);
            var offsetZ = -4f;

            var ToggleButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
            ToggleButton.transform.localPosition = new Vector3(offsetX, offsetY, offsetZ);
            ToggleButton.name = "Btn-" + filename;
            ToggleButton.Background.color = Color.white;
            numItems++;

            offsetX = numItems % 2 == 0 ? -1.3f : 1.3f;
            offsetY = 2.2f - 0.5f * (numItems / 2);
            offsetZ = -6f;

            var previewText =
                Object.Instantiate(optionsMenuBehaviour.DisableMouseMovement.Text, CustomBackground.transform);
            previewText.transform.localPosition = new Vector3(offsetX, offsetY, offsetZ);
            previewText.fontSize = ToggleButton.Text.fontSize;
            previewText.name = "PreText-" + filename;

            Color color;
            string preview;
            var enable = false;

            switch (audio.CurrentAudioStates)
            {
                case AudiosStates.IsPlaying:
                    preview = GetString("Tip.Playing");
                    color = ColorHelper.FSColor;
                    break;
                case AudiosStates.IsDownLoading:
                    color = ColorHelper.DownloadYellow;
                    preview = GetString("Tip.Downloading");
                    break;
                case AudiosStates.IsLoading:
                    color = ColorHelper.FSClientOptionColor;
                    preview = GetString("Tip.Parsing");
                    break;
                case AudiosStates.DownLoadSucceedNotice:
                case AudiosStates.Exist:
                    color = audio.UnOfficial ? Color.green : ColorHelper.FSClientFeatureColor;
                    preview = GetString("MusPlay.CanPlay");
                    enable = true;
                    break;
                case AudiosStates.NotExist:
                case AudiosStates.DownLoadFailureNotice:
                default:
                {
                    color = ColorHelper.FSClientFeatureColor_CanNotUse;
                    preview = GetString("MusPlay.NoFound");
                    break;
                }
            }

            previewText.text = $"{name}{(author != string.Empty ? $" -{author}" : "")}";
            ToggleButton.Background.color = color;
            ToggleButton.GetComponent<PassiveButton>().enabled = enable;
            ToggleButton.Text.text = preview;

            var passiveButton = ToggleButton.GetComponent<PassiveButton>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener(new Action(OnClick));

            void OnClick()
            {
                Info($"Try To Play {filename}:{path}", "MyMusicPanel");
                AudioPlayer.Play(audio);
            }

            Items.Add(ToggleButton.gameObject);
            Items.Add(previewText.gameObject);
        }
        finally
        {
            numItems++;
        }
    }
}