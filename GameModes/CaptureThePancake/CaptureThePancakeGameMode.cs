using System;
using System.Collections.Generic;
using ProjectM;
using PvpArena.Models;
using Unity.Entities;
using static ProjectM.DeathEventListenerSystem;
using ProjectM.Network;
using PvpArena.Data;
using Unity.Mathematics;
using Bloodstone.API;
using PvpArena.Helpers;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using System.Threading;
using PvpArena.Services;
using static PvpArena.Factories.UnitFactory;
using ProjectM.Shared;
using ProjectM.CastleBuilding;
using System.Linq;
using PvpArena.Factories;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using PvpArena.Patches;
using ProjectM.Gameplay.Systems;
using static PvpArena.Configs.ConfigDtos;
using Unity.Collections;
using Unity.Transforms;
using ProjectM.Gameplay.Scripting;
using AsmResolver.PE.Exceptions;
using Epic.OnlineServices.Stats;
using static ProjectM.SpawnBuffsAuthoring.SpawnBuffElement_Editor;
using ProjectM.Shared.Systems;
using static DamageRecorderService;
using Cpp2IL.Core.Extensions;
using UnityEngine.Jobs;
using ProjectM.Gameplay.Clan;

namespace PvpArena.GameModes.CaptureThePancake;

public class CaptureThePancakeGameMode : BaseGameMode
{
    public override Player.PlayerState PlayerGameModeType => Player.PlayerState.CaptureThePancake;
	public int ArenaNumber = 0;
    public override string UnitGameModeType => $"pancake{ArenaNumber}";
    public new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
	{
		RemoveConsumables = false,
		RemoveShapeshifts = true,
		ResetCooldowns = false
	};

	public Helper.ResetOptions ResetOptionsNoHeal { get; set; } = new Helper.ResetOptions
	{
		RemoveConsumables = false,
		RemoveShapeshifts = true,
		ResetCooldowns = false,
		ResetHealth = false
	};

	public bool MatchActive = false;
	private Stopwatch stopwatch = new Stopwatch();
    public HashSet<Player> Players = new();
	public Dictionary<int, List<Player>> Teams = new();
	public Dictionary<int, List<PrefabGUID>> TeamToShardBuffsMap = new Dictionary<int, List<PrefabGUID>>
	{
		{1, new List<PrefabGUID>() },
		{2, new List<PrefabGUID>() }
	};

	public HashSet<Player> DeadPlayers = new();

	private Dictionary<Player, bool> shouldRemoveGallopBuff = new Dictionary<Player, bool>();

	private Dictionary<Player, int> playerKills = new Dictionary<Player, int>();
	private Dictionary<Player, int> playerDeaths = new Dictionary<Player, int>();

	private RectangleZone endZone1;
	private RectangleZone endZone2;
	public RectangleZone EntireMapZone;

	private Dictionary<PrefabGUID, PrefabGUID> shapeshiftGroupToBuff = new Dictionary<PrefabGUID, PrefabGUID>
	{
		{ Prefabs.AB_Shapeshift_Wolf_Group, Prefabs.AB_Shapeshift_Wolf_Buff },
		{ Prefabs.AB_Shapeshift_Wolf_Skin01_Group, Prefabs.AB_Shapeshift_Wolf_Skin01_Buff },
		{ Prefabs.AB_Shapeshift_Bear_Group, Prefabs.AB_Shapeshift_Bear_Buff },
		{ Prefabs.AB_Shapeshift_Bear_Skin01_Group, Prefabs.AB_Shapeshift_Bear_Skin01_Buff },
	};

    private Dictionary<PrefabGUID, PrefabGUID> shapeshiftToShapeshift = new Dictionary<PrefabGUID, PrefabGUID>
    {
        { Prefabs.AB_Shapeshift_Wolf_Group, Prefabs.AB_Shapeshift_Bear_Group},
        { Prefabs.AB_Shapeshift_Wolf_Skin01_Group, Prefabs.AB_Shapeshift_Bear_Skin01_Group},
        { Prefabs.AB_Shapeshift_Bear_Group, Prefabs.AB_Shapeshift_Bear_Group},
        { Prefabs.AB_Shapeshift_Bear_Skin01_Group, Prefabs.AB_Shapeshift_Bear_Skin01_Group},
        { Prefabs.AB_Shapeshift_Rat_Group, Prefabs.AB_Shapeshift_Rat_Group},
        { Prefabs.AB_Shapeshift_CommandingPresence_Group, Prefabs.AB_Shapeshift_CommandingPresence_Group},
        { Prefabs.AB_Shapeshift_BloodMend_Group, Prefabs.AB_Shapeshift_BloodMend_Group}
    };

	public Dictionary<PrefabGUID, bool> allowedShapeshifts = new Dictionary<PrefabGUID, bool>
	{
		{Prefabs.AB_Shapeshift_Wolf_Group, true},
		{Prefabs.AB_Shapeshift_Wolf_Skin01_Group, true},
		{Prefabs.AB_Shapeshift_Rat_Group, true},
		{Prefabs.AB_Shapeshift_Bear_Group, true},
		{Prefabs.AB_Shapeshift_Bear_Skin01_Group, true },
		{Prefabs.AB_Shapeshift_CommandingPresence_Group, true },
        {Prefabs.AB_Shapeshift_BloodMend_Group, true }
    };

	private Dictionary<PrefabGUID, bool> damageableStructurePrefabs = new Dictionary<PrefabGUID, bool>
	{
		{ Prefabs.TM_Castle_Wall_Tier01_Wood, true },
		{ Prefabs.TM_Castle_Wall_Door_Palisade_Tier01, true}
	};

	private Dictionary<PrefabGUID, Action<Player, Entity>> _buffHandlers;

	public Dictionary<int, Player> UserIndexToPlayer = new();

	public HashSet<string> AllowedCommands = new HashSet<string>
	{
		"ping",
		"help",
		"legendary",
		"jewel",
		"forfeit",
		"points",
		"lb ranked",
		"bp",
		"lb pancake",
		"recount"
	};

	public List<Timer> Timers = new List<Timer>();
    public Dictionary<Player, List<Timer>> PlayerRespawnTimers = new Dictionary<Player, List<Timer>>();
	public Dictionary<Player, float> PlayerDamageDealt = new Dictionary<Player, float>();
	public Dictionary<Player, float> PlayerDamageReceived = new Dictionary<Player, float>();

	public Entity WingedHorrorGate;
	public Entity MonsterGate;
    public List<Entity> SpawnGates = new List<Entity>();

	public Dictionary<PrefabGUID, List<PrefabGUID>> AbilitiesToNotCauseBuffDestruction = new Dictionary<PrefabGUID, List<PrefabGUID>>
	{
		{ Prefabs.AB_Interact_Pickup_AbilityGroup, new List<PrefabGUID>{Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff} },
		{ Prefabs.AB_Interact_OpenContainer_AbilityGroup, new List<PrefabGUID>{Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff}},
		{ Prefabs.AB_Interact_Mount_Owner_Buff_Horse, new List<PrefabGUID>{Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff}},
		{ Prefabs.AB_Interact_OpenDoor_AbilityGroup, new List<PrefabGUID>{Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff}},
		{ Prefabs.AB_Interact_OpenGate_AbilityGroup, new List<PrefabGUID>{Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff}},
		{ Prefabs.AB_Interact_HealingOrb_AbilityGroup, new List<PrefabGUID>{Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff, Prefabs.AB_Shapeshift_Bear_Buff, Prefabs.AB_Shapeshift_Bear_Skin01_Buff}},
	};

	public override void Initialize()
	{
		MatchActive = true;
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
		GameEvents.OnPlayerUsedConsumable += HandleOnConsumableUse;
		GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
        GameEvents.OnPlayerBuffRemoved += HandleOnPlayerBuffRemoved;
        GameEvents.OnPlayerWillLoseGallopBuff += HandleOnPlayerWillLoseGallopBuff;
		GameEvents.OnPlayerMounted += HandleOnPlayerMounted;
		GameEvents.OnPlayerDismounted += HandleOnPlayerDismounted;
		GameEvents.OnPlayerInvitedToClan += HandleOnPlayerInvitedToClan;
		GameEvents.OnPlayerKickedFromClan += HandleOnPlayerKickedFromClan;
		GameEvents.OnPlayerLeftClan += HandleOnPlayerLeftClan;
		GameEvents.OnUnitBuffed += HandleOnUnitBuffed;
		GameEvents.OnUnitDeath += HandleOnUnitDeath;
		GameEvents.OnGameFrameUpdate += HandleOnGameFrameUpdate;
        GameEvents.OnDelayedSpawn += HandleOnDelayedSpawnEvent;
        GameEvents.OnPlayerStartedCasting += HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerDamageReported += HandleOnPlayerDamageReported;
		GameEvents.OnPlayerProjectileCreated += HandleOnPlayerProjectileCreated;
		GameEvents.OnItemWasDropped += HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
		GameEvents.OnUnitDamageDealt += HandleOnUnitDamageDealt;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
		GameEvents.OnPlayerPlacedStructure += HandleOnPlayerPlacedStructure;
		GameEvents.OnPlayerInteracted += HandleOnPlayerInteracted;
		GameEvents.OnClanStatusPostUpdate += HandleOnClanStatusPostUpdate;
		GameEvents.OnPlayerMapIconPostUpdate += HandleOnPlayerMapIconPostUpdate;

		stopwatch.Start();
	}

	public void Initialize(List<Player> team1Players, List<Player> team2Players)
	{
		endZone1 = CaptureThePancakeConfig.Config.Arenas[ArenaNumber].Team1EndZone.ToRectangleZone();
		endZone2 = CaptureThePancakeConfig.Config.Arenas[ArenaNumber].Team2EndZone.ToRectangleZone();
		EntireMapZone = CaptureThePancakeConfig.Config.Arenas[ArenaNumber].EntireMapZone.ToRectangleZone();
		Initialize();
		Teams = new Dictionary<int, List<Player>>();
        foreach (var player in team1Players)
        {
			DamageRecorderService.ClearDamageRecord(player);
			Players.Add(player);
        }
        foreach (var player in team2Players)
        {
			DamageRecorderService.ClearDamageRecord(player);
			Players.Add(player);
        }
        Teams[1] = team1Players;
		Teams[2] = team2Players;
		_buffHandlers = new Dictionary<PrefabGUID, Action<Player, Entity>>
		{
			{ Prefabs.AB_Subdue_Channeling, HandleSubdueChannelingBuff },
			{ Prefabs.AB_Interact_Mount_Owner_Buff_Horse, HandleMountBuff },
			{ Prefabs.AB_Interact_HealingOrb_Buff, HandleHealingOrbBuff },
			{ Prefabs.Buff_General_RelicCarryDebuff, HandleRelicDebuff },
			{ Prefabs.Buff_InCombat, HandleInCombatBuff },
			{ Prefabs.AB_Shapeshift_BloodMend_Buff, HandleBloodMendBuff },
			{ Prefabs.AB_Storm_PolarityShift_Travel_Ally, HandleFriendlyPolarityShiftBuff },
			{ Prefabs.HideCharacterBuff, HandleHideCharacterBuff },
			{ Prefabs.Buff_InCombat_PvPVampire, HandlePvPBuff},
		};
		playerKills.Clear();
		playerDeaths.Clear();

		foreach (var team in Teams.Values)
		{
			foreach (var player in team)
			{
				playerKills[player] = 0;
				playerDeaths[player] = 0;
                PlayerRespawnTimers[player] = new List<Timer>();
				PlayerDamageDealt[player] = 0;
				PlayerDamageReceived[player] = 0;
				UserIndexToPlayer[player.User.Read<User>().Index] = player;
			}
		}
	}
	public override void Dispose()
	{
		MatchActive = false;
		GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
		GameEvents.OnPlayerUsedConsumable -= HandleOnConsumableUse;
		GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
        GameEvents.OnPlayerBuffRemoved -= HandleOnPlayerBuffRemoved;
        GameEvents.OnPlayerWillLoseGallopBuff -= HandleOnPlayerWillLoseGallopBuff;
		GameEvents.OnPlayerMounted -= HandleOnPlayerMounted;
		GameEvents.OnPlayerDismounted -= HandleOnPlayerDismounted;
		GameEvents.OnPlayerInvitedToClan -= HandleOnPlayerInvitedToClan;
		GameEvents.OnPlayerKickedFromClan -= HandleOnPlayerKickedFromClan;
		GameEvents.OnPlayerLeftClan -= HandleOnPlayerLeftClan;
		GameEvents.OnUnitBuffed -= HandleOnUnitBuffed;
		GameEvents.OnUnitDeath -= HandleOnUnitDeath;
		GameEvents.OnGameFrameUpdate -= HandleOnGameFrameUpdate;
        GameEvents.OnDelayedSpawn -= HandleOnDelayedSpawnEvent;
        GameEvents.OnPlayerStartedCasting -= HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerDamageReported -= HandleOnPlayerDamageReported;
		GameEvents.OnPlayerProjectileCreated -= HandleOnPlayerProjectileCreated;
		GameEvents.OnItemWasDropped -= HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
		GameEvents.OnUnitDamageDealt -= HandleOnUnitDamageDealt;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
		GameEvents.OnPlayerPlacedStructure -= HandleOnPlayerPlacedStructure;
		GameEvents.OnPlayerInteracted -= HandleOnPlayerInteracted;
		GameEvents.OnClanStatusPostUpdate -= HandleOnClanStatusPostUpdate;
		GameEvents.OnPlayerMapIconPostUpdate -= HandleOnPlayerMapIconPostUpdate;

        Players.Clear();
		Teams.Clear();
		UserIndexToPlayer.Clear();
		foreach (var shardBuffs in TeamToShardBuffsMap.Values)
		{
			shardBuffs.Clear();
		}
		foreach (var kvp in UnitFactory.UnitToEntity)
        {
            if (kvp.Key.GameMode == UnitGameModeType)
            {
                UnitFactory.UnitToEntity.Remove(kvp.Key);
            }
        }
		foreach (var kvp in PlayerRespawnTimers)
        {
            foreach (var timer in kvp.Value)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
        }
		PlayerRespawnTimers.Clear();

        shouldRemoveGallopBuff.Clear();
		playerKills.Clear();
		playerDeaths.Clear();
		PlayerDamageDealt.Clear();
		PlayerDamageReceived.Clear();
		stopwatch.Reset();
		WingedHorrorGate = Entity.Null;
		MonsterGate = Entity.Null;
	}

	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

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
					string message = $"{coloredVictimName} was killed by {"mysterious forces".EnemyTeam()}".White();
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

		//clear out any queued up respawn actions since we will recreate them now that the player has died (in case they killed themselves twice in a row before the initial respawn actions finished)
		if (PlayerRespawnTimers.TryGetValue(player, out var respawnActions))
		{
			foreach (var respawnAction in respawnActions)
			{
				try
				{
					respawnAction?.Dispose();
				}
				catch (Exception ex)
				{
					Plugin.PluginLog.LogInfo($"Error disposing respawn actions in pancake: {ex.ToString()}");
				}
			}

			respawnActions.Clear();
		}

		DropShardsOnDeathIfApplicable(player);

		float3 pos = default;
		if (player.MatchmakingTeam == 1)
		{
			pos = CaptureThePancakeConfig.Config.Arenas[ArenaNumber].Team1PlayerRespawn.ToFloat3();
		}
		else if (player.MatchmakingTeam == 2)
		{
			pos = CaptureThePancakeConfig.Config.Arenas[ArenaNumber].Team2PlayerRespawn.ToFloat3();
		}

		Action removeSpectatorModeAction = () =>
		{
			player.Reset(ResetOptions);
			Helper.RemoveBuff(player, Prefabs.AB_Shapeshift_Mist_Buff);
			if (Helper.BuffPlayer(player, Prefabs.Buff_General_Phasing, out var buffEntity, 3))
			{
				Helper.ApplyStatModifier(buffEntity, BuffModifiers.FastRespawnMoveSpeed);
				Helper.RemoveBuffModifications(buffEntity, BuffModificationTypes.Immaterial);
			}
			DeadPlayers.Remove(player);
		};

		Action bringSpectatorBackToBaseRootedAction = () =>
		{
			player.Teleport(pos);
			if (Helper.BuffPlayer(player, Helper.CustomBuff1, out var buffEntity, 3))
			{
				Helper.ModifyBuff(buffEntity, BuffModificationTypes.MovementImpair | BuffModificationTypes.Immaterial);
			}
			var timer = ActionScheduler.RunActionOnceAfterDelay(removeSpectatorModeAction, 3);
			PlayerRespawnTimers[player].Add(timer);

			foreach (var buff in TeamToShardBuffsMap[player.MatchmakingTeam])
			{
				if (!Helper.BuffPlayer(player, buff, out buffEntity, Helper.NO_DURATION))
				{
					var action = () => { Helper.BuffPlayer(player, buff, out var buffEntity, Helper.NO_DURATION, true); };
					ActionScheduler.RunActionOnceAfterFrames(action, 2); //some buffs need delays or they won't be applied
				}
			}
		};

        player.Reset(ResetOptionsNoHeal);
        var respawnDelay = CalculateRespawnDelay();
		DeadPlayers.Add(player);
		PlayerRespawnTimers[player].Add(Helper.MakeGhostlySpectator(player, respawnDelay));
		var timer = ActionScheduler.RunActionOnceAfterDelay(bringSpectatorBackToBaseRootedAction, respawnDelay - 3);
		PlayerRespawnTimers[player].Add(timer);
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

	private void DropShardsOnDeathIfApplicable(Player player)
	{
		if (player.HasBuff(Prefabs.Buff_General_RelicCarryDebuff))
		{
			var hasManticoreShard = Helper.PlayerHasItemInInventories(player, Prefabs.Item_Building_Relic_Manticore);
			var hasMonsterShard = Helper.PlayerHasItemInInventories(player, Prefabs.Item_Building_Relic_Monster);
			if (hasManticoreShard && hasMonsterShard)
			{
				CaptureThePancakeManager.DropItemsIntoBag(player, new List<PrefabGUID> { Prefabs.Item_Building_Relic_Monster, Prefabs.Item_Building_Relic_Manticore }, ArenaNumber);
				Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Building_Relic_Monster);
				Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Building_Relic_Manticore);
			}
			else if (hasManticoreShard)
			{
				CaptureThePancakeManager.DropItemsIntoBag(player, new List<PrefabGUID> { Prefabs.Item_Building_Relic_Manticore }, ArenaNumber);
				Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Building_Relic_Manticore);
			}
			else if (hasMonsterShard)
			{
				CaptureThePancakeManager.DropItemsIntoBag(player, new List<PrefabGUID> { Prefabs.Item_Building_Relic_Monster }, ArenaNumber);
				Helper.CompletelyRemoveItemFromInventory(player, Prefabs.Item_Building_Relic_Monster);
			}
		}
	}

	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

        var enterShapeshiftEvent = eventEntity.Read<ProjectM.Network.EnterShapeshiftEvent>();
		if (!shapeshiftToShapeshift.ContainsKey(enterShapeshiftEvent.Shapeshift))
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
			player.ReceiveMessage($"That shapeshift is disabled while in Capture the Pancake.".Error());
		}
		else
		{
			enterShapeshiftEvent.Shapeshift = shapeshiftToShapeshift[enterShapeshiftEvent.Shapeshift];
			eventEntity.Write(enterShapeshiftEvent);
		}
	}
	public void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (player.CurrentState != PlayerGameModeType) return;
        if (!Players.Contains(player)) return;


        if (item.ItemType == Prefabs.Item_Consumable_GlassBottle_BloodRosePotion_T02 || item.ItemType == Prefabs.Item_Consumable_Canteen_BloodRoseBrew_T01)
		{
			eventEntity.Destroy();
			player.ReceiveMessage("You can't drink those during a pancake match!".Error());
		}
	}

	public void HandleSubdueChannelingTargetDebuff(Entity unit, Entity buffEntity)
	{
		var owner = buffEntity.Read<EntityOwner>().Owner;
		if (owner.Has<PlayerCharacter>())
		{
			var player = PlayerService.GetPlayerFromCharacter(owner);
			if (unit.Exists() && Team.IsAllies(player.Character.Read<Team>(), unit.Read<Team>()))
			{
				player.Interrupt();
				player.ReceiveMessage("That's the wrong pancake!".Error());
			}
			else if (unit.Exists())
			{
				var lifetime = buffEntity.Read<LifeTime>();
				lifetime.Duration += 5;
				buffEntity.Write(lifetime);
				var buffer = buffEntity.ReadBuffer<CreateGameplayEventsOnTimePassed>();

				for (var i = 0; i < buffer.Length; i++)
				{
					var gameplayEvent = buffer[i];
					//gameplayEvent.Target = GameplayEventTarget.None;
					gameplayEvent.Duration += 5;
					buffer[i] = gameplayEvent;
				}
				foreach (var team in Teams.Values)
				{
					foreach (var teamPlayer in team)
					{
						bool isAllied = teamPlayer.MatchmakingTeam == player.MatchmakingTeam;
						string message;
						if (isAllied)
						{
							message = $"{player.Name.FriendlyTeam()} is returning your {"pancake".Emphasize()}! Help them!".White();
						}
						else
						{
							message = $"{player.Name.EnemyTeam()} is returning the enemy's {"pancake".Emphasize()}! Stop them!".White();
						}
						teamPlayer.ReceiveMessage(message);
					}
				}
			}
		}
	}

	public static void HandleSubdueChannelingBuff(Player player, Entity buffEntity)
	{
		var lifetime = buffEntity.Read<LifeTime>();
		lifetime.Duration += 5;
		buffEntity.Write(lifetime);
		buffEntity.Add<DestroyBuffOnDamageTaken>();
	}
	public void HandleSubdueActiveBuff(Entity unit, Entity buffEntity)
	{
		var owner = buffEntity.Read<EntityOwner>().Owner;
		if (owner.Has<PlayerCharacter>())
		{
			var player = PlayerService.GetPlayerFromCharacter(owner);
			foreach (var unitSpawn in CaptureThePancakeConfig.Config.Arenas[ArenaNumber].UnitSpawns)
			{
				if (unitSpawn.Type.ToLower() == "horse")
				{
					if (unitSpawn.Team != player.MatchmakingTeam)
					{
						unit.Teleport(unitSpawn.Location.ToFloat3());
						var newUnit = new ObjectiveHorse(unitSpawn.Team);
						newUnit.MaxHealth = unitSpawn.Health;
						newUnit.GameMode = UnitGameModeType;
						UnitFactory.SpawnUnit(newUnit, unitSpawn.Location.ToFloat3(), Teams[unitSpawn.Team][0]);
						break;
					}
				}
			}
		}
	}

	public static void HandleMountBuff(Player player, Entity buffEntity)
	{
		Helper.BuffPlayer(player, Prefabs.AB_Gallop_Buff, out var buffEntity2, Helper.NO_DURATION); //I don't know how to make horses go below 6 speed without gallop buff
		Helper.ModifyBuff(buffEntity2, BuffModificationTypes.DisableDynamicCollision);
	}

	public static void HandleHealingOrbBuff(Player player, Entity buffEntity)
	{
		var buffer = buffEntity.ReadBuffer<HealOnGameplayEvent>();
		for (var i = 0; i < buffer.Length; i++)
		{
			var heal = buffer[i];
			heal.HealthPercent = 0.2f;
			buffer[i] = heal;
		}
	}

	public void HandleRelicDebuff(Player player, Entity buffEntity)
	{
		buffEntity.Remove<ShapeshiftImpairBuff>();
		var buffer = buffEntity.ReadBuffer<GameplayEventListeners>();
		buffer.Clear();
		
		Helper.ApplyStatModifier(buffEntity, BuffModifiers.PancakeSlowRelicSpeed);
		Helper.BuffPlayer(player, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff, out var grandmaBuffEntity, Helper.NO_DURATION);
		grandmaBuffEntity.Add<DestroyOnAbilityCast>();
		var scriptBuffShapeshiftDataShared = grandmaBuffEntity.Read<Script_Buff_Shapeshift_DataShared>();
		scriptBuffShapeshiftDataShared.RemoveOnDamageTaken = false;
		grandmaBuffEntity.Write(scriptBuffShapeshiftDataShared);
		grandmaBuffEntity.Remove<ModifyMovementSpeedBuff>();
		Helper.ModifyBuff(grandmaBuffEntity, BuffModificationTypes.TargetSpellImpaired | BuffModificationTypes.DisableDynamicCollision);
		Helper.FixIconForShapeshiftBuff(player, grandmaBuffEntity, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Group);

		Helper.RemoveNewAbilitiesFromBuff(grandmaBuffEntity);

		foreach (var team in Teams.Values)
		{
			foreach (var teamPlayer in team)
			{
				var isTeammate = player.MatchmakingTeam == teamPlayer.MatchmakingTeam;
				var colorizedName = isTeammate ? player.Name.FriendlyTeam() : player.Name.EnemyTeam();
				var message = $"{colorizedName} is moving a shard!".White();
				teamPlayer.ReceiveMessage(message);
			}
		}
	}

	public static void HandleShapeshiftBuff(Player player, Entity buffEntity)
	{
		buffEntity.Add<DestroyBuffOnDamageTaken>();
		buffEntity.Add<DestroyOnAbilityCast>();
		var buffer = buffEntity.ReadBuffer<ReplaceAbilityOnSlotBuff>();
		buffer.Clear();
		Helper.ApplyStatModifier(buffEntity, BuffModifiers.PancakeShapeshiftSpeed);
	}

	public static void HandleInCombatBuff(Player player, Entity buffEntity)
	{
		Helper.DestroyBuff(buffEntity);
	}

	public static void HandleBloodMendBuff(Player player, Entity buffEntity)
	{
		var buffer = buffEntity.ReadBuffer<ChangeBloodOnGameplayEvent>();
		for (var i = 0; i < buffer.Length; i++)
		{
			var changeBloodOnGameplayEvent = buffer[i];
			changeBloodOnGameplayEvent.BloodValue = 10;
			buffer[i] = changeBloodOnGameplayEvent;
		}
	}

	public static void HandleFriendlyPolarityShiftBuff(Player player, Entity buffEntity)
	{	
		if (Helper.HasBuff(player, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff))
		{
			var travelBuff = buffEntity.Read<TravelBuff>();
			travelBuff.EndPosition = travelBuff.StartPosition;
			buffEntity.Write(travelBuff);
			Helper.DestroyBuff(buffEntity);
		}
	}

	public static void HandleHideCharacterBuff(Player player, Entity buffEntity)
	{
		Action action = () => {
			if (buffEntity.Exists())
			{
				//Sometimes respawning takes forever to remove your hidden character buff which roots you. This is a failsafe
				//I am not removing it right away as it will do an awkward stutter when it finally loads if you aren't rooted
				//That usually happens after the 1st second or so. But if we'll end up rooted for 10 seconds it's worth the stutter to be able to move
				Helper.ModifyBuff(buffEntity, BuffModificationTypes.None, true);
			}
		};
		ActionScheduler.RunActionOnceAfterDelay(action, 2);
	}

    public static void HandlePvPBuff(Player player, Entity buffEntity)
    {
        if (Helper.HasBuff(player, Prefabs.Buff_General_HideCorpse))
        {
            Helper.DestroyBuff(buffEntity);
        }
        else
        {
            Helper.SetBuffDuration(buffEntity, CaptureThePancakeConfig.Config.PvpTimerDuration);
        }
    }

    public void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
        if (player.CurrentState != this.PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

        var prefabGuid = buffEntity.Read<PrefabGUID>();
		var buff = buffEntity.Read<Buff>();
		var target = buff.Target;
		Player targetPlayer;
		if (buff.Target.Has<PlayerCharacter>())
		{
			targetPlayer = PlayerService.GetPlayerFromCharacter(target);
		}
		else
		{
			targetPlayer = player;
		}
        
		if (_buffHandlers.TryGetValue(prefabGuid, out var handler))
		{
			handler(targetPlayer, buffEntity);
		}
		else if (shapeshiftGroupToBuff.ContainsValue(prefabGuid))
		{
			HandleShapeshiftBuff(targetPlayer, buffEntity);
		}
		else if (buffEntity.Has<AbsorbBuff>())/* && BuffHelper.HasBuff(player, Prefabs.AB_Interact_Mount_Owner_Buff_Horse))*/
		{
			if (targetPlayer.HasBuff(Prefabs.AB_Interact_Mount_Owner_Buff_Horse)) 
			{
				Helper.DestroyBuff(buffEntity);
			}
		}
	}

    public void HandleOnPlayerBuffRemoved(Player player, Entity buffEntity)
    {
        if (player.CurrentState != this.PlayerGameModeType) return;

        if (!Players.Contains(player)) return;

        var prefabGuid = buffEntity.Read<PrefabGUID>();
        if (prefabGuid == Prefabs.Buff_General_RelicCarryDebuff)
        {
            Helper.RemoveBuff(player, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff);
			Helper.MakePlayerCcDefault(player);
		}
        else if (prefabGuid == Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff)
        {
            if (player.IsAlive)
            {
                Helper.DropItemFromInventory(player, Prefabs.Item_Building_Relic_Manticore);
                Helper.DropItemFromInventory(player, Prefabs.Item_Building_Relic_Monster);
            }
        }
    }

	public void HandleOnUnitBuffed(Entity unit, Entity buffEntity)
	{
		if (!UnitFactory.HasGameMode(unit, UnitGameModeType)) return;
		if (!TryGetSpawnedUnitFromEntity(unit, out SpawnedUnit spawnedUnit)) return;
		var buffPrefabGUID = buffEntity.Read<PrefabGUID>();
		if (unit.Read<PrefabGUID>() == Prefabs.CHAR_Gloomrot_Purifier_VBlood)
		{
			if (buffPrefabGUID == Prefabs.Buff_Purifier_Return)
			{
				unit.Teleport(spawnedUnit.SpawnPosition); //put him back where he started
			}
			else if (buffPrefabGUID == Prefabs.Buff_InCombat_VBlood_Purifier)
			{
				buffEntity.Add<BlockHealBuff>();
				buffEntity.Write(new BlockHealBuff
				{
					PercentageBlocked = 1
				});
			}
			else if (buffPrefabGUID == Prefabs.AB_Purifier_JetPunch_Dash)
			{
				Helper.RemoveBuff(unit, Prefabs.AB_Purifier_JetPunch_Dash); //try to keep him where he is
			}
			else if (buffPrefabGUID == Prefabs.AB_Purifier_JetPunchSetup_Forward_Phase)
			{
				Helper.RemoveBuff(unit, Prefabs.AB_Purifier_JetPunchSetup_Forward_Phase);
			}
		}
		else if (unit.Read<PrefabGUID>() == Prefabs.CHAR_Gloomrot_SpiderTank_LightningRod)
		{
			Helper.BuffEntity(unit.Read<EntityOwner>().Owner, Prefabs.AB_LightningStrike_RodHit_EmpowerTankBuff, out var buffEntity2, Helper.NO_DURATION); //perma charge lightning rod turrets	
		}
		else if (buffPrefabGUID == Prefabs.AB_Subdue_Channeling_Target_Debuff)
		{
			HandleSubdueChannelingTargetDebuff(unit, buffEntity);
		}
		else if (buffPrefabGUID == Prefabs.AB_Subdue_CaptureBuff_Target)
		{
			HandleSubdueActiveBuff(unit, buffEntity);
		}
	}

	public override void HandleOnPlayerConnected(Player player)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;

        if (!Players.Contains(player)) return;

        if (player.MatchmakingTeam == 1)
		{
			Helper.RespawnPlayer(player, CaptureThePancakeConfig.Config.Arenas[ArenaNumber].Team1PlayerRespawn.ToFloat3());
		}
		else if (player.MatchmakingTeam == 2)
		{
			Helper.RespawnPlayer(player, CaptureThePancakeConfig.Config.Arenas[ArenaNumber].Team2PlayerRespawn.ToFloat3());
		}
	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

        Helper.SoftKillPlayer(player);
        var action = () => player.TeleportToOfflinePosition();
		ActionScheduler.RunActionOnceAfterFrames(action, 2);
    }

	public void HandleOnPlayerWillLoseGallopBuff(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

        if (!(shouldRemoveGallopBuff.TryGetValue(player, out var value) && value))
		{
			eventEntity.Remove<DestroyTag>(); //the game tries to remove the gallop buff whenever you stop moving, stop this so the horse stays slow
		}
	}

	public void HandleOnPlayerMounted(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

        shouldRemoveGallopBuff[player] = false; //flag gallop buff for destruction once they dismount

		Helper.RemoveAllShieldBuffs(player);

		foreach (var team in Teams.Values)
		{
			foreach (var teamPlayer in team)
			{
				bool isTeammate = player.MatchmakingTeam == teamPlayer.MatchmakingTeam;
				var colorizedName = isTeammate ? player.Name.FriendlyTeam() : player.Name.EnemyTeam();
				string message;
				if (isTeammate)
				{
					message = $"{colorizedName} is stealing the enemy's {"pancake".Emphasize()}!";
				}
				else
				{
					message = $"{colorizedName} is stealing your {"pancake".Emphasize()}! Stop them!";
				}
				
				teamPlayer.ReceiveMessage(message.White());
			}
		}
	}

	public void HandleOnPlayerDismounted(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

        shouldRemoveGallopBuff[player] = true; // when they re-mount, re-flag gallop buff to not be destroyed
	}

	public void HandleOnPlayerInvitedToClan(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

        VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not invite players to your clan while in Capture the Pancake".Error());
	}

	public void HandleOnPlayerKickedFromClan(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

        VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not kick players from your clan while in Capture the Pancake".Error());
	}

	public void HandleOnPlayerLeftClan(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

        VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not leave your clan while in Capture the Pancake".Error());
	}

	public void HandleOnUnitDeath(Entity unitEntity, DeathEvent deathEvent)
	{
		if (!MatchActive) return;
        if (!UnitFactory.HasGameMode(unitEntity, UnitGameModeType)) return;

        if (TryGetSpawnedUnitFromEntity(unitEntity, out var spawnedUnit))
		{
			if (spawnedUnit.Unit.PrefabGuid == Prefabs.CHAR_Gloomrot_Purifier_VBlood)
			{
				if (spawnedUnit.Unit.Team == 1)
				{
					var team = Teams[2];
					foreach (var player in team)
					{
						Helper.BuffPlayer(player, Prefabs.AB_Consumable_PhysicalBrew_T02_Buff, out var buffEntity, Helper.NO_DURATION, true);
						Helper.BuffPlayer(player, Prefabs.AB_Consumable_SpellBrew_T02_Buff, out buffEntity, Helper.NO_DURATION, true);
						player.ReceiveMessage("You have been empowered for killing an enemy boss!".Success());
					}
					team = Teams[1];
					foreach (var player in team)
					{
						player.ReceiveMessage("The enemy team has been empowered for killing your boss!".Error());
					}
				}
				else if (spawnedUnit.Unit.Team == 2)
				{
					var team = Teams[1];
					foreach (var player in team)
					{
						Helper.BuffPlayer(player, Prefabs.AB_Consumable_PhysicalBrew_T02_Buff, out var buffEntity, Helper.NO_DURATION, true);
						Helper.BuffPlayer(player, Prefabs.AB_Consumable_SpellBrew_T02_Buff, out buffEntity, Helper.NO_DURATION, true);
						player.ReceiveMessage("You have been empowered for killing an enemy boss!".Success());
					}
					team = Teams[2];
					foreach (var player in team)
					{
						player.ReceiveMessage("The enemy team has been empowered for killing your boss!".Error());
					}
				}
			}
			else if (spawnedUnit.Unit.PrefabGuid == Prefabs.CHAR_Gloomrot_SpiderTank_LightningRod)
			{
				foreach (var configUnitSpawn in CaptureThePancakeConfig.Config.Arenas[ArenaNumber].UnitSpawns)
				{
					if (configUnitSpawn.Location.ToFloat3().Equals(spawnedUnit.SpawnPosition))
					{
						if (configUnitSpawn.Description == "Monster Chest Guard")
						{
							var door = MonsterGate.Read<Door>();
							door.OpenState = true;
							MonsterGate.Write(door);
                            Helper.RemoveBuff(MonsterGate, Prefabs.Buff_Voltage_Stage2);
                        }
						else if (configUnitSpawn.Description == "Winged Horror Chest Guard")
						{
							var door = WingedHorrorGate.Read<Door>();
							door.OpenState = true;
							WingedHorrorGate.Write(door);
                            Helper.RemoveBuff(WingedHorrorGate, Prefabs.Buff_Voltage_Stage2);
                        }
					}
				}
			}
		}
	}

	public override void HandleOnItemWasDropped(Player player, Entity eventEntity, PrefabGUID itemType, int slotIndex)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

        var prefabEntity = Helper.GetPrefabEntityByPrefabGUID(itemType);
		
		if (!prefabEntity.Has<Relic>())
		{
			base.HandleOnItemWasDropped(player, eventEntity, itemType, slotIndex);
		}
	}

	public override void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

        if (!eventEntity.Exists()) return;

		var damageDealtEvent = eventEntity.Read<DealDamageEvent>();
		var targetPrefab = damageDealtEvent.Target.Read<PrefabGUID>();
		var spellPrefab = damageDealtEvent.SpellSource.GetPrefabGUID();

		bool isSpawnedByGameMode = false;
		var isStructure = damageDealtEvent.Target.Has<CastleHeartConnection>();
		if (UnitFactory.TryGetSpawnedUnitFromEntity(damageDealtEvent.Target, out var unit))
		{
			if (unit.Unit.GameMode == UnitGameModeType)
			{
				isSpawnedByGameMode = true;
			}
		}
		if (isStructure)
		{
			if (damageableStructurePrefabs.ContainsKey(targetPrefab) || isSpawnedByGameMode)
			{
				damageDealtEvent.MaterialModifiers.StoneStructure = 1;
				eventEntity.Write(damageDealtEvent);
			}
			else
			{
				VWorld.Server.EntityManager.DestroyEntity(eventEntity);
			}
		}
		else if (spellPrefab == Prefabs.AB_Storm_RagingTempest_Other_Self_Buff)
		{
			if (damageDealtEvent.Target.Has<PlayerCharacter>())
			{
				var targetPlayer = PlayerService.GetPlayerFromCharacter(damageDealtEvent.Target);
				if (targetPlayer.IsImmaterial())
				{
					eventEntity.Destroy();
				}
			}
		}
	}

	public override void HandleOnUnitDamageDealt(Entity unit, Entity eventEntity)
	{
        if (!UnitFactory.HasGameMode(unit, UnitGameModeType)) return;

        var damageDealtEvent = eventEntity.Read<DealDamageEvent>();
		var source = damageDealtEvent.SpellSource.Read<EntityOwner>().Owner;
		var target = damageDealtEvent.Target;
		if (target.Exists() && target.Has<PlayerCharacter>())
		{
			if (source.Read<PrefabGUID>() == Prefabs.CHAR_Gloomrot_Purifier_VBlood)
			{
				damageDealtEvent.MaterialModifiers.PlayerVampire *= 1.5f;
				eventEntity.Write(damageDealtEvent);
			}
		}
	}

    public void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
    {
        if (player.CurrentState != this.PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

        var abilityCastStartedEvent = eventEntity.Read<AbilityCastStartedEvent>();
        if (!abilityCastStartedEvent.AbilityGroup.Exists()) return;

        var abilityGuid = abilityCastStartedEvent.AbilityGroup.Read<PrefabGUID>();

        //prevent shapeshift spells from breaking players out of their shapeshift (due to the abnormal way we gave them shapeshift)
        if (AbilitiesToNotCauseBuffDestruction.TryGetValue(abilityGuid, out var buffs))
        {
			PreventBuffDestructionIfBuffPresent(abilityCastStartedEvent, buffs);
		}
	}

	private void PreventBuffDestructionIfBuffPresent(AbilityCastStartedEvent abilityCastStartedEvent, List<PrefabGUID> buffs)
	{
		foreach (var buff in buffs)
		{
			if (BuffUtility.TryGetBuff(VWorld.Server.EntityManager, abilityCastStartedEvent.Character, buff, out Entity buffEntity))
			{
				if (buffEntity.Has<DestroyOnAbilityCast>())
				{
					var destroyOnAbilityCast = buffEntity.Read<DestroyOnAbilityCast>();
					destroyOnAbilityCast.CastCount = 0;
					buffEntity.Write(destroyOnAbilityCast);
				}
			}
		}
	}

	public void HandleOnDelayedSpawnEvent(Unit unit, int timeToSpawn)
    {
        if (unit.GameMode != UnitGameModeType) return;

		if (timeToSpawn <= 0)
        {
			if (UnitFactory.UnitToEntity.ContainsKey(unit))
            {
				var entity = UnitFactory.UnitToEntity[unit];
				Helper.RemoveBuff(entity, Prefabs.Buff_General_VampireMount_Dead);
			}
        }

        foreach (var team in Teams.Values)
        {
            foreach (var player in team)
            {
                if (player.MatchmakingTeam == unit.Team && unit.AnnounceSpawn)
                {
					if (timeToSpawn > 60)
					{
						player.ReceiveMessage($"{unit.Name.Emphasize()} will spawn in {timeToSpawn.ToString().Emphasize()} seconds!".White());
					}
					else if (timeToSpawn >= 30)
                    {
                        player.ReceiveMessage($"{unit.Name.Emphasize()} will spawn in {timeToSpawn.ToString().Emphasize()} seconds!".White());
                    }
                    else if (timeToSpawn <= 0)
                    {
                        player.ReceiveMessage($"{unit.Name.Emphasize()} has spawned!".White());
                    }
                }
            }
        }
    }

	public void HandleOnPlayerDamageReported(Player source, Entity target, PrefabGUID ability, DamageInfo damageInfo)
	{
		if (source.CurrentState != PlayerGameModeType) return;
        if (!Players.Contains(source)) return;

        if (!target.Has<PlayerCharacter>()) return;

		var targetPlayer = PlayerService.GetPlayerFromCharacter(target);
		if (source.CurrentState != PlayerGameModeType || targetPlayer.CurrentState != PlayerGameModeType) return;

		DamageRecorderService.RecordDamageDone(source, ability, damageInfo);

		if (!PlayerDamageDealt.ContainsKey(source)) 
		{
			PlayerDamageDealt[source] = 0;
		}
		if (!PlayerDamageReceived.ContainsKey(targetPlayer))
		{
			PlayerDamageReceived[targetPlayer] = 0;
		}
		PlayerDamageDealt[source] += damageInfo.TotalDamage;
		PlayerDamageReceived[targetPlayer] += damageInfo.TotalDamage;
	}

	public void HandleOnPlayerProjectileCreated(Player player, Entity projectile)
	{
		if (player.CurrentState != PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

        var prefabGuid = projectile.Read<PrefabGUID>();
		if (prefabGuid == Prefabs.AB_Subdue_Projectile)
		{
			var buffer = projectile.ReadBuffer<HitColliderCast>();
			for (var i = 0; i < buffer.Length; i++)
			{
				var hitCollider = buffer[i];
				hitCollider.IgnoreImmaterial = true;
				buffer[i] = hitCollider;
			}
		}
	}

	public void HandleOnPlayerInteracted(Player player, Interactor interactor)
	{
		if (player.CurrentState != PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

        if (interactor.Target.Has<Mountable>() && !Team.IsAllies(interactor.Target.Read<Team>(), player.Character.Read<Team>()))
		{
			interactor.Target = player.Character;
			player.Character.Write(interactor);
		}
	}

	public void HandleOnPlayerMapIconPostUpdate(Player player, Entity mapIconEntity)
	{
		if (player.CurrentState != PlayerGameModeType) return;
        if (!Players.Contains(player)) return;

        if (DeadPlayers.Contains(player))
		{
			var position = mapIconEntity.Read<MapIconPosition>();
			position.TilePosition = new int2(0, 0);
			mapIconEntity.Write(position);
		}
	}

	public void HandleOnClanStatusPostUpdate()
	{
		try
		{
			var clan1 = Teams[1][0].Clan;
			var clan2 = Teams[2][0].Clan;
			var clans = new List<Entity>
		{
			clan1, clan2
		};

			foreach (var clan in clans)
			{
				if (clan.Exists())
				{
					var buffer = clan.ReadBuffer<ClanMemberStatus>();
					for (var i = 0; i < buffer.Length; i++)
					{
						var clanMemberStatus = buffer[i];
						var player = UserIndexToPlayer[clanMemberStatus.UserIndex];
						if (DeadPlayers.Contains(player))
						{
							if (Helper.TryGetBuff(player, Prefabs.AB_Shapeshift_Mist_Buff, out var buffEntity))
							{
								var age = buffEntity.Read<Age>();
								var lifetime = buffEntity.Read<LifeTime>();
								var percent = (int)((age.Value / lifetime.Duration) * 100);
								clanMemberStatus.HealthPercent = percent;
								clanMemberStatus.IsConnected = false;
								buffer[i] = clanMemberStatus;
							}
						}
					}
				}
			}
		}
		catch
		{

		}
	}

	public void HandleOnGameFrameUpdate()
	{
		//check for shard captures
		foreach (var team in Teams.Values)
		{
			foreach (var player in team)
			{
				if (IsInFriendlyEndZone(player))
				{
                    if (player.HasBuff(Prefabs.AB_Interact_Mount_Owner_Buff_Horse))
					{
						foreach (var teamPlayer in team)
						{
							string message = player != teamPlayer ? $"{player.Name} has captured the pancake! You win!" : $"You have captured the pancake! You win!";
							teamPlayer.ReceiveMessage(message.Success());
						}
						var enemyTeam = GetOpposingTeam(player);
						foreach (var enemyTeamPlayer in enemyTeam)
						{
							string message = $"{player.Name} has captured the pancake! You lose!";
							enemyTeamPlayer.ReceiveMessage(message.Error());
						}
						//output the stats to everyone
						ReportStats();
						CaptureThePancakeManager.EndMatch(ArenaNumber, player.MatchmakingTeam);
						return;
					}
					else if (player.HasBuff(Prefabs.Buff_General_RelicCarryDebuff))
					{
						CheckForShardAndUpdate(player, Shards.Monster);
						CheckForShardAndUpdate(player, Shards.WingedHorror);
					}
				}
			}
		}
	}

	private void CheckForShardAndUpdate(Player player, ShardData shardData)
	{
		if (Helper.PlayerHasItemInInventories(player, shardData.ItemPrefabGUID))
		{
            Helper.CompletelyRemoveItemFromInventory(player, shardData.ItemPrefabGUID);
			var friendlyTeam = GetFriendlyTeam(player);
			var enemyTeam = GetOpposingTeam(player);
			if (!TeamToShardBuffsMap[player.MatchmakingTeam].Contains(shardData.BuffPrefabGUID))
			{
				TeamToShardBuffsMap[player.MatchmakingTeam].Add(shardData.BuffPrefabGUID);
			}

			foreach (var teamPlayer in friendlyTeam)
			{
				Helper.BuffPlayer(teamPlayer, shardData.BuffPrefabGUID, out Entity buffEntity, Helper.NO_DURATION, true);
				string message = player != teamPlayer ? $"{player.Name} has captured the {shardData.ShardName}!" : $"You have captured the {shardData.ShardName}!";
				teamPlayer.ReceiveMessage(message.Success());
			}
			foreach (var enemyTeamPlayer in enemyTeam)
			{
				string message = $"{player.Name} has captured the {shardData.ShardName}!";
				enemyTeamPlayer.ReceiveMessage(message.Error());
			}
		}
	}

	private bool IsInFriendlyEndZone(Player player)
	{
		RectangleZone endZone;
		if (player.MatchmakingTeam == 1)
		{
			endZone = endZone1;
		}
		else
		{
			endZone = endZone2;
		}
		return endZone.Contains(player);
	}

	private List<Player> GetOpposingTeam(Player player)
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

	private List<Player> GetFriendlyTeam(Player player)
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

	private int CalculateRespawnDelay()
	{
		var initialRespawnDelay = CaptureThePancakeConfig.Config.PlayerStartingRespawnTime;
		var maximumRespawnDelay = CaptureThePancakeConfig.Config.PlayerMaxRespawnTime;
		var timeElapsedInSeconds = stopwatch.ElapsedMilliseconds / 1000.0; // Ensure floating point division
		var respawnScalingDuration = CaptureThePancakeConfig.Config.SecondsBeforeMatchScalingStops;

		// Calculate the scaling factor based on elapsed time and scaling duration
		var scalingFactor = Math.Min(timeElapsedInSeconds / respawnScalingDuration, 1.0); // Ensure it does not exceed 1

		// Calculate and clamp the respawn time within the initial and maximum limits
		var scaledRespawnTime = (int)(initialRespawnDelay + (maximumRespawnDelay - initialRespawnDelay) * scalingFactor);
		scaledRespawnTime = Math.Clamp(scaledRespawnTime, initialRespawnDelay, maximumRespawnDelay);

		return scaledRespawnTime;
	}

	public static new HashSet<string> GetAllowedCommands()
	{
		return BaseGameMode.AllowedCommands;
	}

	public void ReportStatsToPlayer(Player requestor)
	{
		// Assuming Teams is accessible and has team information
		var team1 = Teams[1];
		var team2 = Teams[2];

		// Gather player statistics similar to ReportStats method
		var playerStats = playerKills.Keys.Select(player => new
		{
			Player = player,
			Kills = playerKills[player],
			Deaths = playerDeaths.ContainsKey(player) ? playerDeaths[player] : 0,
			Damage = PlayerDamageDealt.ContainsKey(player) ? PlayerDamageDealt[player] : 0,
			DamageTaken = PlayerDamageReceived.ContainsKey(player) ? PlayerDamageReceived[player] : 0,
		})
		.OrderByDescending(player => player.Kills)
		.ToList();

		// Send team totals to the requestor
		SendTeamTotals(requestor, team1, team2);
		int matchMakingTeam = 1;
		if (requestor.IsInCaptureThePancake())
		{
			matchMakingTeam = requestor.MatchmakingTeam;
		}
		
		// Send individual player stats to the requestor
		foreach (var stat in playerStats)
		{
			bool isStatPlayerAlly = matchMakingTeam == stat.Player.MatchmakingTeam;			
			string colorizedPlayerName = isStatPlayerAlly ? stat.Player.Name.FriendlyTeam() : stat.Player.Name.EnemyTeam();
			string colorizedKills = isStatPlayerAlly ? stat.Kills.ToString().FriendlyTeam() : stat.Kills.ToString().EnemyTeam();
			string colorizedDeaths = isStatPlayerAlly ? stat.Deaths.ToString().EnemyTeam() : stat.Deaths.ToString().FriendlyTeam();
			string colorizedDamages = isStatPlayerAlly ? stat.Damage.ConvertToEngineeringNotation().FriendlyTeam() : stat.Damage.ConvertToEngineeringNotation().EnemyTeam();
			string colorizedDamagesTaken = isStatPlayerAlly ? stat.DamageTaken.ConvertToEngineeringNotation().EnemyTeam() : stat.DamageTaken.ConvertToEngineeringNotation().FriendlyTeam();

			requestor.ReceiveMessage($"{colorizedPlayerName} - K/D: {colorizedKills} / {colorizedDeaths} - DMG: {colorizedDamages} / {colorizedDamagesTaken}".White());
		}
	}

	public void ReportStats()
	{
		var team1 = Teams[1];
		var team2 = Teams[2];
		// Merge kills and deaths information
		var playerStats = playerKills.Keys.Select(player => new
		{
			Player = player,
			Kills = playerKills[player],
			Deaths = playerDeaths.ContainsKey(player) ? playerDeaths[player] : 0,
			Damage = PlayerDamageDealt.ContainsKey(player) ? PlayerDamageDealt[player] : 0,
			DamageTaken = PlayerDamageReceived.ContainsKey(player) ? PlayerDamageReceived[player] : 0,
		})
		.OrderByDescending(player => player.Kills)
		.ToList();
		
		// Send individual and team stats to each player
		foreach (var receiver in playerKills.Keys)
		{
			// Calculate and send team totals
			SendTeamTotals(receiver, team1, team2);

			receiver.ReceiveMessage("Player Detail:".Colorify(ExtendedColor.LightServerColor));
			// Send individual stats
			foreach (var stat in playerStats)
			{
				bool isStatPlayerAlly = receiver.MatchmakingTeam == stat.Player.MatchmakingTeam;
				string colorizedPlayerName = isStatPlayerAlly ? stat.Player.Name.FriendlyTeam() : stat.Player.Name.EnemyTeam();
				string colorizedKills = isStatPlayerAlly ? stat.Kills.ToString().FriendlyTeam() : stat.Kills.ToString().EnemyTeam();
				string colorizedDeaths = isStatPlayerAlly ? stat.Deaths.ToString().EnemyTeam() : stat.Deaths.ToString().FriendlyTeam();
				string colorizedDamages = isStatPlayerAlly ? stat.Damage.ConvertToEngineeringNotation().FriendlyTeam() : stat.Damage.ConvertToEngineeringNotation().EnemyTeam();
				string colorizedDamagesTaken = isStatPlayerAlly ? stat.DamageTaken.ConvertToEngineeringNotation().EnemyTeam() : stat.DamageTaken.ConvertToEngineeringNotation().FriendlyTeam();
				receiver.ReceiveMessage($"{colorizedPlayerName} - K/D: {colorizedKills} / {colorizedDeaths} - DMG: {colorizedDamages} / {colorizedDamagesTaken}".White());
			}
		}
	}

	private void SendTeamTotals(Player receiver, List<Player> team1, List<Player> team2)
	{
		var team1Kills = team1.Sum(player => playerKills.ContainsKey(player) ? playerKills[player] : 0);
		var team2Kills = team2.Sum(player => playerKills.ContainsKey(player) ? playerKills[player] : 0);
		var team1Deaths = team1.Sum(player => playerDeaths.ContainsKey(player) ? playerDeaths[player] : 0);
		var team2Deaths = team2.Sum(player => playerDeaths.ContainsKey(player) ? playerDeaths[player] : 0);
		var team1Damages = team1.Sum(player => PlayerDamageDealt.ContainsKey(player) ? PlayerDamageDealt[player] : 0);
		var team2Damages = team2.Sum(player => PlayerDamageDealt.ContainsKey(player) ? PlayerDamageDealt[player] : 0);

		string team1NameColorized;
		string team2NameColorized;
		string team1KillsColorized;
		string team2KillsColorized;
		string team1DeathsColorized;
		string team2DeathsColorized;
		string team1DamagesColorized;
		string team2DamagesColorized;

		var matchmakingTeam = 1;
		if (receiver.IsInCaptureThePancake())
		{
			matchmakingTeam = receiver.MatchmakingTeam;
		}
		if (matchmakingTeam == 1)
		{
			team1NameColorized = "Team 1".FriendlyTeam();
			team2NameColorized = "Team 2".EnemyTeam();
			
			team1KillsColorized = team1Kills.ToString().FriendlyTeam();
			team2KillsColorized = team2Kills.ToString().EnemyTeam();
			
			team1DeathsColorized = team1Deaths.ToString().EnemyTeam();
			team2DeathsColorized = team2Deaths.ToString().FriendlyTeam();
			
			team1DamagesColorized = team1Damages.ConvertToEngineeringNotation().FriendlyTeam();
			team2DamagesColorized = team2Damages.ConvertToEngineeringNotation().EnemyTeam();
		}
		else
		{
			team1NameColorized = "Team 1".EnemyTeam();
			team2NameColorized = "Team 2".FriendlyTeam();
			
			team1KillsColorized = team1Kills.ToString().EnemyTeam();
			team2KillsColorized = team2Kills.ToString().FriendlyTeam();
			
			team1DeathsColorized = team1Deaths.ToString().FriendlyTeam();
			team2DeathsColorized = team2Deaths.ToString().EnemyTeam();
			
			team1DamagesColorized = team1Damages.ConvertToEngineeringNotation().EnemyTeam();
			team2DamagesColorized = team2Damages.ConvertToEngineeringNotation().FriendlyTeam();
		}

		var elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000;
		var minutes = elapsedSeconds / 60;
		var seconds = elapsedSeconds % 60;

		receiver.ReceiveMessage($"Match Time: {($"{minutes}m {seconds}s").White()}".Colorify(ExtendedColor.LightServerColor));
		receiver.ReceiveMessage($"Team Recap:".Colorify(ExtendedColor.LightServerColor));
		receiver.ReceiveMessage($"{team1NameColorized} - K/D: {team1KillsColorized} / {team1DeathsColorized} - DMG: {team1DamagesColorized}".White());
		receiver.ReceiveMessage($"{team2NameColorized} - K/D: {team2KillsColorized} / {team2DeathsColorized} - DMG: {team2DamagesColorized}".White());
	}
}


