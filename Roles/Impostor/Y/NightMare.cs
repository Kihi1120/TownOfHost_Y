using AmongUs.GameOptions;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using TownOfHostY.Modules;
using TownOfHostY.Roles.Core;
using TownOfHostY.Roles.Core.Class;
using TownOfHostY.Roles.Core.Interfaces;
using UnityEngine;

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
        DarkSeconds = OptionDarkSeconds.GetFloat();
        IsAccelerated = false;
        darkenedPlayers = null;
        darkenTimer = DarkSeconds;
    }
    private static OptionItem OptionKillCooldownInLightsOut;
    private static OptionItem OptionSpeedInLightsOut;
    private static OptionItem OptionDarkSeconds;
    enum OptionName
    {
        NightMareSpeedInLightsOut,
        NightMareKillCooldownInLightsOut,
        NightMareDarkSeconds,
    }
    private float SpeedInLightsOut;             //停電時の移動速度の加速値
    private float KillCooldownInLightsOut;      //停電時のキルクール
    private static float DarkSeconds;           //通常キル時の暗転する秒数
    private bool IsAccelerated;                 //加速済みかフラグ
    public static PlayerControl Killer;
    private static bool NameColorRED = false;   // NameColorRED フラグ
    private static bool OnElecKill = false;
    private PlayerControl[] darkenedPlayers;    // 暗転させたプレイヤー
    private float darkenTimer;                  // 暗転タイマー
    private static LogHandler logger = Logger.Handler(nameof(NightMare));
    public static void SetupOptionItem()
    {
        OptionSpeedInLightsOut = FloatOptionItem.Create(RoleInfo, 10, OptionName.NightMareSpeedInLightsOut, new(1.2f, 10.0f, 0.2f), 1.2f, false)
            .SetValueFormat(OptionFormat.Multiplier);
        OptionKillCooldownInLightsOut = FloatOptionItem.Create(RoleInfo, 11, OptionName.NightMareKillCooldownInLightsOut, new(0.0f, 180f, 2.5f), 15f, false)
            .SetValueFormat(OptionFormat.Seconds);
        OptionDarkSeconds = FloatOptionItem.Create(RoleInfo, 13, OptionName.NightMareDarkSeconds, new(0, 100, 1), 7, false)
        .SetValueFormat(OptionFormat.Seconds);
    }
    public override void ApplyGameOptions(IGameOptions opt)
    {
        if (Utils.IsActive(SystemTypes.Electrical) && !IsAccelerated && OnElecKill)
        { //停電中にkillをして加速済みでない時。
            IsAccelerated = true;
            OnElecKill = true;
            NameColorRED = true;
            Main.AllPlayerSpeed[Player.PlayerId] += SpeedInLightsOut;//Mareの速度を加算
        }
        else if (!Utils.IsActive(SystemTypes.Electrical) && IsAccelerated)
        { //停電中ではなく加速済みになっている場合
            IsAccelerated = false;
            OnElecKill = false;
            NameColorRED = false;
            Main.AllPlayerSpeed[Player.PlayerId] -= SpeedInLightsOut;//Mareの速度を減算
        }
        else if (!Utils.IsActive(SystemTypes.Electrical))
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
        if (darkenedPlayers != null)
        {
            // タイマーを減らす
            darkenTimer -= Time.fixedDeltaTime;
            // タイマーが0になったらみんなの視界を戻してタイマーと暗転プレイヤーをリセットする
            if (darkenTimer <= 0)
            {
                ResetDarkenState();
            }
        }
    }
    public void OnCheckMurderAsKiller(MurderInfo info)
    {
        (var killer, var target) = info.AttemptTuple;
        if (killer.Is(CustomRoles.NightMare) && Utils.IsActive(SystemTypes.Electrical))
        {                                                                           //キルした際に停電中ならキルクールを短く...
            OnElecKill = true;
            Killer = killer;
            Logger.Info($"{killer?.Data?.PlayerName}: 停電中にキルに成功", "NightMare");
            Main.AllPlayerKillCooldown[killer.PlayerId] = KillCooldownInLightsOut;  //キルクールを設定する。
            killer.SyncSettings();                                                  //キルクール処理を同期
            NameColorManager.Add(Killer.PlayerId, Killer.PlayerId, RoleInfo.RoleColorCode);
        }
        else
        {                                                                           //通常時のキルなら視界を操作する為トリガーを作る。
            Player.RpcResetAbilityCooldown();　　　　　　　                          //通常のキルクールタイムにする。
            var playersToDarken = Main.AllAlivePlayerControls.Where(player => !player.Is(CustomRoleTypes.Impostor));
            DarkenPlayers(playersToDarken);
        }
    }
    public override void AfterMeetingTasks()
    {
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
        if (AmongUsClient.Instance.AmHost)
        {
            ResetDarkenState();
        }
    }
    /// <summary>渡されたプレイヤーを<see cref="DarkSeconds"/>秒分視界ゼロにする</summary>
    private void DarkenPlayers(IEnumerable<PlayerControl> playersToDarken)
    {
        darkenedPlayers = playersToDarken.ToArray();
        foreach (var player in playersToDarken)
        {
            PlayerState.GetByPlayerId(player.PlayerId).IsBlackOut = true;
            player.MarkDirtySettings();
        }
        RpcDarken();
    }
    private void ResetDarkenState()
    {
        if (darkenedPlayers != null)
        {
            foreach (var player in darkenedPlayers)
            {
                PlayerState.GetByPlayerId(player.PlayerId).IsBlackOut = false;
                player.MarkDirtySettings();
            }
            darkenedPlayers = null;
        }
        darkenTimer = DarkSeconds;
        RpcResetDarken();
    }
    private void RpcDarken()
    {
        logger.Info($"暗転を開始");
        using var sender = CreateSender(CustomRPC.NightMareDarken);
        sender.Writer.Write(true);
    }
    private void RpcResetDarken()
    {
        logger.Info($"暗転を解除");
        using var sender = CreateSender(CustomRPC.NightMareDarken);
        sender.Writer.Write(false);
    }
    public override void ReceiveRPC(MessageReader reader, CustomRPC rpcType)
    {
        if (rpcType == CustomRPC.NightMareDarken)
        {
            var isDarkened = reader.ReadBoolean();
            if (!isDarkened)
            {
                ResetDarkenState();
            }
        }
    }
    public static bool KnowTargetRoleColor(PlayerControl target, bool isMeeting)
        => !isMeeting && NameColorRED && target.Is(CustomRoles.NightMare);
}