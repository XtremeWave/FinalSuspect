using FinalSuspect.Helpers;

namespace FinalSuspect.Patches.System;

[HarmonyPatch(typeof(CreditsController))]
public class CreditsControllerPatch
{
    private static List<CreditsController.CreditStruct> GetModCredits()
    {
        var devList = new List<string>
        {
            $"<size=120%><color={ColorHelper.FSColorHex}>{Main.ModName}</color></size>",
            $"<color=#fffcde>By</color> <color={ColorHelper.AuthorColorHex}>Slok</color> & <color=#ffff00>LezaiYa</color>",
            //Others
            $"<size=120%>{GetString("Id.Contributor")}</size>",

            "- Nonalus",
            "- Elinmei",
            "- Yu(Night_瓜)",
            "- QingFeng",
            "- FangKuai",
            "",
            "- KpCam",
            "- 小黄117",
            "- 白糖咖啡",
            "- Zeyan",
            "",
            "- KARPED1EM",
            "- Niko233",
            "- Amongus(水木年华)",
            "- 天寸梦初"
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
            credits.Add(new CreditsController.CreditStruct
            {
                format = "title",
                columns = new[] { title }
            });
        }

        void AddPersonToCredits(List<string> list)
        {
            foreach (var cols in list.Select(line => line.Split(" - ").ToList()))
            {
                if (cols.Count < 2) cols.Add(string.Empty);
                credits.Add(new CreditsController.CreditStruct
                {
                    format = "person",
                    columns = cols.ToArray()
                });
            }
        }
    }

    [HarmonyPatch(nameof(CreditsController.AddCredit))]
    [HarmonyPrefix]
    public static void AddCreditPrefix(CreditsController __instance,
        [HarmonyArgument(0)] CreditsController.CreditStruct originalCredit)
    {
        //这里安卓还有点小问题
        if (originalCredit?.columns[0] != "logoImage") return;

        foreach (var credit in GetModCredits())
        {
            __instance.AddCredit(credit);
            __instance.AddFormat(credit.format);
        }
    }
}