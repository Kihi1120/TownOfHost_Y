using AmongUs.GameOptions;
using System.Linq;

using TownOfHostY.Roles.Core;
using TownOfHostY.Roles.Core.Class;
using TownOfHostY.Roles.Core.Interfaces;

namespace TownOfHostY.Roles.Impostor;
public sealed class NightMare : RoleBase, IImpostor
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
        OriginalCrewVision = OptionNormalKillCrewVision.GetFloat();
        IsAccelerated = false;
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
    private float SpeedInLightsOut;          //停電時の移動速度の加速値
    private float KillCooldownInLightsOut;   //停電時のキルクール
    public static float NormalKillCrewVision;//通常キル時のクルー陣営の視界
    public static float DarkSeconds;         //通常キル時の暗転する秒数
    private bool IsAccelerated;  　　　　　　 //加速済みかフラグ
    private float OriginalCrewVision;        //クルーメイトの視界を保持する為の物。
    public static PlayerControl Killer;
    private static bool NameColorRED = false; // NameColorRED フラグ
    private static bool OnDefaultKill = false;
    private static bool OnElecKill = false;
    public static void SetupOptionItem()
    {
        OptionSpeedInLightsOut = FloatOptionItem.Create(RoleInfo, 10, OptionName.NightMareSpeedInLightsOut, new(1.2f, 10.0f, 0.2f), 1.2f, false)
            .SetValueFormat(OptionFormat.Multiplier);
        OptionKillCooldownInLightsOut = FloatOptionItem.Create(RoleInfo, 11, OptionName.NightMareKillCooldownInLightsOut, new(0.0f, 180f, 2.5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionNormalKillCrewVision = FloatOptionItem.Create(RoleInfo, 12, OptionName.NightMareNormalKillCrewVision, new(0.0f, 3.0f, 0.1f), 0.1f, false)
            .SetValueFormat(OptionFormat.Multiplier);
        OptionDarkSeconds = FloatOptionItem.Create(RoleInfo, 13, OptionName.NightMareDarkSeconds, new(0, 100, 1), 7, false)
        .SetValueFormat(OptionFormat.Seconds);
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        if (Utils.IsActive(SystemTypes.Electrical) && !IsAccelerated && OnElecKill)
        { //停電中にkillをして加速済みでない時。
            IsAccelerated = true;
            OnElecKill = true;
            Main.AllPlayerSpeed[Player.PlayerId] += SpeedInLightsOut;//Mareの速度を加算
        }
        else if (!Utils.IsActive(SystemTypes.Electrical) && IsAccelerated)
        { //停電中ではなく加速済みになっている場合
            IsAccelerated = false;
            OnElecKill = false;
            Main.AllPlayerSpeed[Player.PlayerId] -= SpeedInLightsOut;//Mareの速度を減算
        }
        else if (!Utils.IsActive(SystemTypes.Electrical) && NameColorRED)
        { //停電中ではなく名前の色が変わっている場合
            NameColorRED = false;
        }
    }
    public override void OnFixedUpdate(PlayerControl player)
    {
        if (GameStates.IsInTask && NameColorRED)
        {
            if (!Utils.IsActive(SystemTypes.Electrical))
            {
                //停電解除されたら名前の色を元に戻す。
                NameColorRED = false;
            }
        }
    }
    public void OnCheckMurderAsKiller(MurderInfo info)
    {
        (var killer, var target) = info.AttemptTuple;
        if (killer.Is(CustomRoles.NightMare) && Utils.IsActive(SystemTypes.Electrical))
        {                                                                           //キルした際に停電中ならキルクールを短く...
            OnElecKill = true;
            NameColorRED = true;
            Killer = killer;
            Logger.Info($"{killer?.Data?.PlayerName}: 停電中にキルに成功", "NightMare");
            Main.AllPlayerKillCooldown[killer.PlayerId] = KillCooldownInLightsOut;  //キルクールを設定する。
            killer.SyncSettings();                                                  //キルクール処理を同期
            NameColorManager.Add(Killer.PlayerId, Killer.PlayerId, RoleInfo.RoleColorCode);
        }
        else
        {                                                                           //通常時のキルなら視界を操作する為トリガーを作る。
            Player.RpcResetAbilityCooldown();　　　　　　　                          //通常のキルクールタイムにする。
            OnDefaultKill = true;
        }
    }
    public override void AfterMeetingTasks()
    {
        OnDefaultKill = false;
        if (Player.IsAlive())
        {//生存していたらキルクールリセット
            Player.RpcResetAbilityCooldown();
            _ = new LateTask(() =>
            {
                //まだ停電が直っていなければ10秒後に名前の色が変わる。
                if (Utils.IsActive(SystemTypes.Electrical))
                {
                    NameColorRED = true;
                }
            }, 10.0f, "NightMare NameColorRED");
        }
    }
    public static void ApplyGameOptionsByOther(byte id, IGameOptions opt)
    {

    }
    public static bool KnowTargetRoleColor(PlayerControl target, bool isMeeting)
        => !isMeeting && NameColorRED && target.Is(CustomRoles.NightMare);
}