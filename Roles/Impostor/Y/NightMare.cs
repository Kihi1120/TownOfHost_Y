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
            (int)Options.offsetId.ImpY + 1400,
            SetupOptionItem,
            "ナイトメアー"
        );
    public NightMare(PlayerControl player)
    : base(
        RoleInfo,
        player)
    {

    }
    enum OptionName
    {
    }
    public static void SetupOptionItem()
    {
    }
}