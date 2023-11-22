using AmongUs.GameOptions;
using Hazel;

using TownOfHostY.Roles.Core;
using TownOfHostY.Roles.Core.Interfaces;
using static TownOfHostY.Options;

namespace TownOfHostY.Roles.Impostor;

public sealed class Mare : RoleBase, IImpostor
{
    public static readonly SimpleRoleInfo RoleInfo =
        SimpleRoleInfo.Create(
            typeof(Mare),
            player => new Mare(player),
            CustomRoles.Mare,
            () => RoleTypes.Impostor,
            CustomRoleTypes.Impostor,
            (int)Options.offsetId.ImpTOH + 1100,
            SetupCustomOption,
            "メアー",
            assignInfo: new(CustomRoles.Mare, CustomRoleTypes.Impostor)
            {
                IsInitiallyAssignableCallBack = () => ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Electrical, out var systemType) && systemType.TryCast<SwitchSystem>(out _),  // 停電が存在する
            }
        );
    public Mare(PlayerControl player)
    : base(
        RoleInfo,
        player
    )
    {
        KillCooldownInLightsOut = OptionKillCooldownInLightsOut.GetFloat();
        SpeedInLightsOut = OptionSpeedInLightsOut.GetFloat();

        IsActivateKill = false;
        IsAccelerated = false;

    }

    private static OptionItem OptionKillCooldownInLightsOut;
    private static OptionItem OptionSpeedInLightsOut;
    enum OptionName
    {
        MareAddSpeedInLightsOut,
        MareKillCooldownInLightsOut,
    }
    private float KillCooldownInLightsOut;
    private float SpeedInLightsOut;
    private static bool IsActivateKill;
    private bool IsAccelerated;  //加速済みかフラグ

    public static void SetupCustomOption()
    {
        OptionSpeedInLightsOut = FloatOptionItem.Create(RoleInfo, 10, OptionName.MareAddSpeedInLightsOut, new(0.2f, 10.0f, 0.2f), 0.3f, false)
        .SetValueFormat(OptionFormat.Multiplier);
            OptionKillCooldownInLightsOut = FloatOptionItem.Create(RoleInfo, 11, OptionName.MareKillCooldownInLightsOut, new(2.5f, 180f, 2.5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
    }
    public bool CanUseKillButton() => IsActivateKill;
    public float CalculateKillCooldown() => IsActivateKill ? KillCooldownInLightsOut : DefaultKillCooldown;
    public override void ApplyGameOptions(IGameOptions opt)
    {
        if (IsActivateKill && !IsAccelerated)
        { //停電中で加速済みでない場合
            IsAccelerated = true;
            Main.AllPlayerSpeed[Player.PlayerId] += SpeedInLightsOut;//Mareの速度を加算
        }
        else if (!IsActivateKill && IsAccelerated)
        { //停電中ではなく加速済みになっている場合
            IsAccelerated = false;
            Main.AllPlayerSpeed[Player.PlayerId] -= SpeedInLightsOut;//Mareの速度を減算
        }
    }
    private void ActivateKill(bool activate)
    {
        IsActivateKill = activate;
        if (AmongUsClient.Instance.AmHost)
        {
            SendRPC();
            Player.MarkDirtySettings();
            Utils.NotifyRoles();
        }
    }
    public void SendRPC()
    {
        using var sender = CreateSender(CustomRPC.MareSync);
        sender.Writer.Write(IsActivateKill);
    }
    public override void ReceiveRPC(MessageReader reader, CustomRPC rpcType)
    {
        if (rpcType != CustomRPC.MareSync) return;

        IsActivateKill = reader.ReadBoolean();
    }

    public override void OnFixedUpdate(PlayerControl player)
    {
        if (GameStates.IsInTask && IsActivateKill)
        {
            if (!Utils.IsActive(SystemTypes.Electrical))
            {
                //停電解除されたらキルモード解除
                ActivateKill(false);
            }
        }
    }
    public override bool OnSabotage(PlayerControl player, SystemTypes systemType)
    {
        if (systemType == SystemTypes.Electrical)
        {
            _ = new LateTask(() =>
            {
                //まだ停電が直っていなければキル可能モードに
                if (Utils.IsActive(SystemTypes.Electrical))
                {
                    ActivateKill(true);
                }
            }, 4.0f, "Mare Activate Kill");
        }
        return true;
    }
    public static bool KnowTargetRoleColor(PlayerControl target, bool isMeeting)
        => !isMeeting && IsActivateKill && target.Is(CustomRoles.Mare);
}