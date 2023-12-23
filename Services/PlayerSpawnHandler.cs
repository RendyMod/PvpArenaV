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
using PvpArena.Persistence.MySql.MainDatabase;
using PvpArena.Services.Moderation;

namespace PvpArena.Services;


public static class PlayerSpawnHandler
{
	public static Dictionary<Player, bool> PlayerFirstTimeSpawn = new Dictionary<Player, bool>();
	public static Dictionary<Player, bool> PlayerIsDead = new Dictionary<Player, bool>();

	public static void Initialize()
	{
		GameEvents.OnPlayerBuffRemoved += HandleOnPlayerUnbuffed;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
	}

	public static void Dispose()
	{
		GameEvents.OnPlayerBuffRemoved -= HandleOnPlayerUnbuffed;
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

		Core.pointsDataRepository.LoadPointsForPlayerAsync(player).ContinueWith(task =>
			ActionScheduler.RunActionOnMainThread(() => LoginPointsService.TryGrantDailyLoginPoints(player, PvpArenaConfig.Config.PointsPerDailyLogin))
		);
		Core.playerBulletHellDataRepository.LoadDataForPlayerAsync(player);
		Core.muteDataRepository.LoadMuteInfoForPlayerAsync(player);
		Core.imprisonDataRepository.LoadDataForPlayerAsync(player).ContinueWith(task =>
		{
			ActionScheduler.RunActionOnMainThread(() =>
			{
				if (player.IsImprisoned())
				{
					player.Teleport(PrisonConfig.Config.CellCoordinateList[player.ImprisonInfo.PrisonCellNumber].ToFloat3());
				}
			});
		});

		if (PlayerService.OnlinePlayers.Add(player))
		{
			PlayerService.OnOnlinePlayerAmountChanged?.Invoke();
		}
	}

	private static void GiveJewelsAndScheduleEquipment(Player player)
	{
		Helper.GiveDefaultJewels(player);
		ScheduleAction(Helper.GiveDefaultLegendaries, player, frameDelay: 5);
		ScheduleAction(Helper.GiveArmorAndNecks, player, frameDelay: 10);
	}


}

