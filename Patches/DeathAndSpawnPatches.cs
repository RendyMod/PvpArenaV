using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Bloodstone.API;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using PvpArena.Services;
using PvpArena.GameModes;
using System;

namespace PvpArena.Patches;


[HarmonyPatch(typeof(DeathEventListenerSystem), nameof(DeathEventListenerSystem.OnUpdate))]
public static class DeathAndSpawnPatches
{
	public static void Postfix(DeathEventListenerSystem __instance)
	{
		var entities = __instance._DeathEventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var deathEvent = entity.Read<DeathEvent>();
			try
			{
				if (deathEvent.Died.Has<PlayerCharacter>())
				{
					if (deathEvent.Died.Has<ControlledBy>())
					{
						var user = deathEvent.Died.Read<ControlledBy>().Controller;
						if (user.Exists())
						{
							var player = PlayerService.GetPlayerFromCharacter(deathEvent.Died);
							Plugin.PluginLog.LogInfo("raising player death");
							GameEvents.RaisePlayerDeath(player, deathEvent);
						}
					}
				}
				else
				{
					GameEvents.RaiseUnitDeath(deathEvent.Died, deathEvent);
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
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
			try
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
				if (victimEntity.Has<ControlledBy>())
				{
					var user = victimEntity.Read<ControlledBy>().Controller;
					if (user.Exists())
					{
						var victimPlayer = PlayerService.GetPlayerFromCharacter(victimEntity);
						GameEvents.RaisePlayerDowned(victimPlayer, killerEntity);
					}
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
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
	public static void Prefix(SpawnCharacterSystem __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var spawnCharacter = entity.Read<SpawnCharacter>();

				if (spawnCharacter.User.Exists())
				{
					var Character = spawnCharacter.User.Read<User>().LocalCharacter._Entity;

					if (Character.Exists())
					{
						var player = PlayerService.GetPlayerFromUser(spawnCharacter.User);
						GameEvents.RaisePlayerSpawning(player, spawnCharacter);
					}
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
		entities.Dispose();
	}
}


[HarmonyPatch(typeof(KillEventSystem), nameof(KillEventSystem.OnUpdate))]
public static class KillEventSystemPatch
{
	public static void Prefix(KillEventSystem __instance)
	{
		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var fromCharacter = entity.Read<FromCharacter>();
			var player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			player.ReceiveMessage("Unstuck is disabled in this game mode.".Error());
			entity.Destroy();
		}
		entities.Dispose();
	}
}
