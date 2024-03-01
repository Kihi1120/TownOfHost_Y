using AmongUs.GameOptions;

using TownOfHostY.Roles.Core;
using TownOfHostY.Roles.Core.Class;
using TownOfHostY.Roles.Core.Interfaces;

namespace TownOfHostY.Roles.Impostor;
public sealed class NightMare : VoteGuesser, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
         SimpleRoleInfo.Create(
            typeof(NightMare),
            player => new NightMare(player),
            CustomRoles.NightMare,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Impostor,
            (int)Options.offsetId.ImpY + 1500,
            SetupOptionItem,
            "ナイトメアー"
        );
    public NightMare(PlayerControl player)
    : base(
        RoleInfo,
        player)
    {
        SpeedInLightsOut = OptionSpeedInLightsOut.GetFloat();
        KillCooldownInLightsOut = OptionKillCooldownInLightsOut.GetFloat();
        NormalKillCrewVision = OptionNormalKillCrewVision.GetFloat();
        DarkSeconds = OptionDarkSeconds.GetFloat();
    }
    private static OptionItem OptionKillCooldownInLightsOut;
    private static OptionItem OptionSpeedInLightsOut;
    private static OptionItem OptionNormalKillCrewVision;
    private static OptionItem OptionDarkSeconds;
    enum OptionName
    {
        NightMareSpeedInLightsOut,
        NightMareKillCooldownInLightsOut,
        NightMareNormalKillCrewVision,
        NightMareDarkSeconds,
    }
    private float SpeedInLightsOut;          //停電時の移動速度
    private float KillCooldownInLightsOut;   //停電時のキルクール
    private float NormalKillCrewVision;　　　 //通常キル時のクルー陣営の視界
    private float DarkSeconds;               //通常キル時の暗転する秒数
    public static void SetupOptionItem()
    {
        OptionSpeedInLightsOut = FloatOptionItem.Create(RoleInfo, 10, OptionName.NightMareSpeedInLightsOut, new(0.1f, 0.5f, 0.1f), 0.3f, false)
            .SetValueFormat(OptionFormat.Multiplier);
        OptionKillCooldownInLightsOut = FloatOptionItem.Create(RoleInfo, 11, OptionName.NightMareKillCooldownInLightsOut, new(2.5f, 180f, 2.5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionNormalKillCrewVision = FloatOptionItem.Create(RoleInfo, 12, OptionName.NightMareNormalKillCrewVision, new(0f, 5f, 0.25f), 2f, false)
            .SetValueFormat(OptionFormat.Multiplier);
        OptionDarkSeconds = FloatOptionItem.Create(RoleInfo, 13, OptionName.NightMareDarkSeconds, new(0, 100, 1), 7, false)
        .SetValueFormat(OptionFormat.Seconds);
    }
}