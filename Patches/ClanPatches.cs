using HarmonyLib;
using Unity.Collections;
using ProjectM.Network;
using ProjectM.Gameplay.Clan;
using PvpArena.Services;
using PvpArena.GameModes;
using System;

namespace PvpArena.Patches;


[HarmonyPatch(typeof(ClanSystem_Server), nameof(ClanSystem_Server.OnUpdate))]
public static class ClanSystem_ServerPatch
{
	public static void Prefix(ClanSystem_Server __instance)
	{
		var inviteEventEntities = __instance._InvitePlayerToClanQuery.ToEntityArray(Allocator.Temp);
		var kickEventEntities = __instance._KickRequestQuery.ToEntityArray(Allocator.Temp);
		var leaveEventEntities = __instance._LeaveClanEventQuery.ToEntityArray(Allocator.Temp);

		foreach (var entity in inviteEventEntities)
		{
			try
			{
				var fromCharacter = entity.Read<FromCharacter>();
				var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				GameEvents.RaisePlayerInvitedToClan(player, entity);
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
				continue;
			}
		}

		foreach (var entity in kickEventEntities)
		{
			try
			{
				var fromCharacter = entity.Read<FromCharacter>();
				var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				GameEvents.RaisePlayerKickedFromClan(player, entity);
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
				continue;
			}
		}

		foreach (var entity in leaveEventEntities)
		{
			try
			{
				var fromCharacter = entity.Read<FromCharacter>();
				var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				GameEvents.RaisePlayerLeftClan(player, entity);
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
				continue;
			}
		}

		inviteEventEntities.Dispose();
		kickEventEntities.Dispose();
		leaveEventEntities.Dispose();
	}
}

