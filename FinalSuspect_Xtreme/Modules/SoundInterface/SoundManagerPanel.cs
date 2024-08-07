using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static FinalSuspect_Xtreme.AudioManager;
using static FinalSuspect_Xtreme.Translator;
using Object = UnityEngine.Object;

using Il2CppSystem.IO;
using AmongUs.HTTP;

namespace FinalSuspect_Xtreme.Modules.SoundInterface;

public static class SoundManagerPanel
{
    public static SpriteRenderer CustomBackground { get; private set; }
    public static GameObject Slider { get; private set; }
    public static Dictionary<string, GameObject> Items { get; private set; }

    static int numItems = 0;
    static List<string> IsDownloading = new();
    static Dictionary<string, bool> DownloadDone = new();
    public static void Hide()
    {
        if (CustomBackground != null)
            CustomBackground?.gameObject?.SetActive(false);
    }
    public static void Init(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
        if (!GameStates.IsNotJoined) return;

        if (CustomBackground == null)
        {
            numItems = 0;
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

            var newButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
            newButton.transform.localPosition = new(1.3f, -1.88f, -16f);
            newButton.name = "New Tag";
            newButton.Text.text = GetString("NewSound");
            newButton.Background.color = Palette.White;
            var newPassiveButton = newButton.GetComponent<PassiveButton>();
            newPassiveButton.OnClick = new();
            newPassiveButton.OnClick.AddListener(new Action(SoundManagerNewWindow.Open));

            var helpText = Object.Instantiate(CustomPopup.InfoTMP.gameObject, CustomBackground.transform);
            helpText.name = "Help Text";
            helpText.transform.localPosition = new(-1.25f, -2.15f, -15f);
            helpText.transform.localScale = new(1f, 1f, 1f);
            var helpTextTMP = helpText.GetComponent<TextMeshPro>();
            helpTextTMP.text = GetString("CustomSoundManagerHelp");
            helpText.gameObject.GetComponent<RectTransform>().sizeDelta = new(2.45f, 1f);

            var sliderTemplate = AccountManager.Instance.transform.FindChild("MainSignInWindow/SignIn/AccountsMenu/Accounts/Slider").gameObject;
            if (sliderTemplate != null && Slider == null)
            {
                Slider = Object.Instantiate(sliderTemplate, CustomBackground.transform);
                Slider.name = "Name Tags Slider";
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

        foreach (var soundp in AllFiles)
        {
            var sound = soundp.Key;
            numItems++;
            var button = Object.Instantiate(buttonPrefab, scroller.Inner);
            button.transform.localPosition = new(-1f, 1.6f - 0.6f * numItems, -10.5f);
            button.transform.localScale = new(1.2f, 1.2f, 1.2f);
            button.name = "Sound Item For " + sound;
            Object.Destroy(button.GetComponent<UIScrollbarHelper>());
            Object.Destroy(button.GetComponent<NumberButton>());

            var path = @$"{Environment.CurrentDirectory.Replace(@"\", "/")}./FinalSuspect_Data/Sounds/{sound}.wav";
            var renderer = button.GetComponent<SpriteRenderer>();
            var rollover = button.GetComponent<ButtonRolloverHandler>();

            var previewText = Object.Instantiate(button.transform.GetChild(0).GetComponent<TextMeshPro>(), button.transform);
            previewText.transform.SetLocalX(1.9f);
            previewText.fontSize = 1f;
            
            string buttontext;
            Color buttonColor;
            bool enable = true;
            string preview = "???";

            var isDownloading = IsDownloading.Contains(sound);
            var InTip = DownloadDone.ContainsKey(sound);
            var audioExist = ConvertExtension(ref path);
            var audioNameExist = File.Exists(@$"{Environment.CurrentDirectory.Replace(@"\", "/")}./FinalSuspect_Data/SoundNames/{sound}.json");
            var isXWMus = AllFinalSuspect.ContainsKey(sound);
            var unpublished = NotUp.Contains(sound);
            
            if (isDownloading)
            {
                buttontext = GetString("downloadInProgress");
                buttonColor = Color.yellow;
                enable = false;
            }
            else if (InTip)
            {
                DownloadDone.TryGetValue(sound, out var succeed);
                buttontext = succeed ? GetString("downloadSucceed") : GetString("downloadFailure");
                buttonColor = succeed ? Color.cyan : Palette.Brown;
                enable = false;
            }
            else
            {
                if (audioExist)
                {
                    buttontext = GetString("delete");
                    buttonColor = isXWMus ? Color.red : Palette.Purple;
                }
                else
                {
                    buttontext = isXWMus ? GetString("download") : GetString("NoFound");
                    buttonColor = isXWMus ? Color.green : Color.black;
                }
            }
            if (unpublished)
            {
                buttonColor = Palette.DisabledGrey;
                enable = false;
            }
            if (sound != null)
                preview = isXWMus ? GetString($"Mus.{sound}") : sound;


            var passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new();
            passiveButton.OnClick.AddListener(new Action(() =>
            {


                if (audioExist || audioNameExist)
                {
                    try
                    {
                        Delete(sound);
                        ReloadTag(sound);
                        RefreshTagList();
                        SoundPanel.RefreshTagList();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"{ex}", "Delete");
                    }

                }
                else
                {
                    IsDownloading.Remove(sound);
                    IsDownloading.Add(sound);
                    RefreshTagList();
                    var task = MusicDownloader.StartDownload(sound);
                    task.ContinueWith(t => 
                    {
                        new LateTask(() =>
                        {
                            IsDownloading.Remove(sound);
                            ReloadTag(sound);
                            DownloadDone.Remove(sound);
                            if (t.Result)
                            {
                                DownloadDone.Add(sound, true);
                                RefreshTagList();
                                new LateTask(() =>
                                {
                                    DownloadDone.Remove(sound);
                                    RefreshTagList();
                                }, 3f);
                            }
                            else
                            {
                                DownloadDone.Add(sound, false);
                                RefreshTagList();
                                new LateTask(() =>
                                {
                                    DownloadDone.Remove(sound);
                                    RefreshTagList();
                                }, 3f);
                            }
                            SoundPanel.RefreshTagList();
                        },0.01f);
                    });
                }
            }));

            button.transform.GetChild(0).GetComponent<TextMeshPro>().text = buttontext;
            rollover.OutColor = renderer.color = buttonColor;
            button.GetComponent<PassiveButton>().enabled = enable;
            previewText.text = preview;
            Items.Add(sound, button);
        }

        scroller.SetYBoundsMin(0f);
        scroller.SetYBoundsMax(0.6f * numItems);
    }


    static void Delete(string sound)
    {
        DeleteSoundInName(sound);
            DeleteSoundInFile(sound);
    }
    static void DeleteSoundInName(string soundname)
    {
        if (AllFinalSuspect.ContainsKey(soundname)) return;
        try
        {

            var path = @$"{Environment.CurrentDirectory.Replace(@"\", "/")}./FinalSuspect_Data/SoundNames/{soundname}.json";
            Logger.Info($"{soundname} Deleted", "DeleteSound");
                File.Delete(path);
            
        }
        catch (Exception e)
        {
            Logger.Error($"清除文件名称失败\n{e}", "DeleteOldFiles");
        }
        return;
    }
    static void DeleteSoundInFile(string sound)
    {
        try
        {
            var path2 = @$"{Environment.CurrentDirectory.Replace(@"\", "/")}./FinalSuspect_Data/Sounds/{sound}.wav";
            Logger.Info($"{Path.GetFileName(path2)} Deleted", "DeleteSound");
            File.Delete(path2);
        }
        catch (Exception e)
        {
            Logger.Error($"清除文件失败\n{e}", "DeleteSound");
        }
        return;
    }
}
