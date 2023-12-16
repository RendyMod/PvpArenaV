using ProjectM;
using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;
using PvpArena.Data;
using PvpArena.GameModes;
using PvpArena.Configs;
using PvpArena.Helpers;
using PvpArena.Models;
using static ProjectM.DeathEventListenerSystem;

namespace PvpArena.Services;


public static class PlayerSpawnHandler
{
	public static Dictionary<Player, bool> PlayerFirstTimeSpawn = new Dictionary<Player, bool>();
	public static Dictionary<Player, bool> PlayerIsDead = new Dictionary<Player, bool>();

	public static void Initialize()
	{
		GameEvents.OnPlayerBuffRemoved += HandleOnPlayerUnbuffed;
		GameEvents.OnPlayerSpawning += HandleOnPlayerSpawning;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
	}

	public static void Dispose()
	{
		GameEvents.OnPlayerBuffRemoved -= HandleOnPlayerUnbuffed;
		GameEvents.OnPlayerSpawning -= HandleOnPlayerSpawning;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
	}

	private static void HandleOnPlayerUnbuffed(Player player, Entity buffEntity)
	{
		if (buffEntity.Read<PrefabGUID>() == Prefabs.HideCharacterBuff)
		{
			if (PlayerFirstTimeSpawn.TryGetValue(player, out var firstTimeSpawn) && firstTimeSpawn)
			{
				HandleOnFirstSpawn(player);
				PlayerFirstTimeSpawn.Remove(player);
			}
			else if (PlayerIsDead.TryGetValue(player, out var playerIsDead) && playerIsDead)
			{
				GameEvents.RaisePlayerRespawn(player);
				PlayerIsDead[player] = false;
			}
		}
	}

	private static void HandleOnPlayerSpawning(Player player, SpawnCharacter spawnCharacter)
	{
		if (spawnCharacter.FirstTimeSpawn && !PlayerFirstTimeSpawn.TryGetValue(player, out var firstTimeSpawn) && !firstTimeSpawn)
		{
			PlayerFirstTimeSpawn[player] = true;
		}
	}

	private static void HandleOnPlayerDeath(Player player, DeathEvent deathEvent)
	{
		PlayerIsDead[player] = true;
	}

	// Helper method to schedule actions with delay.
	private static void ScheduleAction(System.Action<Player> action, Player player, int frameDelay)
	{
		var scheduledAction = () => action(player);
		ActionScheduler.RunActionOnceAfterFrames(scheduledAction, frameDelay);
	}

	private static void HandleOnFirstSpawn(Player player)
	{
		if (PvpArenaConfig.Config.UseCustomSpawnLocation)
		{
			float3 pos = PvpArenaConfig.Config.CustomSpawnLocation.ToFloat3();
			var alreadyTeleported = pos == player.Position;
			if (!(alreadyTeleported.x && alreadyTeleported.z))
			{
				player.Teleport(pos);
			}
		}

		GiveJewelsAndScheduleEquipment(player);
		Helper.SetPlayerBlood(player, Prefabs.BloodType_Warrior);

		Helper.ApplyBuildImpairBuffToPlayer(player); //if a player connects before they have a character, some of our on-connect logic won't be able to work, so it's duplicated here
		if(PlayerService.OnlinePlayers.TryAdd(player, true)) //this is only needed for re-made characters
			PlayerService.OnOnlinePlayerAmountChanged?.Invoke();
	}

	private static void GiveJewelsAndScheduleEquipment(Player player)
	{
		Helper.GiveDefaultJewels(player);
		ScheduleAction(Helper.GiveDefaultLegendaries, player, frameDelay: 3);
		ScheduleAction(Helper.GiveArmorAndNecks, player, frameDelay: 4);
	}


}

