using AmongUs.GameOptions;
using TownOfHost.Roles.Core;

namespace TownOfHost.Roles.Crewmate;
public sealed class NiceWatcher : RoleBase
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(NiceWatcher),
            player => new NiceWatcher(player),
            CustomRoles.NiceWatcher,
            () => RoleTypes.Crewmate,
            CustomRoleTypes.Crewmate,
            49900,
            null,
            "ナイスウォッチャー",
            "#800080"
        );
    public NiceWatcher(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
    }

    public override void ApplyGameOptions(IGameOptions opt)
    {
        opt.SetBool(BoolOptionNames.AnonymousVotes, false);
    }
}