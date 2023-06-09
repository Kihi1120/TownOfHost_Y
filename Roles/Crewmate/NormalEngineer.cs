using AmongUs.GameOptions;

using TownOfHost.Roles.Core;

namespace TownOfHost.Roles.Crewmate;
public sealed class NormalEngineer : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        new(
            typeof(NormalEngineer),
            player => new NormalEngineer(player),
            CustomRoles.NormalEngineer,
            () => RoleTypes.Engineer,
            CustomRoleTypes.Crewmate,
            19990,
            SetupOptionItem,
            "doc",
            "#8cffff"
        );
    public NormalEngineer(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        ventCooldown = OptionVentCooldown.GetFloat();
        ventMaxTime = OptionVentMaxTime.GetFloat();
    }

    private static OptionItem OptionVentCooldown;
    private static OptionItem OptionVentMaxTime;
    public static float ventCooldown;
    public static float ventMaxTime;

    private static void SetupOptionItem()
    {
        OptionVentCooldown = FloatOptionItem.Create(RoleInfo, 3, GeneralOption.VentCooldown, new(0f, 180f, 5f), 30f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionVentMaxTime = FloatOptionItem.Create(RoleInfo, 4, GeneralOption.VentMaxTime, new(0f, 180f, 5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.EngineerCooldown = ventCooldown;
        AURoleOptions.EngineerInVentMaxTime = ventMaxTime;
    }
}