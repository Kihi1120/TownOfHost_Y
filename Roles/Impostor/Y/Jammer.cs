using AmongUs.GameOptions;

using TownOfHostY.Roles.Core;
using TownOfHostY.Roles.Core.Interfaces;

namespace TownOfHostY.Roles.Impostor;
public sealed class Jammer : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Jammer),
            player => new Jammer(player),
            CustomRoles.Jammer,
            () => RoleTypes.Shapeshifter,
            CustomRoleTypes.Impostor,
            (int)Options.offsetId.ImpY + 1500,//後ほど...
            SetUpOptionItem,
            "ジャマー"
        );
    public Jammer(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        KillCooldown = OptionKillCooldown.GetFloat();
        ShapeshiftCount = OptionShapeshiftCount.GetInt();
        DownSpeed = OptionDownSpeed.GetFloat();
        SaboDownSpeedTime = OptionSaboDownSpeedTime.GetFloat();
        ShapeDownSpeedTime = OptionShapeDownSpeedTime.GetFloat();
    }
    private static OptionItem OptionKillCooldown;
    private static OptionItem OptionShapeshiftCount;
    public static OptionItem OptionDownSpeed;
    private static OptionItem OptionSaboDownSpeedTime;
    private static OptionItem OptionShapeDownSpeedTime;

    enum OptionName
    {
        JammerShapeshiftMaxCount,
        JammerDownSpeed,
        JammerSaboDownSpeedTime,
        JammerShapeDownSpeedTime,
    }
    private static float KillCooldown;
    int ShapeshiftCount;
    private static float DownSpeed;
    private static float SaboDownSpeedTime;
    private static float ShapeDownSpeedTime;


    private static void SetUpOptionItem()
    {
        //OptionCanDeadReport = BooleanOptionItem.Create(RoleInfo, 10, OptionName.ShapeKillerCanDeadReport, true, false);
        OptionKillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, new(2.5f, 180f, 2.5f), 30f, false)
                .SetValueFormat(OptionFormat.Seconds);
        OptionShapeshiftCount = IntegerOptionItem.Create(RoleInfo, 11, OptionName.JammerShapeshiftMaxCount, new(1, 3, 1), 1, false)
        .SetValueFormat(OptionFormat.Times);
        OptionDownSpeed = FloatOptionItem.Create(RoleInfo, 12, OptionName.JammerDownSpeed, new(0.1f, 1f, 0.1f), 1f, false)
               .SetValueFormat(OptionFormat.Multiplier);
        OptionSaboDownSpeedTime = FloatOptionItem.Create(RoleInfo, 13, OptionName.JammerSaboDownSpeedTime, new(1f, 180f, 1f), 5f, false)
               .SetValueFormat(OptionFormat.Seconds);
        OptionShapeDownSpeedTime = FloatOptionItem.Create(RoleInfo, 14, OptionName.JammerShapeDownSpeedTime, new(1f, 180f, 1f), 5f, false)
        .SetValueFormat(OptionFormat.Seconds);
    }
}