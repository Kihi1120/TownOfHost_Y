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
    }
    enum OptionName
    {
    }
    private static void SetUpOptionItem()
    {
    }
}