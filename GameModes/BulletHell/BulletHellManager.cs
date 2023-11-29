using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ProjectM;
using ProjectM.Behaviours;
using PvpArena.Factories;
using PvpArena.GameModes.Domination;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Mathematics;
using UnityEngine;
using static PvpArena.Factories.UnitFactory;

namespace PvpArena.GameModes.BulletHell;

public static class BulletHellManager
{
	private static List<BulletHellGameMode> bulletHellGameModes = new List<BulletHellGameMode>();
	private static List<Player> playerQueue = new List<Player>();
	public static Dictionary<BulletHellGameMode, List<Timer>> gameModeTimers = new Dictionary<BulletHellGameMode, List<Timer>>();
	private static bool HasInitialized = false;

	static BulletHellManager()
	{
		Initialize();
	}
	public static void Initialize()
	{
		if (!HasInitialized)
		{
			for (var i = 0; i < BulletHellConfig.Config.BulletHellArenas.Count; i++)
			{
				var bulletHellArena = BulletHellConfig.Config.BulletHellArenas[i];
				bulletHellGameModes.Add(new BulletHellGameMode(bulletHellArena.FightZone.ToCircleZone(), bulletHellArena.TemplateUnitSpawn, i));
			}
		}
		HasInitialized = true;
	}


	public static void Dispose()
	{
		foreach (var gameMode in bulletHellGameModes)
		{
			EndMatch(gameMode);
		}
		bulletHellGameModes.Clear();

		playerQueue.Clear();
		foreach (var kvp in gameModeTimers)
		{
			foreach (var timer in kvp.Value)
			{
				if (timer != null)
				{
					timer.Dispose();
				}
			}
		}
		gameModeTimers.Clear();
		HasInitialized = false;
	}

	private static void DisposeTimers(BulletHellGameMode bulletHellGameMode)
	{
		if (gameModeTimers.ContainsKey(bulletHellGameMode))
		{
			foreach (var timer in gameModeTimers[bulletHellGameMode])
			{
				if (timer != null)
				{
					timer.Dispose();
				}
			}
			gameModeTimers[bulletHellGameMode].Clear();
		}
	}


	public static void QueueForMatch(Player player)
	{
		var foundArena = false;
		foreach (var arena in bulletHellGameModes)
		{
			if (arena.player == null)
			{
				foundArena = true;
				arena.player = player; //mark it as occupied before the delay
				Action action = () =>
				{
					StartMatch(arena, player);
					player.ReceiveMessage("Match started".Success());
				};
				ActionScheduler.RunActionOnceAfterDelay(action, 1);
				break;
			}
		}
		if (!foundArena)
		{
			playerQueue.Add(player);
		}
	}

	public static void LeaveQueue(Player player)
	{
		if (playerQueue.Contains(player))
		{
			playerQueue.Remove(player);
			player.ReceiveMessage("You have been removed from the Bullet Hell queue.");
		}
		else
		{
			player.ReceiveMessage("You are not in the Bullet Hell queue.");
		}
	}

	public static void StartMatch(BulletHellGameMode arena, Player player)
	{
		SpawnUnits(arena);
		player.CurrentState = Player.PlayerState.BulletHell;
		player.Reset(true, true);
		Helper.SetDefaultBlood(player, BulletHellConfig.Config.DefaultBlood.ToLower());
		player.Teleport(arena.FightZone.Center);
		Helper.BuffPlayer(player, Helper.CustomBuff, out var buffEntity, Helper.NO_DURATION);
		Helper.ModifyBuff(buffEntity, BuffModificationTypes.AbilityCastImpair | BuffModificationTypes.TargetSpellImpaired | BuffModificationTypes.Immaterial, true);
		Helper.CompletelyRemoveAbilityBarFromBuff(buffEntity);
		arena.Initialize();
		Action action = () =>
		{
			arena.HasStarted = true;
			arena.stopwatch.Start();
		};
		var timer = ActionScheduler.RunActionOnceAfterDelay(action, 1);
		arena.Timers.Add(timer);
	}

	public static void EndMatch(BulletHellGameMode arena)
	{
		try
		{
			arena.stopwatch.Stop();
			var player = arena.player;
			if (player != null)
			{
				if (player.IsInBulletHell())
				{
					player.CurrentState = Player.PlayerState.Normal;
					player.MatchmakingTeam = 0;
					player.Reset(true, true);
					if (!player.IsAlive)
					{
						Helper.RespawnPlayer(player, BulletHellConfig.Config.RespawnPoint.ToFloat3());
					}
					else
					{
						player.Teleport(BulletHellConfig.Config.RespawnPoint.ToFloat3());
					}
				}
				var sortedPlayers = PlayerService.UserCache.Values.OrderByDescending(p =>
				{
					float.TryParse(p.PlayerBulletHellData.BestTime, out float longestTime);
					return longestTime;
				}).ToList();

				var timeSurvived = arena.stopwatch.ElapsedMilliseconds / 1000.0;
				var personalRecordTime = float.Parse(player.PlayerBulletHellData.BestTime);
				var globalRecordTime = float.Parse(sortedPlayers[0].PlayerBulletHellData.BestTime);

				string message = "";
				if (timeSurvived > globalRecordTime)
				{
					message = $"You have set a new global record! New time: {timeSurvived.ToString("F2").Success()}, Old time: {sortedPlayers[0].Name.Error()} - {globalRecordTime.ToString("F2").Error()}".White();
					player.PlayerBulletHellData.BestTime = timeSurvived.ToString("F2");
					Core.playerBulletHellDataRepository.SaveDataAsync(new List<PlayerBulletHellData> { arena.player.PlayerBulletHellData });
				}
				else if (timeSurvived > personalRecordTime)
				{
					message = $"You have set a new personal record! New time: {timeSurvived.ToString("F2").Success()}, Old time: {personalRecordTime.ToString("F2").Error()}".White();
					player.PlayerBulletHellData.BestTime = timeSurvived.ToString("F2");
					Core.playerBulletHellDataRepository.SaveDataAsync(new List<PlayerBulletHellData> { player.PlayerBulletHellData });
				}
				else
				{
					message = $"You survived for {timeSurvived.ToString("F2").Emphasize()} seconds".White();
				}
				player.ReceiveMessage(message);
			}

			Helper.KillPreviousEntities($"bullethell{arena.ArenaNumber}");
			arena.Dispose();
			UnitFactory.DisposeTimers($"bullethell{arena.ArenaNumber}");
			DisposeTimers(arena);

			if (playerQueue.Any())
			{
				var nextPlayer = playerQueue.First();
				playerQueue.RemoveAt(0); // Remove the first player from the list
				arena.player = nextPlayer;
				Action action = () =>
				{
					StartMatch(arena, nextPlayer);
					nextPlayer.ReceiveMessage("Match started".Success());
				};
				ActionScheduler.RunActionOnceAfterDelay(action, 1);
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogError(e.ToString());
		}
	}


	private static void SpawnUnits(BulletHellGameMode arena)
	{
		var unitSettings = arena.UnitSpawns.UnitSpawn;
		var quantity = arena.UnitSpawns.Quantity;
		// Calculate angle step for each unit around the circle
		float angleStep = 360f / quantity;
		for (int i = 0; i < quantity; i++)
		{
			// Calculate the position of each unit
			float angle = angleStep * i;
			float radian = angle * Mathf.Deg2Rad;
			float x = arena.FightZone.Center.x + arena.FightZone.Radius * Mathf.Cos(radian);
			float z = arena.FightZone.Center.z + arena.FightZone.Radius * Mathf.Sin(radian);
			float3 spawnPosition = new float3(x, arena.FightZone.Center.y, z);

			// Instantiate the unit based on the type
			Unit unitToSpawn;
			var unitType = unitSettings.Type.ToLower();
			if (unitType == "turret")
			{
				unitToSpawn = new Turret(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
			}
			else
			{
				unitToSpawn = new Unit(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
			}
			// Set unit properties
			unitToSpawn.IsImmaterial = true;
			unitToSpawn.IsRooted = true;
			unitToSpawn.IsInvulnerable = true;
			unitToSpawn.KnockbackResistance = true;
			unitToSpawn.DrawsAggro = true;
			unitToSpawn.MaxHealth = 10000;
			unitToSpawn.Category = $"bullethell{arena.ArenaNumber}";
			unitToSpawn.AggroRadius = 25;
			unitToSpawn.SpawnDelay = unitSettings.SpawnDelay;
			unitToSpawn.Level = 150;
			// Spawn the unit at the calculated position
			UnitFactory.SpawnUnit(unitToSpawn, spawnPosition);
		}
	}
}
