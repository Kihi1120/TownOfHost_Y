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
    private float NormalKillCrewVision;　　　 //通常キル時のクルー陣営の視界
    private float DarkSeconds;               //通常キル時の暗転する秒数
    private bool IsAccelerated;  　　　　　　 //加速済みかフラグ
    public static PlayerControl Killer;
    private static bool NameColorRED = false; // NameColorRED フラグ
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
        if (Utils.IsActive(SystemTypes.Electrical) && !IsAccelerated)
        { //停電中で加速済みでない時。
            IsAccelerated = true;
            Main.AllPlayerSpeed[Player.PlayerId] += SpeedInLightsOut;//Mareの速度を加算
        }
        else if (!Utils.IsActive(SystemTypes.Electrical) && IsAccelerated)
        { //停電中ではなく加速済みになっている場合
            IsAccelerated = false;
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
                //停電解除されたらキルモード解除
                NameColorRED = false;
            }
        }
    }
    public void OnCheckMurderAsKiller(MurderInfo info)
    {
        (var killer, var target) = info.AttemptTuple;
        if (killer.Is(CustomRoles.NightMare) && Utils.IsActive(SystemTypes.Electrical))
        {                                                                           //キルした際に停電中ならキルクールを短く...
            NameColorRED = true;
            Killer = killer;
            Logger.Info($"{killer?.Data?.PlayerName}: 停電中にキルに成功", "NightMare");
            Main.AllPlayerKillCooldown[killer.PlayerId] = KillCooldownInLightsOut;  //キルクールを設定する。
            killer.SyncSettings();                                                  //キルクール処理を同期
            NameColorManager.Add(Killer.PlayerId, Killer.PlayerId, RoleInfo.RoleColorCode);
        }
    }
    public static bool KnowTargetRoleColor(PlayerControl target, bool isMeeting)
        => !isMeeting && NameColorRED && target.Is(CustomRoles.NightMare);
}