using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TownOfHost.Roles.Core;
using static TownOfHost.Options;

namespace TownOfHost.Roles.AddOns.Common;

public static class TieBreaker
{
    private static readonly int Id = 77100;
    private static Color RoleColor = Utils.GetRoleColor(CustomRoles.TieBreaker);
    public static string SubRoleMark = Utils.ColorString(RoleColor, "Ｔ");
    private static List<byte> playerIdList = new();
    private static Dictionary<byte, byte> TieBreakerVote = new();

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.Addons, CustomRoles.TieBreaker);
    }
    public static void Init()
    {
        playerIdList = new();
        TieBreakerVote = new();
    }
    public static void Add(byte playerId)
    {
        if (!playerIdList.Contains(playerId))
            playerIdList.Add(playerId);
    }

    public static void OnVote(byte voter, byte votedFor)
    {
        if (!playerIdList.Contains(voter)) return;

        Logger.Info($"{Utils.GetPlayerById(voter).GetNameWithRole()} が タイブレーカー投票({Utils.GetPlayerById(votedFor).GetNameWithRole()})", "TieBreaker");
        TieBreakerVote.Add(voter, votedFor);
    }
    public static (bool, GameData.PlayerInfo) BreakingVote(bool IsTie, GameData.PlayerInfo Exiled, Dictionary<byte, int> votedCounts, int maxVoteNum)
    {
        if (IsTie)
        {
            var tiebreakerUse = false;
            var tiebreakerCollision = false;
            foreach (var data in votedCounts.Where(x => x.Value == maxVoteNum))
            {
                if (TieBreakerVote.ContainsValue(data.Key))
                {
                    if (tiebreakerUse) tiebreakerCollision = true;
                    Exiled = Utils.GetPlayerInfoById(data.Key);
                    tiebreakerUse = true;
                    Logger.Info($"{Exiled.PlayerName}がTieBreakerで優先", "TieBreaker");
                }
            }
            if (tiebreakerCollision)
            {
                Logger.Info("TieBreakerの衝突", "TieBreaker");
                Exiled = null;
            }
            else
                IsTie = false;
        }
        TieBreakerVote.Clear();

        return (IsTie, Exiled);
    }

    public static bool IsEnable => playerIdList.Count > 0;
    public static bool IsThisRole(byte playerId) => playerIdList.Contains(playerId);
}