using AmongUs.GameOptions;

using TownOfHost.Roles.Core;
using TownOfHost.Roles.Core.Interfaces;

namespace TownOfHost.Roles.Impostor;
public sealed class NormalShapeshifter : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
         SimpleRoleInfo.Create(
            typeof(NormalShapeshifter),
            player => new NormalShapeshifter(player),
            CustomRoles.NormalShapeshifter,
            () => RoleTypes.Shapeshifter,
            CustomRoleTypes.Impostor,
            995,
            SetupOptionItem,
            "シェイプシフター"
        );
    public NormalShapeshifter(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        shapeshiftDuration = OptionShapeshiftDuration.GetFloat();
        shapeshifterLeaveSkin = OptionShapeshifterLeaveSkin.GetBool();
    }
    private static OptionItem OptionShapeshiftDuration;
    private static OptionItem OptionShapeshifterLeaveSkin;
    enum OptionName
    {
        ShapeshiftDuration,
        ShapeshifterLeaveSkin,
    }
    private static float shapeshiftDuration;
    private static bool shapeshifterLeaveSkin;

    public static void SetupOptionItem()
    {
        OptionShapeshiftDuration = FloatOptionItem.Create(RoleInfo, 3, OptionName.ShapeshiftDuration, new(0f, 180f, 5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionShapeshifterLeaveSkin = BooleanOptionItem.Create(RoleInfo, 4, OptionName.ShapeshifterLeaveSkin, false, false);
    }

    public override void ApplyGameOptions(IGameOptions opt)
    {
        AURoleOptions.ShapeshifterDuration = shapeshiftDuration;
        AURoleOptions.ShapeshifterLeaveSkin = shapeshifterLeaveSkin;
    }
}