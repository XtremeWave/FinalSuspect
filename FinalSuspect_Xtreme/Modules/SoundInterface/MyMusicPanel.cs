using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static FinalSuspect_Xtreme.Modules.Managers.AudioManager;
using static FinalSuspect_Xtreme.Translator;
using Object = UnityEngine.Object;

namespace FinalSuspect_Xtreme.Modules.SoundInterface;

public static class MyMusicPanel
{
    public static SpriteRenderer CustomBackground { get; private set; }
    public static List<GameObject> Items { get; private set; }
    public static OptionsMenuBehaviour OptionsMenuBehaviourNow { get; private set; }

    public static int currentPage { get; private set; } = 1;
    public static int itemsPerPage => 7;
    public static int totalPageCount => (AllMusics.Count + itemsPerPage - 1) / itemsPerPage;

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

            OptionsMenuBehaviourNow = optionsMenuBehaviour;
        if (CustomBackground == null)
        {
            currentPage = 1;
            numItems = 0;
            PlayMode = 0;
            CustomBackground = Object.Instantiate(optionsMenuBehaviour.Background, optionsMenuBehaviour.transform);
            CustomBackground.name = "Name Tag Panel Background";
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
            stopPassiveButton.OnClick.AddListener(new Action(CustomSoundsManager.StopPlay));

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
        Logger.Info($"cp:{currentPage}", "test");

        
        int startIndex = (currentPage - 1) * itemsPerPage; 

        int count = 0;
        foreach (var soundp in AllMusics.Skip(startIndex))
        {
            if (count >= itemsPerPage)
            {
                break; 
            }

            var sound = soundp.Key;
            var name = AllFinalSuspect.ContainsKey(sound) ? GetString($"Mus.{sound}") : sound;
            var path = @$"{Environment.CurrentDirectory.Replace(@"\", "/")}./FinalSuspect_Data/Sounds/{sound}.wav";
            RefreshTags(optionsMenuBehaviour, name, sound, path); 

            count++;
           
        }
        
    }
    public static void RefreshTags(OptionsMenuBehaviour optionsMenuBehaviour, string name, string sound, string path)
    {
       
        try
        {
            var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;

            // 计算标签在当前页面中的位置
            float offsetX = numItems % 2 == 0 ? -1.3f : 1.3f;
            float offsetY = 2.2f - (0.5f * (numItems / 2));
            float offsetZ = -4f;

            // 创建标签按钮
            var ToggleButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
            ToggleButton.transform.localPosition = new Vector3(offsetX, offsetY, offsetZ);
            numItems++; // 增加当前页面的标签数量

            ToggleButton.name = name;
            ToggleButton.Text.text = name;
            ToggleButton.Background.color = Color.white;
            var audioExist = ConvertExtension(ref path);
            var passiveButton = ToggleButton.GetComponent<PassiveButton>();
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener(new Action( () =>
            {
                Logger.Info($"Play {sound}:{path}", "SoundsPanel");
                if (ConvertExtension(ref path))
                {
                    CustomSoundsManager.Play(sound, 1);
                }
                
            }));

            ToggleButton.Background.color = audioExist ?
                (FinalSuspectMusic.ContainsKey(sound) ? Color.cyan : Color.green) :
                Palette.DisabledGrey;


            offsetX = numItems % 2 == 0 ? -1.3f : 1.3f;
            offsetY = 2.2f - (0.5f * (numItems / 2));
            offsetZ = -6f;

            var previewText = Object.Instantiate(optionsMenuBehaviour.DisableMouseMovement.Text, CustomBackground.transform);
            previewText.transform.localPosition = new Vector3(offsetX, offsetY, offsetZ);
            previewText.fontSize = ToggleButton.Text.fontSize;


            string preview = audioExist ? GetString("CanPlay") : GetString("NoFound");
            if (SoundManager.Instance != null && SoundManager.Instance.allSources != null)
            {
                foreach (var aso in SoundManager.Instance.allSources)
                {
                    if (aso.Key != null && AllSoundClips.Values.Any(ac => ac != null && ac.name == aso.Key.name &&ac.name == sound))
                    {
                        preview = GetString("Playing");
                        ToggleButton.Background.color = Main.OutColor;
                        break;
                    }
                }
            }
            previewText.text = preview;
            Items.Add(ToggleButton.gameObject);
            Items.Add(previewText.gameObject);
        }
        finally
        {
            numItems++;
        }

    }

}
