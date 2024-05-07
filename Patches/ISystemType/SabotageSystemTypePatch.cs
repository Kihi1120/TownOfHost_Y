using System;
using HarmonyLib;
using Hazel;
using TownOfHostY.Attributes;
using TownOfHostY.Modules;
using TownOfHostY.Roles.Core;
using TownOfHostY.Roles.Core.Interfaces;

namespace TownOfHostY.Patches.ISystemType;

[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
public static class SabotageSystemTypeUpdateSystemPatch
{
    private static bool isCooldownModificationEnabled;
    private static float modifiedCooldownSec;
    private static readonly LogHandler logger = Logger.Handler(nameof(SabotageSystemType));

    [GameModuleInitializer]
    public static void Initialize()
    {
        isCooldownModificationEnabled = Options.ModifySabotageCooldown.GetBool();
        modifiedCooldownSec = Options.SabotageCooldown.GetFloat();
    }

    public static bool Prefix([HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] MessageReader msgReader)
    {
        byte amount;
        {
            var newReader = MessageReader.Get(msgReader);
            amount = newReader.ReadByte();
            newReader.Recycle();
        }

        var nextSabotage = (SystemTypes)amount;
        logger.Info($"PlayerName: {player.GetNameWithRole()}, SabotageType: {nextSabotage}");

        //HASモードではサボタージュ不可
        if (Options.CurrentGameMode == CustomGameMode.HideAndSeek || Options.IsStandardHAS) return false;
        if (Options.IsCCMode) return false;
        if (!CustomRoleManager.OnSabotage(player, nextSabotage))
        {
            return false;
        }
        var roleClass = player.GetRoleClass();
        if (roleClass is IKiller killer)
        {
            //そもそもサボタージュボタン使用不可ならサボタージュ不可
            if (!killer.CanUseSabotageButton()) return false;
            //その他処理が必要であれば処理
            return roleClass.OnInvokeSabotage(nextSabotage);
        }
        else
        {
            return CanSabotage(player);
        }
    }
    private static bool CanSabotage(PlayerControl player)
    {
        //サボタージュ出来ないキラー役職はサボタージュ自体をキャンセル
        if (!player.Is(CustomRoleTypes.Impostor))
        {
            return false;
        }
        return true;
    }
    //[HarmonyPatch(typeof(SwitchSystem), nameof(SwitchSystem.RepairDamage))]
    public static class SwitchSystemRepairDamagePatch
    {
        private static LogHandler logger = Logger.Handler(nameof(SwitchSystem));
        public static bool Prefix(SwitchSystem __instance, [HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] byte amount)
        {
            if (!AmongUsClient.Instance.AmHost)
            {
                return true;
            }

            var isSabotage = amount.HasBit(SwitchSystem.DamageSystem);
            // サボタージュならこのあと下がる配電盤の位置に変換する : 配電盤操作ならamount分だけ1を左にずらす
            // ref: SwitchSystem.RepairDamage
            var switchedKnobs = (ElectricSwitches)(isSabotage ? amount & SwitchSystem.SwitchesMask /* 0b_11111 */ : 0b_00001 << amount);

            if (isSabotage)
            {
                logger.Info($"{player.GetNameWithRole()} による配電盤サボタージュ OFFにされたスイッチ: {switchedKnobs}");
            }
            else
            {
                logger.Info($"{player.GetNameWithRole()} による配電盤操作: {switchedKnobs}");
            }

            return true;
        }
    }
    public static void Postfix(SabotageSystemType __instance, bool __runOriginal /* Prefixの結果，本体処理が実行されたかどうか */ )
    {
        if (!__runOriginal || !isCooldownModificationEnabled || !AmongUsClient.Instance.AmHost)
        {
            return;
        }
        // サボタージュクールダウンを変更
        __instance.Timer = modifiedCooldownSec;
        __instance.IsDirty = true;
    }
}

[HarmonyPatch(typeof(ElectricTask), nameof(ElectricTask.Initialize))]
public static class ElectricTaskInitializePatch
{
    public static void Postfix()
    {
        Utils.MarkEveryoneDirtySettings();
        if (!GameStates.IsMeeting)
            Utils.NotifyRoles(ForceLoop: true);
    }
}
/// <summary>
/// 配電盤のツマミを左から順にABCDEと名付け，ツマミやその組み合わせを表現する
/// </summary>
[Flags]
public enum ElectricSwitches : byte
{
    A = 0b_00001,
    B = 0b_00010,
    C = 0b_00100,
    D = 0b_01000,
    E = 0b_10000,

    /// <summary>全て</summary>
    All = 0b_11111,
    /// <summary>なし</summary>
    None = 0b_00000,
}
[HarmonyPatch(typeof(ElectricTask), nameof(ElectricTask.Complete))]
public static class ElectricTaskCompletePatch
{
    public static void Postfix()
    {
        Utils.MarkEveryoneDirtySettings();
        if (!GameStates.IsMeeting)
            Utils.NotifyRoles(ForceLoop: true);
    }
}
