using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Bloodstone.API;
using ProjectM.Gameplay.Systems;
using System.Collections.Generic;
using ProjectM.Network;
using PvpArena.Services;
using PvpArena.Data;
using PvpArena.GameModes;
using PvpArena.Configs;
using PvpArena.Helpers;
using UnityEngine.Jobs;
using PvpArena.Models;
using System;

namespace PvpArena.Patches;


[HarmonyPatch(typeof(DeathEventListenerSystem), nameof(DeathEventListenerSystem.OnUpdate))]
public static class DeathAndSpawnPatches
{
	public static void Postfix(DeathEventListenerSystem __instance)
	{
		foreach (var killCall in __instance._OnKillCalls)
		{
			if (killCall.Killed.Has<PlayerCharacter>())
			{
				var Player = PlayerService.GetPlayerFromCharacter(killCall.Killed);
				GameEvents.RaisePlayerDeath(Player, killCall);
				SpawnCharacterSystemPatch.HasRespawned[Player.User] = false;
			}
			else
			{
				GameEvents.RaiseUnitDeath(killCall.Killed, killCall);
			}
		}
	}
}

[HarmonyPatch(typeof(VampireDownedServerEventSystem), nameof(VampireDownedServerEventSystem.OnUpdate))]
public static class VampireDownedPatch
{
	public static void Postfix(VampireDownedServerEventSystem __instance)
	{
		var downedEvents = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in downedEvents)
		{
			if (!VampireDownedServerEventSystem.TryFindRootOwner(entity, 1, VWorld.Server.EntityManager, out var victimEntity))
			{
				Plugin.PluginLog.LogMessage("Couldn't get victim entity");
				return;
			}

			var downBuff = entity.Read<VampireDownedBuff>();

			if (!VampireDownedServerEventSystem.TryFindRootOwner(downBuff.Source, 1, VWorld.Server.EntityManager, out var killerEntity))
			{
				Plugin.PluginLog.LogMessage("Couldn't get killer entity");
				return;
			}

			var VictimPlayer = PlayerService.GetPlayerFromCharacter(victimEntity);
			GameEvents.RaisePlayerDowned(VictimPlayer, killerEntity);
		}
		downedEvents.Dispose();
	}
}

/*[HarmonyPatch(typeof(RespawnCharacterSystem), nameof(RespawnCharacterSystem.OnUpdate))]
public static class RespawnCharacterSystemPatch
{
	public static Dictionary<Player, bool> PlayerIsAlive = new Dictionary<Player, bool>();
	public static void Prefix(RespawnCharacterSystem __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob1_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var respawnCharacter = entity.Read<RespawnCharacter>();
			if (!entity.Read<Health>().IsDead)
			{
				var player = PlayerService.GetPlayerFromCharacter(entity);
				//GameEvents.RaisePlayerRespawn(player);
			}
			
		}
	}
}*/

[HarmonyPatch(typeof(SpawnCharacterSystem), nameof(SpawnCharacterSystem.OnUpdate))]
public static class SpawnCharacterSystemPatch
{
	public static Dictionary<Entity, bool> FirstTimeSpawn = new Dictionary<Entity, bool>();
	public static Dictionary<Entity, bool> HasRespawned = new Dictionary<Entity, bool>();

	public static void Prefix(SpawnCharacterSystem __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var spawnCharacter = entity.Read<SpawnCharacter>();

			var Character = spawnCharacter.User.Read<User>().LocalCharacter._Entity;
			
			if (!spawnCharacter.FirstTimeSpawn && spawnCharacter.HasSpawned)
			{
				try
				{
					if (!HasRespawned.TryGetValue(entity, out bool hasRespawned) || !hasRespawned)
					{
						var player = PlayerService.GetPlayerFromUser(spawnCharacter.User);
						GameEvents.RaisePlayerRespawn(player);
						HasRespawned[entity] = true;
					}
				}
				catch (Exception e)
				{
					Plugin.PluginLog.LogInfo("error on first spawn: " + e.ToString());
				}
			}
			//this hook will run multiple times during spawn, and the character won't be set at the beginning. Wait until it's set and then record it so we only run once
			if (spawnCharacter.FirstTimeSpawn && !FirstTimeSpawn.ContainsKey(spawnCharacter.User) && Character.Index > 0)
			{
				FirstTimeSpawn[spawnCharacter.User] = true;

				var player = PlayerService.GetPlayerFromUser(spawnCharacter.User);

				HandleOnFirstSpawn(player);
				if (PvpArenaConfig.Config.UseCustomSpawnLocation)
				{
					float3 pos = PvpArenaConfig.Config.CustomSpawnLocation.ToFloat3();
					var alreadyTeleported = pos == spawnCharacter.User.Read<LocalToWorld>().Position;
					if (!(alreadyTeleported.x && alreadyTeleported.z))
					{
						player.Teleport(pos);
					}
				}
				continue;
			}
		}
		entities.Dispose();
	}

	// Helper method to schedule actions with delay.
	private static void ScheduleAction(System.Action<Player> action, Player player, int delay)
	{
		var scheduledAction = new ScheduledAction(action, new object[] { player });
		ActionScheduler.ScheduleAction(scheduledAction, delay);
	}


	private static void HandleOnFirstSpawn(Player player)
	{
		// Check if the character has their abilities unlocked, if not, reschedule this method.
		if (!Helper.UserHasAbilitiesUnlocked(player.User))
		{
			ScheduleAction(HandleOnFirstSpawn, player, delay: 50);
			return;
		}

		// Abilities are now unlocked, proceed to give jewels.
		GiveJewelsAndScheduleEquipment(player);
		Helper.SetPlayerBlood(player, Prefabs.BloodType_Warrior);
	}

	private static void GiveJewelsAndScheduleEquipment(Player player)
	{
		var power = Helper.Clamp(1, 0, PvpArenaConfig.Config.MaxJewelPower);

		var steamId = player.SteamID;
		// Generate jewels for the character.
		foreach (var jewel in JewelData.abilityToPrefabDictionary)
		{
			if (PlayerJewels.JewelData.ContainsKey(player.SteamID))
			{
				if (PlayerJewels.JewelData[steamId].ContainsKey(jewel.Key))
				{
					Helper.GenerateJewelViaEvent(player, jewel.Key, PlayerJewels.JewelData[steamId][jewel.Key], power);
					continue;
				}
			}
			Helper.GenerateJewelViaEvent(player, jewel.Key, PvpArenaConfig.Config.DefaultJewels[jewel.Key].Mods, power);	
		}
		ScheduleAction(EquipJewels, player, delay: 2);
		ScheduleAction(Helper.GiveDefaultLegendaries, player, delay: 3);
		ScheduleAction(Helper.GiveArmorAndNecks, player, delay: 4);
	}

	public static void EquipJewels(Player player)
	{
		int inventoryIndex = 0;
		foreach (var jewel in JewelData.abilityToPrefabDictionary)
		{
			Helper.EquipJewelAtSlot(player, inventoryIndex);
			inventoryIndex++;
		}	
	}
}

