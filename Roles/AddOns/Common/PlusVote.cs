using System.Collections.Generic;
using UnityEngine;
using TownOfHost.Roles.Core;
using static TownOfHost.Options;

namespace TownOfHost.Roles.AddOns.Common;

public static class PlusVote
{
    private static readonly int Id = 77300;
    private static Color RoleColor = Utils.GetRoleColor(CustomRoles.PlusVote);
    public static string SubRoleMark = Utils.ColorString(RoleColor, "Ｐ");
    private static List<byte> playerIdList = new();

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.Addons, CustomRoles.PlusVote);
    }
    public static void Init()
    {
        playerIdList = new();
    }
    public static void Add(byte playerId)
    {
        if (!playerIdList.Contains(playerId))
            playerIdList.Add(playerId);
    }
    public static int OnVote(byte voter, int numVotes)
    {
        if (playerIdList.Contains(voter)) numVotes += 1;

        return numVotes;
    }

    public static bool IsEnable => playerIdList.Count > 0;
    public static bool IsThisRole(byte playerId) => playerIdList.Contains(playerId);

}