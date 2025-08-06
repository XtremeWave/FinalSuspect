using System;
using FinalSuspect.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FinalSuspect.Templates;

public class SimpleButton
{
    private static PassiveButton baseButton;
    private readonly BoxCollider2D buttonCollider;

    private float _fontSize;
    private Vector2 _scale;

    /// <summary>创建新按钮</summary>
    /// <param name="parent">父对象</param>
    /// <param name="name">对象名称</param>
    /// <param name="localPosition">本地坐标</param>
    /// <param name="normalColor">正常状态背景色</param>
    /// <param name="hoverColor">鼠标悬停时背景色</param>
    /// <param name="action">点击时触发的动作</param>
    /// <param name="label">按钮标签文本</param>
    /// <param name="isActive">初始激活状态(默认true)</param>
    public SimpleButton(
        Transform parent,
        string name,
        Vector3 localPosition,
        Color32 normalColor,
        Color32 hoverColor,
        Action action,
        string label,
        bool isActive = true)
    {
        if (!baseButton) throw new InvalidOperationException("baseButtonが未設定");

        Button = Object.Instantiate(baseButton, parent);
        Label = Button.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>();
        NormalSprite = Button.inactiveSprites.GetComponent<SpriteRenderer>();
        HoverSprite = Button.activeSprites.GetComponent<SpriteRenderer>();
        buttonCollider = Button.GetComponent<BoxCollider2D>();

        var container = Label.transform.parent;
        Object.Destroy(Label.GetComponent<AspectPosition>());
        container.SetLocalX(0f);
        Label.transform.SetLocalX(0f);
        Label.horizontalAlignment = HorizontalAlignmentOptions.Center;

        Button.name = name;
        Button.transform.localPosition = localPosition;
        NormalSprite.color = normalColor;
        HoverSprite.color = hoverColor;
        Button.OnClick.AddListener(action);
        Label.text = label;
        Button.gameObject.SetActive(isActive);
    }

    public PassiveButton Button { get; }
    public TextMeshPro Label { get; }
    private SpriteRenderer NormalSprite { get; }
    private SpriteRenderer HoverSprite { get; }

    public Vector2 Scale
    {
        get => _scale;
        set => _scale = NormalSprite.size = HoverSprite.size = buttonCollider.size = value;
    }

    public float FontSize
    {
        get => _fontSize;
        set => _fontSize = Label.fontSize = Label.fontSizeMin = Label.fontSizeMax = value;
    }

    public static void SetBase(PassiveButton passiveButton)
    {
        if (baseButton || !passiveButton) return;

        // 复制按钮
        baseButton = Object.Instantiate(passiveButton);
        var label = baseButton.transform.Find("FontPlacer/Text_TMP").GetComponent<TextMeshPro>();
        baseButton.gameObject.SetActive(false);
        // 防止场景切换时被销毁
        Object.DontDestroyOnLoad(baseButton);
        baseButton.name = "FinalSuspect_SimpleButtonBase";
        // 移除不需要的组件
        Object.Destroy(baseButton.GetComponent<AspectPosition>());
        label.DestroyTranslator();
        label.fontSize = label.fontSizeMax = label.fontSizeMin = 3.5f;
        label.enableWordWrapping = false;
        label.text = "FinalSuspect SIMPLE BUTTON BASE";
        // 修复碰撞体偏移问题
        var buttonCollider = baseButton.GetComponent<BoxCollider2D>();
        buttonCollider.offset = new Vector2(0f, 0f);
        baseButton.OnClick = new Button.ButtonClickedEvent();
    }

    public static bool IsNullOrDestroyed(SimpleButton button)
    {
        return button == null || !button.Button;
    }
}