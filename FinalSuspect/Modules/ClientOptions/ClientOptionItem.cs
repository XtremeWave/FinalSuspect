using System;
using BepInEx.Configuration;
using FinalSuspect.Helpers;
using UnityEngine;

namespace FinalSuspect.Modules.ClientOptions;

public sealed class ClientOptionItem_Boolean : ClientActionItem
{
    public ConfigEntry<bool> Config { get; private set; }

    private ClientOptionItem_Boolean(
        string name,
        ConfigEntry<bool> config,
        OptionsMenuBehaviour optionsMenuBehaviour)
    : base(
        name,
        optionsMenuBehaviour)
    {
        Config = config;
        UpdateToggle();
    }

    /// <summary>
    /// Modオプション画面にconfigのトグルを追加します
    /// </summary>
    /// <param name="name">ボタンラベルの翻訳キーとボタンのオブジェクト名</param>
    /// <param name="config">対応するconfig</param>
    /// <param name="optionsMenuBehaviour">OptionsMenuBehaviourのインスタンス</param>
    /// <param name="additionalOnClickAction">クリック時に追加で発火するアクション．configが変更されたあとに呼ばれる</param>
    /// <returns>作成したアイテム</returns>
    public static ClientOptionItem_Boolean Create(
        string name,
        ConfigEntry<bool> config,
        OptionsMenuBehaviour optionsMenuBehaviour,
        Action additionalOnClickAction = null)
    {
        var item = new ClientOptionItem_Boolean(name, config, optionsMenuBehaviour);
        item.OnClickAction = () =>
        {
            config.Value = !config.Value;
            item.UpdateToggle();
            additionalOnClickAction?.Invoke();
        };
        return item;
    }
   

    public void UpdateToggle()
    {
        if (ToggleButton == null) return;

        var color = Config.Value ? ColorHelper.ClientOptionColor : ColorHelper.ClientOptionColor_Disable;
        ToggleButton.Background.color = color;
        ToggleButton.Rollover?.ChangeOutColor(color);
    }
}
public sealed class ClientOptionItem_String : ClientActionItem
{
    public ConfigEntry<string> Config { get; private set; }
    public string Name {get; private set;}
    private ClientOptionItem_String(
        string name,
        string showingName,
        ConfigEntry<string> config,
        string[] selections,
        OptionsMenuBehaviour optionsMenuBehaviour)
    : base(
        showingName,
        optionsMenuBehaviour)
    {
        Name = name;
        Config = config;
        UpdateToggle(selections);
    }

    /// <summary>
    /// Modオプション画面にconfigのトグルを追加します
    /// </summary>
    /// <param name="name">ボタンラベルの翻訳キーとボタンのオブジェクト名</param>
    /// <param name="showingName"></param>
    /// <param name="config">対応するconfig</param>
    /// <param name="optionsMenuBehaviour">OptionsMenuBehaviourのインスタンス</param>
    /// <param name="selections"></param>
    /// <param name="additionalOnClickAction">クリック時に追加で発火するアクション．configが変更されたあとに呼ばれる</param>
    /// <returns>作成したアイテム</returns>
    public static ClientOptionItem_String Create(
        string name,
        string showingName,
        ConfigEntry<string> config,
        OptionsMenuBehaviour optionsMenuBehaviour,
        string[] selections,
        Action additionalOnClickAction = null)
    {
        var item = new ClientOptionItem_String(name, showingName, config, selections, optionsMenuBehaviour);
        item.OnClickAction = () =>
        {
            var currentIndex = Array.IndexOf(selections, config.Value);
            
            if (currentIndex == -1)
            {
                XtremeLogger.Error("wrong index", "ClientOptionItem_String");
                return;
            }
            
            var nextIndex = (currentIndex + 1) % selections.Length;
            showingName = 
            config.Value = selections[nextIndex];
            item.UpdateToggle(selections);
            item.UpdateName(showingName);
            additionalOnClickAction?.Invoke();
        };
        return item;
    }

    public void UpdateToggle(string[] selections)
    {
        if (ToggleButton == null) return;

        var color = Config.Value == selections[0] ? Palette.Purple : Color.magenta;
        if (Config.Value == "HorseMode")
            color = Color.gray;
        ToggleButton.Background.color = color;
        ToggleButton.Rollover?.ChangeOutColor(color);
    }
    public void UpdateName(string name)
    {
        if (ToggleButton == null) return;

        ToggleButton.Text.text = GetString(name);
        if (Config.Value == "HorseMode")
            ToggleButton.Text.text += $"({GetString("Broken")})";

    }
}