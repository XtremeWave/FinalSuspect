using System;
using System.Collections.Generic;
using System.Linq;
using FinalSuspect.Helpers;
using TMPro;
using UnityEngine;
using static FinalSuspect.Modules.SoundInterface.SoundManager;
using static FinalSuspect.Modules.SoundInterface.XtremeMusic;
using Object = UnityEngine.Object;
using static FinalSuspect.Modules.SoundInterface.CustomSoundsManager;


namespace FinalSuspect.Modules.SoundInterface;

public static class MyMusicPanel
{
    public static SpriteRenderer CustomBackground { get; set; }
    public static List<GameObject> Items { get; private set; }
    public static OptionsMenuBehaviour OptionsMenuBehaviourNow { get; private set; }

    public static int currentPage { get; private set; } = 1;
    public static int itemsPerPage => 7;
    public static int totalPageCount => (musics.Count + itemsPerPage - 1) / itemsPerPage;

    private static int numItems;
    public static int PlayMode;
    public static ToggleButtonBehaviour ChangePlayMode { get; private set; }
    public static void Hide()
    {
        if (CustomBackground != null)
            CustomBackground?.gameObject?.SetActive(false);
    }
    public static void Init(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
            OptionsMenuBehaviourNow = optionsMenuBehaviour;
        if (CustomBackground == null)
        {
            currentPage = 1;
            numItems = 0;
            PlayMode = 0;
            CustomBackground = Object.Instantiate(optionsMenuBehaviour.Background, optionsMenuBehaviour.transform);
            CustomBackground.name = "My Music Panel Background";
            CustomBackground.transform.localScale = new(0.9f, 0.9f, 1f);
            CustomBackground.transform.localPosition += Vector3.back * 18;
            CustomBackground.gameObject.SetActive(false);

            var closeButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
            closeButton.transform.localPosition = new(1.3f, -2.43f, -16f);
            closeButton.name = "Close";
            closeButton.Text.text = GetString("Close");
            closeButton.Background.color = Color.red;
            var closePassiveButton = closeButton.GetComponent<PassiveButton>();
            closePassiveButton.OnClick = new();
            closePassiveButton.OnClick.AddListener(new Action(() =>
            {
                
                CustomBackground.gameObject.SetActive(false);
            }));

            var stopButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
            stopButton.transform.localPosition = new(1.3f, -1.88f, -16f);
            var stopButtonScale = stopButton.transform.localScale;
            stopButton.transform.localScale = stopButtonScale;
            stopButton.name = "stopButton";
            stopButton.Text.text = GetString("Stop");
            stopButton.Background.color = Color.white;

            var stopPassiveButton = stopButton.GetComponent<PassiveButton>();
            stopPassiveButton.OnClick = new();
            stopPassiveButton.OnClick.AddListener(new Action(StopPlayMod));

            AddPageNavigationButton(optionsMenuBehaviour);

            var helpText = Object.Instantiate(optionsMenuBehaviour.DisableMouseMovement.Text, CustomBackground.transform);
            helpText.name = "Help Text";
            helpText.transform.localPosition = new(-1.25f, -2.15f, -15f);
            helpText.transform.localScale = new(1f, 1f, 1f);
            var helpTextTMP = helpText.GetComponent<TextMeshPro>();
            helpTextTMP.text = GetString("CustomSoundHelp");
            helpText.gameObject.GetComponent<RectTransform>().sizeDelta = new(2.45f, 1f);

            AddChangePlayModeButton(optionsMenuBehaviour);
        }

        ReloadTag();
        RefreshTagList();
    }
    static void AddPageNavigationButton(OptionsMenuBehaviour optionsMenuBehaviour)
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
        nextPagePassiveButton.OnClick = new();
        nextPagePassiveButton.OnClick.AddListener(new Action(() =>
        {
            
            currentPage++;

            if (currentPage > totalPageCount)
            {
                currentPage = 1;
            }

            RefreshTagList() ;
        }));
    }
    static void AddChangePlayModeButton(OptionsMenuBehaviour optionsMenuBehaviour)
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
    }
    public static void RefreshTagList()
    {
        Items?.Do(Object.Destroy);
        Items = [];
        numItems = 0;
        var optionsMenuBehaviour = OptionsMenuBehaviourNow;
        var startIndex = (currentPage - 1) * itemsPerPage;

        var count = 0;
        foreach (var audio in musics.Skip(startIndex))
        {
            if (count >= itemsPerPage)
            {
                break; 
            }

            RefreshTags(optionsMenuBehaviour, audio); 

            count++;
           
        }
        
    }
    public static void RefreshTags(OptionsMenuBehaviour optionsMenuBehaviour, XtremeMusic audio)
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
            ToggleButton.Text.text = $"{name}{(author != string.Empty ? $" -{author}" : "")}";
            ToggleButton.Background.color = Color.white;
            numItems++; 

            offsetX = numItems % 2 == 0 ? -1.3f : 1.3f;
            offsetY = 2.2f - 0.5f * (numItems / 2);
            offsetZ = -6f;

            var previewText = Object.Instantiate(optionsMenuBehaviour.DisableMouseMovement.Text, CustomBackground.transform);
            previewText.transform.localPosition = new Vector3(offsetX, offsetY, offsetZ);
            previewText.fontSize = ToggleButton.Text.fontSize;
            previewText.name = "PreText-" + filename;

            Color color;
            string preview;
            var enable = false;

            switch (audio.CurrectAudioStates)
            {
                
                case AudiosStates.IsPlaying:
                    preview = GetString("Playing");
                    color = ColorHelper.ModColor32;
                    break;
                case AudiosStates.IsDownLoading:
                    color = ColorHelper.DownloadYellow;
                    preview = GetString("Downloading");
                    break;
                case AudiosStates.ReadyLoading:
                    color = ColorHelper.ClientFeatureColor_CanNotUse;
                    preview = GetString("ReadyToLoad");
                    break;
                case AudiosStates.IsLoading:
                    color = ColorHelper.ClientOptionColor;
                    preview = GetString("LoadingMus");
                    break;

                case AudiosStates.DownLoadSucceedNotice:
                case AudiosStates.Exist:
                    color = audio.UnOfficial ? Color.green : ColorHelper.ClientFeatureColor;
                    preview = GetString("CanPlay");
                    enable = true;
                    break;
                case AudiosStates.NotExist:
                case AudiosStates.DownLoadFailureNotice:
                default:
                {
                    
                    color = ColorHelper.ClientFeatureColor_CanNotUse;
                    preview = GetString("NoFound");
                    break;
                }
            }
            

            previewText.text = preview;
            ToggleButton.Background.color = color;
            ToggleButton.GetComponent<PassiveButton>().enabled = enable;

            var passiveButton = ToggleButton.GetComponent<PassiveButton>();
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener(new Action(OnClick));
            void OnClick()
            {
                XtremeLogger.Info($"Try To Play {filename}:{path}", "MyMusicPanel");
                Play(audio);
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
