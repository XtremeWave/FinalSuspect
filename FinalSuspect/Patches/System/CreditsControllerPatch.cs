using System.Collections.Generic;
using System.Linq;
using FinalSuspect.Helpers;
using HarmonyLib;
using static FinalSuspect.Modules.Core.Plugin.Translator;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(CreditsController))]
public class CreditsControllerPatch
{
    private static List<CreditsController.CreditStruct> GetModCredits()
    {
        var devList = new List<string>()
            {
                $"<size=120%><color={ColorHelper.ModColor}>{Main.ModName}</color></size>",
                $"<color=#fffcbe>By</color> <color={ColorHelper.TeamColor}>XtremeWave</color>",
                " ",
                "   ",
                //Others
                $"<size=120%>{GetString("Contributors")}</size>",
                "- KARPED1EM",
                "- Niko233",
                "- Amongus(水木年华)",
                "- Yu(Night_瓜)",
                "- 天寸梦初",
                "<color=#cdfffd>- QingFeng</color>"

            };

        var credits = new List<CreditsController.CreditStruct>();

        AddPersonToCredits(devList);
        AddSpcaeToCredits();

        //AddTitleToCredits(GetString("Translator"));
        //AddSpcaeToCredits();

        //AddTitleToCredits(GetString("Acknowledgement"));
        //AddPersonToCredits(acList);
        //AddSpcaeToCredits();

        return credits;

        void AddSpcaeToCredits()
        {
            AddTitleToCredits(string.Empty);
        }
        void AddTitleToCredits(string title)
        {
            credits.Add(new()
            {
                format = "title",
                columns = new[] { title },
            });
        }
        void AddPersonToCredits(List<string> list)
        {
            foreach (var line in list)
            {
                var cols = line.Split(" - ").ToList();
                if (cols.Count < 2) cols.Add(string.Empty);
                credits.Add(new()
                {
                    format = "person",
                    columns = cols.ToArray(),
                });
            }
        }
    }

    [HarmonyPatch(nameof(CreditsController.AddCredit)), HarmonyPrefix]
    public static void AddCreditPrefix(CreditsController __instance, [HarmonyArgument(0)] CreditsController.CreditStruct originalCredit)
    {
        if (originalCredit.columns[0] != "logoImage") return;

        foreach (var credit in GetModCredits())
        {
            __instance.AddCredit(credit);
            __instance.AddFormat(credit.format);
        }
    }
}