using FinalSuspect.Helpers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FinalSuspect.Patches.Game_Vanilla;

[HarmonyPatch]
public class ShipStatusPatch
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Begin))]
    [HarmonyPostfix]
    public static void ShipStatus_Start(ShipStatus __instance)
    {
        if (__instance.name.Contains("Skeld"))
        {
            var speed = HashRandom.Next(-20, -1);
            var starGen = DestroyableSingleton<StarGen>.Instance;
            if (Main.IsAprilFools)
            {
                speed = -50;
                var posi = starGen.gameObject.transform.localPosition;
                starGen.gameObject.transform.localPosition =
                    new Vector3(posi.x, posi.y, -posi.z);
            }

            starGen.SetDirection(new Vector2(speed, 0));
        }
        else if (__instance.name.Contains("Mira"))
        {
            Retry:
            var speed_x = HashRandom.Next(-10, 10);
            var speed_y = HashRandom.Next(-10, 10);
            if (speed_x is 0 || speed_y is 0) goto Retry;
            var cloudGen = DestroyableSingleton<CloudGenerator>.Instance;
            if (Main.IsAprilFools)
            {
                var posi = cloudGen.gameObject.transform.localPosition;
                cloudGen.gameObject.transform.localPosition =
                    new Vector3(posi.x, posi.y, -posi.z);
            }

            cloudGen.SetDirection(new Vector2(speed_x, speed_y));
        }
        else if (__instance.name.Contains("Polus"))
        {
            var speed = HashRandom.Next(1, 20);
            var size = HashRandom.Next(1, 100);
            size /= 1000;
            var snow = DestroyableSingleton<SnowManager>.Instance.particles;
            if (Main.IsAprilFools)
            {
                size = 10;
                speed = 15;
                snow.startColor = new Color(1f, 1f, 1f, 0.15f);
            }
            else if (Main.IsValentines)
            {
                snow.startColor = new Color(0.85f, 0.5f, 0.6f, 1f);
            }
            else if (Main.IsInitialRelease)
            {
                snow.startColor = ColorHelper.FSColor;
            }

            snow.startSpeed = speed;
            snow.startSize = size;
        }
        else if (__instance.name.Contains("Airship"))
        {
            var speed = HashRandom.Next(1, 10);
            var cloudGenerators = Object.FindObjectsOfType<CloudGenerator>();

            foreach (var generator in cloudGenerators)
            {
                if (Main.IsAprilFools)
                {
                    var posi = generator.gameObject.transform.localPosition;
                    generator.gameObject.transform.localPosition =
                        new Vector3(posi.x, posi.y, -posi.z);
                }

                generator.SetDirection(new Vector2(speed, 0));
            }
        }
        else if (__instance.name.Contains("Fungle"))
        {
            var _base = __instance.transform.FindChild("Backgrounds").FindChild("Base");
            var leftColor = _base.FindChild("WaterLeft").gameObject.GetComponent<SpriteRenderer>();
            var rightColor = _base.FindChild("WaterRight").gameObject.GetComponent<SpriteRenderer>();
            var b = HashRandom.Next(0, 5);
            b /= 10;
            var color = leftColor.color;
            color.b = b;
            leftColor.color = color;
            rightColor.color = color;

            if (Main.IsAprilFools)
            {
                var blood = new Color(0.8f, 0f, 0f, 1f);

                _base.FindChild("OverlayTint").gameObject.GetComponent<SpriteRenderer>().color =
                    new Color(0.6f, 0f, 0f, 0.2f);
                leftColor.color = blood;
                rightColor.color = blood;

                var _bgSunSet = __instance.transform.FindChild("FungleSunsetParallax").FindChild("Contents");
                _bgSunSet.FindChild("Stars").gameObject.GetComponent<SpriteRenderer>().color = blood;
                _bgSunSet.FindChild("Sunset").gameObject.GetComponent<SpriteRenderer>().color = blood;

                __instance.transform.FindChild("FungleWavesAnimated(Clone)").FindChild("Waves").gameObject
                    .GetComponent<MeshRenderer>().material.color = blood;
                DestroyableSingleton<HudManager>.Instance.ShadowQuad.GetComponent<MeshRenderer>().material.color =
                    new Color(0.25f, 0.05f, 0.1f, 1f);
            }
        }
    }
}