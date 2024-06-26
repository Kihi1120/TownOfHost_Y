using TownOfHostY.Roles.Core;
using TownOfHostY.Modules;
using UnityEngine;
using static Il2CppSystem.Uri;

namespace TownOfHostY.CatchCat;

static class Option
{
    public static readonly int Id = (int)Options.offsetId.GModeCC;
    public static OptionItem IgnoreReport;

    public static OptionItem LeaderIgnoreVent;
    public static OptionItem LeaderKilled;
    public static OptionItem LK_CatCount;
    public static OptionItem LK_OneGuard;

    public static OptionItem WhenColorCatKilled;
    public static OptionItem ColorCatShowSameCamp;
    public static OptionItem TaskCompleteAbility;
    public static OptionItem T_KnowAllLeader;
    public static OptionItem T_OneGuardOwn;
    public static OptionItem T_CanUseVent;
    public static OptionItem T_VentCooldown;
    public static OptionItem T_VentMaxTime;
    public static OptionItem T_OwnLeaderKillcoolDecrease;

    public static OptionItem M_LeaderRemain;
    public static OptionItem M_NeutralCatRemain;
    public static OptionItem M_RemainCatShowName;
    public static OptionItem M_RemainCatShowNameNum;
    public static OptionItem M_ColorCatCount;

    public enum ColorCatKill
    {
        CCatJustKill,
        COtherCatKill,
        CCatOverride,
        CCatAlwaysGuard,
    };

    public static void SetupCustomOption()
    {
        TextOptionItem.Create(Id + 1000, "CCCommonSetting", TabGroup.MainSettings)
            .SetColor(Palette.LightBlue)
            .SetGameMode(CustomGameMode.CatchCat);
        IgnoreReport = BooleanOptionItem.Create(Id + 1010, "IgnoreReport", false, TabGroup.MainSettings, false)
            .SetColor(Palette.LightBlue)
            .SetGameMode(CustomGameMode.CatchCat);

        TextOptionItem.Create(Id + 2000, "CCLeaderSetting", TabGroup.MainSettings)
            .SetColor(Palette.Orange)
            .SetGameMode(CustomGameMode.CatchCat);
        SetupLeaderRoleOptions(Id + 2100, CustomRoles.CCRedLeader);
        SetupLeaderRoleOptions(Id + 2200, CustomRoles.CCBlueLeader);
        SetupAddLeaderRoleOptions(Id + 2300, CustomRoles.CCYellowLeader);
        LeaderIgnoreVent = BooleanOptionItem.Create(Id + 2400, "IgnoreLeaderVent", false, TabGroup.MainSettings, false)
            .SetHeader(true)
            .SetColor(Palette.Orange)
            .SetGameMode(CustomGameMode.CatchCat);
        LeaderKilled = BooleanOptionItem.Create(Id + 2500, "CCLeaderKilled", false, TabGroup.MainSettings, false)
            .SetColor(Palette.Orange)
            .SetGameMode(CustomGameMode.CatchCat);
        LK_CatCount = IntegerOptionItem.Create(Id + 2510, "CCLK_CatCount", new(0, 7, 1), 0, TabGroup.MainSettings, false).SetParent(LeaderKilled)
            .SetValueFormat(OptionFormat.Players)
            .SetGameMode(CustomGameMode.CatchCat);
        LK_OneGuard = BooleanOptionItem.Create(Id + 2520, "CCLK_OneGuard", false, TabGroup.MainSettings, false).SetParent(LeaderKilled)
            .SetGameMode(CustomGameMode.CatchCat);

        TextOptionItem.Create(Id + 3000, "CCCatSetting", TabGroup.MainSettings)
            .SetColor(Color.gray)
            .SetGameMode(CustomGameMode.CatchCat);
        WhenColorCatKilled = StringOptionItem.Create(Id + 3100, "CCWhenColorCatKilled", EnumHelper.GetAllNames<ColorCatKill>(), 0, TabGroup.MainSettings, false)
            .SetColor(Color.gray)
            .SetGameMode(CustomGameMode.CatchCat);
        ColorCatShowSameCamp = BooleanOptionItem.Create(Id + 3200, "CCColorCatShowSameCamp", false, TabGroup.MainSettings, false)
            .SetColor(Color.gray)
            .SetGameMode(CustomGameMode.CatchCat);
        TaskCompleteAbility = BooleanOptionItem.Create(Id + 3300, "CCTaskCompleteAbility", true, TabGroup.MainSettings, false)
            .SetColor(Color.gray)
            .SetGameMode(CustomGameMode.CatchCat);
        T_KnowAllLeader = IntegerOptionItem.Create(Id + 3310, "CCT_KnowAllLeader", new(0, 100, 10), 0, TabGroup.MainSettings, false).SetParent(TaskCompleteAbility)
            .SetValueFormat(OptionFormat.Percent)
            .SetGameMode(CustomGameMode.CatchCat);
        T_OwnLeaderKillcoolDecrease = IntegerOptionItem.Create(Id + 3320, "CCT_OwnLeaderKillcoolDecrease", new(0, 100, 10), 0, TabGroup.MainSettings, false).SetParent(TaskCompleteAbility)
            .SetValueFormat(OptionFormat.Percent)
            .SetGameMode(CustomGameMode.CatchCat);
        T_OneGuardOwn = IntegerOptionItem.Create(Id + 3330, "CCT_OneGuardOwn", new(0, 100, 10), 0, TabGroup.MainSettings, false).SetParent(TaskCompleteAbility)
            .SetValueFormat(OptionFormat.Percent)
            .SetGameMode(CustomGameMode.CatchCat);
        T_CanUseVent = IntegerOptionItem.Create(Id + 3340, "CCT_CanUseVent", new(0, 100, 10), 0, TabGroup.MainSettings, false).SetParent(TaskCompleteAbility)
            .SetValueFormat(OptionFormat.Percent)
            .SetGameMode(CustomGameMode.CatchCat);
        T_VentCooldown = FloatOptionItem.Create(Id + 3341, "VentCooldown", new(5f, 60f, 2.5f), 20f, TabGroup.MainSettings, false).SetParent(T_CanUseVent)
            .SetValueFormat(OptionFormat.Seconds)
            .SetGameMode(CustomGameMode.CatchCat);
        T_VentMaxTime = FloatOptionItem.Create(Id + 3342, "VentMaxTime", new(1f, 10f, 1f), 3f, TabGroup.MainSettings, false).SetParent(T_CanUseVent)
            .SetValueFormat(OptionFormat.Seconds)
            .SetGameMode(CustomGameMode.CatchCat);

        TextOptionItem.Create(Id + 5000, "CCMeetingDisplay", TabGroup.MainSettings)
            .SetColor(Color.cyan)
            .SetGameMode(CustomGameMode.CatchCat);
        M_LeaderRemain = BooleanOptionItem.Create(Id + 5010, "CCM_LeaderRemain", true, TabGroup.MainSettings, false)
            .SetColor(Color.cyan)
            .SetGameMode(CustomGameMode.CatchCat);
        M_NeutralCatRemain = BooleanOptionItem.Create(Id + 5020, "CCM_NeutralCatRemain", true, TabGroup.MainSettings, false)
            .SetColor(Color.cyan)
            .SetGameMode(CustomGameMode.CatchCat);
        M_RemainCatShowName = BooleanOptionItem.Create(Id + 5030, "CCM_RemainCatShowName", false, TabGroup.MainSettings, false)
            .SetColor(Color.cyan)
            .SetGameMode(CustomGameMode.CatchCat);
        M_RemainCatShowNameNum = IntegerOptionItem.Create(Id + 5031, "CCM_RemainCatShowNameNum", new(1, 13, 1), 2, TabGroup.MainSettings, false).SetParent(M_RemainCatShowName)
            .SetValueFormat(OptionFormat.Players)
            .SetGameMode(CustomGameMode.CatchCat);
        M_ColorCatCount = BooleanOptionItem.Create(Id + 5040, "CCM_ColorCatCount", false, TabGroup.MainSettings, false)
            .SetColor(Color.cyan)
            .SetGameMode(CustomGameMode.CatchCat);
    }
    private static void SetupLeaderRoleOptions(int id, CustomRoles role)
    {
        var spawnOption = IntegerOptionItem.Create(id, role.ToString() + "Fixed", new(100, 100, 1), 100, TabGroup.MainSettings, false).SetColor(Utils.GetRoleColor(role))
            .SetValueFormat(OptionFormat.Percent)
            .SetFixValue(true)
            .SetGameMode(CustomGameMode.CatchCat) as IntegerOptionItem;
        var countOption = IntegerOptionItem.Create(id + 1, "Maximum", new(1, 1, 1), 1, TabGroup.MainSettings, false).SetParent(spawnOption)
            .SetValueFormat(OptionFormat.Players)
            .SetGameMode(CustomGameMode.CatchCat);

        Options.CustomRoleSpawnChances.Add(role, spawnOption);
        Options.CustomRoleCounts.Add(role, countOption);
    }
    private static void SetupAddLeaderRoleOptions(int id, CustomRoles role)
    {
        var spawnOption = IntegerOptionItem.Create(id, role.ToString(), new(0, 100, 100), 0, TabGroup.MainSettings, false).SetColor(Utils.GetRoleColor(role))
            .SetValueFormat(OptionFormat.Percent)
            .SetGameMode(CustomGameMode.CatchCat) as IntegerOptionItem;
        var countOption = IntegerOptionItem.Create(id + 1, "Maximum", new(1, 1, 1), 1, TabGroup.MainSettings, false).SetParent(spawnOption)
            .SetHidden(true)
            .SetGameMode(CustomGameMode.CatchCat);

        Options.CustomRoleSpawnChances.Add(role, spawnOption);
        Options.CustomRoleCounts.Add(role, countOption);
    }
}