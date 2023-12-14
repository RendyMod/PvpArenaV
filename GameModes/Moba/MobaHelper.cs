using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bloodstone.API;
using Discord;
using Il2CppSystem.Data;
using Il2CppSystem.Linq.Expressions.Interpreter;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.CastleBuilding;
using ProjectM.CastleBuilding.Teleporters;
using ProjectM.Gameplay.Scripting;
using ProjectM.Gameplay.Systems;
using ProjectM.Hybrid;
using ProjectM.Network;
using ProjectM.Sequencer;
using ProjectM.Shared;
using PvpArena.Configs;
using PvpArena.Data;
using PvpArena.Factories;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Rendering.HighDefinition;
using static PvpArena.Configs.ConfigDtos;
using static PvpArena.Factories.UnitFactory;
using static PvpArena.Helpers.Helper;
using static RootMotion.FinalIK.Grounding;

namespace PvpArena.GameModes.Moba;

public static class MobaHelper
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

	public static void SpawnStructures(Player player1, Player player2)
	{
		var players = new List<Player>
		{
			player1, player2
		};
		var hearts = Helper.GetEntitiesByComponentTypes<Pylonstation>(true);
		var foundHearts = new Dictionary<int, Entity>();
		foreach (var heart in hearts) 
		{ 
			if (heart.Read<Translation>().Value.Equals(MobaConfig.Config.Team1Heart.ToFloat3()))
			{
				foundHearts[0] = heart;
			}
			else if (heart.Read<Translation>().Value.Equals(MobaConfig.Config.Team2Heart.ToFloat3()))
			{
				foundHearts[1] = heart;
			}
		}


		foundHearts[0].Remove<DisableWhenNoPlayersInRange>();
		var buffer1 = foundHearts[0].ReadBuffer<CastleTeleporterElement>();
		buffer1.Clear();
		foundHearts[0].Write(players[0].Character.Read<Team>());
		foundHearts[0].Write(players[0].Character.Read<TeamReference>());
		foundHearts[0].Write(new UserOwner
		{
			Owner = NetworkedEntity.ServerEntity(players[0].User)
		});

		foundHearts[1].Remove<DisableWhenNoPlayersInRange>();
		var buffer2 = foundHearts[1].ReadBuffer<CastleTeleporterElement>();
		buffer2.Clear();
		foundHearts[1].Write(players[1].Character.Read<Team>());
		foundHearts[1].Write(players[1].Character.Read<TeamReference>());
		foundHearts[1].Write(new UserOwner
		{
			Owner = NetworkedEntity.ServerEntity(players[1].User)
		});

		var teleporters = Helper.GetEntitiesByComponentTypes<CastleTeleporterComponent>(true);
		foreach (var teleporter in teleporters)
		{
			int index = 0;
			if (teleporter.Read<PrefabGUID>() == Prefabs.TM_Castle_LocalTeleporter_Red)
			{
				index = 1;
			}

			teleporter.Write(players[index].Character.Read<Team>());
			teleporter.Write(players[index].Character.Read<TeamReference>());

			
			teleporter.Write(new CastleHeartConnection
			{
				CastleHeartEntity = NetworkedEntity.ServerEntity(foundHearts[index])
			});

			var castleTeleporterComponent = teleporter.Read<CastleTeleporterComponent>();
			if (index == 0) 
			{
				buffer1.Add(new CastleTeleporterElement
				{
					Entity = NetworkedEntity.ServerEntity(teleporter),
					Group = castleTeleporterComponent.Group
				});
			}
			else
			{
				buffer2.Add(new CastleTeleporterElement
				{
					Entity = NetworkedEntity.ServerEntity(teleporter),
					Group = castleTeleporterComponent.Group
				});
			}
			
			teleporter.Add<UserOwner>();
			teleporter.Write(new UserOwner
			{
				Owner = NetworkedEntity.ServerEntity(players[index].User)
			});
			var range = teleporter.Read<CastleBuildingMaxRange>();
			range.MaxRange = 10000;
			teleporter.Write(range);

		}
		

		foreach (var structureSpawn in MobaConfig.Config.StructureSpawns)
		{
			var spawnPos = structureSpawn.Location.ToFloat3();
			
			Action action = () =>
			{
				PrefabSpawnerService.SpawnWithCallback(structureSpawn.PrefabGUID, spawnPos, (e) =>
				{
					if (e.Has<Health>() && structureSpawn.Health > 0)
					{
						var health = e.Read<Health>();
						health.MaxHealth.Value = structureSpawn.Health;
						health.Value = structureSpawn.Health;
						health.MaxRecoveryHealth = structureSpawn.Health;
						e.Write(health);
					}
					if (structureSpawn.Team == 1)
					{
						e.Write(player1.Character.Read<Team>());
						e.Write(player1.Character.Read<TeamReference>());
					}
					else if (structureSpawn.Team == 2)
					{
						e.Write(player2.Character.Read<Team>());
						e.Write(player2.Character.Read<TeamReference>());
					}
				}, structureSpawn.RotationMode, -1, true, "moba");
			};
			
			Timer timer;
			if (structureSpawn.SpawnDelay > 0)
			{
				timer = ActionScheduler.RunActionOnceAfterDelay(action, structureSpawn.SpawnDelay);
				timers.Add(timer);
			}
			else
			{
				action();
			}
		}
	}

	private static void HandleGateOpeningAtMatchStart(Entity e)
	{
		e.Remove<Interactable>();
		var spawnDoor = e.Read<Door>();
		spawnDoor.OpenState = false;
		e.Write(spawnDoor);

		Action action = () =>
		{
			spawnDoor.OpenState = true;
			e.Write(spawnDoor);
		};
		var timer = ActionScheduler.RunActionOnceAfterDelay(action, 10);
		MobaGameMode.Timers.Add(timer);
	}

	private static void StartMatchCountdown()
	{
		for (int i = 5; i >= 0; i--)
		{
			int countdownNumber = i; // Introduce a new variable

			Action action = () =>
			{
				foreach (var team in MobaGameMode.Teams.Values)
				{
					foreach (var player in team)
					{
						if (countdownNumber > 0)
						{
							player.ReceiveMessage($"The match will start in: {countdownNumber.ToString().Emphasize()}".White());
						}
						else
						{
							player.ReceiveMessage($"The match has started. {"Go!".Emphasize()}".White());
						}
					}
				}

				if (countdownNumber == 0)
				{
					SpawnUnits(MobaGameMode.Teams[1][0], MobaGameMode.Teams[2][0]);
				}
			};

			Timer timer = ActionScheduler.RunActionOnceAfterDelay(action, 5 - countdownNumber);
			MobaGameMode.Timers.Add(timer);
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
					if (spawnedUnit.Unit.Category == "moba")
					{
						Helper.KillOrDestroyEntity(entity);
					}
				}
				else
				{
					if (UnitFactory.HasCategory(entity, "moba"))
					{
						Helper.KillOrDestroyEntity(entity);
					}
				}
			}
		}
		
		var destroySiegeWeaponsAction = () =>
		{
			entities = Helper.GetEntitiesByComponentTypes<SiegeWeapon>(true);
			foreach (var entity in entities)
			{
				Helper.KillOrDestroyEntity(entity);
			}
			entities.Dispose();
		};
		ActionScheduler.RunActionOnceAfterFrames(destroySiegeWeaponsAction, 2);

		var destroyItemPickupsAction = () =>
		{
			entities = Helper.GetEntitiesByComponentTypes<ItemPickup>(true);
			foreach (var entity in entities)
			{
				Helper.KillOrDestroyEntity(entity);
			}
			entities.Dispose();
		};
		ActionScheduler.RunActionOnceAfterDelay(destroyItemPickupsAction, .5);
	}


	public static void RemoveIneligibleItemsFromInventory(Player player)
	{
		Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Consumable_Canteen_BloodRoseBrew_T01);
		Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Consumable_GlassBottle_BloodRosePotion_T02);
		Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Consumable_Salve_Vermin);
		Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Consumable_CrimsonDraught);
		Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Ingredient_Coin_Copper);
		Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Consumable_Canteen_SpellBrew_T01);
		Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Consumable_Canteen_PhysicalBrew_T01);
		Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Consumable_GlassBottle_SpellBrew_T02);
		Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Consumable_GlassBottle_PhysicalBrew_T02);
		Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Building_Siege_Golem_T02);
	}

	public static void StartMatch(Player team1LeaderPlayer, Player team2LeaderPlayer)
	{
		var team1Players = team1LeaderPlayer.GetClanMembers();
		var team2Players = team2LeaderPlayer.GetClanMembers();

		SpawnMercenaries(team1LeaderPlayer, team2LeaderPlayer);
		Core.mobaGameMode.Initialize(team1Players, team2Players);
		SpawnStructures(team1LeaderPlayer, team2LeaderPlayer);
		
		foreach (var team1Player in team1Players)
		{
			team1Player.CurrentState = Player.PlayerState.Moba;
			team1Player.MatchmakingTeam = 1;
			team1Player.Reset(BaseGameMode.ResetOptions);
			Helper.RemoveBuildImpairBuffFromPlayer(team1Player);
			Helper.BuffPlayer(team1Player, Helper.CustomBuff5, out var buffEntity, Helper.NO_DURATION, true);
			buffEntity.Add<AllowJumpFromCliffsBuff>();
			buffEntity.Write(new AllowJumpFromCliffsBuff
			{
				BlockJump = true
			});
			Helper.SetDefaultBlood(team1Player, MobaConfig.Config.DefaultBloodType.ToLower(), MobaConfig.Config.DefaultBloodQuality);
			RemoveIneligibleItemsFromInventory(team1Player);
			var teleportPlayerAction = () => team1Player.Teleport(MobaConfig.Config.Team1PlayerRespawn.ToFloat3());
			ActionScheduler.RunActionOnceAfterDelay(teleportPlayerAction, .1);
			team1Player.ReceiveMessage($"The match will start in {"10".Emphasize()} seconds. {"Get ready!".Emphasize()}".White());
		}

		foreach (var team2Player in team2Players)
		{
			team2Player.CurrentState = Player.PlayerState.Moba;
			team2Player.MatchmakingTeam = 2;
			team2Player.Reset(BaseGameMode.ResetOptions);
			Helper.RemoveBuildImpairBuffFromPlayer(team2Player);
			Helper.BuffPlayer(team2Player, Helper.CustomBuff5, out var buffEntity, Helper.NO_DURATION, true);
			buffEntity.Add<AllowJumpFromCliffsBuff>();
			buffEntity.Write(new AllowJumpFromCliffsBuff
			{
				BlockJump = true
			});
			Helper.SetDefaultBlood(team2Player, MobaConfig.Config.DefaultBloodType.ToLower(), MobaConfig.Config.DefaultBloodQuality);
			RemoveIneligibleItemsFromInventory(team2Player);
			var teleportPlayerAction = () => team2Player.Teleport(MobaConfig.Config.Team2PlayerRespawn.ToFloat3());
			ActionScheduler.RunActionOnceAfterDelay(teleportPlayerAction, .1);
			team2Player.ReceiveMessage($"The match will start in {"10".Emphasize()} seconds. {"Get ready!".Emphasize()}".White());
		}

		var startMatchCountdownAction = () => { StartMatchCountdown(); };

		Timer timer = ActionScheduler.RunActionOnceAfterDelay(startMatchCountdownAction, 5);
		MobaGameMode.Timers.Add(timer);
	}

	private static void SpawnUnits(Player team1LeaderPlayer, Player team2LeaderPlayer)
	{
		foreach (var unitSettings in MobaConfig.Config.UnitSpawns)
		{
			Unit unitToSpawn;
			var unitType = unitSettings.Type.ToLower();
			if (unitType == "turret")
			{
				unitToSpawn = new BaseTurret(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
			}
			else if (unitType == "boss")
			{
				unitToSpawn = new Boss(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
				unitToSpawn.DrawsAggro = true;
				unitToSpawn.AggroRadius = 3.5f;
				unitToSpawn.DynamicCollision = false;
				unitToSpawn.MaxDistanceFromPreCombatPosition = 20;
			}
			else if (unitType == "golem")
			{
				unitToSpawn = new Boss(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
				unitToSpawn.DrawsAggro = true;
				unitToSpawn.AggroRadius = 3.5f;
				unitToSpawn.DynamicCollision = false;
				unitToSpawn.MaxDistanceFromPreCombatPosition = 20;
			}
			else
			{
				unitToSpawn = new Unit(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
				unitToSpawn.IsRooted = true;
			}
			unitToSpawn.MaxHealth = unitSettings.Health;
			unitToSpawn.Category = "moba";
			unitToSpawn.RespawnTime = unitSettings.RespawnTime;
			unitToSpawn.SpawnDelay = unitSettings.SpawnDelay;
			Player teamLeader;
			if (unitToSpawn.Team == 1)
			{
				teamLeader = team1LeaderPlayer;
			}
			else if (unitToSpawn.Team == 2)
			{
				teamLeader = team2LeaderPlayer;
			}
			else
			{
				teamLeader = null;
			}
			UnitFactory.SpawnUnitWithCallback(unitToSpawn, unitSettings.Location.ToFloat3(), (e) =>
			{
				e.Remove<DisableWhenNoPlayersInRange>();
				if (unitToSpawn.Team == 1 || unitToSpawn.Team == 2)
				{
					MobaGameMode.TeamUnits[unitToSpawn.Team].Add(e);
				}
				if (e.Has<VBloodUnit>())
				{
					var immortal = e.Read<Immortal>();
					immortal.IsImmortal = false;
					e.Write(immortal);
				}
				e.Remove<DisableWhenNoPlayersInRange>();
				if (e.Has<BloodConsumeSource>())
				{
					var blood = e.Read<BloodConsumeSource>();
					blood.BloodQuality = 0;
					e.Write(blood);
				}

				if (unitSettings.Description.ToLower() == "fort turret")
				{
					MobaGameMode.TeamMobaStructures[unitToSpawn.Team].FortTowers.Add(e);
				}
				else if (unitSettings.Description.ToLower() == "fort turret")
				{
					MobaGameMode.TeamMobaStructures[unitToSpawn.Team].KeepTowers.Add(e);
				}
				else if (unitSettings.Description.ToLower() == "fort")
				{
					MobaGameMode.TeamMobaStructures[unitToSpawn.Team].Fort = e;
					Helper.BuffEntity(e, Prefabs.Buff_Voltage_Stage2, out var buffEntity, Helper.NO_DURATION);
				}
				else if (unitSettings.Description.ToLower() == "keep")
				{
					MobaGameMode.TeamMobaStructures[unitToSpawn.Team].Keep = e;
					Helper.BuffEntity(e, Prefabs.Buff_Voltage_Stage2, out var buffEntity, Helper.NO_DURATION);
				}
				else if (unitSettings.Description.ToLower() == "core")
				{
					MobaGameMode.TeamMobaStructures[unitToSpawn.Team].Core = e;
					Helper.BuffEntity(e, Prefabs.InvulnerabilityBuff, out var buffEntity, Helper.NO_DURATION);
				}
				else if (unitSettings.Description.ToLower() == "bossmercenary")
				{
					var dropTableBuffer = e.ReadBuffer<DropTableBuffer>();
					dropTableBuffer.Clear();
					dropTableBuffer.Add(new()
					{
						DropTableGuid = Prefabs.DT_Unit_Demon,
						DropTrigger = DropTriggerType.OnDeath,
						RelicType = RelicType.None
					});

				}
			}, teamLeader);
		}

		var initialAction = () =>
		{
			// Spawn the first patrol immediately when this action is triggered
			SpawnPatrols(team1LeaderPlayer, team2LeaderPlayer);

			// Schedule the recurring action to start after 30 seconds from now
			var action = () => SpawnPatrols(team1LeaderPlayer, team2LeaderPlayer);
			var timer = ActionScheduler.RunActionEveryInterval(action, 30);
			MobaGameMode.Timers.Add(timer);
		};

		// Run the initial action after a 15-second delay
		var timer = ActionScheduler.RunActionOnceAfterDelay(initialAction, 15);
		MobaGameMode.Timers.Add(timer);
	}

	private static void SpawnMercenaries(Player team1LeaderPlayer, Player team2LeaderPlayer)
	{
		var i = 0;
		foreach (var mercenaryCampConfig in MobaConfig.Config.MercenaryCamps)
		{
			var mercenaryCamp = new MercenaryCamp
			{
				Point = new CapturePoint(mercenaryCampConfig.Zone.ToRectangleZone(), i, 3, 3)
			};
			mercenaryCamp.Point.IsActive = false;
			mercenaryCamp.CampIndex = i;
			MobaGameMode.MercenaryCamps[i] = mercenaryCamp;
			MobaGameMode.CapturePointIndexToLights[i] = new List<Entity>();
			foreach (var unitSettings in mercenaryCampConfig.UnitSpawns)
			{
				Unit unitToSpawn;
				var unitType = unitSettings.Type.ToLower();

				if (unitType == "boss")
				{
					unitToSpawn = new Boss(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
					unitToSpawn.DrawsAggro = true;
					unitToSpawn.AggroRadius = 3.5f;
					unitToSpawn.DynamicCollision = false;
					unitToSpawn.MaxDistanceFromPreCombatPosition = 20;
				}
				else
				{
					unitToSpawn = new Unit(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
					unitToSpawn.IsRooted = true;
				}
				unitToSpawn.MaxHealth = unitSettings.Health;
				unitToSpawn.Category = "moba";
				unitToSpawn.RespawnTime = unitSettings.RespawnTime;
				unitToSpawn.SoftSpawn = true;
				unitToSpawn.SpawnDelay = mercenaryCampConfig.SpawnDelay;
				Player teamLeader;
				if (unitToSpawn.Team == 1)
				{
					teamLeader = team1LeaderPlayer;
				}
				else if (unitToSpawn.Team == 2)
				{
					teamLeader = team2LeaderPlayer;
				}
				else
				{
					teamLeader = null;
				}
				UnitFactory.SpawnUnitWithCallback(unitToSpawn, unitSettings.Location.ToFloat3(), (e) =>
				{
					e.Remove<DisableWhenNoPlayersInRange>();
					if (unitToSpawn.Team == 1 || unitToSpawn.Team == 2)
					{
						MobaGameMode.TeamUnits[unitToSpawn.Team].Add(e);
					}
					if (e.Has<VBloodUnit>())
					{
						var immortal = e.Read<Immortal>();
						immortal.IsImmortal = false;
						e.Write(immortal);
					}
					e.Remove<DisableWhenNoPlayersInRange>();
					if (e.Has<BloodConsumeSource>())
					{
						var blood = e.Read<BloodConsumeSource>();
						blood.BloodQuality = 0;
						e.Write(blood);
					}

					mercenaryCamp.Entities.Add(e);
					MobaGameMode.UnitToMercenaryCamp[e] = mercenaryCamp;
				}, teamLeader);
			}
			i++;
		}
	}

	public static void SpawnPatrols(Player team1LeaderPlayer, Player team2LeaderPlayer)
	{
		foreach (var patrol in MobaConfig.Config.PatrolSpawns)
		{
			var unitSettings = patrol.UnitSpawn;
			
			Unit unitToSpawn;
			var unitType = unitSettings.Type.ToLower();
			unitToSpawn = new Unit(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
			unitToSpawn.MaxHealth = unitSettings.Health;
			unitToSpawn.Category = "moba";
			unitToSpawn.RespawnTime = unitSettings.RespawnTime;
			unitToSpawn.SpawnDelay = 5;
			unitToSpawn.DynamicCollision = true;
			unitToSpawn.SoftSpawn = true;
			unitToSpawn.MaxDistanceFromPreCombatPosition = 60;
			Player teamLeader;
			if (unitToSpawn.Team == 1)
			{
				teamLeader = team1LeaderPlayer;
			}
			else if (unitToSpawn.Team == 2)
			{
				teamLeader = team2LeaderPlayer;
			}
			else
			{
				teamLeader = null;
			}

			for (var i = 0; i < patrol.Quantity; i++)
			{
				UnitFactory.SpawnUnitWithCallback(unitToSpawn, unitSettings.Location.ToFloat3(), (e) =>
				{
					MobaGameMode.TeamPatrols[unitToSpawn.Team].Add(e);
					MobaGameMode.TeamUnits[unitToSpawn.Team].Add(e);
					e.Remove<DisableWhenNoPlayersInRange>();
					var blood = e.Read<BloodConsumeSource>();
					blood.BloodQuality = 0;
					e.Write(blood);
					ApplyTeamColorBuff(e, unitToSpawn.Team);
					Helper.BuffEntity(e, Helper.CustomBuff5, out var buffEntity, 10);
					Helper.ModifyBuff(buffEntity, BuffModificationTypes.DisableDynamicCollision, true);
				}, teamLeader);
			}
		}
	}

	public static void ApplyTeamColorBuff(Entity e, int team)
	{
		if (team == 1)
		{
			if (Helper.BuffEntity(e, Prefabs.AB_Vampire_VeilOfFrost_Buff, out var colorBuffEntity, Helper.NO_DURATION))
			{
				ModifyTeamColorBuff(e, colorBuffEntity);
			}
		}
		else
		{
			if (Helper.BuffEntity(e, Prefabs.AB_Vampire_VeilOfBlood_Buff, out var colorBuffEntity, Helper.NO_DURATION))
			{
				ModifyTeamColorBuff(e, colorBuffEntity);
			}
		}
	}

	public static void ModifyTeamColorBuff(Entity e, Entity colorBuffEntity)
	{
		colorBuffEntity.Remove<ModifyMovementSpeedBuff>();
		var listeners = colorBuffEntity.ReadBuffer<GameplayEventListeners>();
		listeners.Clear();
		var scriptBuffModifyAggroFactorDataServer = colorBuffEntity.Read<Script_Buff_ModifyAggroFactor_DataServer>();
		scriptBuffModifyAggroFactorDataServer.Factor = 1;
		colorBuffEntity.Write(scriptBuffModifyAggroFactorDataServer);
		Helper.ModifyBuff(colorBuffEntity, BuffModificationTypes.None, true);
		var action = () =>
		{
			if (e.Exists())
			{
				var stealthable = e.Read<Stealthable>();
				stealthable.IsStealthed.Value = false;
				stealthable.ModelInvisible.Value = false;
				e.Write(stealthable);
			}
		};
		ActionScheduler.RunActionOnceAfterFrames(action, 3);
	}

	public static void EndMatch(int winner = 0)
	{
		try
		{
			foreach (var timer in MobaGameMode.Timers)
			{
				if (timer != null)
				{
					timer.Dispose();
				}
			}
			MobaGameMode.Timers.Clear();

			foreach (var team in MobaGameMode.Teams.Values)
			{
				foreach (var player in team)
				{
					player.CurrentState = Player.PlayerState.Normal;
					player.MatchmakingTeam = 0;
					player.Reset(ResetOptions.FreshMatch);
					RemoveIneligibleItemsFromInventory(player);
					Helper.ApplyBuildImpairBuffToPlayer(player);
					Helper.RespawnPlayer(player, player.Position);
				}
			}
			if (winner > 0 && MobaGameMode.Teams.Count > 0)
			{
				var action = () => {
					TeleportTeamsToCenter(MobaGameMode.Teams, winner, TeamSide.West);
					Core.mobaGameMode.Dispose();
					UnitFactory.DisposeTimers("moba");
					DisposeTimers();
					KillPreviousEntities();
				};
				ActionScheduler.RunActionOnceAfterDelay(action, .1);
			}
			else
			{
				Core.mobaGameMode.Dispose();
				UnitFactory.DisposeTimers("moba");
				DisposeTimers();
				KillPreviousEntities();
			}
		}
		catch (Exception e)
		{
			Core.mobaGameMode.Dispose();
			UnitFactory.DisposeTimers("moba");
			DisposeTimers();
			KillPreviousEntities();
			Plugin.PluginLog.LogError(e.ToString());
		}
	}

	public enum TeamSide
	{
		North,
		East,
		South,
		West
	}

	public static void TeleportTeamsToCenter(
	Dictionary<int, List<Player>> Teams,
	int winningTeam,
	TeamSide teamOneSide)
	{
		var mapCenter = MobaConfig.Config.MapCenter;
		float playerSpacing = 2f;  // Adjust this as needed for the distance between players.

		// Determine the center coordinates
		var centerX = (mapCenter.Left + mapCenter.Right) / 2;
		var centerZ = (mapCenter.Top + mapCenter.Bottom) / 2;

		// Calculate the offset to center the team based on the number of players and spacing
		float teamOneOffset = (Teams[1].Count - 1) * playerSpacing / 2;
		float teamTwoOffset = (Teams[2].Count - 1) * playerSpacing / 2;

		// Determine the starting positions based on the side they are starting from
		float teamOneStartX = centerX;
		float teamOneStartZ = centerZ;
		float teamTwoStartX = centerX;
		float teamTwoStartZ = centerZ;

		if (teamOneSide == TeamSide.North || teamOneSide == TeamSide.South)
		{
			teamOneStartZ = teamOneSide == TeamSide.South ? centerZ - playerSpacing : centerZ + playerSpacing;
			teamTwoStartZ = teamOneSide == TeamSide.South ? centerZ + playerSpacing : centerZ - playerSpacing;
		}
		else
		{
			teamOneStartX = teamOneSide == TeamSide.East ? centerX + playerSpacing : centerX - playerSpacing;
			teamTwoStartX = teamOneSide == TeamSide.East ? centerX - playerSpacing : centerX + playerSpacing;
		}
		ApplyBuffsToTeams(Teams, winningTeam);

		// Position the teams
		PositionTeam(Teams[1], teamOneStartX, teamOneStartZ, playerSpacing, teamOneSide);
		PositionTeam(Teams[2], teamTwoStartX, teamTwoStartZ, playerSpacing, teamOneSide);
	}

	private static void PositionTeam(List<Player> team, float startCoordX, float startCoordZ, float spacing, TeamSide side)
	{
		bool isHorizontal = side == TeamSide.North || side == TeamSide.South;
		for (int i = 0; i < team.Count; i++)
		{
			float x = isHorizontal ? startCoordX + i * spacing - (team.Count - 1) * spacing / 2 : startCoordX;
			float z = isHorizontal ? startCoordZ : startCoordZ + i * spacing - (team.Count - 1) * spacing / 2;
			if (team[i].IsOnline)
			{
				team[i].Teleport(new float3(x, team[i].Position.y, z));
			}
			else
			{
				team[i].Teleport(new float3(0, 0, 0));
			}
		}
	}

	private static void ApplyBuffsToTeams(Dictionary<int, List<Player>> Teams, int winningTeam)
	{
		List<Player> winners = Teams[winningTeam];
		List<Player> losers = Teams[winningTeam == 1 ? 2 : 1];

		foreach (var winner in winners)
		{
			Helper.ApplyWinnerMatchEndBuff(winner);
		}
		foreach (var loser in losers)
		{
			Helper.ApplyLoserMatchEndBuff(loser);
		}
	}
}
