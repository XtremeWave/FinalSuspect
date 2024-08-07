using HarmonyLib;
using TMPro;
using UnityEngine;

using static FinalSuspect_Xtreme.Translator;

namespace FinalSuspect_Xtreme;

[HarmonyPatch(typeof(AccountTab), nameof(AccountTab.Awake))]
public static class UpdateFriendCodeUIPatch
{
    private static GameObject VersionShower;
    private static GameObject GameHeader;
    private static GameObject CustomGameHeader;

    public static void Prefix(AccountTab __instance)
    {

        string credentialsText = string.Format(GetString("MainMenuCredential"), $"<color={Main.TeamColor}>XtremeWave</color>");
        credentialsText += "\t\t\t";
        string versionText = $"<color={Main.ModColor}>FSX</color> - <color=#C8FF78>v{Main.ShowVersion}</color>";

#if DEBUG
        versionText = $"<color={Main.ModColor}>{ThisAssembly.Git.Branch}</color> - {ThisAssembly.Git.Commit}";
#endif

        credentialsText += versionText;

        var friendCode = GameObject.Find("FriendCode");
        if (friendCode != null && VersionShower == null)
        {
            VersionShower = Object.Instantiate(friendCode, friendCode.transform.parent);
            VersionShower.name = "FinalSuspect_Xtreme Version Shower";
            VersionShower.transform.localPosition = friendCode.transform.localPosition + new Vector3(2.8f, 0f, 0f);
            VersionShower.transform.localScale *= 1.7f;
            var TMP = VersionShower.GetComponent<TextMeshPro>();
            TMP.alignment = TextAlignmentOptions.Right;
            TMP.fontSize = 30f;
            TMP.SetText(credentialsText);
        }

        var newRequest = GameObject.Find("NewRequest");
        if (newRequest != null)
        {
            newRequest.transform.localPosition -= new Vector3(0f, 0f, 10f);
            newRequest.transform.localScale = new Vector3(0.8f, 1f, 1f);
            
        }

        if (GameHeader = GameObject.Find("BarSprite"))
        {
            CustomGameHeader = new();
            CustomGameHeader.transform.SetParent(GameHeader.transform.parent);
            CustomGameHeader.transform.localScale = GameHeader.transform.localScale;
            CustomGameHeader.transform.localPosition = GameHeader.transform.localPosition;

            static void ResetParent(GameObject obj) 
            {
                obj.transform.SetParent(CustomGameHeader.transform);
            }
            GameHeader.ForEachChild((Il2CppSystem.Action<GameObject>)ResetParent);

            GameHeader.SetActive(false);
            Logger.Info($"{GameHeader.active}", "");

        }
    }
    static void CopyObj(GameObject x)
    {
        var obj = Object.Instantiate(x, x.transform.parent);
        obj.name = x.name + "_";
    }
}