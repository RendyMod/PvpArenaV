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
using Il2CppSystem.Linq.Expressions.Interpreter;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Systems;
using ProjectM.Hybrid;
using ProjectM.Sequencer;
using ProjectM.Shared;
using PvpArena.Configs;
using PvpArena.Data;
using PvpArena.Factories;
using PvpArena.GameModes.BulletHell;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Rendering.HighDefinition;
using static PvpArena.Factories.UnitFactory;
using static RootMotion.FinalIK.Grounding;

namespace PvpArena.GameModes.CaptureThePancake;

public static class CaptureThePancakeManager
{
	public static List<CaptureThePancakeGameMode> captureThePancakeGameModes = new List<CaptureThePancakeGameMode>();
	public static Dictionary<CaptureThePancakeGameMode, List<Timer>> gameModeTimers = new Dictionary<CaptureThePancakeGameMode, List<Timer>>();
	private static bool HasInitialized = false;

	public static void Initialize()
	{
		if (!HasInitialized)
		{
			for (var i = 0; i < CaptureThePancakeConfig.Config.Arenas.Count; i++)
			{
				var pancakeArena = new CaptureThePancakeGameMode();
				pancakeArena.ArenaNumber = i;
				captureThePancakeGameModes.Add(pancakeArena);
				gameModeTimers[pancakeArena] = new List<Timer>();
			}
		}

		HasInitialized = true;
	}

	public static void Dispose()
	{
		for (var i = 0; i < captureThePancakeGameModes.Count; i++)
		{
			EndMatch(i, 0);
		}

		captureThePancakeGameModes.Clear();
		gameModeTimers.Clear();
		HasInitialized = false;
	}

	public static void DisposeTimers(int arenaNumber)
	{
		var gameMode = captureThePancakeGameModes[arenaNumber];
		foreach (var timer in gameModeTimers[gameMode])
		{
			if (timer != null)
			{
				timer.Dispose();
			}
		}
		gameModeTimers[gameMode].Clear();
	}

    public static void StartMatchAtFirstAvailableArena(Player team1LeaderPlayer, Player team2LeaderPlayer)
    {
        var arenaNumber = GetAvailableArena();
        if (arenaNumber == -1)
        {
            throw new Exception();
        }
        else
        {
			CaptureThePancakeManager.EndMatch(arenaNumber, 0);
			var action = () => StartMatch(team1LeaderPlayer, team2LeaderPlayer, arenaNumber);
			ActionScheduler.RunActionOnceAfterDelay(action, 1);
		}
    }

    private static int GetAvailableArena()
    {
        for (var i = 0; i < captureThePancakeGameModes.Count; i++)
        {
			if (!captureThePancakeGameModes[i].MatchActive)
            {
				return i;
            }
        }
        return -1;
    }

	public static void SpawnStructures(Player player1, Player player2, int arenaNumber)
	{
		foreach (var structureSpawn in CaptureThePancakeConfig.Config.Arenas[arenaNumber].StructureSpawns)
		{
			var spawnPos = structureSpawn.Location.ToFloat3();
			if (structureSpawn.Type.ToLower() == "shard chest" && structureSpawn.SpawnDelay >= 30)
			{
				foreach (var team in captureThePancakeGameModes[arenaNumber].Teams.Values)
				{
					foreach (var player in team)
					{
						player.ReceiveMessage($"The {structureSpawn.Description} will spawn in {structureSpawn.SpawnDelay} seconds!".Warning());
					}
				}
			}
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
						if (e.Has<UserOwner>())
						{
							var userOwner = e.Read<UserOwner>();
							userOwner.Owner = player1.User;
							e.Write(userOwner);
						}
					}
					else if (structureSpawn.Team == 2)
					{
						e.Write(player2.Character.Read<Team>());
						e.Write(player2.Character.Read<TeamReference>());
						if (e.Has<UserOwner>())
						{
							var userOwner = e.Read<UserOwner>();
							userOwner.Owner = player2.User;
							e.Write(userOwner);
						}
					}

					if (structureSpawn.InventoryItems.Count > 0)
					{
						e.Add<DestroyWhenInventoryIsEmpty>();
						//InventoryUtilitiesServer.ClearInventory(VWorld.Server.EntityManager, e);
						foreach (var inventoryItem in structureSpawn.InventoryItems)
						{
							Helper.AddItemToInventory(e, inventoryItem, 1, out Entity itemEntity);
						}
					}

					if (structureSpawn.PrefabGUID == Prefabs.TM_Castle_Wall_Door_Metal_Wide_Tier02_Standard)
					{
						if (structureSpawn.Description == "Spawn Gate")
						{
							HandleGateOpeningAtMatchStart(e, arenaNumber);
						}
						else
						{
							if (structureSpawn.Description == "Winged Horror Gate")
							{
								captureThePancakeGameModes[arenaNumber].WingedHorrorGate = e;
							}
							else if (structureSpawn.Description == "Monster Gate")
							{
								captureThePancakeGameModes[arenaNumber].MonsterGate = e;
							}

							e.Remove<Interactable>();
							var door = e.Read<Door>();
							door.OpenState = false;
							Helper.BuffEntity(e, Prefabs.Buff_Voltage_Stage2, out var buffEntity, Helper.NO_DURATION);
							e.Write(door);
						}
					}

					if (structureSpawn.Type.ToLower() == "shard chest" && structureSpawn.SpawnDelay > 0)
					{
						foreach (var team in captureThePancakeGameModes[arenaNumber].Teams.Values)
						{
							foreach (var player in team)
							{
								player.ReceiveMessage($"The {structureSpawn.Description.NeutralTeam()} has {"spawned".Emphasize()}!".White());
							}
						}
					}

				}, structureSpawn.RotationMode, -1, true, $"pancake{arenaNumber}");
			};
			Timer timer;
			if (structureSpawn.SpawnDelay > 0)
			{
				timer = ActionScheduler.RunActionOnceAfterDelay(action, structureSpawn.SpawnDelay);
				gameModeTimers[captureThePancakeGameModes[arenaNumber]].Add(timer);
			}
			else
			{
				action();
			}


			if (structureSpawn.Type.ToLower() == "shard chest" && structureSpawn.SpawnDelay > 60)
			{
				var chestSpawnNotificationAction = () =>
				{
					foreach (var team in captureThePancakeGameModes[arenaNumber].Teams.Values)
					{
						foreach (var player in team)
						{
							player.ReceiveMessage($"The {structureSpawn.Description} will spawn in 30 seconds!".Warning());
						}
					}
				};

				timer = ActionScheduler.RunActionOnceAfterDelay(chestSpawnNotificationAction, structureSpawn.SpawnDelay - 30);
				gameModeTimers[captureThePancakeGameModes[arenaNumber]].Add(timer);
			}
		}
	}

	private static void HandleGateOpeningAtMatchStart(Entity e, int arenaNumber)
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
		captureThePancakeGameModes[arenaNumber].Timers.Add(timer);
	}

	private static void StartMatchCountdown(int arenaNumber)
	{
		for (int i = 5; i >= 0; i--)
		{
			int countdownNumber = i; // Introduce a new variable

			Action action = () =>
			{
				foreach (var team in captureThePancakeGameModes[arenaNumber].Teams.Values)
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
					SpawnUnits(captureThePancakeGameModes[arenaNumber].Teams[1][0], captureThePancakeGameModes[arenaNumber].Teams[2][0], arenaNumber);
				}
			};

			Timer timer = ActionScheduler.RunActionOnceAfterDelay(action, 5 - countdownNumber);
			captureThePancakeGameModes[arenaNumber].Timers.Add(timer);
		}
	}

	public static void KillPreviousEntities(int arenaNumber)
	{
		var entities = Helper.GetNonPlayerSpawnedEntities(true);
		foreach (var entity in entities)
		{
			if (!entity.Has<PlayerCharacter>())
			{
				if (UnitFactory.HasGameMode(entity, $"pancake{arenaNumber}"))
				{
					Helper.DestroyEntity(entity);
				}
			}
		}
		entities.Dispose();
		var relics = Helper.GetEntitiesByComponentTypes<SpawnSequenceForEntity, ItemPickup, PlaySequenceOnPickup>(true);
		foreach (var relic in relics)
		{
			if (relic.Read<PrefabGUID>() == Prefabs.Resource_Drop_Relic)
			{
				if (CaptureThePancakeConfig.Config.Arenas[arenaNumber].EntireMapZone.ToRectangleZone().Contains(relic))
				{
					Helper.DestroyEntity(relic);
				}
			}
		}
		relics.Dispose();
	}

	public static void DropItemsIntoBag(Player player, List<PrefabGUID> items, int arenaNumber, int quantity = 1)
	{
		PrefabSpawnerService.SpawnWithCallback(Prefabs.Resource_PlayerDeathContainer_Drop, player.Position, (Entity e) =>
		{
			e.Write(new PlayerDeathContainer
			{
				DeadUserEntity = player.User
			});
			e.Remove<DestroyWhenInventoryIsEmpty>();
			e.Write(new DestroyAfterDuration()
			{
				EndTime = float.MaxValue,
				Duration = float.MaxValue
			});
			e.Remove<DestroyAfterDurationCounter>();
			foreach (var item in items)
			{
				Helper.AddItemToInventory(e, item, quantity, out var itemEntity);
			}
			e.Add<DestroyWhenInventoryIsEmpty>();
		}, 0, -1, true, $"pancake{arenaNumber}");
	}

	public static void GiveVerminSalvesIfNotPresent(Player player)
	{
		if (!Helper.PlayerHasItemInInventories(player, Prefabs.Item_Consumable_Salve_Vermin))
		{
			Helper.AddItemToInventory(player.Character, Prefabs.Item_Consumable_Salve_Vermin, 1, out Entity entity);
		}
		Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Consumable_Canteen_BloodRoseBrew_T01);
		Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Consumable_GlassBottle_BloodRosePotion_T02);
	}

	public static void StartMatch(Player team1LeaderPlayer, Player team2LeaderPlayer, int arenaNumber)
	{
		var team1Players = team1LeaderPlayer.GetClanMembers();
		var team2Players = team2LeaderPlayer.GetClanMembers();
		captureThePancakeGameModes[arenaNumber].Initialize(team1Players, team2Players);
		SpawnStructures(team1LeaderPlayer, team2LeaderPlayer, arenaNumber);
		
		foreach (var team1Player in team1Players)
		{
			team1Player.CurrentState = Player.PlayerState.CaptureThePancake;
			team1Player.MatchmakingTeam = 1;
			team1Player.Reset(BaseGameMode.ResetOptions);
			Helper.SetDefaultBlood(team1Player, CaptureThePancakeConfig.Config.DefaultBlood.ToLower());
			GiveVerminSalvesIfNotPresent(team1Player);
			var teleportPlayerAction = () => team1Player.Teleport(CaptureThePancakeConfig.Config.Arenas[arenaNumber].Team1PlayerRespawn.ToFloat3());
			ActionScheduler.RunActionOnceAfterDelay(teleportPlayerAction, .1);
			team1Player.ReceiveMessage($"The match will start in {"10".Emphasize()} seconds. {"Get ready!".Emphasize()}".White());
			try
			{
				Helper.CompletelyRemoveItemFromInventory(team1Player, Prefabs.Item_Building_Relic_Monster);
				Helper.CompletelyRemoveItemFromInventory(team1Player, Prefabs.Item_Building_Relic_Manticore);
			}
			catch
			{

			}
		}
		
		foreach (var team2Player in team2Players)
		{
			team2Player.CurrentState = Player.PlayerState.CaptureThePancake;
			team2Player.MatchmakingTeam = 2;
			team2Player.Reset(BaseGameMode.ResetOptions);
			Helper.SetDefaultBlood(team2Player, CaptureThePancakeConfig.Config.DefaultBlood.ToLower());
			GiveVerminSalvesIfNotPresent(team2Player);
			var teleportPlayerAction = () => team2Player.Teleport(CaptureThePancakeConfig.Config.Arenas[arenaNumber].Team2PlayerRespawn.ToFloat3());
			ActionScheduler.RunActionOnceAfterDelay(teleportPlayerAction, .1);
			team2Player.ReceiveMessage($"The match will start in {"10".Emphasize()} seconds. {"Get ready!".Emphasize()}".White());
			try
			{
				Helper.CompletelyRemoveItemFromInventory(team2Player, Prefabs.Item_Building_Relic_Monster);
				Helper.CompletelyRemoveItemFromInventory(team2Player, Prefabs.Item_Building_Relic_Manticore);
			}
			catch
			{

			}
		}

		var action = () => { StartMatchCountdown(arenaNumber); };

		Timer timer = ActionScheduler.RunActionOnceAfterDelay(action, 5);
		captureThePancakeGameModes[arenaNumber].Timers.Add(timer);
	}

	private static void SpawnUnits(Player team1LeaderPlayer, Player team2LeaderPlayer, int arenaNumber)
	{
		foreach (var unitSettings in CaptureThePancakeConfig.Config.Arenas[arenaNumber].UnitSpawns)
		{
			Unit unitToSpawn;
			var unitType = unitSettings.Type.ToLower();
			if (unitType == "turret")
			{
				unitToSpawn = new Turret(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
			}
			else if (unitType == "boss")
			{
				unitToSpawn = new Boss(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
				unitToSpawn.IsRooted = true;
			}
			else if (unitType == "angram")
			{
				unitToSpawn = new AngramBoss(unitSettings.Team, unitSettings.Level);
			}
			else if (unitType == "horse")
			{
				unitToSpawn = new ObjectiveHorse(unitSettings.Team);
			}
			else if (unitType == "lightningrod")
			{
				unitToSpawn = new LightningBoss($"pancake{arenaNumber}", unitSettings.Team, unitSettings.Level);
			}
			else if (unitType == "healingorb")
			{
				unitToSpawn = new HealingOrb();
			}
			else
			{
				unitToSpawn = new Unit(unitSettings.PrefabGUID, unitSettings.Team, unitSettings.Level);
				unitToSpawn.IsRooted = true;
			}
			unitToSpawn.MaxHealth = unitSettings.Health;
			unitToSpawn.GameMode = $"pancake{arenaNumber}";
			unitToSpawn.RespawnTime = unitSettings.RespawnTime;
			unitToSpawn.SpawnDelay = unitSettings.SpawnDelay;
			if (unitSettings.SpawnDelay > 30)
			{
				unitToSpawn.AnnounceSpawn = true;
			}
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
			UnitFactory.SpawnUnit(unitToSpawn, unitSettings.Location.ToFloat3(), teamLeader);
		}
	}

	public static void EndMatch(int arenaNumber, int winner = 0)
	{
		try
		{
			var relics = Helper.GetEntitiesByComponentTypes<Relic>();
			foreach (var relic in relics)
			{
				Helper.KillOrDestroyEntity(relic);
			}
			relics.Dispose();
			foreach (var timer in captureThePancakeGameModes[arenaNumber].Timers)
			{
				if (timer != null)
				{
					timer.Dispose();
				}
			}
			captureThePancakeGameModes[arenaNumber].Timers.Clear();

			foreach (var team in captureThePancakeGameModes[arenaNumber].Teams.Values)
			{
				foreach (var player in team)
				{
					Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Building_Relic_Monster);
					Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Building_Relic_Manticore);
					player.CurrentState = Player.PlayerState.Normal;
					player.MatchmakingTeam = 0;
					player.Reset(BaseGameMode.ResetOptions);
					Helper.RespawnPlayer(player, player.Position);
				}
			}
			if (winner > 0 && captureThePancakeGameModes[arenaNumber].Teams.Count > 0)
			{
				var action = () => {
					TeleportTeamsToCenter(captureThePancakeGameModes[arenaNumber].Teams, winner, TeamSide.East, arenaNumber);
					captureThePancakeGameModes[arenaNumber].Dispose();
					UnitFactory.DisposeTimers($"pancake{arenaNumber}");
					DisposeTimers(arenaNumber);
					KillPreviousEntities(arenaNumber);
				};
				ActionScheduler.RunActionOnceAfterDelay(action, .1);
			}
			else
			{
				captureThePancakeGameModes[arenaNumber].Dispose();
				UnitFactory.DisposeTimers($"pancake{arenaNumber}");
				DisposeTimers(arenaNumber);
				KillPreviousEntities(arenaNumber);
			}
		}
		catch (Exception e)
		{
			captureThePancakeGameModes[arenaNumber].Dispose();
			UnitFactory.DisposeTimers($"pancake{arenaNumber}");
			DisposeTimers(arenaNumber);
			KillPreviousEntities(arenaNumber);
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
	TeamSide teamOneSide,
	int arenaNumber
	)
	{
		var mapCenter = CaptureThePancakeConfig.Config.Arenas[arenaNumber].MapCenter;
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
				team[i].TeleportToOfflinePosition();
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
