using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = Il2CppSystem.Object;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch]
public static class ServerDropDownPatch
{
    [HarmonyPatch(typeof(ServerDropdown), nameof(ServerDropdown.FillServerOptions))]
    [HarmonyPrefix]
    internal static bool FillServerOptions_Prefix(ServerDropdown __instance)
    {
        if (SceneManager.GetActiveScene().name == "FindAGame") return true;
        const int maxPerColumn = 6; // 每列最大按钮数
        const float columnWidth = 4.15f; // 列宽度
        const float buttonSpacing = 0.5f; // 按钮间距


        __instance.background.size = new Vector2(5, 1);

        var num = 0;
        var column = 0;

        var regions = DestroyableSingleton<ServerManager>.Instance.AvailableRegions
            .OrderBy(ServerManager.DefaultRegions.Contains).ToList();
        var totalColumns = Mathf.Max(1, Mathf.CeilToInt(regions.Count / (float)maxPerColumn));

        var maxRows = regions.Count > maxPerColumn ? maxPerColumn : regions.Count;

        foreach (var regionInfo in regions)
        {
            if (DestroyableSingleton<ServerManager>.Instance.CurrentRegion.Name == regionInfo.Name)
            {
                __instance.defaultButtonSelected = __instance.firstOption;
                __instance.firstOption.ChangeButtonText(
                    DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(regionInfo.TranslateName,
                        regionInfo.Name, new Il2CppReferenceArray<Object>(0)));
                continue;
            }

            // 创建服务器按钮
            var region = regionInfo;
            var serverListButton = __instance.ButtonPool.Get<ServerListButton>();

            // 按钮位置
            var xPos = (column - (totalColumns - 1) / 2f) * columnWidth;
            var yPos = __instance.y_posButton - buttonSpacing * (num % maxPerColumn);

            // 按钮位置和缩放
            serverListButton.transform.localPosition = new Vector3(xPos, yPos, -1f);
            serverListButton.transform.localScale = Vector3.one;

            // 设置按钮
            serverListButton.Text.text = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(
                regionInfo.TranslateName,
                regionInfo.Name,
                new Il2CppReferenceArray<Object>(0));
            serverListButton.Text.ForceMeshUpdate();
            serverListButton.Button.OnClick.RemoveAllListeners();
            serverListButton.Button.OnClick.AddListener((Action)(() => __instance.ChooseOption(region)));
            __instance.controllerSelectable.Add(serverListButton.Button);

            num++;
            if (num % maxPerColumn == 0) column++;
        }

        // 调整背景大小和位置
        var backgroundHeight = 1.2f + buttonSpacing * (maxRows - 1);
        var backgroundWidth = totalColumns > 1
            ? columnWidth * (totalColumns - 1) + __instance.background.size.x
            : __instance.background.size.x;

        __instance.background.transform.localPosition = new Vector3(
            0f,
            __instance.initialYPos - (backgroundHeight - 1.2f) / 2f,
            0f);
        __instance.background.size = new Vector2(backgroundWidth, backgroundHeight);

        return false;
    }

    [HarmonyPatch(typeof(ServerDropdown), nameof(ServerDropdown.FillServerOptions))]
    [HarmonyPostfix]
    internal static void FillServerOptions_Postfix(ServerDropdown __instance)
    {
        // 仅在搜索界面生效
        if (SceneManager.GetActiveScene().name != "FindAGame") return;

        const float buttonSpacing = 0.6f;
        const float columnSpacing = 7.2f;

        // 按钮按Y轴排序
        List<ServerListButton> allButtons =
        [
            .. __instance.GetComponentsInChildren<ServerListButton>()
                .OrderByDescending(b => b.transform.localPosition.y)
        ];
        if (allButtons.Count == 0)
            return;

        const int buttonsPerColumn = 7;
        var columnCount = (allButtons.Count + buttonsPerColumn - 1) / buttonsPerColumn;
        Vector3 startPosition = new(0, -buttonSpacing, 0);

        for (var i = 0; i < allButtons.Count; i++)
        {
            var col = i / buttonsPerColumn;
            var row = i % buttonsPerColumn;
            allButtons[i].transform.localPosition =
                startPosition + new Vector3(col * columnSpacing, -row * buttonSpacing, 0f);
        }

        // 计算背景大小和位置
        var maxRows = Math.Min(buttonsPerColumn, allButtons.Count);
        var backgroundHeight = 1.2f + buttonSpacing * (maxRows - 1);
        var backgroundWidth = columnCount > 1 ? columnSpacing * (columnCount - 1) + 5 : 5;

        __instance.background.transform.localPosition = new Vector3(
            0f,
            __instance.initialYPos - (backgroundHeight - 1.2f) / 2f,
            0f);
        __instance.background.size = new Vector2(backgroundWidth, backgroundHeight);
        __instance.background.transform.localPosition += new Vector3(4f, 0, 0);
    }
}