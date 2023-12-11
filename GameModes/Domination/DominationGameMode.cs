using System;
using System.Collections.Generic;
using ProjectM;
using PvpArena.Models;
using Unity.Entities;
using static ProjectM.DeathEventListenerSystem;
using ProjectM.Network;
using PvpArena.Data;
using Unity.Mathematics;
using Unity.Transforms;
using Bloodstone.API;
using PvpArena.Helpers;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using System.Threading;
using PvpArena.Services;
using static PvpArena.Factories.UnitFactory;
using ProjectM.CastleBuilding;
using System.Linq;
using PvpArena.GameModes.Domination.PvpArena.Models;

namespace PvpArena.GameModes.Domination;

public class DominationGameMode : BaseGameMode
{
	public override Player.PlayerState GameModeType => Player.PlayerState.Domination;
	public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
	{
		ResetCooldowns = false,
		RemoveShapeshifts = true,
		RemoveConsumables = false,
		BuffsToIgnore = buffsToIgnore
	};
	public static bool MatchActive = false;

	public static Dictionary<int, List<Player>> Teams = new Dictionary<int, List<Player>>();
	private static List<CapturePoint> CapturePoints = new List<CapturePoint>();
	private static Dictionary<int, PrefabGUID> CapturePointIndexToBuffs = new Dictionary<int, PrefabGUID>();
	private static Dictionary<int, string> CapturePointIndexToNames = new Dictionary<int, string>();
	public static Dictionary<int, List<Entity>> CapturePointIndexToLights = new Dictionary<int, List<Entity>>();
	public static Dictionary<int, int> TeamPoints = new Dictionary<int, int>();
	public static List<Timer> Timers = new List<Timer>();
	private static HashSet<PrefabGUID> buffsToIgnore = new HashSet<PrefabGUID>
	{
		Prefabs.Buff_General_Silver_Sickness_Burn_Debuff
	};


	private static Dictionary<Player, int> playerKills = new Dictionary<Player, int>();
	private static Dictionary<Player, int> playerDeaths = new Dictionary<Player, int>();

	private static Dictionary<PrefabGUID, PrefabGUID> shapeshiftGroupToBuff = new Dictionary<PrefabGUID, PrefabGUID>
	{
		{ Prefabs.AB_Shapeshift_Wolf_Group, Prefabs.AB_Shapeshift_Wolf_Buff },
		{ Prefabs.AB_Shapeshift_Wolf_Skin01_Group, Prefabs.AB_Shapeshift_Wolf_Skin01_Buff },
		{ Prefabs.AB_Shapeshift_Bear_Group, Prefabs.AB_Shapeshift_Bear_Buff },
		{ Prefabs.AB_Shapeshift_Bear_Skin01_Group, Prefabs.AB_Shapeshift_Bear_Skin01_Buff },
	};

	private static Dictionary<PrefabGUID, PrefabGUID> shapeshiftToShapeshift = new Dictionary<PrefabGUID, PrefabGUID>
	{
		{ Prefabs.AB_Shapeshift_Wolf_Group, Prefabs.AB_Shapeshift_Bear_Group},
		{ Prefabs.AB_Shapeshift_Wolf_Skin01_Group, Prefabs.AB_Shapeshift_Bear_Skin01_Group},
		{ Prefabs.AB_Shapeshift_Bear_Group, Prefabs.AB_Shapeshift_Bear_Group},
		{ Prefabs.AB_Shapeshift_Bear_Skin01_Group, Prefabs.AB_Shapeshift_Bear_Skin01_Group},
		{ Prefabs.AB_Shapeshift_Rat_Group, Prefabs.AB_Shapeshift_Rat_Group},
	};

	public Dictionary<PrefabGUID, bool> allowedShapeshifts = new Dictionary<PrefabGUID, bool>
	{
		{Prefabs.AB_Shapeshift_Wolf_Group, true},
		{Prefabs.AB_Shapeshift_Wolf_Skin01_Group, true},
		{Prefabs.AB_Shapeshift_Rat_Group, true},
		{Prefabs.AB_Shapeshift_Bear_Group, true},
		{Prefabs.AB_Shapeshift_Bear_Skin01_Group, true }
	};

	public static HashSet<string> AllowedCommands = new HashSet<string>
	{
		"ping",
		"help",
		"legendary",
		"jewel",
		"forfeit",
		"points",
		"lb ranked",
		"bp",
		"j",
		"lw",
	};

	public static List<Timer> QueuedRespawns = new List<Timer>();


	public override void Initialize()
	{
		MatchActive = true;
		var index = 0;
		foreach (var capturePointConfig in DominationConfig.Config.CapturePoints)
		{
			var capturePoint = new CapturePoint(capturePointConfig.Zone.ToRectangleZone(), index);
			capturePoint.OnPointCaptured += HandlePointCapture;
			capturePoint.OnCaptureProgress += HandleCaptureProgress;
			CapturePoints.Add(capturePoint);
			CapturePointIndexToBuffs[index] = capturePointConfig.BuffToApplyOnCapture;
			CapturePointIndexToNames[index] = capturePointConfig.Description;
			CapturePointIndexToLights[index] = new List<Entity>();
			index++;
		}

		var dyableEntities = Helper.GetEntitiesByComponentTypes<CastleHeartConnection, DyeableCastleObject>(true);
		foreach (var dyeableEntity in dyableEntities)
		{
			foreach (var capturePoint in CapturePoints)
			{
				if (capturePoint.Zone.Contains(dyeableEntity.Read<LocalToWorld>().Position))
				{
					CapturePointIndexToLights[capturePoint.PointIndex].Add(dyeableEntity);
					/*BuffHelper.BuffEntity(dyeableEntity, BuffHelper.CustomBuff, out var buffEntity, BuffHelper.NO_DURATION);
					BuffHelper.ModifyBuff(buffEntity, BuffModificationTypes.Immaterial | BuffModificationTypes.Invulnerable | BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.DisableMapCollision, true);*/
				}
			}
		}
		GameEvents.OnPlayerRespawn += HandleOnPlayerRespawn;
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
		GameEvents.OnPlayerUsedConsumable += HandleOnConsumableUse;
		GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
		GameEvents.OnPlayerInvitedToClan += HandleOnPlayerInvitedToClan;
		GameEvents.OnPlayerKickedFromClan += HandleOnPlayerKickedFromClan;
		GameEvents.OnPlayerLeftClan += HandleOnPlayerLeftClan;
		GameEvents.OnUnitBuffed += HandleOnUnitBuffed;
		GameEvents.OnGameFrameUpdate += HandleOnGameFrameUpdate;
		GameEvents.OnItemWasDropped += HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;

		foreach (var timer in Timers)
		{
			if (timer != null)
			{
				timer.Dispose();
			}
		}
		Timers.Clear();
	}

	public void Initialize(List<Player> team1Players, List<Player> team2Players)
	{
		Initialize();
		Teams[1] = team1Players;
		Teams[2] = team2Players;
		TeamPoints[1] = 0;
		TeamPoints[2] = 0;
		playerKills.Clear();
		playerDeaths.Clear();

		foreach (var team in Teams.Values)
		{
			foreach (var player in team)
			{
				playerKills[player] = 0;
				playerDeaths[player] = 0;
			}
		}
	}
	public override void Dispose()
	{
		MatchActive = false;
		GameEvents.OnPlayerRespawn -= HandleOnPlayerRespawn;
		GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
		GameEvents.OnPlayerUsedConsumable -= HandleOnConsumableUse;
		GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
		GameEvents.OnPlayerInvitedToClan += HandleOnPlayerInvitedToClan;
		GameEvents.OnPlayerKickedFromClan += HandleOnPlayerKickedFromClan;
		GameEvents.OnPlayerLeftClan += HandleOnPlayerLeftClan;
		GameEvents.OnUnitBuffed -= HandleOnUnitBuffed;
		GameEvents.OnGameFrameUpdate -= HandleOnGameFrameUpdate;
		GameEvents.OnItemWasDropped -= HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
		Teams.Clear();
		playerKills.Clear();
		playerDeaths.Clear();
		CapturePoints.Clear();
		CapturePointIndexToBuffs.Clear();
		CapturePointIndexToNames.Clear();
		foreach (var lightList in CapturePointIndexToLights.Values)
		{
			foreach (var light in lightList)
			{
				var dyeable = light.Read<DyeableCastleObject>();
				dyeable.ActiveColorIndex = 9;
				light.Write(dyeable);
			}
		}
		CapturePointIndexToLights.Clear();
		TeamPoints.Clear();
	}
	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (player.CurrentState != GameModeType) return;

		if (killer.Exists())
		{
			Player killerPlayer = null;
			if (killer.Has<PlayerCharacter>())
			{
				killerPlayer = PlayerService.GetPlayerFromCharacter(killer);

				if (killer != player.Character)
				{
					if (playerKills.ContainsKey(killerPlayer))
					{
						playerKills[killerPlayer]++;
					}
					else
					{
						playerKills[killerPlayer] = 1;
					}
				}
			}

			foreach (var team in Teams.Values)
			{
				foreach (var teamPlayer in team)
				{
					string message = CreatePlayerDownedMessage(player, killerPlayer, teamPlayer);
					teamPlayer.ReceiveMessage(message);
				}
			}
		}
		else
		{
			// Handle admin abuse case
			foreach (var team in Teams.Values)
			{
				foreach (var teamPlayer in team)
				{
					bool isTeammate = player.MatchmakingTeam == teamPlayer.MatchmakingTeam;
					string coloredVictimName = isTeammate ? $"{player.Name.FriendlyTeam()}" : $"{player.Name.EnemyTeam()}";
					string message = $"{coloredVictimName} died to {"admin abuse".EnemyTeam()}".White();
					teamPlayer.ReceiveMessage(message);
				}
			}
		}

		// Increment death count for the downed player
		if (playerDeaths.ContainsKey(player))
		{
			playerDeaths[player]++;
		}
		else
		{
			playerDeaths[player] = 1;
		}
	}

	private string CreatePlayerDownedMessage(Player victim, Player killer, Player observer)
	{
		bool isVictimTeammate = victim.MatchmakingTeam == observer.MatchmakingTeam;
		string coloredVictimName = isVictimTeammate ? $"{victim.Name.FriendlyTeam()}" : $"{victim.Name.EnemyTeam()}".White();

		if (killer != null)
		{
			bool isKillerTeammate = killer.MatchmakingTeam == observer.MatchmakingTeam;
			string coloredKillerName = isKillerTeammate ? $"{killer.Name.FriendlyTeam()}" : $"{killer.Name.EnemyTeam()}".White();
			return $"{coloredKillerName} killed {coloredVictimName}".White();
		}
		else
		{
			return $"{coloredVictimName} died to {"PvE".NeutralTeam()}".White();
		}
	}
	public override void HandleOnPlayerDeath(Player player, DeathEvent deathEvent)
	{
		if (player.CurrentState != GameModeType) return;

		if (!BuffUtility.HasBuff(VWorld.Server.EntityManager, player.Character, Prefabs.Buff_General_Vampire_Wounded_Buff))
		{
			if (playerDeaths.ContainsKey(player))
			{
				playerDeaths[player]++;
			}
			else
			{
				playerDeaths[player] = 1;
			}
			foreach (var team in Teams.Values)
			{
				foreach (var teamPlayer in team)
				{
					bool isTeammate = player.MatchmakingTeam == teamPlayer.MatchmakingTeam;
					string coloredVictimName = isTeammate ? $"{player.Name.FriendlyTeam()}" : $"{player.Name.EnemyTeam()}";
					var message = $"{coloredVictimName} killed themselves".White();
					teamPlayer.ReceiveMessage(message);
				}
			}
		}

		float3 pos = default;
		if (player.MatchmakingTeam == 1)
		{
			pos = DominationConfig.Config.Team1PlayerRespawn.ToFloat3();
		}
		else if (player.MatchmakingTeam == 2)
		{
			pos = DominationConfig.Config.Team2PlayerRespawn.ToFloat3();
		}
		Helper.RemoveBuff(player.Character, Prefabs.Buff_General_VampirePvPDeathDebuff);

		Action respawnAction = () =>
		{
			Helper.RespawnPlayer(player, pos);
		};
		var timer = ActionScheduler.RunActionOnceAfterDelay(respawnAction, 2.9);
		QueuedRespawns.Add(timer);
		player.Reset(ResetOptions);
		
		var blood = player.Character.Read<Blood>();
		Helper.SetPlayerBlood(player, blood.BloodType, blood.Quality);
		
		foreach (var capturePoint in CapturePoints)
		{
			if (capturePoint.GetControllingTeamId() == player.MatchmakingTeam)
			{
				if (!BuffUtility.TryGetBuff(VWorld.Server.EntityManager, player.Character, CapturePointIndexToBuffs[capturePoint.PointIndex], out var buffEntity))
				{
					var action = new ScheduledAction(Helper.BuffPlayer, new object[] { player, CapturePointIndexToBuffs[capturePoint.PointIndex], buffEntity, Helper.NO_DURATION, true, true });
					ActionScheduler.ScheduleAction(action, 2);
				}
			}
		}
	}
	
	private void ApplyBuffStacks(Player player)
	{
		int enemyTeam = 1;
		if (player.MatchmakingTeam == 1) 
		{
			enemyTeam = 2;
		}
		if (TeamPoints[enemyTeam] > 0)
		{
			if (!BuffUtility.TryGetBuff(VWorld.Server.EntityManager, player.Character, Prefabs.Buff_General_VampirePvPDeathDebuff, out var buffEntity))
			{
				Helper.BuffPlayer(player, Prefabs.Buff_General_VampirePvPDeathDebuff, out buffEntity, Helper.NO_DURATION);
			}

			var buffData = buffEntity.Read<Buff>();
			buffData.MaxStacks = 255;
			buffData.Stacks = (byte)TeamPoints[enemyTeam];
			buffEntity.Write(buffData);
		}

		if (TeamPoints[player.MatchmakingTeam] > 0)
		{
			if (!BuffUtility.TryGetBuff(VWorld.Server.EntityManager, player.Character, Prefabs.Buff_General_Silver_Sickness_Burn_Debuff, out var buffEntity))
			{
				Helper.BuffPlayer(player, Prefabs.Buff_General_Silver_Sickness_Burn_Debuff, out buffEntity, Helper.NO_DURATION);
			}
			
			var buffData = buffEntity.Read<Buff>();
			buffData.Stacks = (byte)TeamPoints[player.MatchmakingTeam];
			buffEntity.Write(buffData);
		}

	}

	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

		var enterShapeshiftEvent = eventEntity.Read<EnterShapeshiftEvent>();
		if (!shapeshiftToShapeshift.ContainsKey(enterShapeshiftEvent.Shapeshift))
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
			player.ReceiveMessage($"That shapeshift is disabled while in a Domination match.".Error());
		}
		else
		{
			enterShapeshiftEvent.Shapeshift = shapeshiftToShapeshift[enterShapeshiftEvent.Shapeshift];
			eventEntity.Write(enterShapeshiftEvent);
		}
	}
	public void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (player.CurrentState != GameModeType) return;

		if (item.ItemType != Prefabs.Item_Consumable_GlassBottle_BloodRosePotion_T02)
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
			player.ReceiveMessage("You can't drink those during a Domination match!".Error());
		}
		//BuffClanMembersOnConsume(player, item);
	}

	public void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (player.CurrentState != GameModeType) return;

		var prefabGuid = buffEntity.Read<PrefabGUID>();
		if (prefabGuid == Prefabs.AB_Feed_02_Bite_Abort_Trigger)
		{
			Helper.RemoveBuff(player.Character, Prefabs.AB_FeedEnemyVampire_01_Initiate_DashChannel);
		}
		else if (prefabGuid == Prefabs.AB_Interact_HealingOrb_Buff)
		{
			var buffer = buffEntity.ReadBuffer<HealOnGameplayEvent>();
			for (var i = 0; i < buffer.Length; i++)
			{
				var heal = buffer[i];
				heal.HealthPercent = 0.2f;
				buffer[i] = heal;
			}
		}
		else if (prefabGuid == Prefabs.Buff_General_Silver_Sickness_Burn_Debuff)
		{
			Helper.ApplyStatModifier(buffEntity, BuffModifiers.SilverResistance);
		}
		else if (shapeshiftGroupToBuff.ContainsValue(prefabGuid))
		{
			buffEntity.Add<DestroyBuffOnDamageTaken>();
			buffEntity.Add<DestroyOnAbilityCast>();
			var buffer = buffEntity.ReadBuffer<ReplaceAbilityOnSlotBuff>();
			buffer.Clear();
			Helper.ApplyStatModifier(buffEntity, BuffModifiers.PancakeShapeshiftSpeed);
		}
	}

	public void HandleOnUnitBuffed(Entity unit, Entity buffEntity)
	{
		if (TryGetSpawnedUnitFromEntity(unit, out SpawnedUnit spawnedUnit))
		{
			if (spawnedUnit.Unit.Category != "domination")
			{
				return;
			}
		}
		else
		{
			return;
		}

		var buffPrefabGUID = buffEntity.Read<PrefabGUID>();
	}

	public override void HandleOnPlayerConnected(Player player)
	{
		if (player.CurrentState != GameModeType) return;


		Helper.DestroyEntity(player.Character);
		if (player.MatchmakingTeam == 1)
		{
			Helper.RespawnPlayer(player, DominationConfig.Config.Team1PlayerRespawn.ToFloat3());
		}
		else if (player.MatchmakingTeam == 2)
		{
			Helper.RespawnPlayer(player, DominationConfig.Config.Team2PlayerRespawn.ToFloat3());
		}
	}

	public void HandleOnPlayerInvitedToClan(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not invite players to your clan while in Domination".Error());
	}

	public void HandleOnPlayerKickedFromClan(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not kick players from your clan while in Domination".Error());
	}

	public void HandleOnPlayerLeftClan(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not leave your clan while in Domination".Error());
	}

	private static void RespawnUnitIfEligible(Entity unitEntity)
	{
		if (TryGetSpawnedUnitFromEntity(unitEntity, out SpawnedUnit unit))
		{
			if (unit.Unit.RespawnTime != -1)
			{
				Action action = () =>
				{
					SpawnUnit(unit.Unit, unit.SpawnPosition, unit.Player);
				};
				Timer timer = ActionScheduler.RunActionOnceAfterDelay(action, unit.Unit.RespawnTime);
			}
		}
	}

	public void HandleOnPlayerChatCommand(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;


	}

	public void HandleOnGameFrameUpdate()
	{
		if (!MatchActive)
		{
			return;
		}
		foreach (var point in CapturePoints)
		{
			point.Update(Teams[1], Teams[2], (int teamIndex) =>
			{
				// Check if the team controls all capture points
				bool controlsAllPoints = CapturePoints.All(p => p.GetControllingTeamId() == teamIndex);
				int pointsToGain = 2;
				// Award points
				if (controlsAllPoints)
				{
					pointsToGain = 4;
				}
				
				TeamPoints[teamIndex] += pointsToGain;
				bool gameWon = false;
				foreach (var team in Teams.Values)
				{
					foreach (var player in team)
					{
						if (!gameWon && player.IsAlive)
						{
							ApplyBuffStacks(player);
						}
							
						if (TeamPoints[teamIndex] >= 100)
						{
							gameWon = true;
							string message = "";

							if (player.MatchmakingTeam == teamIndex)
							{
								message = $"Your team has won the game!".Success();
							}
							else
							{
								message = $"Your team has lost the game!".Error();
							}
							player.ReceiveMessage(message);
						}
					}
				}
				// Apply buff to all players of the team
				if (gameWon)
				{
					ReportStats();
					DominationHelper.EndMatch();
				}
			});
		}
	}

	private static void HandleCaptureProgress(int pointIndex, int gainingTeamId, int controllingTeamId, int breakpoint)
	{
		// Define color indices for each team and neutral state
		int team1ColorIndex = 5; // Example color index for Team 1
		int team2ColorIndex = 8; // Example color index for Team 2
		int neutralColorIndex = 9; // Example color index for neutral

		// Determine the target color index based on the gaining team
		int targetColorIndex = (gainingTeamId == 1) ? team1ColorIndex : (gainingTeamId == 2) ? team2ColorIndex : neutralColorIndex;
		// Process the lights
		foreach (var lightEntity in CapturePointIndexToLights[pointIndex])
		{
			var dyeable = lightEntity.Read<DyeableCastleObject>();

			if (gainingTeamId != 0)
			{
				// If progress is increasing, change a light not belonging to the gaining team
				if (dyeable.ActiveColorIndex != targetColorIndex)
				{
					dyeable.ActiveColorIndex = (byte)targetColorIndex;
					lightEntity.Write(dyeable);
					break; // Exit the loop after changing one light
				}
			}
			else
			{
				// If progress is decreasing, revert a light belonging to the gaining team
				if (dyeable.ActiveColorIndex != targetColorIndex)
				{
					// Revert to the controlling team's color or neutral if no team controls it
					int revertColorIndex = (controllingTeamId == 1) ? team1ColorIndex : (controllingTeamId == 2) ? team2ColorIndex : neutralColorIndex;
					dyeable.ActiveColorIndex = (byte)revertColorIndex;
					lightEntity.Write(dyeable);
					break; // Exit the loop after reverting one light
				}
			}
		}
	}

	private static void HandlePointCapture(int pointIndex, int previousTeamIndex, int newTeamIndex)
	{
		//the point was lost
		if (previousTeamIndex != 0)
		{
			var team = Teams[previousTeamIndex];
			foreach (var player in team)
			{
				Helper.RemoveBuff(player.Character, CapturePointIndexToBuffs[pointIndex]);
			}
		}

		var newTeam = Teams[newTeamIndex];
		foreach (var player in newTeam)
		{
			Helper.BuffPlayer(player, CapturePointIndexToBuffs[pointIndex], out var buffEntity, Helper.NO_DURATION, true);
		}

		foreach (var team in Teams.Values)
		{
			foreach (var player in team)
			{
				string message = "";
				if (player.MatchmakingTeam == newTeamIndex)
				{
					message = $"{($"Team {newTeamIndex}").FriendlyTeam()} has captured the {CapturePointIndexToNames[pointIndex].ToString().Emphasize()}!".White();
				}
				else
				{
					message = $"{($"Team {newTeamIndex}").EnemyTeam()} has captured the {CapturePointIndexToNames[pointIndex].ToString().Emphasize()}!".White();
				}
				player.ReceiveMessage(message);
			}
		}
	}

	public void HandleOnPlayerRespawn(Player player)
	{
		if (Helper.BuffPlayer(player, Prefabs.Witch_SheepTransformation_Buff, out var buffEntity, DominationConfig.Config.PlayerRespawnTime, true))
		{
			Helper.ModifyBuff(buffEntity, BuffModificationTypes.MovementImpair | BuffModificationTypes.Immaterial);
		}
		ApplyBuffStacks(player);
	}
	private static bool IsInFriendlyEndZone(Player player)
	{
		RectangleZone endZone;
		if (player.MatchmakingTeam == 1)
		{
			endZone = CaptureThePancakeConfig.Config.Team1EndZone.ToRectangleZone();
		}
		else
		{
			endZone = CaptureThePancakeConfig.Config.Team2EndZone.ToRectangleZone();
		}
		return endZone.Contains(player);
	}

	private static List<Player> GetOpposingTeam(Player player)
	{
		if (player.MatchmakingTeam == 1)
		{
			return Teams[2];
		}
		else
		{
			return Teams[1];
		}
	}

	private static List<Player> GetFriendlyTeam(Player player)
	{
		if (player.MatchmakingTeam == 1)
		{
			return Teams[1];
		}
		else
		{
			return Teams[2];
		}
	}

	public static new HashSet<string> GetAllowedCommands()
	{
		return AllowedCommands;
	}

	public static void ReportStats()
	{
		var team1 = Teams[1];
		var team2 = Teams[2];
		// Merge kills and deaths information
		var playerStats = playerKills.Keys.Select(player => new
		{
			Player = player,
			Kills = playerKills[player],
			Deaths = playerDeaths.ContainsKey(player) ? playerDeaths[player] : 0
		})
		.OrderByDescending(player => player.Kills)
		.ToList();

		// Send individual and team stats to each player
		foreach (var receiver in playerKills.Keys)
		{
			// Calculate and send team totals
			SendTeamTotals(receiver, team1, team2);

			// Send individual stats
			foreach (var stat in playerStats)
			{
				bool isStatPlayerAlly = receiver.MatchmakingTeam == stat.Player.MatchmakingTeam;
				string colorizedPlayerName = isStatPlayerAlly ? stat.Player.Name.FriendlyTeam() : stat.Player.Name.EnemyTeam();
				string colorizedKills = isStatPlayerAlly ? stat.Kills.ToString().FriendlyTeam() : stat.Kills.ToString().EnemyTeam();
				string colorizedDeaths = isStatPlayerAlly ? stat.Deaths.ToString().EnemyTeam() : stat.Deaths.ToString().FriendlyTeam();
				receiver.ReceiveMessage($"{colorizedPlayerName} - Kills: {colorizedKills}, Deaths: {colorizedDeaths}".White());
			}
		}
	}

	private static void SendTeamTotals(Player receiver, List<Player> team1, List<Player> team2)
	{
		var team1Kills = team1.Sum(player => playerKills.ContainsKey(player) ? playerKills[player] : 0);
		var team2Kills = team2.Sum(player => playerKills.ContainsKey(player) ? playerKills[player] : 0);
		var team1Deaths = team1.Sum(player => playerDeaths.ContainsKey(player) ? playerDeaths[player] : 0);
		var team2Deaths = team2.Sum(player => playerDeaths.ContainsKey(player) ? playerDeaths[player] : 0);

		string team1NameColorized;
		string team2NameColorized;
		string team1KillsColorized;
		string team2KillsColorized;
		string team1DeathsColorized;
		string team2DeathsColorized;

		if (receiver.MatchmakingTeam == 1)
		{
			team1NameColorized = "Team 1".FriendlyTeam();
			team2NameColorized = "Team 2".EnemyTeam();
			team1KillsColorized = team1Kills.ToString().FriendlyTeam();
			team2KillsColorized = team2Kills.ToString().EnemyTeam();
			team1DeathsColorized = team1Deaths.ToString().EnemyTeam();
			team2DeathsColorized = team2Deaths.ToString().FriendlyTeam();
		}
		else
		{
			team1NameColorized = "Team 1".EnemyTeam();
			team2NameColorized = "Team 2".FriendlyTeam();
			team1KillsColorized = team1Kills.ToString().EnemyTeam();
			team2KillsColorized = team2Kills.ToString().FriendlyTeam();
			team1DeathsColorized = team1Deaths.ToString().FriendlyTeam();
			team2DeathsColorized = team2Deaths.ToString().EnemyTeam();
		}

		receiver.ReceiveMessage($"{team1NameColorized} - Kills: {team1KillsColorized}, Deaths: {team1DeathsColorized}".White());
		receiver.ReceiveMessage($"{team2NameColorized} - Kills: {team2KillsColorized}, Deaths: {team2DeathsColorized}".White());
	}
}

