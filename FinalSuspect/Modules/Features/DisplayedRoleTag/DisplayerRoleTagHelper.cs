using System;
using System.Diagnostics.CodeAnalysis;
using AmongUs.GameOptions;
using FinalSuspect.Helpers;
using FinalSuspect.Modules.Core.Game;
using FinalSuspect.Modules.Core.Game.PlayerControlExtension;
using Rewired.UI.ControlMapper;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements.UIR;

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
        { IdentityTypes.Hard_Cleared, "#FFD700" },
        { IdentityTypes.Silver_Clear, "#C0C0C0" },
        { IdentityTypes.Wolf_Bucket, "#B22222" },
        { IdentityTypes.No_Kill, "#191970" },
        { IdentityTypes.Outside_Position, "#228B22" },
        { IdentityTypes.Inside_Position, "#663399" }
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
            var container = UnityEngine.Object.Instantiate(
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
            var exitButton = UnityEngine.Object.Instantiate(buttonTemplate, exitButtonParent);
            exitButton.FindChild("ControllerHighlight").gameObject.SetActive(false);

            var exitButtonMask = UnityEngine.Object.Instantiate(maskTemplate, exitButtonParent);
            exitButtonMask.transform.localScale = new Vector3(2.88f, 0.8f, 1f);
            exitButtonMask.transform.localPosition = new Vector3(0f, 0f, 1f);

            exitButton.GetComponent<SpriteRenderer>().sprite =
                smallButtonTemplate.GetComponent<SpriteRenderer>().sprite;
            exitButtonParent.localPosition = new Vector3(3.88f, -2.12f, -200f);
            exitButtonParent.localScale = new Vector3(0.22f, 0.9f, 1f);
            exitButtonParent.transform.SetAsFirstSibling();

            exitButton.GetComponent<PassiveButton>().OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            exitButton.GetComponent<PassiveButton>().OnClick.AddListener(new Action(() =>
            {
                __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                UnityEngine.Object.Destroy(container.gameObject);
            }));
            exitButton.GetComponent<PassiveButton>();


            var clearButtonParent = new GameObject("ClearButton").transform;
            clearButtonParent.SetParent(container);
            var clearButton = UnityEngine.Object.Instantiate(buttonTemplate, clearButtonParent);
            clearButton.FindChild("ControllerHighlight").gameObject.SetActive(false);

            var clearLabel = UnityEngine.Object.Instantiate(
                textTemplate, clearButton);
            var clearButtonMask = UnityEngine.Object.Instantiate(maskTemplate, clearButtonParent);
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

            clearButton.GetComponent<PassiveButton>().OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            clearButton.GetComponent<PassiveButton>().OnClick.AddListener(new Action(() =>
            {
                thisTag.setTag("");
                thisTag.setColor(Color.white);
                __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                UnityEngine.Object.Destroy(container.gameObject);
            }));
            clearButton.GetComponent<PassiveButton>();

            var tabCount = 0;

            foreach (var category in EnumHelper.GetAllValues<CategoryType>())
            {
                var color = category switch
                {
                    CategoryType.PlayerIdentityTag => GetIdentityColor(IdentityTypes.Hard_Cleared),
                    CategoryType.Role => GetRoleColor(RoleTypes.Crewmate),
                    _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
                };

                var catButtonParent = new GameObject(category + "Tab").transform;
                catButtonParent.SetParent(container);

                var catButton = UnityEngine.Object.Instantiate(buttonTemplate, catButtonParent);
                catButton.FindChild("ControllerHighlight").gameObject.SetActive(false);

                UnityEngine.Object.Instantiate(maskTemplate, catButtonParent);
                var catLabel = UnityEngine.Object.Instantiate(
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

            void CreateOption(CategoryType category, string value)
            {
                if (!CategoryButtons.ContainsKey(category))
                    CategoryButtons.Add(category, []);

                Color color;
                switch (category)
                {
                    case CategoryType.PlayerIdentityTag:
                        var identity = Enum.Parse<IdentityTypes>(value);
                        color = GetIdentityColor(identity);
                        break;
                    case CategoryType.Role:
                        var role = Enum.Parse<RoleTypes>(value);
                        color = GetRoleColor(role);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(category), category, null);
                }

                var optionParent = new GameObject(value).transform;
                optionParent.SetParent(container);

                var option = UnityEngine.Object.Instantiate(buttonTemplate, optionParent);
                option.FindChild("ControllerHighlight").gameObject.SetActive(false);

                UnityEngine.Object.Instantiate(maskTemplate, optionParent);
                var optionLabel = UnityEngine.Object.Instantiate(
                    textTemplate, option);
                optionLabel.enabled = true;
                option.GetComponent<SpriteRenderer>().sprite = LoadSprite("Plate_Content.png", 115f);
                option.GetComponent<SpriteRenderer>().color = Color.white;

                CategoryButtons[category].Add(option);

                var index = CategoryButtons[category].Count - 1;
                var row = index / 5;
                var col = index % 5;

                optionParent.localPosition = new Vector3(-3.47f + 1.75f * col, 1.5f - 0.45f * row, -200f);
                optionParent.localScale = new Vector3(0.55f, 0.55f, 1f);

                var text = GetString($"{category}.{value}");
                optionLabel.text = text;

                optionLabel.color = color;
                optionLabel.alignment = TextAlignmentOptions.Center;
                optionLabel.transform.localPosition = new Vector3(0, 0, optionLabel.transform.localPosition.z);
                optionLabel.transform.localScale *= 1.6f;
                optionLabel.autoSizeTextContainer = true;

                option.GetComponent<PassiveButton>().OnClick.AddListener(new Action(() =>
                {
                    thisTag.setTag(text);
                    thisTag.setColor(color);
                    __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                    UnityEngine.Object.Destroy(container.gameObject);
                }));
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
        PlayerIdentityTag
    }

    private enum IdentityTypes
    {
        Hard_Cleared,
        Silver_Clear,
        Wolf_Bucket,
        No_Kill,
        Outside_Position,
        Inside_Position
    }
}

public class DisplayerRoleTag(string tagStr, Color tagColor)
{
    public string TagStr { get; private set; } = tagStr;
    public Color TagColor { get; private set; } = tagColor;

    public void setTag(string tagStr) => TagStr = tagStr;
    public void setColor(Color tagColor) => TagColor = tagColor;
}