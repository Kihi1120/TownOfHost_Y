using AmongUs.GameOptions;
using System.Linq;
using System.Collections.Generic;
using TownOfHostY.Roles.Core;
using TownOfHostY.Roles.Core.Interfaces;
using TownOfHostY.Patches.ISystemType;

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
        JammerTarget = byte.MaxValue;
        PreviousTarget = null;
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
    public byte JammerTarget;
    private PlayerControl PreviousTarget;


    private static void SetUpOptionItem()
    {
        //OptionCanDeadReport = BooleanOptionItem.Create(RoleInfo, 10, OptionName.ShapeKillerCanDeadReport, true, false);
        OptionKillCooldown = FloatOptionItem.Create(RoleInfo, 10, GeneralOption.KillCooldown, new(2.5f, 180f, 2.5f), 30f, false)
                .SetValueFormat(OptionFormat.Seconds);
        OptionShapeshiftCount = IntegerOptionItem.Create(RoleInfo, 11, OptionName.JammerShapeshiftMaxCount, new(1, 10, 1), 1, false)
        .SetValueFormat(OptionFormat.Times);
        OptionDownSpeed = FloatOptionItem.Create(RoleInfo, 12, OptionName.JammerDownSpeed, new(0.1f, 1f, 0.1f), 1f, false)
               .SetValueFormat(OptionFormat.Multiplier);
        OptionSaboDownSpeedTime = FloatOptionItem.Create(RoleInfo, 13, OptionName.JammerSaboDownSpeedTime, new(1f, 180f, 1f), 5f, false)
               .SetValueFormat(OptionFormat.Seconds);
        OptionShapeDownSpeedTime = FloatOptionItem.Create(RoleInfo, 14, OptionName.JammerShapeDownSpeedTime, new(1f, 180f, 1f), 5f, false)
        .SetValueFormat(OptionFormat.Seconds);
    }
    public float CalculateKillCooldown() => KillCooldown;
    public override bool OnInvokeSabotage(SystemTypes systemType)
    {
        if (Player.IsAlive() && JammerTarget == byte.MaxValue)
        {                                                                               //ジャマーが生きていて、JammerTargetに登録済みでない場合。
            var rand = IRandom.Instance;
            List<PlayerControl> targetPlayers = Main.AllAlivePlayerControls.ToList();
            if (targetPlayers.Any())
            {
                var target = targetPlayers[rand.Next(0, targetPlayers.Count)];          //リスト内の中からランダムに1人選択する。
                var NormalSpeed = Main.AllPlayerSpeed[target.PlayerId];                      //選択したターゲットの現在の移動速度を一時的に保存
                Logger.Info("ダウンスピード先:" + target.GetNameWithRole(), "Jammer");
                JammerTarget = target.PlayerId;
                Main.AllPlayerSpeed[JammerTarget] *= DownSpeed;
                target.MarkDirtySettings();
                _ = new LateTask(() =>                                                  //「SaboDownSpeedTime」で指定された時間後に実行される。
                {
                    Main.AllPlayerSpeed[target.PlayerId] = NormalSpeed;                      //ターゲットに選択されたプレイヤーの移動速度を元に値に戻す
                    target.MarkDirtySettings();
                }, SaboDownSpeedTime, "Jammer SabotageDownSpeed");
                JammerTarget = byte.MaxValue;
            }
            else                                                                        //ターゲットが0ならアップ先をプレイヤーをnullに
            {
                JammerTarget = byte.MaxValue;
                Logger.SendInGame("Error.JammerNullException");
                Logger.Warn("スピードダウン先がnullです。", "Jammer");
            }
        }
        return true;
    }
    public override bool OnCheckShapeshift(PlayerControl target, ref bool animate)
    {
        if (ShapeshiftCount == 0 || target.Is(CustomRoleTypes.Impostor) || target == PreviousTarget) return false;       //Countが0より少ない、またはターゲットが味方の場合は処理しない。
        var JammerShapeshiftTarget = target;                                                // ターゲットの情報を保持。
        var NormalSpeed = Main.AllPlayerSpeed[JammerShapeshiftTarget.PlayerId];             // 選択したターゲットの現在の移動速度を一時的に保存.
        Main.AllPlayerSpeed[JammerShapeshiftTarget.PlayerId] *= DownSpeed;
        JammerShapeshiftTarget.MarkDirtySettings();
        ShapeshiftCount--;
        PreviousTarget = JammerShapeshiftTarget;
        _ = new LateTask(() =>
            {
                Main.AllPlayerSpeed[JammerShapeshiftTarget.PlayerId] = NormalSpeed;         // ターゲットに選択されたプレイヤーの移動速度を元に値に戻す
                JammerShapeshiftTarget.MarkDirtySettings();
            }, ShapeDownSpeedTime, "Jammer ShapeshiftDownSpeed");
        return false;//モーションのカット
    }
    public override bool OnFlipSwitch(SwitchSystem switchSystem, PlayerControl player, bool isSabotage, ElectricSwitches switches, bool wasOn)
    {
        var unfixedBit = switchSystem.ActualSwitches ^ switchSystem.ExpectedSwitches;

        Logger.Info($"actual  :{System.Convert.ToString(switchSystem.ActualSwitches, 2).PadLeft(8, '0')}", "RepairDamage");
        Logger.Info($"expected:{System.Convert.ToString(switchSystem.ExpectedSwitches, 2).PadLeft(8, '0')}", "RepairDamage");
        Logger.Info($"Unfixed :{System.Convert.ToString(unfixedBit, 2).PadLeft(8, '0')}", "RepairDamage");

        //妨害するスイッチ
        var brakeBit = 1 << IRandom.Instance.Next(3);

        //妨害するスイッチが直すスイッチと一致していない、かつそのスイッチが直っているなら妨害
        if (brakeBit != (byte)switches && (brakeBit & unfixedBit) == 0)
        {
            switchSystem.ActualSwitches ^= (byte)brakeBit;
        }
        return true;
    }
}