using System.Collections.Generic;
using UnityEngine;
using TownOfHost.Roles.Core;
using static TownOfHost.Options;
using Epic.OnlineServices.Presence;
using Il2CppSystem.Reflection;

namespace TownOfHost.Roles.AddOns.Common;

public static class AddBait
{
    private static readonly int Id = 77500;
    private static Color RoleColor = Utils.GetRoleColor(CustomRoles.AddBait);
    public static string SubRoleMark = Utils.ColorString(RoleColor, "Ｂ");
    private static List<byte> playerIdList = new();

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.Addons, CustomRoles.AddBait);
    }
    public static void Init()
    {
        playerIdList = new();
    }
    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
    }
    public static void OnMurderPlayer(MurderInfo info)
    {
        var (killer, target) = info.AttemptTuple;

        if (playerIdList.Contains(target.PlayerId) && !info.IsSuicide)
            new LateTask(() =>
            {
                killer.CmdReportDeadBody(target.Data);
            }, 0.15f, "AddBait Self Report");
    }

    public static bool IsEnable => playerIdList.Count > 0;
    public static bool IsThisRole(byte playerId) => playerIdList.Contains(playerId);
}