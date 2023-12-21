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
public static class DeathEventListenerSystemPatch
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
							GameEvents.RaisePlayerDeath(player, deathEvent);
						}
					}
				}
				else
				{
					AiDamageTakenEventSystemPatch.SummonToGrandparentPlayerCharacter.Remove(deathEvent.Died);
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

[HarmonyPatch(typeof(OnDeathSystem), nameof(OnDeathSystem.DropInventoryOnDeath))]
public static class OnDeathSystemPatch
{
	public static bool Prefix(OnDeathSystem __instance)
	{
		return false;
	}
}


[HarmonyPatch(typeof(OnDeathSystem), nameof(OnDeathSystem.YieldEssenceOnDeath))]
public static class OnDeathSystemPatch2
{
	public static bool Prefix(OnDeathSystem __instance)
	{
		return false;
	}
}

[HarmonyPatch(typeof(LinkMinionToOwnerOnSpawnSystem), nameof(LinkMinionToOwnerOnSpawnSystem.OnUpdate))]
public static class LinkMinionToOwnerOnSpawnSystemPatch
{
	public static void Prefix(LinkMinionToOwnerOnSpawnSystem __instance)
	{
		var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var owner = entity.Read<EntityOwner>().Owner;
			if (owner.Exists())
			{
				if (owner.Has<EntityOwner>())
				{
					var grandParent = owner.Read<EntityOwner>().Owner;
					if (grandParent.Exists())
					{
						if (grandParent.Has<PlayerCharacter>())
						{
							AiDamageTakenEventSystemPatch.SummonToGrandparentPlayerCharacter[entity] = grandParent;
						}
					}
				}
			}
		}
	}
}
