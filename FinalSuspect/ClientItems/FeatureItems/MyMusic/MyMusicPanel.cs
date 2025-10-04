using System;
using System.Diagnostics.CodeAnalysis;
using FinalSuspect.Helpers;
using TMPro;
using UnityEngine.UI;
using static FinalSuspect.ClientItems.FeatureItems.MyMusic.AudioPlayer;
using Object = UnityEngine.Object;

namespace FinalSuspect.ClientItems.FeatureItems.MyMusic;

[SuppressMessage("ReSharper", "PossibleLossOfFraction")]
public static class MyMusicPanel
{
    private const int ItemsPerPage = 7;
    private static TextMeshPro _pageText;
    private static TextMeshPro _timeText;
    private static ToggleButtonBehaviour _playModeButton;
    private static ToggleButtonBehaviour _pausePlayButton;
    private static int _numItems;
    private static int _playMode;
    public static SpriteRenderer CustomBackground { get; set; }
    private static List<GameObject> Items { get; set; }
    private static OptionsMenuBehaviour OptionsMenuBehaviourNow { get; set; }

    private static int CurrentPage { get; set; } = 1;
    private static int TotalPageCount => (FinalMusic.Musics.Count + ItemsPerPage - 1) / ItemsPerPage;

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
            _numItems = 0;
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

            CreatePageNavigationButtons(mouseMoveToggle);
            CreateFeatureButtons(mouseMoveToggle);
            CreatePageText(optionsMenuBehaviour);
            CreateTimeText(optionsMenuBehaviour);

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

    private static void CreatePageNavigationButtons(ToggleButtonBehaviour template)
    {
        var prevButton = Object.Instantiate(template, CustomBackground.transform);
        prevButton.transform.localPosition = new Vector3(0.4f, -1.33f, -6f);
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
            RefreshTagList();
        }));

        var nextButton = Object.Instantiate(template, CustomBackground.transform);
        nextButton.transform.localPosition = new Vector3(2.2f, -1.33f, -6f);
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
            RefreshTagList();
        }));
    }

    private static void CreateFeatureButtons(ToggleButtonBehaviour template)
    {
        _playModeButton = Object.Instantiate(template, CustomBackground.transform);
        _playModeButton.transform.localPosition = new Vector3(0.4f, -1.88f, -6f);
        _playModeButton.name = "PlayModeButton";
        _playModeButton.Text.text = "Repeat";
        _playModeButton.Background.color = Color.white;
        _playModeButton.Background.size = new Vector2(0.4f, 0.4f);
        _playModeButton.transform.FindChild("ButtonHighlight").gameObject.GetComponent<SpriteRenderer>().size =
            new Vector2(0.55f, 0.55f);
        _playModeButton.GetComponent<BoxCollider2D>().size = new Vector2(0.39f, 0.39f);

        var playModePassiveButton = _playModeButton.GetComponent<PassiveButton>();
        playModePassiveButton.OnClick = new Button.ButtonClickedEvent();
        playModePassiveButton.OnClick.AddListener(new Action(() =>
        {
            _playMode = (_playMode + 1) % 3;
            UpdatePlayModeButton();
        }));

        UpdatePlayModeButton();
        _pausePlayButton = Object.Instantiate(template, CustomBackground.transform);
        _pausePlayButton.transform.localPosition = new Vector3(0.85f, -1.88f, -6f);
        _pausePlayButton.name = "PausePlayButton";
        _pausePlayButton.Text.text = "▶";
        _pausePlayButton.Background.color = Color.white;
        _pausePlayButton.Background.size = new Vector2(0.4f, 0.4f);
        _pausePlayButton.transform.FindChild("ButtonHighlight").gameObject.GetComponent<SpriteRenderer>().size =
            new Vector2(0.55f, 0.55f);
        _pausePlayButton.GetComponent<BoxCollider2D>().size = new Vector2(0.39f, 0.39f);

        var pausePlayPassiveButton = _pausePlayButton.GetComponent<PassiveButton>();
        pausePlayPassiveButton.OnClick = new Button.ButtonClickedEvent();
        pausePlayPassiveButton.OnClick.AddListener(new Action(() =>
        {
            var state = CurrentMusic?.CurrentAudioStates is AudiosStates.Playing;
            if (CurrentMusic == null) return;
            CurrentMusic.CurrentAudioStates = state ? AudiosStates.Pausing : AudiosStates.Playing;
            var currentSource = SoundManager.Instance.soundPlayers.ToArray().ToList()
                .First(x => x.Name == CurrentMusic.FileName).Player;
            if (state)
            {
                currentSource.Pause();
            }
            else
            {
                currentSource.UnPause();
            }

            RefreshTagList();
        }));

        UpdatePausePlayButton();

        var lastButton = Object.Instantiate(template, CustomBackground.transform);
        lastButton.transform.localPosition = new Vector3(1.3f, -1.88f, -6f);
        lastButton.name = "PreciousButton";
        lastButton.Text.text = "|◀";
        lastButton.Background.color = Color.white;
        lastButton.Background.size = new Vector2(0.4f, 0.4f);
        lastButton.transform.FindChild("ButtonHighlight").gameObject.GetComponent<SpriteRenderer>().size =
            new Vector2(0.55f, 0.55f);
        lastButton.GetComponent<BoxCollider2D>().size = new Vector2(0.39f, 0.39f);

        var lastPassiveButton = lastButton.GetComponent<PassiveButton>();
        lastPassiveButton.OnClick = new Button.ButtonClickedEvent();
        lastPassiveButton.OnClick.AddListener(new Action((() => { HandlePlayMode(true, false); })));

        var nextButton = Object.Instantiate(template, CustomBackground.transform);
        nextButton.transform.localPosition = new Vector3(1.75f, -1.88f, -6f);
        nextButton.name = "NextButton";
        nextButton.Text.text = "▶|";
        nextButton.Background.color = Color.white;
        nextButton.Background.size = new Vector2(0.4f, 0.4f);
        nextButton.transform.FindChild("ButtonHighlight").gameObject.GetComponent<SpriteRenderer>().size =
            new Vector2(0.55f, 0.55f);
        nextButton.GetComponent<BoxCollider2D>().size = new Vector2(0.39f, 0.39f);

        var nextPassiveButton = nextButton.GetComponent<PassiveButton>();
        nextPassiveButton.OnClick = new Button.ButtonClickedEvent();
        nextPassiveButton.OnClick.AddListener(new Action((() => { HandlePlayMode(true); })));

        var stopButton = Object.Instantiate(template, CustomBackground.transform);
        stopButton.transform.localPosition = new Vector3(2.2f, -1.88f, -6f);
        stopButton.name = "StopButton";
        stopButton.Text.text = "■";
        stopButton.Background.color = Color.white;
        stopButton.Background.size = new Vector2(0.4f, 0.4f);
        stopButton.transform.FindChild("ButtonHighlight").gameObject.GetComponent<SpriteRenderer>().size =
            new Vector2(0.55f, 0.55f);
        stopButton.GetComponent<BoxCollider2D>().size = new Vector2(0.39f, 0.39f);

        var stopPassiveButton = stopButton.GetComponent<PassiveButton>();
        stopPassiveButton.OnClick = new Button.ButtonClickedEvent();
        stopPassiveButton.OnClick.AddListener(new Action(() => { StopPlayMod(); }));
    }

    private static void CreatePageText(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        _pageText = Object.Instantiate(
            optionsMenuBehaviour.DisableMouseMovement.Text,
            CustomBackground.transform
        );
        _pageText.name = "PageText";
        _pageText.transform.localPosition = new Vector3(1.3f, -1.33f, -5f);
        _pageText.transform.localScale = Vector3.one;

        var tmp = _pageText.GetComponent<TextMeshPro>();
        tmp.text = "PageCount";
        tmp.alignment = TextAlignmentOptions.Center;

        _pageText.GetComponent<RectTransform>().sizeDelta = new Vector2(1f, 0.3f);
    }

    private static void CreateTimeText(OptionsMenuBehaviour optionsMenuBehaviour)
    {
        _timeText = Object.Instantiate(
            optionsMenuBehaviour.DisableMouseMovement.Text,
            CustomBackground.transform
        );
        _timeText.name = "TimeText";
        _timeText.transform.localPosition = new Vector3(-1.8f, -1.33f, -5f);
        _timeText.transform.localScale = Vector3.one;

        var tmp = _timeText.GetComponent<TextMeshPro>();
        tmp.text = "TimeData";
        tmp.alignment = TextAlignmentOptions.Left;

        _timeText.GetComponent<RectTransform>().sizeDelta = new Vector2(1f, 0.3f);
    }

    public static void RefreshTagList()
    {
        try
        {
            AudioManager.ReloadTag();
            Items?.Do(Object.Destroy);
            Items = [];
            _numItems = 0;
            var optionsMenuBehaviour = OptionsMenuBehaviourNow;
            var startIndex = (CurrentPage - 1) * ItemsPerPage;

            var count = 0;
            foreach (var audio in FinalMusic.Musics.Skip(startIndex))
            {
                if (count >= ItemsPerPage)
                {
                    break;
                }

                RefreshTags(optionsMenuBehaviour, audio);
                count++;
            }

            UpdatePausePlayButton();
            UpdatePlayModeButton();
            if (_pageText)
                _pageText.GetComponent<TextMeshPro>().text = $"{CurrentPage}/{TotalPageCount}";
        }
        catch
        {
            /* ignored */
        }
    }

    private static void RefreshTags(OptionsMenuBehaviour optionsMenuBehaviour, FinalMusic audio)
    {
        try
        {
            var mouseMoveToggle = optionsMenuBehaviour.DisableMouseMovement;
            var name = audio.Name;
            var path = audio.FilePath;
            var filename = audio.FileName;
            var author = audio.Author;

            var offsetX = _numItems % 2 == 0 ? -1.3f : 1.3f;
            var offsetY = 2.2f - 0.5f * (_numItems / 2);
            var offsetZ = -4f;

            var toggleButton = Object.Instantiate(mouseMoveToggle, CustomBackground.transform);
            toggleButton.transform.localPosition = new Vector3(offsetX, offsetY, offsetZ);
            toggleButton.name = "Btn-" + filename;
            toggleButton.Background.color = Color.white;
            _numItems++;

            offsetX = _numItems % 2 == 0 ? -1.3f : 1.3f;
            offsetY = 2.2f - 0.5f * (_numItems / 2);
            offsetZ = -6f;

            var previewText =
                Object.Instantiate(optionsMenuBehaviour.DisableMouseMovement.Text, CustomBackground.transform);
            previewText.transform.localPosition = new Vector3(offsetX, offsetY, offsetZ);
            previewText.fontSize = toggleButton.Text.fontSize;
            previewText.name = "PreText-" + filename;

            Color color;
            string preview;
            var enable = false;

            switch (audio.CurrentAudioStates)
            {
                case AudiosStates.Playing:
                    preview = GetString("Tip.Playing");
                    color = ColorHelper.FSColor;
                    break;
                case AudiosStates.DownLoading:
                    color = ColorHelper.DownloadYellow;
                    preview = GetString("Tip.Downloading");
                    break;
                case AudiosStates.Parsing:
                    color = ColorHelper.FSClientOptionColor;
                    preview = GetString("Tip.Parsing");
                    break;
                case AudiosStates.Exist:
                    color = audio.UnOfficial ? Color.green : ColorHelper.FSClientFeatureColor;
                    preview = GetString("MusPlay.CanPlay");
                    enable = FinalMusic.Musics.All(x => x.CurrentAudioStates is not AudiosStates.Parsing);
                    break;
                case AudiosStates.Pausing:
                    color = ColorHelper.ShadeColor(ColorHelper.FSClientOptionColor, -0.2f);
                    preview = GetString("Tip.Pausing");
                    break;
                case AudiosStates.NotExist:
                default:
                {
                    color = ColorHelper.FSClientFeatureColor_CanNotUse;
                    preview = GetString("MusPlay.NoFound");
                    break;
                }
            }

            previewText.text = $"{name}{(author != string.Empty ? $" -{author}" : "")}";
            toggleButton.Background.color = color;
            toggleButton.GetComponent<PassiveButton>().enabled = enable;
            toggleButton.Text.text = preview;

            var passiveButton = toggleButton.GetComponent<PassiveButton>();
            passiveButton.OnClick = new Button.ButtonClickedEvent();
            passiveButton.OnClick.AddListener(new Action(OnClick));

            void OnClick()
            {
                Info($"Try To Play {filename}:{path}", "MyMusicPanel");
                Play(audio);
            }

            Items.Add(toggleButton.gameObject);
            Items.Add(previewText.gameObject);
        }
        finally
        {
            _numItems++;
        }
    }

    public static void Update()
    {
        try
        {
            if (!CustomBackground || !CustomBackground.gameObject.active || !_timeText) return;
            var currentMusic = CurrentMusic;
            if (currentMusic == null || currentMusic.CurrentAudioStates is AudiosStates.Parsing)
            {
                _timeText.text = FinalMusic.Musics.Any(x => x.CurrentAudioStates is AudiosStates.Parsing)
                    ? GetString("Tip.Parsing")
                    : "--/--";
                return;
            }

            var currentSource = SoundManager.Instance.soundPlayers.ToArray().ToList()
                .First(x => x.Name == currentMusic.FileName).Player;

            var currentTimeInSeconds = (int)currentSource.time;
            var clipLengthInSeconds = (int)currentSource.clip.length;

            var currentTimeSpan = TimeSpan.FromSeconds(currentTimeInSeconds);
            var clipLengthSpan = TimeSpan.FromSeconds(clipLengthInSeconds);

            var formattedCurrentTime = currentTimeSpan.ToString(@"mm\:ss");
            var formattedClipLength = clipLengthSpan.ToString(@"mm\:ss");

            _timeText.text = $"{formattedCurrentTime}/{formattedClipLength}";

            if (formattedCurrentTime != formattedClipLength && currentSource.GetRemainingTime() >= 0.1f) return;
            HandlePlayMode();
        }
        catch
        {
            /* ignored */
        }
    }

    private static void HandlePlayMode(bool press = false, bool next = true)
    {
        switch ((PlayMode)_playMode)
        {
            case PlayMode.Repeat:
                if (press)
                    PlayByDirection(next);
                break;

            case PlayMode.Sequential:
                if (!press) next = true;
                PlayByDirection(next);
                break;

            case PlayMode.Random:
                PlayRandomTrack();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static void UpdatePlayModeButton()
    {
        _playModeButton.Text.text = (PlayMode)_playMode switch
        {
            PlayMode.Repeat => "○", // 循环播放
            PlayMode.Sequential => "|=", // 顺序播放
            PlayMode.Random => "R", // 随机播放
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static void UpdatePausePlayButton()
    {
        if (CurrentMusic == null) return;
        var state = CurrentMusic.CurrentAudioStates is AudiosStates.Playing;
        _pausePlayButton.Text.text = state ? "||" : "▶";
    }

    private enum PlayMode
    {
        Repeat,
        Sequential,
        Random
    }
}