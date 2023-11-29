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
using PvpArena.Services;
using ProjectM.UI;
using ProjectM.LightningStorm;
using ProjectM.Sequencer;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using ProjectM.Debugging;
using static ProjectM.Gameplay.Systems.DealDamageSystem;
using System.Runtime.InteropServices;
using ProjectM.CastleBuilding;
using ProjectM.Presentation;
using PvpArena.GameModes;
using ProjectM.Scripting;
using ProjectM.Shared.Systems;
using static ProjectM.SpawnRegionSpawnSystem;
using Unity.Physics.Authoring;
using static ProjectM.HitColliderCast;

namespace PvpArena.Patches;


/*[HarmonyPatch(typeof(SpawnSequenceForEntitySystem_Server), nameof(SpawnSequenceForEntitySystem_Server.OnUpdate))]
public static class SpawnSequenceForEntitySystem_ServerPatch
{
	public static void Prefix(SpawnSequenceForEntitySystem_Server __instance)
	{
		__instance.__OnUpdate_LambdaJob0_entityQuery.LogComponentTypes();
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var spawnSequenceForEntity = entity.Read<SpawnSequenceForEntity>();
			if (spawnSequenceForEntity.Target._Entity.Has<PlayerCharacter>())
			{
				var player = PlayerService.GetPlayerFromCharacter(spawnSequenceForEntity.Target._Entity);
				if (player.IsInCaptureThePancake() && spawnSequenceForEntity.SequenceGuid == Sequences.ShardSequence)
				{
					Unity.Debug.Log("sequence");
				}
			}
		}
	}
}
*/

/*[HarmonyPatch(typeof(MultiplyAbsorbCapByUnitStatsSystem), nameof(MultiplyAbsorbCapByUnitStatsSystem.OnUpdate))]
public static class MultiplyAbsorbCapByUnitStatsSystemPatch
{
	public static void Prefix(MultiplyAbsorbCapByUnitStatsSystem __instance)
	{
		__instance.__OnUpdate_LambdaJob0_entityQuery.LogComponentTypes();
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			entity.LogComponentTypes();
		}
	}
}*/

/*[HarmonyPatch(typeof(AiMoveSystem_Server), nameof(AiMoveSystem_Server.OnUpdate))]
public static class AiMoveSystem_ServerPatch
{
	public static void Prefix(AiMoveSystem_Server __instance)
	{
		__instance.__OnUpdate_LambdaJob0_entityQuery.LogComponentTypes();
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			entity.LogComponentTypes();
		}
	}
}*/

/*[HarmonyPatch(typeof(SetTeamOnSpawnSystem), nameof(SetTeamOnSpawnSystem.OnUpdate))]
public static class OnSpawnedSystemPatch
{
	
	public static void Prefix(SetTeamOnSpawnSystem __instance)
	{	
		//__instance.__OnUpdate_LambdaJob0_entityQuery.LogComponentTypes();
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var position = entity.Read<LocalToWorld>(); //check if this is the one we want
			//entity.LogPrefabName();
			//entity.LogComponentTypes();
		}
	}
}
*/

/*[HarmonyPatch(typeof(Update_ReplaceAbilityOnSlotSystem), nameof(Update_ReplaceAbilityOnSlotSystem.OnUpdate))]
public static class Update_ReplaceAbilityOnSlotSystemPatch
{

	public static void Prefix(Update_ReplaceAbilityOnSlotSystem __instance)
	{
		//__instance._UpdateAddQuery.LogComponentTypes();
		var entities = __instance._UpdateAddQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var buffer = entity.ReadBuffer<AbilityGroupSlotModificationBuffer>();
			for (var i = 0; i < buffer.Length; i++)
			{
				var mod = buffer[i];
				//Plugin.PluginLog.LogInfo("Changing ability bar");
				//mod.NewAbilityGroup.LogPrefabName();
				//Unity.Debug.Log($"priority: {mod.Priority}");
				//Unity.Debug.Log(mod.Slot);
				//Unity.Debug.Log("===");
			}
			//entity.LogComponentTypes();
		}

		entities = __instance.__Update_Remove_entityQuery.ToEntityArray(Allocator.Temp);

		foreach (var entity in entities)
		{
			__instance.__Update_Remove_entityQuery.LogComponentTypes();
			var buffer = entity.ReadBuffer<AbilityGroupSlotModificationBuffer>();
			for (var i = 0; i < buffer.Length; i++)
			{
				var mod = buffer[i];
				//Plugin.PluginLog.LogInfo("Attempting to destroy ability bar");
				*//*mod.NewAbilityGroup.LogPrefabName();
				Unity.Debug.Log($"priority: {mod.Priority}");
				Unity.Debug.Log(mod.Slot);
				Unity.Debug.Log("===");*//*
			}
		}
	}
}*/
/*
[HarmonyPatch(typeof(Destroy_ReplaceAbilityOnSlotSystem), nameof(Destroy_ReplaceAbilityOnSlotSystem.OnUpdate))]
public static class Destroy_ReplaceAbilityOnSlotSystemPatch
{
	public static void Prefix(Destroy_ReplaceAbilityOnSlotSystem __instance)
	{
		__instance.__Destroy_entityQuery.LogComponentTypes();
		var entities = __instance.__Destroy_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			Unity.Debug.Log("hi destruction");
			entity.LogComponentTypes();
		}
	}
}

[HarmonyPatch(typeof(ReplaceAbilityOnSlotSystem), nameof(ReplaceAbilityOnSlotSystem.OnUpdate))]
public static class ReplaceAbilityOnSlotSystemPatch
{
	public static void Prefix(ReplaceAbilityOnSlotSystem __instance)
	{
		__instance.__Spawn_entityQuery.LogComponentTypes();
		var entities = __instance.__Spawn_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var data = entity.Read<ReplaceAbilityOnSlotData>();
			if (data.ModificationEntity.Index > 0)
			{
				Unity.Debug.Log("greetings1");
			}
			Unity.Debug.Log("greetings2");
			//entity.LogComponentTypes();
		}
	}
}
*/

/*[HarmonyPatch(typeof(HitCastColliderSystem_OnUpdate), nameof(HitCastColliderSystem_OnUpdate.OnUpdate))]
public static class CollisionDetectionSystemPatch
{

	public static void Prefix(HitCastColliderSystem_OnUpdate __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			if (__instance._NewHitTriggersCached.Length > 0)
			{
				Unity.Debug.Log("hi");
			}
			else
			{
				entity.LogComponentTypes();
			}
		}
	}
}
*/

[HarmonyPatch(typeof(AnnounceSiegeWeaponSystem), nameof(AnnounceSiegeWeaponSystem.OnUpdate))]
public static class AnnounceSiegeWeaponSystemPatch
{

	public static void Prefix(AnnounceSiegeWeaponSystem __instance)
	{
		__instance._AnnounceSiegeWeaponQuery.LogComponentTypes();
		var entities = __instance._AnnounceSiegeWeaponQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			Unity.Debug.Log("hi");
		}
	}
}

