using System;
using System.Globalization;
using AmongUs.Data;
using FinalSuspect.Helpers;
using Il2CppSystem.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using static FinalSuspect.ClientActions.FeatureItems.NameTag.NameTagManager;
using Component = FinalSuspect.ClientActions.FeatureItems.NameTag.NameTagManager.Component;
using File = System.IO.File;
using Path = System.IO.Path;

namespace FinalSuspect.ClientActions.FeatureItems.NameTag;

public static class NameTagEditMenu
{
    public enum ComponentType
    {
        DisplayName,
        Title,
        Prefix,
        Suffix,
        Name,
        LastTag
    }

    // UI 常量
    private const float ButtonSpacing = 1.8f;
    private const float ButtonRowHeight = 0.5f;
    private const float ButtonStartX = -3.8f;
    private const float ButtonStartY = 2.1f;

    // 数据
    private static string _friendCode;
    private static NameTagManager.NameTag _cacheTag;

    private static ComponentType _currentComponent;

    // UI 元素
    public static GameObject Menu { get; set; }
    private static Dictionary<ComponentType, GameObject> ComponentButtons { get; } = new();
    public static GameObject Preview { get; private set; }
    public static GameObject TextEnter { get; private set; }
    public static GameObject SizeEnter { get; private set; }
    public static GameObject Color1Enter { get; private set; }
    public static GameObject Color2Enter { get; private set; }
    public static GameObject Color3Enter { get; private set; }

    public static void Hide()
    {
        Menu?.SetActive(false);
    }

    public static void Toggle(string friendCode, bool? on = null)
    {
        on ??= Menu == null || !Menu.activeSelf;
        if (!on.Value)
        {
            Hide();
            return;
        }

        if (Menu == null) Init();
        if (Menu == null) return;

        Menu.SetActive(true);
        _friendCode = friendCode;
        _cacheTag = friendCode != null && AllExternalNameTags.TryGetValue(friendCode, out var tag)
            ? DeepClone(tag)
            : new NameTagManager.NameTag();

        LoadComponent(GetComponent(_cacheTag, ComponentType.DisplayName));
        SetButtonHighlight(ComponentType.DisplayName);
        _currentComponent = ComponentType.DisplayName;
        UpdatePreview();
    }

    private static void SetButtonHighlight(ComponentType componentType)
    {
        foreach (var button in ComponentButtons.Values)
        {
            var text = button.transform.Find("Text_TMP").GetComponent<TextMeshPro>();
            text.color = Palette.DisabledGrey;
        }

        if (ComponentButtons.TryGetValue(componentType, out var activeButton))
        {
            var activeText = activeButton.transform.Find("Text_TMP").GetComponent<TextMeshPro>();
            activeText.color = ColorHelper.FSColor;
        }
    }

    private static void LoadComponent(Component com, bool name = false)
    {
        TextEnter.GetComponent<TextBoxTMP>().enabled = !name;
        TextEnter.GetComponent<TextBoxTMP>().SetText(!name ? com?.Text ?? "" : GetString("CanNotEdit"));
        SizeEnter.GetComponent<TextBoxTMP>()
            .SetText((com?.SizePercentage ?? 100).ToString(CultureInfo.CurrentCulture));

        Color1Enter.GetComponent<TextBoxTMP>().Clear();
        Color2Enter.GetComponent<TextBoxTMP>().Clear();
        Color3Enter.GetComponent<TextBoxTMP>().Clear();

        if (com?.Gradient?.IsValid ?? false)
            for (var i = 0; i < Mathf.Min(3, com.Gradient.Colors.Count); i++)
            {
                var color = com.Gradient.Colors[i];
                var textBox = i switch
                {
                    0 => Color1Enter.GetComponent<TextBoxTMP>(),
                    1 => Color2Enter.GetComponent<TextBoxTMP>(),
                    2 => Color3Enter.GetComponent<TextBoxTMP>(),
                    _ => null
                };
                textBox?.SetText(ColorUtility.ToHtmlStringRGBA(color)[..6]);
            }
        else if (com?.TextColor != null)
            Color1Enter.GetComponent<TextBoxTMP>().SetText(
                ColorUtility.ToHtmlStringRGBA(com.TextColor.Value)[..6]);
    }

    private static void UpdatePreview()
    {
        if (!Menu.activeSelf || _cacheTag == null || Preview == null) return;
        var displayName = _cacheTag.Apply(null, true);
        Preview.GetComponent<TextMeshPro>().text = displayName.title;
    }

    private static void SaveToCache(ComponentType type)
    {
        var com = new Component();
        var text = TextEnter.GetComponent<TextBoxTMP>().text.Trim();
        if (text != "" && type != ComponentType.Name) com.Text = text;

        var size = SizeEnter.GetComponent<TextBoxTMP>().text.Trim();
        if (size != "" && float.TryParse(size, out var sizef)) com.SizePercentage = sizef;

        List<Color> colors = new();
        AddColorIfValid(Color1Enter, colors);
        AddColorIfValid(Color2Enter, colors);
        AddColorIfValid(Color3Enter, colors);

        if (colors.Count > 1) com.Gradient = new ColorGradient(colors.ToArray());
        else if (colors.Count == 1) com.TextColor = colors[0];
        com.Spaced = default;

        switch (type)
        {
            case ComponentType.Title: _cacheTag.Title = com; break;
            case ComponentType.Prefix: _cacheTag.Prefix = com; break;
            case ComponentType.Suffix: _cacheTag.Suffix = com; break;
            case ComponentType.Name: _cacheTag.Name = com; break;
            case ComponentType.DisplayName: _cacheTag.DisplayName = com; break;
            case ComponentType.LastTag: _cacheTag.LastTag = com; break;
        }
    }

    private static void AddColorIfValid(GameObject input, List<Color> colors)
    {
        var colorHex = input.GetComponent<TextBoxTMP>().text.Trim();
        if (!string.IsNullOrEmpty(colorHex)
            && ColorUtility.DoTryParseHtmlColor("#" + colorHex, out var color))
            colors.Add(color);
    }

    private static bool SaveToFile(string friendCode, NameTagManager.NameTag tag)
    {
        if (string.IsNullOrEmpty(friendCode)) return false;

        StringWriter sw = new();
        JsonWriter writer = new JsonTextWriter(sw);
        writer.WriteStartObject();
        var components = new Dictionary<string, Component>
        {
            ["Title"] = tag.Title,
            ["Prefix"] = tag.Prefix,
            ["Suffix"] = tag.Suffix,
            ["Name"] = tag.Name,
            ["DisplayName"] = tag.DisplayName,
            ["LastTag"] = tag.LastTag
        };
        foreach (var (name, com) in components)
        {
            if (com == null) continue;

            writer.WritePropertyName(name);
            writer.WriteStartObject();

            if (com.Text != null && name != "Name")
            {
                writer.WritePropertyName("Text");
                writer.WriteValue(com.Text);
            }

            if (com.SizePercentage != null)
            {
                writer.WritePropertyName("SizePercentage");
                writer.WriteValue(com.SizePercentage.ToString());
            }

            if (com.Gradient != null && com.Gradient.IsValid)
            {
                var colors = string.Join(",",
                    com.Gradient.Colors.Select(c => "#" + ColorUtility.ToHtmlStringRGBA(c)[..6]));
                writer.WritePropertyName("Gradient");
                writer.WriteValue(colors);
            }
            else if (com.TextColor != null)
            {
                writer.WritePropertyName("Color");
                writer.WriteValue("#" + ColorUtility.ToHtmlStringRGBA(com.TextColor.Value)[..6]);
            }

            if (name != "Title" && name != "Name")
            {
                writer.WritePropertyName("Spaced");
                writer.WriteValue(com.Spaced.ToString());
            }

            writer.WriteEndObject();
        }

        writer.WriteEndObject();

        var fileName = Path.Combine(TagsDirectoryPath, friendCode.Trim() + ".json");
        File.WriteAllText(fileName, sw.ToString());
        return true;
    }

    public static void Init()
    {
        Menu = UiHelper.CreateBaseWindow(
            "Name Tag Edit Menu",
            NameTagPanel.CustomBackground.transform.parent,
            -30f,
            1.4f
        );

        var closeButton = UiHelper.CreateCloseButton(Menu.transform, () => Toggle(null, false));
        closeButton.transform.localPosition = new Vector3(4.9f, 2.5f, -1f);
        CreateComponentButtons();
        CreateActionButtons();
        CreatePreviewSection();
        CreateInputFields();
        UiHelper.HideTemplateObjects(Menu.transform);
    }

    private static void CreateComponentButtons()
    {
        // 第一行按钮：DisplayName, Title, Prefix
        CreateComponentButton(ComponentType.DisplayName, 0, 0);
        CreateComponentButton(ComponentType.Title, 1, 0);
        CreateComponentButton(ComponentType.Prefix, 2, 0);

        // 第二行按钮：Suffix, Name, LastTag
        CreateComponentButton(ComponentType.Suffix, 0, 1);
        CreateComponentButton(ComponentType.Name, 1, 1);
        CreateComponentButton(ComponentType.LastTag, 2, 1);
    }

    private static void CreateComponentButton(ComponentType type, int col, int row)
    {
        var x = ButtonStartX + col * ButtonSpacing;
        var y = ButtonStartY - row * ButtonRowHeight;

        var button = UiHelper.CreateButton(
            Menu.transform.Find("Button Prefab").gameObject,
            Menu.transform,
            new Vector3(x, y, 0),
            type.ToString()
        );

        button.name = $"Edit{type}Button";
        button.transform.Find("Background").localScale = new Vector3(0.85f, 0.85f, 1f);

        button.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() =>
        {
            SaveToCache(_currentComponent);
            LoadComponent(GetComponent(_cacheTag, type), type == ComponentType.Name);
            SetButtonHighlight(type);
            _currentComponent = type;
        }));

        ComponentButtons[type] = button;
    }

    private static void CreateActionButtons()
    {
        var previewButton = UiHelper.CreateButton(
            Menu.transform.Find("Button Prefab").gameObject,
            Menu.transform,
            new Vector3(1.2f, -2.5f, 0f),
            "RefreshPreview"
        );
        previewButton.name = "RefreshPreviewButton";

        var previewPassive = previewButton.GetComponent<PassiveButton>();
        if (previewPassive != null)
        {
            previewPassive.OnClick.RemoveAllListeners();
            previewPassive.OnClick.AddListener((Action)(() =>
            {
                SaveToCache(_currentComponent);
                UpdatePreview();
            }));
        }

        // 保存按钮
        var saveButton = UiHelper.CreateButton(
            Menu.transform.Find("Button Prefab").gameObject,
            Menu.transform,
            new Vector3(3.5f, -2.5f, 0f),
            "SaveAndClose"
        );
        saveButton.name = "SaveAndExitButton";

        var savePassive = saveButton.GetComponent<PassiveButton>();
        if (savePassive != null)
        {
            savePassive.OnClick.RemoveAllListeners();
            savePassive.OnClick.AddListener((Action)(() =>
            {
                SaveToCache(_currentComponent);
                if (SaveToFile(_friendCode, _cacheTag))
                {
                    ReloadTag(_friendCode);
                    NameTagPanel.RefreshTagList();
                }

                Toggle(null, false);
            }));
        }

        // 删除按钮
        var deleteButton = UiHelper.CreateButton(
            Menu.transform.Find("Button Prefab").gameObject,
            Menu.transform,
            new Vector3(-3.5f, -2.5f, 0f),
            GetString("Delete"),
            false
        );
        deleteButton.name = "DeleteButton";

        // 设置删除按钮文本为红色
        var deleteText = deleteButton.transform.Find("Text_TMP")?.GetComponent<TextMeshPro>();
        if (deleteText != null) deleteText.color = Color.red;

        var deletePassive = deleteButton.GetComponent<PassiveButton>();
        if (deletePassive != null)
        {
            deletePassive.OnClick.RemoveAllListeners();
            deletePassive.OnClick.AddListener((Action)(() =>
            {
                if (string.IsNullOrEmpty(_friendCode)) return;

                var fileName = Path.Combine(TagsDirectoryPath, $"{_friendCode.Trim()}.json");
                if (File.Exists(fileName))
                    try
                    {
                        File.Delete(fileName);
                        ReloadTag(_friendCode);
                        NameTagPanel.RefreshTagList();
                        Toggle(null, false);
                    }
                    catch (Exception ex)
                    {
                        Error($"Delete name tag failed: {ex}", "NameTagEditMenu");
                    }
            }));
        }
    }

    private static void CreatePreviewSection()
    {
        Preview = UiHelper.CreateText(
            Menu.transform.Find("Title Prefab").gameObject,
            Menu.transform,
            new Vector3(0f, 1.2f, 0f),
            DataManager.player.Customization.Name,
            0.6f
        );
        Preview.name = "Preview Text";
    }

    private static void CreateInputFields()
    {
        // 文本输入区域
        TextEnter = UiHelper.CreateInputField(
            Menu.transform,
            new Vector3(-2.9f, 0f, 0f),
            true
        );
        TextEnter.name = "Edit Text Enter Box";

        // 尺寸输入区域
        SizeEnter = UiHelper.CreateInputField(
            Menu.transform,
            new Vector3(-2.9f, -1.2f, 0f),
            false
        );
        SizeEnter.name = "Edit Size Enter Box";

        // 颜色输入区域
        Color1Enter = UiHelper.CreateInputField(
            Menu.transform,
            new Vector3(1.95f, -0f, 0f),
            true
        );
        Color1Enter.name = "Edit Color 1 Enter Box";

        Color2Enter = UiHelper.CreateInputField(
            Menu.transform,
            new Vector3(1.95f, -0.6f, 0f),
            true
        );
        Color2Enter.name = "Edit Color 2 Enter Box";

        Color3Enter = UiHelper.CreateInputField(
            Menu.transform,
            new Vector3(1.95f, -1.2f, 0f),
            true
        );
        Color3Enter.name = "Edit Color 3 Enter Box";

        // 创建标签文本
        CreateLabel(GetString("Tip.TextContent"), new Vector3(-2.95f, 0f, 0f));
        CreateLabel(GetString("Tip.TextSizeDescription"), new Vector3(-2.95f, -1.2f, 0f));
        CreateLabel(GetString("Tip.TextColorDescription"), new Vector3(1.95f, 0.2f, 0f));
    }

    private static void CreateLabel(string text, Vector3 position)
    {
        var label = UiHelper.CreateText(
            Menu.transform.Find("Info Prefab").gameObject,
            Menu.transform,
            position,
            text,
            1f
        );
        label.name = $"Label_{text[..Mathf.Min(10, text.Length)]}";
    }

    private static Component GetComponent(NameTagManager.NameTag tag, ComponentType type)
    {
        return type switch
        {
            ComponentType.DisplayName => tag.DisplayName,
            ComponentType.Title => tag.Title,
            ComponentType.Prefix => tag.Prefix,
            ComponentType.Suffix => tag.Suffix,
            ComponentType.Name => tag.Name,
            ComponentType.LastTag => tag.LastTag,
            _ => null
        };
    }
}