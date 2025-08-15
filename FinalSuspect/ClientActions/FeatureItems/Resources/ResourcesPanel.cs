using System;
using System.IO;
using System.Threading.Tasks;
using FinalSuspect.ClientActions.FeatureItems.MainMenuStyle;
using FinalSuspect.ClientActions.FeatureItems.MyMusic;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Features;
using FinalSuspect.Modules.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static FinalSuspect.ClientActions.FeatureItems.Resources.ResourcesManager;
using Object = UnityEngine.Object;

namespace FinalSuspect.ClientActions.FeatureItems.Resources;

public static class ResourcesPanel
{
    private static int _numItems;

    private static readonly Dictionary<string, CurrentState> packageStates = new();
    public static SpriteRenderer CustomBackground { get; set; }
    private static GameObject Slider { get; set; }
    private static Dictionary<string, GameObject> Items { get; set; }

    public static void Hide()
    {
        if (CustomBackground != null)
            CustomBackground?.gameObject.SetActive(false);
    }

    public static void Init(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
        if (!IsNotJoined) return;

        if (CustomBackground == null)
        {
            _numItems = 0;
            CustomBackground = Object.Instantiate(optionsMenuBehaviour.Background, optionsMenuBehaviour.transform);
            CustomBackground.name = "Resource Manager";
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

            if (CustomPopup.InfoTMP != null)
            {
                var helpText = Object.Instantiate(CustomPopup.InfoTMP.gameObject, CustomBackground.transform);
                helpText.name = "Help Text";
                helpText.transform.localPosition = new Vector3(-1.25f, -2.15f, -15f);
                helpText.transform.localScale = new Vector3(1f, 1f, 1f);
                var helpTextTMP = helpText.GetComponent<TextMeshPro>();
                helpTextTMP.text = GetString("Tip.ResourceManager");
                helpText.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(2.45f, 1f);
            }

            var sliderTemplate = AccountManager.Instance.transform
                .FindChild("MainSignInWindow/SignIn/AccountsMenu/Accounts/Slider").gameObject;
            if (sliderTemplate != null && Slider == null)
            {
                Slider = Object.Instantiate(sliderTemplate, CustomBackground.transform);
                Slider.name = "Slider";
                Slider.transform.localPosition = new Vector3(0f, 0.5f, -11f);
                Slider.transform.localScale = new Vector3(1f, 1f, 1f);
                Slider.GetComponent<SpriteRenderer>().size = new Vector2(5f, 4f);
                var scroller = Slider.GetComponent<Scroller>();
                scroller.ScrollWheelSpeed = 0.3f;
                var mask = Slider.transform.FindChild("Mask");
                mask.transform.localScale = new Vector3(4.9f, 3.92f, 1f);
            }
        }

        RefreshTagList();
    }

    public static void RefreshTagList()
    {
        if (!IsNotJoined) return;
        _numItems = 0;
        var scroller = Slider.GetComponent<Scroller>();
        scroller.Inner.gameObject.ForEachChild((Action<GameObject>)DestroyObj);

        var numberSetter = AccountManager.Instance.transform.FindChild("DOBEnterScreen/EnterAgePage/MonthMenu/Months")
            .GetComponent<NumberSetter>();
        var buttonPrefab = numberSetter.ButtonPrefab.gameObject;

        Items?.Values.Do(Object.Destroy);
        Items = new Dictionary<string, GameObject>();

        foreach (var (packageName, fileList) in AllResources)
        {
            packageStates.TryAdd(packageName, CurrentState.None);
            _numItems++;

            var button = Object.Instantiate(buttonPrefab, scroller.Inner);
            button.transform.localPosition = new Vector3(-1f, 1.6f - 0.6f * _numItems, -10.5f);
            button.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            button.name = "Btn-" + packageName;

            var renderer = button.GetComponent<SpriteRenderer>();
            var rollover = button.GetComponent<ButtonRolloverHandler>();

            var previewText =
                Object.Instantiate(button.transform.GetChild(0).GetComponent<TextMeshPro>(), button.transform);
            previewText.transform.SetLocalX(1.9f);
            previewText.fontSize = 1f;
            previewText.name = "PreText-" + packageName;

            Object.Destroy(button.GetComponent<UIScrollbarHelper>());
            Object.Destroy(button.GetComponent<NumberButton>());

            string buttonText;
            Color buttonColor;
            var enable = true;
            var preview = $"{GetString($"Package.{packageName}")}";

            var tempFileList = new List<string>();
            if (fileList.All(name =>
                {
                    var type = GetType(name);
                    var path = GetLocalFilePath(type, name);
                    bool exists;
                    switch (type)
                    {
                        case FileType.SoundEffects:
                        case FileType.Images:
                            exists = File.Exists(path);
                            break;
                        case FileType.Musics:
                            exists = AudioManager.ConvertExtension(ref path);
                            break;
                        case FileType.Unknown:
                        case FileType.Depends:
                        case FileType.ModNews:
                        case FileType.Languages:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (exists)
                        tempFileList.Add(name);
                    return exists;
                }))
                packageStates[packageName] = CurrentState.Complete;

            var state = packageStates[packageName];
            switch (state)
            {
                case CurrentState.IsDownloading:
                    buttonText = GetString("Tip.Downloading");
                    buttonColor = ColorHelper.DownloadYellow;
                    enable = false;
                    break;
                case CurrentState.DownLoadSucceeded:
                case CurrentState.DownLoadFailed:
                    var succeed = state == CurrentState.DownLoadSucceeded;
                    buttonText = GetString($"Tip.{state}");
                    buttonColor = succeed ? Color.cyan : Palette.Brown;
                    enable = false;
                    break;
                case CurrentState.Complete:
                    buttonText = GetString("Tip.PackageExists");
                    buttonColor = ColorHelper.CompleteGreen;
                    enable = false;
                    break;
                default:
                    buttonText = GetString("Download");
                    buttonColor = Color.green;
                    break;
            }

            var passiveButton = button.GetComponent<PassiveButton>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener(new Action(() =>
            {
                packageStates[packageName] = CurrentState.IsDownloading;
                RefreshTagList();

                var downloadTasks = (from fileName in fileList.Except(tempFileList)
                    let type = GetType(fileName)
                    select ResourcesDownloader.StartDownloadAsPackage(packageName, type, fileName)).ToList();
                var allSucceeded = false;
                Task.Run(async () =>
                {
                    try
                    {
                        var results = await Task.WhenAll(downloadTasks);
                        allSucceeded = results.All(success => success);
                    }
                    finally
                    {
                        packageStates[packageName] =
                            allSucceeded ? CurrentState.DownLoadSucceeded : CurrentState.DownLoadFailed;
                        _ = new MainThreadTask(RefreshTagList, "Notice");
                        _ = new LateTask(() =>
                        {
                            packageStates[packageName] = CurrentState.None;
                            RefreshTagList();
                            MainMenuStylePanel.Refresh(MainMenuStyleManager.MainMenuStyles[Main.CurrentStyleId.Value]);
                            MyMusicPanel.RefreshTagList();
                        }, 3F, "Refresh Tag List");
                    }
                });
            }));

            button.transform.GetChild(0).GetComponent<TextMeshPro>().text = buttonText;
            rollover.OutColor = renderer.color = buttonColor;
            button.GetComponent<PassiveButton>().enabled = enable;
            previewText.text = preview;
            Items.Add(packageName, button);
        }

        scroller.SetYBoundsMin(0f);
        scroller.SetYBoundsMax(0.6f * _numItems);
        return;

        static void DestroyObj(GameObject obj)
        {
            if (obj.name.StartsWith("AccountButton")) Object.Destroy(obj);
        }

        static FileType GetType(string fileName)
        {
            var type = Path.GetExtension(fileName) switch
            {
                ".jpg" or ".png" => FileType.Images,
                ".wav" or ".zip" => FileType.Musics,
                _ => FileType.Unknown
            };
            return type;
        }
    }

    private enum CurrentState
    {
        None,
        IsDownloading,
        DownLoadSucceeded,
        DownLoadFailed,
        Complete
    }
}