using PvpArena.Commands;
using HarmonyLib;
using Il2CppSystem;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Bloodstone.API;
using PvpArena.Data;
using ProjectM.Gameplay.Systems;
using System.Collections.Generic;
using ProjectM.Network;
using Stunlock.Network;
using PvpArena.Matchmaking;
using ProjectM.Gameplay.Clan;
using PvpArena.Services;
using static ProjectM.Network.ClanEvents_Client;
using PvpArena.GameModes;

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
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			GameEvents.RaisePlayerInvitedToClan(player, entity);
		}

		foreach (var entity in kickEventEntities)
		{
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			GameEvents.RaisePlayerKickedFromClan(player, entity);
		}

		foreach (var entity in leaveEventEntities)
		{
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			GameEvents.RaisePlayerLeftClan(player, entity);
		}

		inviteEventEntities.Dispose();
		kickEventEntities.Dispose();
		leaveEventEntities.Dispose();
	}
}

