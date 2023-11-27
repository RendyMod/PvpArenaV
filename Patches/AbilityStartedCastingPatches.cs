using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Bloodstone.API;
using PvpArena.Data;
using System.Collections.Generic;
using ProjectM.Network;
using PvpArena.Services;
using PvpArena.GameModes;
using static ProjectM.CastleBuilding.CastleBlockSystem;
using UnityEngine.TextCore;
using PvpArena.Helpers;
using PvpArena.Models;
using ProjectM.UI;

namespace PvpArena.Patches;

[HarmonyPatch(typeof(AbilityCastStarted_SpawnPrefabSystem_Server), nameof(AbilityCastStarted_SpawnPrefabSystem_Server.OnUpdate))]
public static class AbilityCastStarted_SpawnPrefabSystem_ServerPatch
{

	public static void Prefix(AbilityCastStarted_SpawnPrefabSystem_Server __instance)
	{
		//__instance.__OnUpdate_LambdaJob0_entityQuery.LogComponentTypes();
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var abilityCastStartedEvent = entity.Read<AbilityCastStartedEvent>();

			if (abilityCastStartedEvent.Character.Has<PlayerCharacter>())
			{
				var player = PlayerService.GetPlayerFromCharacter(abilityCastStartedEvent.Character);
				GameEvents.RaisePlayerStartedCasting(player, entity);
			}
		}
		entities.Dispose();
	}
}
