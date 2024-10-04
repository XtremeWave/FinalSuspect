using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static FinalSuspect_Xtreme.Modules.Managers.AudioManager;
using static FinalSuspect_Xtreme.Modules.Managers.FinalMusic;
using static FinalSuspect_Xtreme.Translator;
using Object = UnityEngine.Object;
using static FinalSuspect_Xtreme.Modules.Managers.CustomSoundsManager;
using FinalSuspect_Xtreme.Modules.Managers;
using UnityEngine.UIElements;


namespace FinalSuspect_Xtreme.Modules.SoundInterface;

public static class MyMusicPanel
{
    static bool first;
    public static SpriteRenderer CustomBackground { get; private set; }
    public static List<GameObject> Items { get; private set; }
    public static OptionsMenuBehaviour OptionsMenuBehaviourNow { get; private set; }

    public static int currentPage { get; private set; } = 1;
    public static int itemsPerPage => 7;
    public static int totalPageCount => (finalMusics.Count + itemsPerPage - 1) / itemsPerPage;

    private static int numItems = 0;
    public static int PlayMode = 0;
    public static ToggleButtonBehaviour ChangePlayMode { get; private set; }
    public static void Hide()
    {
        if (CustomBackground != null)
            CustomBackground?.gameObject?.SetActive(false);
    }
    public static void Init(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
        first = true;
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
            closeButton.Background.color = Palette.DisabledGrey;
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
            stopButton.Background.color = Palette.DisabledGrey;

            var stopPassiveButton = stopButton.GetComponent<PassiveButton>();
            stopPassiveButton.OnClick = new();
            stopPassiveButton.OnClick.AddListener(new Action(StopPlay));

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

        ReloadTag(null);
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
        nextPageButton.Background.color = Palette.DisabledGrey;

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
        Items = new();
        numItems = 0;
        var optionsMenuBehaviour = OptionsMenuBehaviourNow;
        Logger.Info($"currentPage:{currentPage}", "MyMusicPanel");

        
        int startIndex = (currentPage - 1) * itemsPerPage;

        int count = 0;
        foreach (var audio in finalMusics.Skip(startIndex))
        {
            if (count >= itemsPerPage)
            {
                break; 
            }

            RefreshTags(optionsMenuBehaviour, audio); 

            count++;
           
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
            var audioExist = audio.CurrectAudioStates is not AudiosStates.NotExist || CustomAudios.Contains(filename);


            float offsetX = numItems % 2 == 0 ? -1.3f : 1.3f;
            float offsetY = 2.2f - (0.5f * (numItems / 2));
            float offsetZ = -4f;

            var ToggleButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
            ToggleButton.transform.localPosition = new Vector3(offsetX, offsetY, offsetZ);
            ToggleButton.name = "Btn-" + filename;
            ToggleButton.Text.text = $"{name}{(author != string.Empty ? $" -{author}" : "")}";
            ToggleButton.Background.color = Color.white;
            numItems++; 

            offsetX = numItems % 2 == 0 ? -1.3f : 1.3f;
            offsetY = 2.2f - (0.5f * (numItems / 2));
            offsetZ = -6f;

            var previewText = Object.Instantiate(optionsMenuBehaviour.DisableMouseMovement.Text, CustomBackground.transform);
            previewText.transform.localPosition = new Vector3(offsetX, offsetY, offsetZ);
            previewText.fontSize = ToggleButton.Text.fontSize;
            previewText.name = "PreText-" + filename;

            Color color;
            string preview;

            if (audio.CurrectAudioStates is AudiosStates.IsPlaying)
            {
                preview = GetString("Playing");
                color = ColorHelper.OutColor;
            }
            else if (audioExist)
            {
                color = audio.UnOfficial ? Color.green : Color.cyan;
                preview = GetString("CanPlay");
            }
            else
            {
                color = Palette.DisabledGrey;
                preview = GetString("NoFound");
            }
            

            previewText.text = preview;
            ToggleButton.Background.color = color;


            var passiveButton = ToggleButton.GetComponent<PassiveButton>();
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener(new Action(OnClick));
            void OnClick()
            {
                Logger.Info($"Try To Play {filename}:{path}", "MyMusicPanel");
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
