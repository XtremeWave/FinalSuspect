using System.IO;
using FinalSuspect.ClientItems.FeatureItems.MyMusic;
using FinalSuspect.Modules.Resources;
using UnityEngine;

namespace FinalSuspect.ClientItems.FeatureItems.MainMenuStyle;

public abstract class MainMenuStyleManager
{
    public static readonly List<MainMenuStyle> MainMenuStyles =
    [
        new(
            "MiraHQ",
            true,
            [
                new Color(0.5216f, 1f, 0.9490f, 0.8f),
                new Color(0.5216f, 0.7765f, 1f, 0.8f),
                new Color(0.7294f, 0.6353f, 1.0f, 0.8f),
                new Color(0.0235f, 0f, 0.8f, 0.8f)
            ]),
        new(
            "Security",
            true,
            [
                new Color(1f, 0.524f, 0.549f, 0.8f),
                new Color(1f, 0.825f, 0.686f, 0.8f),
                new Color(0.526f, 1f, 0.792f, 0.8f),
                new Color(0.526f, 0.731f, 1f, 0.8f)
            ],
            2),
        new(
            "NewYear",
            false,
            [
                new Color(0.8f, 0.251f, 0.1f, 0.8f),
                new Color(1f, 1f, 0.286f, 0.8f),
                new Color(0.7f, 0.7765f, 0.245f, 0.8f),
                new Color(1f, 0.324f, 0.211f, 0.8f),
            ]),
        new(
            "MiraStudio",
            false,
            [
                new Color(0.9f, 0.551f, 0.9f, 0.8f),
                new Color(0.9f, 0.951f, 0.5f, 0.8f),
                new Color(0.8f, 0.251f, 0.1f, 0.8f),
                new Color(0.526f, 0.731f, 1f, 0.8f)
            ]),
        new(
            "XtremeWave",
            false,
            [
                new Color(0.0235f, 0.6f, 1f, 0.8f),
                new Color(0.526f, 0.731f, 1f, 0.8f),
                new Color(0.7294f, 0.6353f, 1.0f, 0.8f),
                new Color(0.9f, 0.551f, 0.9f, 0.8f),
            ]),
        new(
            "EmergencyMeeting",
            true,
            [
                new Color(0.0235f, 0.6f, 1f, 0.8f),
                new Color(0.255f, 0.482f, 1f, 0.8f),
                new Color(0.333f, 0.255f, 1f, 0.8f),
                new Color(0.0235f, 0.6f, 1f, 0.8f),
            ]),
        //new(
        //"WhenLookingBackAtTheEnd",
        //false,
        //[])
    ];

    public class MainMenuStyle(
        string BGName,
        bool starFieldActive,
        List<Color> mainUIColors,
        int starGenDire = -2,
        SupportedMusics supportedMusic = SupportedMusics.FinalSuspect__Slok)
    {
        public readonly List<Color> MainUIColors = mainUIColors;
        private CurrentState _currentState = CurrentState.NotFound;
        public bool Applied => CurrentState == CurrentState.Applied;
        public bool StarFieldActive { get; } = starFieldActive;
        public int StarGenDire { get; } = starGenDire;
        public SupportedMusics MainMenuMusic { get; } = supportedMusic;

        public CurrentState CurrentState
        {
            get
            {
                if (_currentState != CurrentState.NotFound)
                    return _currentState;

                return File.Exists(GetLocalFilePath(FileType.Images, $"FinalSuspect-BG-{BGName}.png"))
                    ? CurrentState.NotApply
                    : CurrentState.NotFound;
            }
            set => _currentState = value;
        }

        public string Title => GetString($"MainMenuStyle.Title_{BGName}");
        public string Author => GetString($"MainMenuStyle.Author_{BGName}");
        public string Description => GetString($"MainMenuStyle.Description_{BGName}");
        public Sprite PreviewSprite => LoadSprite($"FinalSuspect-BG-{BGName}-Preview.png", 450f);
        public Sprite Sprite => LoadSprite($"FinalSuspect-BG-{BGName}.png", 179f);
    }
}

public enum CurrentState
{
    NotFound,
    NotApply,
    Applied
}