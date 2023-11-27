using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bloodstone.API;
using Discord;
using Il2CppSystem.Linq.Expressions.Interpreter;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Shared;
using PvpArena.Configs;
using PvpArena.Data;
using PvpArena.Factories;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Rendering.HighDefinition;
using static PvpArena.Factories.UnitFactory;
using static RootMotion.FinalIK.Grounding;

namespace PvpArena.GameModes.BulletHell;

public static class BulletHellHelper
{
	public static List<Timer> timers = new List<Timer>();

	public static void DisposeTimers()
	{
		foreach (var timer in timers)
		{
			if (timer != null)
			{
				timer.Dispose();
			}
		}
		timers.Clear();
	}

	private static void SpawnUnits(Player player)
	{
		foreach (var unitSettings in BulletHellConfig.Config.UnitSpawns)
		{
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
			unitToSpawn.IsImmaterial = true;
			unitToSpawn.IsRooted = true;
			unitToSpawn.IsInvulnerable = true;
			unitToSpawn.KnockbackResistance = true;
			unitToSpawn.DrawsAggro = true;
			unitToSpawn.MaxHealth = 10000;
			unitToSpawn.Category = "bullethell";
			unitToSpawn.AggroRadius = 25;
			unitToSpawn.SpawnDelay = unitSettings.SpawnDelay;
			unitToSpawn.Level = 150;

			Plugin.PluginLog.LogInfo("Spawning harpy");
			UnitFactory.SpawnUnit(unitToSpawn, unitSettings.Location.ToFloat3());
		}
	}

	public static void StartMatch(Player player)
	{
		SpawnUnits(player);
		player.CurrentState = Player.PlayerState.BulletHell;
		player.MatchmakingTeam = 1;
		player.Reset(true, true);
		Helper.SetDefaultBlood(player, BulletHellConfig.Config.DefaultBlood.ToLower());
		player.Teleport(BulletHellConfig.Config.StartPosition.ToFloat3());
		Helper.BuffPlayer(player, Helper.CustomBuff, out var buffEntity, Helper.NO_DURATION);
		Helper.ModifyBuff(buffEntity, BuffModificationTypes.AbilityCastImpair, true);
		Core.bulletHellGameMode.player = player;
		Core.bulletHellGameMode.Initialize();
		Action action = () =>
		{
			Core.bulletHellGameMode.HasStarted = true;
			Core.bulletHellGameMode.stopwatch.Start();
		};
		var timer = ActionScheduler.RunActionOnceAfterDelay(action, 1);
		Core.bulletHellGameMode.Timers.Add(timer);
	}

	public static void EndMatch()
	{
		try
		{
			var player = Core.bulletHellGameMode.player;
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
			}

			KillPreviousEntities();
			Core.bulletHellGameMode.stopwatch.Stop();
			Core.bulletHellGameMode.player.ReceiveMessage($"You survived for {(Core.bulletHellGameMode.stopwatch.ElapsedMilliseconds / 1000.0).ToString("F2").Emphasize()} seconds".White());
			Core.bulletHellGameMode.Dispose();
			UnitFactory.DisposeTimers("bullethell");
			DisposeTimers();
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogError(e.ToString());
		}
	}

	public static void KillPreviousEntities()
	{
		var entities = Helper.GetEntitiesByComponentTypes<CanFly>(true);
		foreach (var entity in entities)
		{
			if (!entity.Has<PlayerCharacter>())
			{
				if (UnitFactory.TryGetSpawnedUnitFromEntity(entity, out SpawnedUnit spawnedUnit))
				{
					if (spawnedUnit.Unit.Category == "bullethell")
					{
						Helper.DestroyEntity(entity);
					}
				}
				else
				{
					if (entity.Has<ResistanceData>() && entity.Read<ResistanceData>().GarlicResistance_IncreasedExposureFactorPerRating == StringToFloatHash("bullethell"))
					{
						Helper.DestroyEntity(entity);
					}
				}
			}
		}
	}
}
