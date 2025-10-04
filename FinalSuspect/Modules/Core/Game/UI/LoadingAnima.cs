using System.Collections;
using BepInEx.Unity.IL2CPP.Utils;
using FinalSuspect.Attributes;

namespace FinalSuspect.Modules.Core.Game.UI;

public static class LoadingAnima
{
    private static GameObject ModLoading;
    private static int currentIndex;

    private static readonly List<Sprite> AllRoleRoleIllustration =
        [];

    private static readonly string[] CI_Order =
    [
        "Crewmate",
        "HnSEngineer",
        "Engineer",
        "GuardianAngel",
        "Scientist",
        "Tracker",
        "Noisemaker",
        "Detective",
        "CrewmateGhost",
        "Impostor",
        "HnSImpostor",
        "Shapeshifter",
        "Phantom",
        "Viper",
        "ImpostorGhost",
    ];

    [PluginModuleInitializer]
    public static void OnInitialization()
    {
        foreach (var role in CI_Order)
        {
            AllRoleRoleIllustration.Add(LoadSprite($"CI_{role}.png", 450f));
        }
    }

    public static void Create(HudManager __instance)
    {
        if (!ModLoading)
        {
            ModLoading = new GameObject("ModLoading") { layer = 5 };
            ModLoading.transform.SetParent(__instance.GameLoadAnimation.transform.parent);

            var Sprite = ModLoading.AddComponent<SpriteRenderer>();
            Sprite.color = Color.white;
            Sprite.flipX = false;
            ModLoading.SetActive(false);
            __instance.StartCoroutine(SwitchRoleIllustration(Sprite));

            var ap = ModLoading.AddComponent<AspectPosition>();
            ap.Alignment = AspectPosition.EdgeAlignments.RightBottom;
            ap.DistanceFromEdge = new Vector3(0.6f, 0.5f, -1000);
            ap.updateAlways = true;

            ModLoading.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
        }

        ModLoading.SetActive(!IsInGame && !IsLobby);
    }

    public static void Create(AccountManager __instance)
    {
        try
        {
            var loading = GameObject.Find("Loading");
            loading.SetActive(false);

            var bgf = GameObject.Find("BackgroundFill");
            var _crewpetWalk0001 = bgf.transform.FindChild("crewpet_walk0001").gameObject;
            var r = _crewpetWalk0001.GetComponent<WaitingRotate>();
            r.speed = 0f;
            var modLoading = new GameObject("Final Suspect Loading Anima") { layer = 5 };
            modLoading.transform.SetParent(_crewpetWalk0001.transform.parent);
            modLoading.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            var sprite = modLoading.AddComponent<SpriteRenderer>();
            sprite.color = Color.white;
            sprite.flipX = false;
            __instance.StartCoroutine(SwitchRoleIllustration(sprite));
            _crewpetWalk0001.SetActive(false);

            var aspectPosition = modLoading.AddComponent<AspectPosition>();
            aspectPosition.Alignment = AspectPosition.EdgeAlignments.RightBottom;
            aspectPosition.DistanceFromEdge = new Vector3(0.6f, 0.5f, -1000);
            aspectPosition.updateAlways = true;
        }
        catch
        {
            /* ignored */
        }
    }

    private static IEnumerator SwitchRoleIllustration(SpriteRenderer renderer)
    {
        while (true)
        {
            if (AllRoleRoleIllustration.Count == 0) yield break;

            renderer.sprite = AllRoleRoleIllustration[currentIndex];
            var p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                var alpha = 1 - p;
                renderer.color = Color.white.AlphaMultiplied(alpha);
                yield return null;
            }

            currentIndex = (currentIndex + 1) % AllRoleRoleIllustration.Count;

            yield return new WaitForSeconds(1f);
            p = 1f;
            while (p > 0f)
            {
                p -= Time.deltaTime * 2.8f;
                renderer.color = Color.white.AlphaMultiplied(p);
                yield return null;
            }
        }
    }
}