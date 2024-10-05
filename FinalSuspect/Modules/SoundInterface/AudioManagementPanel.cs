﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static FinalSuspect.Modules.Managers.AudioManager;
using static FinalSuspect.Modules.Managers.FinalMusic;
using FinalSuspect.Modules.Managers;
using static FinalSuspect.Translator;
using Object = UnityEngine.Object;
using Il2CppSystem.IO;
using AmongUs.HTTP;
using FinalSuspect.Modules.Managers.ResourcesManager;

namespace FinalSuspect.Modules.SoundInterface;

public static class AudioManagementPanel
{
    public static SpriteRenderer CustomBackground { get; private set; }
    public static GameObject Slider { get; private set; }
    public static Dictionary<string, GameObject> Items { get; private set; }

    static int numItems = 0;
    public static void Hide()
    {
        if (CustomBackground != null)
            CustomBackground?.gameObject?.SetActive(false);
    }
    public static void Init(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
        if (!XtremeGameData.GameStates.IsNotJoined) return;

        if (CustomBackground == null)
        {
            numItems = 0;
            CustomBackground = Object.Instantiate(optionsMenuBehaviour.Background, optionsMenuBehaviour.transform);
            CustomBackground.name = "Audio Management Panel Background";
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

            var newButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
            newButton.transform.localPosition = new(1.3f, -1.88f, -16f);
            newButton.name = "New Audio";
            newButton.Text.text = GetString("NewSound");
            newButton.Background.color = Palette.White;
            var newPassiveButton = newButton.GetComponent<PassiveButton>();
            newPassiveButton.OnClick = new();
            newPassiveButton.OnClick.AddListener(new Action(AudioManagementNewWindow.Open));

            var helpText = Object.Instantiate(CustomPopup.InfoTMP.gameObject, CustomBackground.transform);
            helpText.name = "Help Text";
            helpText.transform.localPosition = new(-1.25f, -2.15f, -15f);
            helpText.transform.localScale = new(1f, 1f, 1f);
            var helpTextTMP = helpText.GetComponent<TextMeshPro>();
            helpTextTMP.text = GetString("CustomAudioManagementHelp");
            helpText.gameObject.GetComponent<RectTransform>().sizeDelta = new(2.45f, 1f);

            var sliderTemplate = AccountManager.Instance.transform.FindChild("MainSignInWindow/SignIn/AccountsMenu/Accounts/Slider").gameObject;
            if (sliderTemplate != null && Slider == null)
            {
                Slider = Object.Instantiate(sliderTemplate, CustomBackground.transform);
                Slider.name = "Audio Management Slider";
                Slider.transform.localPosition = new Vector3(0f, 0.5f, -11f);
                Slider.transform.localScale = new Vector3(1f, 1f, 1f);
                Slider.GetComponent<SpriteRenderer>().size = new(5f, 4f);
                var scroller = Slider.GetComponent<Scroller>();
                scroller.ScrollWheelSpeed = 0.3f;
                var mask = Slider.transform.FindChild("Mask");
                mask.transform.localScale = new Vector3(4.9f, 3.92f, 1f);
            }
        }

        ReloadTag(null);
        RefreshTagList();
    }
    public static void RefreshTagList()
    {
        if (!XtremeGameData.GameStates.IsNotJoined) return;
        numItems = 0;
        var scroller = Slider.GetComponent<Scroller>();
        scroller.Inner.gameObject.ForEachChild((Action<GameObject>)(DestroyObj));
        static void DestroyObj(GameObject obj)
        {
            if (obj.name.StartsWith("AccountButton")) Object.Destroy(obj);
        }

        var numberSetter = AccountManager.Instance.transform.FindChild("DOBEnterScreen/EnterAgePage/MonthMenu/Months").GetComponent<NumberSetter>();
        var buttonPrefab = numberSetter.ButtonPrefab.gameObject;

        Items?.Values?.Do(Object.Destroy);
        Items = new();
        foreach (var audio in finalMusics)
        
        {
            numItems++;
            var filename = audio.FileName;

            var button = Object.Instantiate(buttonPrefab, scroller.Inner);
            button.transform.localPosition = new(-1f, 1.6f - 0.6f * numItems, -10.5f);
            button.transform.localScale = new(1.2f, 1.2f, 1.2f);
            button.name = "Btn-" + filename;

            var renderer = button.GetComponent<SpriteRenderer>();
            var rollover = button.GetComponent<ButtonRolloverHandler>();

            var previewText = Object.Instantiate(button.transform.GetChild(0).GetComponent<TextMeshPro>(), button.transform);
            previewText.transform.SetLocalX(1.9f);
            previewText.fontSize = 1f;
            previewText.name = "PreText-" + filename;


            Object.Destroy(button.GetComponent<UIScrollbarHelper>());
            Object.Destroy(button.GetComponent<NumberButton>());

            string buttontext;
            Color buttonColor;
            bool enable = true;
            string preview = "???";

            var audioExist = audio.CurrectAudioStates is not AudiosStates.NotExist || CustomAudios.Contains(filename);
            var unpublished = audio.unpublished;
            
            if (audio.CurrectAudioStates == AudiosStates.IsDownLoading)
            {
                buttontext = GetString("downloadInProgress");
                buttonColor = Color.yellow;
                enable = false;
            }
            else if (audio.CurrectAudioStates is AudiosStates.DownLoadSucceedNotice or AudiosStates.DownLoadFailureNotice)
            {
                var succeed = audio.CurrectAudioStates is AudiosStates.DownLoadSucceedNotice;
                buttontext = GetString($"{audio.CurrectAudioStates}");
                buttonColor = succeed ? Color.cyan : Palette.Brown;
                enable = false;
            }
            else
            {
                if (audio.CurrectAudioStates == AudiosStates.IsPlaying)
                {
                    buttontext = GetString("Playing");
                    buttonColor = Color.red;
                    enable = false;
                }
                else if (audioExist)
                {
                    buttontext = GetString("delete");
                    buttonColor = !audio.UnOfficial ? Color.red : Palette.Purple;
                }
                else
                {
                    buttontext = !audio.UnOfficial ? GetString("download") : GetString("NoFound");
                    buttonColor = !audio.UnOfficial ? Color.green : Color.black;
                }
            }
            if (unpublished)
            {
                buttonColor = Palette.DisabledGrey;
                enable = false;
            }
            preview = audio.Name;


            var passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener(new Action(() =>
            {
                if (audioExist)
                {
                    Delete(audio);

                }
                else
                {
                    audio.CurrectAudioStates = audio.LastAudioStates = AudiosStates.IsDownLoading;
                    RefreshTagList();
                    var task = MusicDownloader.StartDownload(filename);
                    task.ContinueWith(t => 
                    {
                        new LateTask(() =>
                        {
                            audio.CurrectAudioStates = audio.LastAudioStates = t.Result ? AudiosStates.DownLoadSucceedNotice : AudiosStates.DownLoadFailureNotice;
                            RefreshTagList();

                            new LateTask(() =>
                            {
                                new FinalMusic(music: audio.CurrectAudio);
                                RefreshTagList();
                                MyMusicPanel.RefreshTagList();
                            }, 3f);
                        },0.01f);
                    });
                }
            }));

            button.transform.GetChild(0).GetComponent<TextMeshPro>().text = buttontext;
            rollover.OutColor = renderer.color = buttonColor;
            button.GetComponent<PassiveButton>().enabled = enable;
            previewText.text = preview;
            Items.Add(filename, button);
        }

        scroller.SetYBoundsMin(0f);
        scroller.SetYBoundsMax(0.6f * numItems);
    }


    public static void Delete(FinalMusic audio)
    {
        var sound = audio.FileName;
        DeleteSoundInName(sound);
        DeleteSoundInFile(sound);
        new FinalMusic(music: audio.CurrectAudio);
        RefreshTagList();
        MyMusicPanel.RefreshTagList();
    }
    static void DeleteSoundInName(string name)
    {
        var lines = File.ReadAllLines(TAGS_PATH);
        var updatedLines = lines.Where(line => !line.Trim().Equals(name, StringComparison.Ordinal)).ToArray();
        var writer = new StreamWriter(TAGS_PATH);
        foreach (var line in updatedLines)
        {
            writer.WriteLine(line);
        }
        
        ReloadTag(name);
    }
    static void DeleteSoundInFile(string sound)
    {
        var path = @$"FinalSuspect_Data/Resources/Audios/{sound}.wav";
        File.Delete(path);
    }
}