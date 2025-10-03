using System;
using AmongUs.GameOptions;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// ReSharper disable UnusedMember.Local

namespace FinalSuspect.Modules.Features.DisplayedRoleTag;

public static class DisplayerRoleTagHelper
{
    private const int MaxOneScreenRole = 40;

    private static int Page;
    public static TextMeshPro textTemplate;
    public static GameObject selectionUI;
    private static Dictionary<CategoryType, List<Transform>> CategoryButtons;
    private static Dictionary<CategoryType, SpriteRenderer> CategorySelectButtons;
    private static CategoryType currentCategory = CategoryType.Role;

    private static readonly Dictionary<IdentityTypes, string> identityColors = new()
    {
        { IdentityTypes.Safe, "#FFD700" },
        { IdentityTypes.Likely_Safe, "#C0C0C0" },
        { IdentityTypes.Suspect, "#B22222" },
        { IdentityTypes.Alibi, "#94AA24" },
        { IdentityTypes.Confirmed_Position, "#228B22" },
        { IdentityTypes.Unconfirmed_Position, "#663399" }
    };

    private static void SelectCategory(CategoryType category, bool SetPage = true)
    {
        currentCategory = category;
        if (SetPage) Page = 1;

        foreach (var catButton in CategoryButtons)
        {
            var index = 0;
            foreach (var btn in catButton.Value.Where(btn => btn != null))
            {
                index++;

                // 分页控制
                if (index <= (Page - 1) * MaxOneScreenRole ||
                    Page * MaxOneScreenRole < index)
                {
                    btn.gameObject.SetActive(false);
                    continue;
                }

                btn.gameObject.SetActive(catButton.Key == category);
            }
        }

        foreach (var catSelect in CategorySelectButtons.Where(catSelect => catSelect.Value != null))
        {
            catSelect.Value.color = catSelect.Value.color.SetAlpha(catSelect.Key == category ? 1f : 0.25f);
        }
    }

    public static void ShowSelectionPanel(MeetingHud __instance, PlayerControl pc)
    {
        if (selectionUI != null ||
            !PlayerControl.LocalPlayer.IsAlive() &&
            PlayerControl.LocalPlayer.GetRoleType() is not RoleTypes.GuardianAngel ||
            __instance.playerStates == null) return;

        try
        {
            var thisTag = pc.GetFinalData().RoleTag;
            Page = 1;
            CategoryButtons = new Dictionary<CategoryType, List<Transform>>();
            CategorySelectButtons = new Dictionary<CategoryType, SpriteRenderer>();
            __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(false));

            // 创建UI容器
            var container = Object.Instantiate(
                GameObject.Find("PhoneUI").transform, __instance.transform);
            var to = container.gameObject.AddComponent<TransitionOpen>();
            to.targetSize = 0.75f;
            container.transform.localPosition = new Vector3(0, 0, -200f);

            selectionUI = container.gameObject;

            // 创建退出按钮
            var buttonTemplate = __instance.playerStates[0]!.transform.FindChild("votePlayerBase");
            var maskTemplate = __instance.playerStates[0].transform.FindChild("MaskArea");
            var smallButtonTemplate = __instance.playerStates[0].Buttons.transform.Find("CancelButton");

            var exitButtonParent = new GameObject("ExitButton").transform;
            exitButtonParent.SetParent(container);
            var exitButton = Object.Instantiate(buttonTemplate, exitButtonParent);
            exitButton.FindChild("ControllerHighlight").gameObject.SetActive(false);

            var exitButtonMask = Object.Instantiate(maskTemplate, exitButtonParent);
            exitButtonMask.transform.localScale = new Vector3(2.88f, 0.8f, 1f);
            exitButtonMask.transform.localPosition = new Vector3(0f, 0f, 1f);

            exitButton.GetComponent<SpriteRenderer>().sprite =
                smallButtonTemplate.GetComponent<SpriteRenderer>().sprite;
            exitButtonParent.localPosition = new Vector3(3.88f, -2.12f, -200f);
            exitButtonParent.localScale = new Vector3(0.22f, 0.9f, 1f);
            exitButtonParent.transform.SetAsFirstSibling();

            exitButton.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
            exitButton.GetComponent<PassiveButton>().OnClick.AddListener(new Action(() =>
            {
                __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                Object.Destroy(container.gameObject);
                selectionUI = null;
            }));
            exitButton.GetComponent<PassiveButton>();


            var clearButtonParent = new GameObject("ClearButton").transform;
            clearButtonParent.SetParent(container);
            var clearButton = Object.Instantiate(buttonTemplate, clearButtonParent);
            clearButton.FindChild("ControllerHighlight").gameObject.SetActive(false);

            var clearLabel = Object.Instantiate(
                textTemplate, clearButton);
            var clearButtonMask = Object.Instantiate(maskTemplate, clearButtonParent);
            clearButtonMask.transform.localScale = new Vector3(2.88f, 0.8f, 1f);
            clearButtonMask.transform.localPosition = new Vector3(0f, 0f, 1f);

            clearButton.GetComponent<SpriteRenderer>().sprite = LoadSprite("Plate_Clear.png", 115f);
            clearButton.GetComponent<SpriteRenderer>().color = Color.white;
            clearButtonParent.localPosition = new Vector3(2.5f, -2.12f, -200f);
            clearButtonParent.localScale = new Vector3(0.53f, 0.53f, 1f);
            clearButtonParent.transform.SetAsFirstSibling();

            clearLabel.text = GetString("Clear");
            clearLabel.color = Color.green;
            clearLabel.alignment = TextAlignmentOptions.Center;
            clearLabel.transform.localPosition = new Vector3(0, 0, -1f);
            clearLabel.transform.localScale *= 1.6f;
            clearLabel.autoSizeTextContainer = true;
            clearLabel.enabled = true;

            clearButton.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
            clearButton.GetComponent<PassiveButton>().OnClick.AddListener(new Action(() =>
            {
                thisTag.setTag("");
                thisTag.setColor(Color.white);
                thisTag.setRoom("");
                __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                Object.Destroy(container.gameObject);
                selectionUI = null;
            }));
            clearButton.GetComponent<PassiveButton>();

            var tabCount = 0;

            foreach (var category in EnumHelper.GetAllValues<CategoryType>())
            {
                var color = category switch
                {
                    CategoryType.Role => RoleHelper.GetRoleColor(RoleTypes.Crewmate),
                    CategoryType.PlayerIdentityTag => GetIdentityColor(IdentityTypes.Safe),
                    CategoryType.Room => (Color)ColorHelper.ClientlessColor,
                    _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
                };

                var catButtonParent = new GameObject(category + "Tab").transform;
                catButtonParent.SetParent(container);

                var catButton = Object.Instantiate(buttonTemplate, catButtonParent);
                catButton.FindChild("ControllerHighlight").gameObject.SetActive(false);

                Object.Instantiate(maskTemplate, catButtonParent);
                var catLabel = Object.Instantiate(
                    textTemplate, catButton);

                catButton.GetComponent<SpriteRenderer>().sprite = LoadSprite("Plate_Category.png", 115f);
                catButton.GetComponent<SpriteRenderer>().color = Color.white;

                CategorySelectButtons.Add(category, catButton.GetComponent<SpriteRenderer>());

                catButtonParent.localPosition = new Vector3(-2.5f + tabCount++ * 1.73f, 2.225f, -200);
                catButtonParent.localScale = new Vector3(0.53f, 0.53f, 1f);

                catLabel.text = GetString($"DisplayedRoleTag.{category}");
                catLabel.color = color;
                catLabel.alignment = TextAlignmentOptions.Center;
                catLabel.transform.localPosition = new Vector3(0, 0, -1f);
                catLabel.transform.localScale *= 1.6f;
                catLabel.autoSizeTextContainer = true;
                catLabel.enabled = true;

                catButton.GetComponent<PassiveButton>().OnClick.AddListener(new Action(() =>
                {
                    if (category == currentCategory) return;
                    SelectCategory(category);
                    ReloadPage();
                }));
            }

            foreach (var role in EnumHelper.GetAllValues<RoleTypes>())
            {
                CreateOption(CategoryType.Role, role.ToString());
            }

            foreach (IdentityTypes identity in Enum.GetValues(typeof(IdentityTypes)))
            {
                CreateOption(CategoryType.PlayerIdentityTag, identity.ToString());
            }

            foreach (var room in ShipStatus.Instance.AllRooms)
            {
                CreateOption(CategoryType.Room, room.RoomId.ToString());
            }

            void CreateOption(CategoryType category, string value)
            {
                // 初始化列表（.NET 6+ 语法）
                CategoryButtons.TryAdd(category, []);
                if (CategoryButtons[category].Any(x => x.parent.name == value)) return;

                var color = category switch
                {
                    CategoryType.Role => RoleHelper.GetRoleColor(Enum.Parse<RoleTypes>(value)),
                    CategoryType.PlayerIdentityTag => GetIdentityColor(Enum.Parse<IdentityTypes>(value)),
                    CategoryType.Room => (Color)ColorHelper.ClientlessColor,
                    _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
                };

                var displayText = category switch
                {
                    CategoryType.Role or CategoryType.PlayerIdentityTag => GetString($"{category}.{value}"),
                    CategoryType.Room => GetString(value),
                    _ => throw new ArgumentOutOfRangeException(nameof(category))
                };

                var optionParent = new GameObject(value).transform;
                optionParent.SetParent(container);

                var option = Object.Instantiate(buttonTemplate, optionParent);
                option.FindChild("ControllerHighlight").gameObject.SetActive(false);

                Object.Instantiate(maskTemplate, optionParent);
                var optionLabel = Object.Instantiate(textTemplate, option);
                optionLabel.enabled = true;

                var spriteRenderer = option.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = LoadSprite("Plate_Content.png", 115f);
                spriteRenderer.color = Color.white;

                var index = CategoryButtons[category].Count;
                var (row, col) = (index / 5, index % 5);
                optionParent.localPosition = new Vector3(-3.47f + 1.75f * col, 1.5f - 0.45f * row, -200f);
                optionParent.localScale = Vector3.one * 0.55f;

                optionLabel.text = displayText;
                optionLabel.color = color;
                optionLabel.alignment = TextAlignmentOptions.Center;
                optionLabel.transform.localPosition = new Vector3(0, 0, optionLabel.transform.localPosition.z);
                optionLabel.transform.localScale *= 1.6f;
                optionLabel.autoSizeTextContainer = true;

                option.GetComponent<PassiveButton>().OnClick.AddListener(new Action(() =>
                {
                    if (category == CategoryType.Room)
                        thisTag.setRoom($"({displayText})");
                    else
                    {
                        thisTag.setColor(color);
                        thisTag.setTag(displayText);
                    }


                    __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                    Object.Destroy(container.gameObject);
                }));

                CategoryButtons[category].Add(option);
            }

            void ReloadPage()
            {
                SelectCategory(currentCategory, false);
            }

            SelectCategory(CategoryType.Role);
            ReloadPage();
        }
        catch (Exception ex)
        {
            Error(ex.ToString(), "SelectionUI");
        }
    }

    private static Color GetIdentityColor(IdentityTypes identity)
    {
        identityColors.TryGetValue(identity, out var hexColor);
        _ = ColorUtility.TryParseHtmlString(hexColor, out var c);
        return c;
    }

    private enum CategoryType
    {
        Role,
        PlayerIdentityTag,
        Room
    }

    private enum IdentityTypes
    {
        Safe,
        Likely_Safe,
        Suspect,
        Alibi,
        Confirmed_Position,
        Unconfirmed_Position
    }
}

public class DisplayerRoleTag
{
    public string TagStr { get; private set; } = "";
    public Color TagColor { get; private set; } = Color.white;
    public string Room { get; private set; } = "";

    public void setTag(string tagStr) => TagStr = tagStr;
    public void setColor(Color tagColor) => TagColor = tagColor;
    public void setRoom(string room) => Room = room;
}