using System;
using AmongUs.GameOptions;

namespace FinalSuspect.Helpers;

public static class RoleHelper
{
    private static readonly Dictionary<RoleTypes, string> roleColors = new()
    {
        { RoleTypes.CrewmateGhost, "#8CFFFF" },
        { RoleTypes.GuardianAngel, "#8CFFDB" },
        { RoleTypes.Crewmate, "#8CFFFF" },
        { RoleTypes.Scientist, "#F8FF8C" },
        { RoleTypes.Engineer, "#A5A8FF" },
        { RoleTypes.Noisemaker, "#FFC08C" },
        { RoleTypes.Tracker, "#93FF8C" },
        { RoleTypes.ImpostorGhost, "#FF1919" },
        { RoleTypes.Impostor, "#FF1919" },
        { RoleTypes.Shapeshifter, "#FF819E" },
        { RoleTypes.Phantom, "#CA8AFF" },
        { RoleTypes.Detective, "#70A1DA" },
        { RoleTypes.Viper, "#F06762" }
    };

    public static RoleTypes GetRoleType(byte id)
    {
        return GetRoleById(id);
    }

    public static bool IsImpostor(this RoleTypes role)
    {
        return role switch
        {
            RoleTypes.Impostor
                or RoleTypes.Shapeshifter
                or RoleTypes.Phantom
                or RoleTypes.ImpostorGhost
                or RoleTypes.Viper => true,
            _ => false
        };
    }

    public static bool IsGhost(RoleTypes role)
    {
        return role switch
        {
            RoleTypes.ImpostorGhost or RoleTypes.CrewmateGhost or RoleTypes.GuardianAngel => true,
            _ => false
        };
    }

    public static string GetRoleName(RoleTypes role)
    {
        return GetRoleString(Enum.GetName(typeof(RoleTypes), role));
    }

    public static Color GetRoleColor(RoleTypes role)
    {
        roleColors.TryGetValue(role, out var hexColor);
        _ = ColorUtility.TryParseHtmlString(hexColor, out var c);
        return c;
    }

    public static string GetRoleColorCode(RoleTypes role)
    {
        roleColors.TryGetValue(role, out var hexColor);
        return hexColor;
    }

    public static string GetRoleInfoForVanilla(this RoleTypes role, bool roleHelp = false)
    {
        return IsNormalGame ? GetNormalGameRoleInfo(role, roleHelp) : GetHideNSeekRoleInfo(role, roleHelp);
    }

    private static string GetNormalGameRoleInfo(RoleTypes role, bool roleHelp)
    {
        var text = role.ToString();

        if (!roleHelp || role is RoleTypes.Crewmate or RoleTypes.Impostor)
        {
            return GetString($"{text}Blurb");
        }

        return $"{GetString($"RolesHelp_{text}_01")}\n{GetString($"RolesHelp_{text}_02")}";
    }

    private static string GetHideNSeekRoleInfo(RoleTypes role, bool roleHelp)
    {
        var text = role.ToString();

        if (!roleHelp)
        {
            return GetString($"HnS{text}Blurb");
        }

        return role switch
        {
            RoleTypes.Engineer => GetCrewmateRules(),
            RoleTypes.Impostor => GetImpostorRules(),
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, $"Unsupported role type: {role}")
        };
    }

    private static string GetCrewmateRules() =>
        string.Join("\n",
            GetString(StringNames.RuleOneCrewmates),
            GetString(StringNames.RuleTwoCrewmates),
            GetString(StringNames.RuleThreeCrewmates));

    private static string GetImpostorRules() =>
        string.Join("\n",
            GetString(StringNames.RuleOneImpostor),
            GetString(StringNames.RuleTwoImpostor),
            GetString(StringNames.RuleThreeImpostor));
}