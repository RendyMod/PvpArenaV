using System;
using System.Collections.Generic;
using ProjectM;
using PvpArena.Models;
using Unity.Entities;
using PvpArena.Data;
using Unity.Mathematics;
using Bloodstone.API;
using PvpArena.Helpers;
using System.Threading;
using PvpArena.Services;
using static PvpArena.Factories.UnitFactory;
using ProjectM.CastleBuilding;
using System.Linq;
using PvpArena.Factories;
using System.Diagnostics;
using Unity.Transforms;
using static DamageRecorderService;
using ProjectM.Pathfinding;
using ProjectM.Tiles;

namespace PvpArena.GameModes.Moba;

public class MobaGameMode : BaseGameMode
{
    public override Player.PlayerState GameModeType => Player.PlayerState.Moba;
	public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
	{
		RemoveConsumables = false,
		RemoveShapeshifts = false,
		ResetCooldowns = false
	};
	private static bool MatchActive = false;
	private static Stopwatch stopwatch = new Stopwatch();

	public static Dictionary<int, List<Player>> Teams = new Dictionary<int, List<Player>>();
	public static Dictionary<int, List<PrefabGUID>> TeamToShardBuffsMap = new Dictionary<int, List<PrefabGUID>>
	{
		{1, new List<PrefabGUID>() },
		{2, new List<PrefabGUID>() }
	};

	public static Dictionary<int, HashSet<Entity>> TeamPatrols = new Dictionary<int, HashSet<Entity>>
	{
		{1, new HashSet<Entity>() },
		{2, new HashSet<Entity>() },
	};

	public static Dictionary<int, HashSet<Entity>> TeamUnits = new Dictionary<int, HashSet<Entity>>
	{
		{1, new HashSet<Entity>() },
		{2, new HashSet<Entity>() },
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

	private static Dictionary<PrefabGUID, bool> damageableStructurePrefabs = new Dictionary<PrefabGUID, bool>
	{
		{ Prefabs.TM_Castle_Wall_Tier01_Wood, true },
		{ Prefabs.TM_Castle_Wall_Door_Palisade_Tier01, true}
	};

	private static Dictionary<PrefabGUID, Action<Player, Entity>> playerBuffHandlers = new Dictionary<PrefabGUID, Action<Player, Entity>>
	{
		{ Prefabs.AB_Feed_02_Bite_Abort_Trigger, HandleBiteAbortBuff },
		{ Prefabs.AB_Interact_HealingOrb_Buff, HandleHealingOrbBuff },
		{ Prefabs.AB_Shapeshift_BloodMend_Buff, HandleBloodMendBuff },
		{ Prefabs.HideCharacterBuff, HandleHideCharacterBuff },
		{ Prefabs.AB_Shapeshift_Golem_T02_Buff, HandleGolemBuff },
	};

	public static new HashSet<string> AllowedCommands = new HashSet<string>
	{
		"ping",
		"help",
		"legendary",
		"jewel",
		"forfeit",
		"points",
		"lb ranked",
		"bp",
	};

	public static List<Timer> Timers = new List<Timer>();
    public static Dictionary<Player, List<Timer>> PlayerRespawnTimers = new Dictionary<Player, List<Timer>>();
	public static Dictionary<Player, float> PlayerDamageDealt = new Dictionary<Player, float>();
	public static Dictionary<Player, float> PlayerDamageReceived = new Dictionary<Player, float>();

	public static HashSet<PrefabGUID> TrackingProjectiles = new HashSet<PrefabGUID>
	{
		Prefabs.AB_Gloomrot_SpiderTank_Gattler_Minigun_Projectile01,
		Prefabs.AB_Gloomrot_SpiderTank_Gattler_Minigun_Projectile02,
		Prefabs.AB_Gloomrot_SentryTurret_RangedAttack_Projectile
	};

    public static List<Entity> SpawnGates = new List<Entity>();

	public static Dictionary<PrefabGUID, List<PrefabGUID>> AbilitiesToNotCauseBuffDestruction = new Dictionary<PrefabGUID, List<PrefabGUID>>
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
		GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
        GameEvents.OnPlayerBuffRemoved += HandleOnPlayerBuffRemoved;
		GameEvents.OnPlayerInvitedToClan += HandleOnPlayerInvitedToClan;
		GameEvents.OnPlayerKickedFromClan += HandleOnPlayerKickedFromClan;
		GameEvents.OnPlayerLeftClan += HandleOnPlayerLeftClan;
		GameEvents.OnUnitBuffed += HandleOnUnitBuffed;
		GameEvents.OnUnitBuffRemoved += HandleOnUnitBuffRemoved;
		GameEvents.OnUnitDeath += HandleOnUnitDeath;
		GameEvents.OnUnitDamageDealt += HandleOnUnitDamageDealt;
		GameEvents.OnGameFrameUpdate += HandleOnGameFrameUpdate;
        GameEvents.OnDelayedSpawn += HandleOnDelayedSpawnEvent;
        GameEvents.OnPlayerStartedCasting += HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerDamageReported += HandleOnPlayerDamageReported;
		GameEvents.OnUnitProjectileCreated += HandleOnUnitProjectileCreated;
		GameEvents.OnUnitProjectileUpdate += HandleOnUnitProjectileUpdate;
		GameEvents.OnAggroPostUpdate += HandleOnAggroPostUpdate;
		GameEvents.OnItemWasDropped += HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;

		stopwatch.Start();
	}

	public void Initialize(List<Player> team1Players, List<Player> team2Players)
	{
		Initialize();
		Teams[1] = team1Players;
		Teams[2] = team2Players;
		TeamPatrols[1] = new HashSet<Entity>();
		TeamPatrols[2] = new HashSet<Entity>();
		TeamUnits[1] = new HashSet<Entity>();
		TeamUnits[2] = new HashSet<Entity>();
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
			}
		}
	}
	public override void Dispose()
	{
		MatchActive = false;
		GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
		GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
		GameEvents.OnUnitBuffRemoved -= HandleOnUnitBuffRemoved;
		GameEvents.OnPlayerInvitedToClan -= HandleOnPlayerInvitedToClan;
		GameEvents.OnPlayerKickedFromClan -= HandleOnPlayerKickedFromClan;
		GameEvents.OnPlayerLeftClan -= HandleOnPlayerLeftClan;
		GameEvents.OnUnitBuffed -= HandleOnUnitBuffed;
		GameEvents.OnUnitDeath -= HandleOnUnitDeath;
		GameEvents.OnGameFrameUpdate -= HandleOnGameFrameUpdate;
        GameEvents.OnDelayedSpawn -= HandleOnDelayedSpawnEvent;
        GameEvents.OnPlayerStartedCasting -= HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerDamageReported -= HandleOnPlayerDamageReported;
		GameEvents.OnUnitProjectileCreated -= HandleOnUnitProjectileCreated;
		GameEvents.OnUnitProjectileUpdate -= HandleOnUnitProjectileUpdate;
		GameEvents.OnAggroPostUpdate -= HandleOnAggroPostUpdate;
		GameEvents.OnUnitDamageDealt -= HandleOnUnitDamageDealt;
		GameEvents.OnItemWasDropped -= HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;

		Teams.Clear();
		TeamPatrols.Clear();
		TeamUnits.Clear();
		
		foreach (var shardBuffs in TeamToShardBuffsMap.Values)
		{
			shardBuffs.Clear();
		}
        foreach (var kvp in UnitFactory.UnitToEntity)
        {
            if (kvp.Key.Category == "moba")
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

		playerKills.Clear();
		playerDeaths.Clear();
		PlayerDamageDealt.Clear();
		PlayerDamageReceived.Clear();
		stopwatch.Reset();
	}

	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (player.CurrentState != this.GameModeType) return;


		var totalCoins = InventoryUtilities.GetItemAmount(VWorld.Server.EntityManager, player.Character, Prefabs.Item_Ingredient_Coin_Copper);
		if (totalCoins > 0)
		{
			InventoryUtilitiesServer.RemoveItemGetRemainder(VWorld.Server.EntityManager, player.Character, Prefabs.Item_Ingredient_Coin_Copper, MobaConfig.Config.CoinsLostPerDeath, out int remainder);
			var newCoins = totalCoins - MobaConfig.Config.CoinsLostPerDeath;
			if (newCoins < 0) newCoins = 0;
			player.ReceiveMessage($"You have died and have lost {(totalCoins - newCoins).ToString().EnemyTeam()} coin(s)".White());
		}
		

		if (killer.Exists())
		{
			Player killerPlayer = null;
			if (killer.Has<PlayerCharacter>())
			{
				killerPlayer = PlayerService.GetPlayerFromCharacter(killer);

				if (killer != player.Character)
				{
					if (Helper.AddItemToInventory(killerPlayer.Character, Prefabs.Item_Ingredient_Coin_Copper, 100, out var item))
					{
						killerPlayer.ReceiveMessage($"You gained 100 coins for killing {player.Name.EnemyTeam()}".White());
					}

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

		


		float3 pos = default;
		if (player.MatchmakingTeam == 1)
		{
			pos = MobaConfig.Config.Team1PlayerRespawn.ToFloat3();
		}
		else if (player.MatchmakingTeam == 2)
		{
			pos = MobaConfig.Config.Team2PlayerRespawn.ToFloat3();
		}

		var respawnDelay = CalculateRespawnDelay();



		Action respawnPlayerActionPart2 = () =>
		{
			player.Reset(ResetOptions);
			Helper.RemoveBuff(player, Prefabs.AB_Shapeshift_Mist_Buff);
			if (Helper.BuffPlayer(player, Prefabs.Buff_General_Phasing, out var buffEntity, 3))
			{
				Helper.ApplyStatModifier(buffEntity, BuffModifiers.FastRespawnMoveSpeed);
				Helper.RemoveBuffModifications(buffEntity, BuffModificationTypes.Immaterial);
				buffEntity.Add<DestroyBuffOnDamageTaken>();
			}
		};

		Action respawnPlayerActionPart1 = () =>
		{
			player.Teleport(pos);
			Helper.RemoveBuff(player, Prefabs.AB_Scarecrow_Idle_Buff);
			if (Helper.BuffPlayer(player, Helper.CustomBuff2, out var buffEntity, 3))
			{
				Helper.ModifyBuff(buffEntity, BuffModificationTypes.MovementImpair | BuffModificationTypes.Immaterial);
			}
			var timer = ActionScheduler.RunActionOnceAfterDelay(respawnPlayerActionPart2, 3);
			PlayerRespawnTimers[player].Add(timer);
		};

		player.Reset(ResetOptions);
		Helper.MakeGhostlySpectator(player, respawnDelay);
		ActionScheduler.RunActionOnceAfterDelay(respawnPlayerActionPart1, respawnDelay - 3);
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
	
	public void HandleOnGameModeBegin(Player player)
	{
		if (player.CurrentState != this.GameModeType) return;
	}
	public void HandleOnGameModeEnd(Player player)
	{
		if (player.CurrentState != this.GameModeType) return;
	}
	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.GameModeType) return;

		var enterShapeshiftEvent = eventEntity.Read<ProjectM.Network.EnterShapeshiftEvent>();
		if (!shapeshiftToShapeshift.ContainsKey(enterShapeshiftEvent.Shapeshift))
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
			player.ReceiveMessage($"That shapeshift is disabled while in this game mode.".Error());
		}
		else
		{
			enterShapeshiftEvent.Shapeshift = shapeshiftToShapeshift[enterShapeshiftEvent.Shapeshift];
			eventEntity.Write(enterShapeshiftEvent);
		}
	}

	public static void HandleBiteAbortBuff(Player player, Entity buffEntity)
	{
		Action action = () =>
		{
			Helper.RemoveBuff(player.Character, Prefabs.AB_FeedEnemyVampire_01_Initiate_DashChannel);
		};
		ActionScheduler.RunActionOnceAfterDelay(action, .1);
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

	public static void HandleShapeshiftBuff(Player player, Entity buffEntity)
	{
		buffEntity.Add<DestroyBuffOnDamageTaken>();
		buffEntity.Add<DestroyOnAbilityCast>();
		var buffer = buffEntity.ReadBuffer<ReplaceAbilityOnSlotBuff>();
		buffer.Clear();
		Helper.ApplyStatModifier(buffEntity, BuffModifiers.PancakeShapeshiftSpeed);
	}

	public  static void HandleBloodMendBuff(Player player, Entity buffEntity)
	{
		var buffer = buffEntity.ReadBuffer<ChangeBloodOnGameplayEvent>();
		for (var i = 0; i < buffer.Length; i++)
		{
			var changeBloodOnGameplayEvent = buffer[i];
			changeBloodOnGameplayEvent.BloodValue = 10;
			buffer[i] = changeBloodOnGameplayEvent;
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

	public static void HandleGolemBuff(Player player, Entity buffEntity)
	{
		var absorb = buffEntity.Read<AbsorbBuff>();
		absorb.AbsorbValue = 5000;
		var lifetime = buffEntity.Read<LifeTime>();
		lifetime.Duration = 60;
		lifetime.EndAction = LifeTimeEndAction.Destroy;
		buffEntity.Write(absorb);
		buffEntity.Write(lifetime);

		AbilityBar abilityBar = new AbilityBar
		{
			Weapon2 = PrefabGUID.Empty
		};
		abilityBar.ApplyChangesSoft(buffEntity);
	}

	public void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (player.CurrentState != this.GameModeType) return;
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
		if (playerBuffHandlers.TryGetValue(prefabGuid, out var handler))
		{
			handler(targetPlayer, buffEntity);
		}
		else if (shapeshiftGroupToBuff.ContainsValue(prefabGuid))
		{
			HandleShapeshiftBuff(targetPlayer, buffEntity);
		}
	}

    public static void HandleOnPlayerBuffRemoved(Player player, Entity buffEntity)
    {
        var prefabGuid = buffEntity.Read<PrefabGUID>();
    }

	public void HandleOnUnitBuffRemoved(Entity unit, Entity buffEntity)
	{
		if (TryGetSpawnedUnitFromEntity(unit, out SpawnedUnit spawnedUnit))
		{
			if (spawnedUnit.Unit.Category != "moba")
			{
				return;
			}
		}
		else
		{
			return;
		}

		var prefabGuid = buffEntity.Read<PrefabGUID>();
		if (prefabGuid == Prefabs.AB_Gloomrot_SentryTurret_BunkerDown_Buff)
		{
			var destroyState = buffEntity.Read<DestroyState>();
			buffEntity.Remove<DestroyTag>();
			destroyState.Value = DestroyStateEnum.NotDestroyed;
			buffEntity.Write(destroyState);
		}
		else if (prefabGuid == Prefabs.Buff_General_Chill)
		{
			var destroyState = buffEntity.Read<DestroyState>();
			buffEntity.Remove<DestroyTag>();
			destroyState.Value = DestroyStateEnum.NotDestroyed;
			buffEntity.Write(destroyState);
		}
	}

	public void HandleOnUnitBuffed(Entity unit, Entity buffEntity)
	{
		if (TryGetSpawnedUnitFromEntity(unit, out SpawnedUnit spawnedUnit))
		{
			if (spawnedUnit.Unit.Category != "moba")
			{
				return;
			}
		}
		else
		{
			return;
		}

		var buffPrefabGUID = buffEntity.Read<PrefabGUID>();
		if (buffPrefabGUID == Helper.CustomBuff3)
		{
			if (unit.Read<PrefabGUID>() == BaseTurret.PrefabGUID)
			{
				Helper.ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
				{
					Id = ModificationIdFactory.NewId(),
					ModificationType = ModificationType.Set,
					StatType = UnitStatType.AttackSpeed,
					Value = 5,
					Priority = 100
				}, false);

				Helper.ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
				{
					Id = ModificationIdFactory.NewId(),
					ModificationType = ModificationType.Set,
					StatType = UnitStatType.CooldownModifier,
					Value = 0,
					Priority = 100
				}, false);

				Helper.ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
				{
					Id = ModificationIdFactory.NewId(),
					ModificationType = ModificationType.Set,
					StatType = UnitStatType.PhysicalPower,
					Value = 125,
					Priority = 100
				}, false);
			}
			else
			{
				Helper.ApplyStatModifier(buffEntity, new ModifyUnitStatBuff_DOTS
				{
					Id = ModificationIdFactory.NewId(),
					ModificationType = ModificationType.Set,
					StatType = UnitStatType.MovementSpeed,
					Value = 3.5f,
					Priority = 100
				}, false);
			}
		}
	}

	public override void HandleOnPlayerConnected(Player player)
	{
		if (player.CurrentState != this.GameModeType) return;


		if (player.MatchmakingTeam == 1)
		{
			Helper.RespawnPlayer(player, MobaConfig.Config.Team1PlayerRespawn.ToFloat3());
		}
		else if (player.MatchmakingTeam == 2)
		{
			Helper.RespawnPlayer(player, MobaConfig.Config.Team2PlayerRespawn.ToFloat3());
		}
	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (player.CurrentState != this.GameModeType) return;

		Helper.DestroyEntity(player.Character);
		base.HandleOnPlayerDisconnected(player);
	}

	public void HandleOnPlayerInvitedToClan(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.GameModeType) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not invite players to your clan while in this game mode".Error());
	}

	public void HandleOnPlayerKickedFromClan(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.GameModeType) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not kick players from your clan while in this game mode".Error());
	}

	public void HandleOnPlayerLeftClan(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.GameModeType) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not leave your clan while in this game mode".Error());
	}

	public void HandleOnUnitDeath(Entity unitEntity, DeathEvent deathEvent)
	{
		if (!MatchActive) return;

		if (TeamPatrols[1].Contains(unitEntity))
		{
			TeamPatrols[1].Remove(unitEntity);
			TeamUnits[1].Remove(unitEntity);
		}
		else if (TeamPatrols[2].Contains(unitEntity))
		{
			TeamPatrols[2].Remove(unitEntity);
			TeamUnits[2].Remove(unitEntity);
		}
		if (TryGetSpawnedUnitFromEntity(unitEntity, out var spawnedUnit))
		{
			if (!UnitFactory.HasCategory(unitEntity, "moba")) return;

			var killer = deathEvent.Killer;
			if (killer.Exists() && killer.Has<PlayerCharacter>())
			{
				Helper.AddItemToInventory(killer, Prefabs.Item_Ingredient_Coin_Copper, MobaConfig.Config.CoinsGainedPerUnitKill, out var itemEntity);
			}
		}
	}



	public void HandleOnPlayerChatCommand(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.GameModeType) return;


	}

	public override void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
	{
		if (player.CurrentState != this.GameModeType) return;

		if (!eventEntity.Exists()) return;

		var dealDamageEvent = eventEntity.Read<DealDamageEvent>();
		var isStructure = dealDamageEvent.Target.Has<CastleHeartConnection>();
		if (isStructure)
		{
			eventEntity.Destroy();
			return;
		}

		if (player.HasBuff(Prefabs.AB_Interact_Siege_Structure_T02_PlayerBuff))
		{
			dealDamageEvent.MaterialModifiers.Mechanical *= 10;
			dealDamageEvent.MaterialModifiers.Human *= 8;
			dealDamageEvent.MaterialModifiers.PlayerVampire *= 2;
			eventEntity.Write(dealDamageEvent);
		}
	}

	public void HandleOnUnitDamageDealt(Entity unit, Entity eventEntity)
	{
		if (!UnitFactory.HasCategory(unit, "moba")) return;

		var dealDamageEvent = eventEntity.Read<DealDamageEvent>();
		var source = dealDamageEvent.SpellSource;
		if (source.Exists() && source.Read<PrefabGUID>() == Prefabs.AB_Gloomrot_SentryTurret_RangedAttack_Projectile)
		{
			source.Add<DestroyOnSpawn>();
			dealDamageEvent.MaterialModifiers.Human *= 2f;
			eventEntity.Write(dealDamageEvent);
			return;
		}
		if (!dealDamageEvent.Target.Has<PlayerCharacter>())
		{
			dealDamageEvent.MaterialModifiers.Human *= 1.5f;
			eventEntity.Write(dealDamageEvent);
		}
	}

	public void HandleOnUnitProjectileCreated(Entity unit, Entity projectile)
	{
		if (!UnitFactory.HasCategory(unit, "moba")) return;

		var prefabGuid = projectile.Read<PrefabGUID>();
		if (TrackingProjectiles.Contains(prefabGuid))
		{
			var projectileTarget = unit.Read<EntityInput>().HoveredEntity;
			if (projectileTarget.Exists())
			{
				if (projectileTarget.Has<PlayerCharacter>())
				{
					projectile.Add<SpellTarget>();
					projectile.Write(new SpellTarget
					{
						Target = projectileTarget
					});
				}
			}
		}
	}

	public void HandleOnUnitProjectileUpdate(Entity unit, Entity projectile)
	{
		if (!UnitFactory.HasCategory(unit, "moba")) return;

		if (projectile.Has<SpellTarget>())
		{
			var projectileTarget = projectile.Read<SpellTarget>().Target;
			
			if (projectileTarget.Exists())
			{
				if (projectileTarget.Has<PlayerCharacter>())
				{		
					var targetPlayer = PlayerService.GetPlayerFromCharacter(projectileTarget);
					if (targetPlayer.IsAlive)
					{
						var spellMovement = projectile.Read<SpellMovement>();
						spellMovement.TargetPosition = targetPlayer.Position;
						projectile.Write(spellMovement);
					}
				}
			}
		}
	}

    public void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
    {
        if (player.CurrentState != this.GameModeType) return;


        var abilityCastStartedEvent = eventEntity.Read<AbilityCastStartedEvent>();
        if (!abilityCastStartedEvent.AbilityGroup.Exists()) return;

        var abilityGuid = abilityCastStartedEvent.AbilityGroup.Read<PrefabGUID>();

        //prevent shapeshift spells from breaking players out of their shapeshift (due to the abnormal way we gave them shapeshift)
        if (AbilitiesToNotCauseBuffDestruction.TryGetValue(abilityGuid, out var buffs))
        {
			PreventBuffDestructionIfBuffPresent(abilityCastStartedEvent, buffs);
		}
	}

	private static void PreventBuffDestructionIfBuffPresent(AbilityCastStartedEvent abilityCastStartedEvent, List<PrefabGUID> buffs)
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
        if (unit.Category != "moba") return;

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
		if (!target.Has<PlayerCharacter>()) return;

		var targetPlayer = PlayerService.GetPlayerFromCharacter(target);
		if (source.CurrentState != GameModeType || targetPlayer.CurrentState != GameModeType) return;

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

	public static int count = 0;

	private float3 GetLaneCenter(float3 unitPosition)
	{
		float3 targetPosition = unitPosition.xyz;
		targetPosition.z = MobaConfig.Config.Team1PlayerRespawn.Z;

		return targetPosition;
	}

	private bool IsAtLaneCenter(float3 unitPosition)
	{
		float threshold = 2.0f;
		return math.distance(unitPosition, GetLaneCenter(unitPosition)) <= threshold;
	}

	public void HandleOnGameFrameUpdate()
	{
		foreach (var patrol in TeamPatrols)
		{
			foreach (var unit in patrol.Value)
			{
				if (unit.Exists())
				{
					TileCoordinate targetCoordinate;
					float3 unitPosition = unit.Read<LocalToWorld>().Position;

					if (Helper.HasBuff(unit, Prefabs.Buff_InCombat_Npc) || Helper.HasBuff(unit, Prefabs.Buff_Shared_Return_NoInvulernable))
					{
						targetCoordinate = TileCoordinate.FromWorldPos(unit.Read<LocalToWorld>().Position);
					}
					else
					{
						// Check if the unit is at the center of the lane
						if (!IsAtLaneCenter(unitPosition))
						{
							// If not, set the target to the lane center
							targetCoordinate = TileCoordinate.FromWorldPos(GetLaneCenter(unitPosition));
						}
						else
						{
							targetCoordinate = TileCoordinate.FromWorldPos(MobaConfig.Config.Team2PlayerRespawn.ToFloat3());
							if (patrol.Key == 2)
							{
								targetCoordinate = TileCoordinate.FromWorldPos(MobaConfig.Config.Team1PlayerRespawn.ToFloat3());
							}
						}
					}

					// Set the path for the unit
					var buffer = unit.ReadBuffer<PathBuffer>();
					var pathBuffer = new PathBuffer { Value = targetCoordinate };

					buffer.Clear();
					buffer.Add(pathBuffer);
				}
			}
		}
	}

	public void HandleOnAggroPostUpdate(Entity entity)
	{
		if (!(MobaGameMode.TeamUnits[1].Contains(entity) || MobaGameMode.TeamUnits[2].Contains(entity))) return;

		var aggroConsumer = entity.Read<AggroConsumer>();
		var aggroBuffer = entity.ReadBuffer<AggroBuffer>();
		if (MobaGameMode.TeamPatrols[1].Contains(entity) || MobaGameMode.TeamPatrols[2].Contains(entity))
		{
			aggroConsumer.MaxDistanceFromPreCombatPosition = 10;
		}

		var target = aggroConsumer.AggroTarget._Entity;
		var aggroerPrefabGuid = entity.Read<PrefabGUID>();
		
		if (target.Exists() && target.Has<PlayerCharacter>())
		{
			for (var i = 0; i < aggroBuffer.Length; i++)
			{
				var aggroEntity = aggroBuffer[i].Entity;
				if (aggroBuffer[i].Entity.Exists() && !aggroEntity.Has<PlayerCharacter>())
				{
					if (aggroerPrefabGuid == BaseTurret.PrefabGUID)
					{
						if (math.distance(aggroEntity.Read<LocalToWorld>().Position, entity.Read<LocalToWorld>().Position) > 18)
						{
							continue;
						}
					}
					
					aggroConsumer.AlertTarget = NetworkedEntity.ServerEntity(aggroBuffer[i].Entity);
					aggroConsumer.AggroTarget = NetworkedEntity.ServerEntity(aggroBuffer[i].Entity);
					break;
				}
			}
		}
		
		entity.Write(aggroConsumer);
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

	private static int CalculateRespawnDelay()
	{
		var initialRespawnDelay = MobaConfig.Config.PlayerStartingRespawnTime;
		var maximumRespawnDelay = MobaConfig.Config.PlayerMaxRespawnTime;
		var timeElapsedInSeconds = stopwatch.ElapsedMilliseconds / 1000.0; // Ensure floating point division
		var respawnScalingDuration = MobaConfig.Config.SecondsBeforeMatchScalingStops;

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

	public static void ReportStats()
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

	private static void SendTeamTotals(Player receiver, List<Player> team1, List<Player> team2)
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

		if (receiver.MatchmakingTeam == 1)
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

		receiver.ReceiveMessage("Team Recap:".Colorify(ExtendedColor.LightServerColor));
		receiver.ReceiveMessage($"{team1NameColorized} - K/D: {team1KillsColorized} / {team1DeathsColorized} - DMG: {team1DamagesColorized}".White());
		receiver.ReceiveMessage($"{team2NameColorized} - K/D: {team2KillsColorized} / {team2DeathsColorized} - DMG: {team2DamagesColorized}".White());
	}
}


