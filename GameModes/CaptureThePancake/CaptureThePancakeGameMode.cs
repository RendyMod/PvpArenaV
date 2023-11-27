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

namespace PvpArena.GameModes.CaptureThePancake;

public class CaptureThePancakeGameMode : BaseGameMode
{
	private static bool MatchActive = false;
	private static Stopwatch stopwatch = new Stopwatch();

	public static Dictionary<int, List<Player>> Teams = new Dictionary<int, List<Player>>();
	public static Dictionary<int, List<PrefabGUID>> TeamToShardBuffsMap = new Dictionary<int, List<PrefabGUID>>
	{
		{1, new List<PrefabGUID>() },
		{2, new List<PrefabGUID>() }
	};

	private static Dictionary<Player, bool> shouldRemoveGallopBuff = new Dictionary<Player, bool>();

	private static Dictionary<Player, int> playerKills = new Dictionary<Player, int>();
	private static Dictionary<Player, int> playerDeaths = new Dictionary<Player, int>();

	private static RectangleZone endZone1 = CaptureThePancakeConfig.Config.Team1EndZone.ToRectangleZone();
	private static RectangleZone endZone2 = CaptureThePancakeConfig.Config.Team2EndZone.ToRectangleZone();

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

	public static Dictionary<string, bool> AllowedCommands = new Dictionary<string, bool>
	{
		{ "ping", true },
		{ "help", true },
		{ "legendary", true },
		{ "jewel", true },
		{ "forfeit", true },
		{ "points", true },
		{ "ranked lb", true },
		{ "bp", true },
	};

	public static List<Timer> Timers = new List<Timer>();
    public static Dictionary<Player, List<Timer>> PlayerRespawnTimers = new Dictionary<Player, List<Timer>>();

	public static Entity WingedHorrorGate;
	public static Entity MonsterGate;
    public static List<Entity> SpawnGates = new List<Entity>();

    public static Dictionary<PrefabGUID, PrefabGUID> AbilitiesToNotCauseBuffDestruction = new Dictionary<PrefabGUID, PrefabGUID>
    {
        { Prefabs.AB_Interact_Pickup_AbilityGroup, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff},
        { Prefabs.AB_Interact_OpenContainer_AbilityGroup, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff},
        { Prefabs.AB_Interact_HealingOrb_AbilityGroup, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff},
        { Prefabs.AB_Interact_Mount_Owner_Buff_Horse, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff},
        { Prefabs.AB_Interact_OpenDoor_AbilityGroup, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff},
        { Prefabs.AB_Interact_OpenGate_AbilityGroup, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff},
    };

    public override void Initialize()
	{
		MatchActive = true;
		GameEvents.OnPlayerRespawn += HandleOnPlayerRespawn;
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
		GameEvents.OnPlayerUsedConsumable += HandleOnConsumableUse;
		GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
        GameEvents.OnPlayerBuffRemoved += HandleOnPlayerBuffRemoved;
        GameEvents.OnPlayerWillLoseGallopBuff += HandleOnPlayerWillLoseGallopBuff;
		GameEvents.OnPlayerMounted += HandleOnPlayerMounted;
		GameEvents.OnPlayerDismounted += HandleOnPlayerDismounted;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnPlayerInvitedToClan += HandleOnPlayerInvitedToClan;
		GameEvents.OnPlayerKickedFromClan += HandleOnPlayerKickedFromClan;
		GameEvents.OnPlayerLeftClan += HandleOnPlayerLeftClan;
		GameEvents.OnUnitBuffed += HandleOnUnitBuffed;
		GameEvents.OnUnitDeath += HandleOnUnitDeath;
		GameEvents.OnItemWasThrown += HandleOnItemWasThrown;
		GameEvents.OnGameFrameUpdate += HandleOnGameFrameUpdate;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDamageReceived += HandleOnPlayerDamageReceived;
        GameEvents.OnDelayedSpawn += HandleOnDelayedSpawnEvent;
        GameEvents.OnPlayerStartedCasting += HandleOnPlayerStartedCasting;

        stopwatch.Start();
	}

	public void Initialize(List<Player> team1Players, List<Player> team2Players)
	{
		Initialize();
		Teams[1] = team1Players;
		Teams[2] = team2Players;
		playerKills.Clear();
		playerDeaths.Clear();

		foreach (var team in Teams.Values)
		{
			foreach (var player in team)
			{
				playerKills[player] = 0;
				playerDeaths[player] = 0;
                PlayerRespawnTimers[player] = new List<Timer>();
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
        GameEvents.OnPlayerBuffRemoved -= HandleOnPlayerBuffRemoved;
        GameEvents.OnPlayerWillLoseGallopBuff -= HandleOnPlayerWillLoseGallopBuff;
		GameEvents.OnPlayerMounted -= HandleOnPlayerMounted;
		GameEvents.OnPlayerDismounted -= HandleOnPlayerDismounted;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnPlayerInvitedToClan -= HandleOnPlayerInvitedToClan;
		GameEvents.OnPlayerKickedFromClan -= HandleOnPlayerKickedFromClan;
		GameEvents.OnPlayerLeftClan -= HandleOnPlayerLeftClan;
		GameEvents.OnUnitBuffed -= HandleOnUnitBuffed;
		GameEvents.OnUnitDeath -= HandleOnUnitDeath;
		GameEvents.OnItemWasThrown -= HandleOnItemWasThrown;
		GameEvents.OnGameFrameUpdate -= HandleOnGameFrameUpdate;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDamageReceived -= HandleOnPlayerDamageReceived;
        GameEvents.OnDelayedSpawn -= HandleOnDelayedSpawnEvent;
        GameEvents.OnPlayerStartedCasting -= HandleOnPlayerStartedCasting;

        Teams.Clear();
		foreach (var shardBuffs in TeamToShardBuffsMap.Values)
		{
			shardBuffs.Clear();
		}
        foreach (var kvp in UnitFactory.UnitToEntity)
        {
            if (kvp.Key.Category == "pancake")
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
		stopwatch.Reset();
		WingedHorrorGate = Entity.Null;
		MonsterGate = Entity.Null;
	}
	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (!player.IsInCaptureThePancake()) return;

		if (killer.Index > 0)
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
	public override void HandleOnPlayerDeath(Player player, OnKillCallResult killCallResult)
	{
		if (!player.IsInCaptureThePancake()) return;

        foreach (var respawnAction in PlayerRespawnTimers[player])
        {
            if (respawnAction != null)
            {
                respawnAction.Dispose();
            }
        }
        PlayerRespawnTimers[player].Clear();

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
			pos = CaptureThePancakeConfig.Config.Team1PlayerRespawn.ToFloat3();
		}
		else if (player.MatchmakingTeam == 2)
		{
			pos = CaptureThePancakeConfig.Config.Team2PlayerRespawn.ToFloat3();
		}
		if (player.HasBuff(Prefabs.Buff_General_RelicCarryDebuff))
		{
			var hasManticoreShard = Helper.PlayerHasItemInInventories(player, Prefabs.Item_Building_Relic_Manticore);
			var hasMonsterShard = Helper.PlayerHasItemInInventories(player, Prefabs.Item_Building_Relic_Monster);
			if (hasManticoreShard && hasMonsterShard)
			{
				CaptureThePancakeHelper.DropItemsIntoBag(player, new List<PrefabGUID> { Prefabs.Item_Building_Relic_Monster, Prefabs.Item_Building_Relic_Manticore });
                Helper.RemoveItemFromInventory(player, Prefabs.Item_Building_Relic_Monster);
                Helper.RemoveItemFromInventory(player, Prefabs.Item_Building_Relic_Manticore);
            }
			else if (hasManticoreShard)
			{
				CaptureThePancakeHelper.DropItemsIntoBag(player, new List<PrefabGUID> { Prefabs.Item_Building_Relic_Manticore });
                Helper.RemoveItemFromInventory(player, Prefabs.Item_Building_Relic_Manticore);
            }
			else if (hasMonsterShard)
			{
				CaptureThePancakeHelper.DropItemsIntoBag(player, new List<PrefabGUID> { Prefabs.Item_Building_Relic_Monster });
                Helper.RemoveItemFromInventory(player, Prefabs.Item_Building_Relic_Monster);
            }
		}

        Action respawnPlayerActionPart2 = () =>
        {
            ResetPlayer(player);
            Helper.RemoveBuff(player, Prefabs.AB_Shapeshift_Mist_Buff);
            Helper.BuffPlayer(player, Prefabs.Buff_General_Phasing, out var buffEntity, 3);
            Helper.ApplyStatModifier(buffEntity, BuffModifiers.FastRespawnMoveSpeed);
            Helper.RemoveBuffModifications(buffEntity, BuffModificationTypes.Immaterial);
            buffEntity.Add<DestroyBuffOnDamageTaken>();
        };

        Action respawnPlayerActionPart1 = () =>
        {
            player.Teleport(pos);
            Helper.BuffPlayer(player, Helper.CustomBuff2, out var buffEntity, 3);
            Helper.ModifyBuff(buffEntity, BuffModificationTypes.MovementImpair | BuffModificationTypes.Immaterial);
            var timer = ActionScheduler.RunActionOnceAfterDelay(respawnPlayerActionPart2, 3);
            PlayerRespawnTimers[player].Add(timer);
            //BuffHelper.RemoveBuff(player, Prefabs.AB_Shapeshift_Mist_Buff);
            /*if (BuffHelper.BuffPlayer(player, Prefabs.Witch_SheepTransformation_Buff, out var buffEntity, 4, true))
            {
                BuffHelper.ModifyBuff(buffEntity, BuffModificationTypes.MovementImpair | BuffModificationTypes.Immaterial);
            }*/
        };

        

        Action makeSpectatorAction = () =>
		{
            ResetPlayer(player);
            Helper.BuffPlayer(player, Prefabs.AB_Shapeshift_Mist_Buff, out var buffEntity, CalculateRespawnDelay());
            Helper.CompletelyRemoveAbilityBarFromBuff(buffEntity);
            Helper.FixIconForShapeshiftBuff(player, buffEntity, Prefabs.AB_Shapeshift_Mist_Group);


            Helper.ModifyBuff(buffEntity, BuffModificationTypes.Invulnerable | BuffModificationTypes.Immaterial | BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.AbilityCastImpair | BuffModificationTypes.PickupItemImpaired | BuffModificationTypes.TargetSpellImpaired, true);
            Helper.BuffPlayer(player, Prefabs.Admin_Observe_Invisible_Buff, out var invisibleBuff, CalculateRespawnDelay());
            Helper.ModifyBuff(invisibleBuff, BuffModificationTypes.None, true);
			Helper.RespawnPlayer(player, player.Position);
            var timer = ActionScheduler.RunActionOnceAfterDelay(respawnPlayerActionPart1, CalculateRespawnDelay()-3);
            PlayerRespawnTimers[player].Add(timer);
        };


		var timer = ActionScheduler.RunActionOnceAfterDelay(makeSpectatorAction, 2.9);
        PlayerRespawnTimers[player].Add(timer);
		Helper.RemoveBuff(player, Prefabs.Buff_General_VampirePvPDeathDebuff);
		var blood = player.Character.Read<Blood>();
		Helper.SetPlayerBlood(player, blood.BloodType, blood.Quality);
		foreach (var buff in TeamToShardBuffsMap[player.MatchmakingTeam])
		{
			if (!BuffUtility.TryGetBuff(VWorld.Server.EntityManager, player.Character, buff, out var buffEntity))
			{
				var action = new ScheduledAction(Helper.BuffPlayer, new object[] { player, buff, buffEntity, Helper.NO_DURATION, true, true });
				ActionScheduler.ScheduleAction(action, 2);
			}
		}
	}

	public void HandleOnGameModeBegin(Player player)
	{
		if (!player.IsInCaptureThePancake()) return;
	}
	public void HandleOnGameModeEnd(Player player)
	{
		if (!player.IsInCaptureThePancake()) return;
	}
	public override void HandleOnPlayerChatCommand(Player player, CommandAttribute command)
	{
		if (!player.IsInCaptureThePancake()) return;

	}
	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (!player.IsInCaptureThePancake()) return;

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
	public override void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (!player.IsInCaptureThePancake()) return;
		if (item.ItemType == Prefabs.Item_Consumable_GlassBottle_BloodRosePotion_T02 || item.ItemType == Prefabs.Item_Consumable_Canteen_BloodRoseBrew_T01)
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
			player.ReceiveMessage("You can't drink those during a pancake match!".Error());
		}
		//BuffClanMembersOnConsume(player, item);
	}

	public override void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (!player.IsInCaptureThePancake()) return;

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
		if (prefabGuid == Prefabs.AB_Subdue_Channeling_Target_Debuff)
		{
			
			if (target.Index > 0 && Team.IsAllies(player.Character.Read<Team>(), target.Read<Team>()) )
			{
				player.Interrupt();
				player.ReceiveMessage("That's the wrong pancake!".Error());
			}
			else if (target.Index > 0)
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
		else if (prefabGuid == Prefabs.AB_Subdue_Channeling)
		{
			var lifetime = buffEntity.Read<LifeTime>();
			lifetime.Duration += 5;
			buffEntity.Write(lifetime);
			buffEntity.Add<DestroyBuffOnDamageTaken>();
		}
		else if (prefabGuid == Prefabs.AB_Subdue_Active_Capture_Buff)
		{
			foreach (var unitSpawn in CaptureThePancakeConfig.Config.UnitSpawns)
			{
				if (unitSpawn.Type.ToLower() == "horse")
				{
					if (unitSpawn.Team != player.MatchmakingTeam)
					{
						var unit = new ObjectiveHorse(unitSpawn.Team);
						unit.MaxHealth = unitSpawn.Health;
						unit.Category = "pancake";
						UnitFactory.SpawnUnit(unit, unitSpawn.Location.ToFloat3(), Teams[unitSpawn.Team][0]);
						break;
					}
				}
			}
		}
		else if (prefabGuid == Prefabs.AB_Feed_02_Bite_Abort_Trigger)
		{
			Helper.RemoveBuff(player.Character, Prefabs.AB_FeedEnemyVampire_01_Initiate_DashChannel);
		}
		else if (prefabGuid == Prefabs.AB_Interact_Mount_Owner_Buff_Horse)
		{
			Helper.BuffPlayer(player, Prefabs.AB_Gallop_Buff, out var buffEntity2, Helper.NO_DURATION); //I don't know how to make horses go below 6 speed without gallop buff																				//an option for sudden death -- mounting gives you a shield;
			/*BuffHelper.BuffPlayer(player, Prefabs.AB_Illusion_PhantomAegis_Buff, out var aegisBuffEntity, BuffHelper.NO_DURATION);
			aegisBuffEntity.Add<AbsorbBuff>();
			aegisBuffEntity.Write(new AbsorbBuff
			{
				AbsorbCap = 100,
				AbsorbValue = 100,
				AbsorbModifier = 1
			});
			aegisBuffEntity.Remove<MultiplyAbsorbCapBySpellPower>();*/
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
		else if (prefabGuid == Prefabs.Buff_General_RelicCarryDebuff)
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
		else if (shapeshiftGroupToBuff.ContainsValue(prefabGuid))
		{
			buffEntity.Add<DestroyBuffOnDamageTaken>();
			buffEntity.Add<DestroyOnAbilityCast>();
			var buffer = buffEntity.ReadBuffer<ReplaceAbilityOnSlotBuff>();
			buffer.Clear();
			Helper.ApplyStatModifier(buffEntity, BuffModifiers.PancakeShapeshiftSpeed);
		}
		else if (buffEntity.Has<AbsorbBuff>())/* && BuffHelper.HasBuff(player, Prefabs.AB_Interact_Mount_Owner_Buff_Horse))*/
		{
			if (targetPlayer.HasBuff(Prefabs.AB_Interact_Mount_Owner_Buff_Horse)) 
			{
				Helper.DestroyBuff(buffEntity);
			}
		}
        else if (buffEntity.Read<PrefabGUID>() == Prefabs.AB_Shapeshift_BloodMend_Buff)
        {
            var buffer = buffEntity.ReadBuffer<ChangeBloodOnGameplayEvent>();
            for (var i = 0; i < buffer.Length; i++)
            {
                var changeBloodOnGameplayEvent = buffer[i];
                changeBloodOnGameplayEvent.BloodValue = 10;
                buffer[i] = changeBloodOnGameplayEvent;
            }
        }
	}

    public static void HandleOnPlayerBuffRemoved(Player player, Entity buffEntity)
    {
        var prefabGuid = buffEntity.Read<PrefabGUID>();
        if (prefabGuid == Prefabs.Buff_General_RelicCarryDebuff)
        {
            Helper.RemoveBuff(player, Prefabs.AB_Shapeshift_Human_Grandma_Skin01_Buff);
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
		if (TryGetSpawnedUnitFromEntity(unit, out SpawnedUnit spawnedUnit))
		{
			if (spawnedUnit.Unit.Category != "pancake")
			{
				return;
			}
		}
		else
		{
			return;
		}


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
		/*		if (buffEntity.Read<PrefabGUID>() == Prefabs.AB_Interact_Siege_Structure_T01_PlayerBuff)
				{
					PrefabGUID icon;
					if (UnitFactory.TryGetSpawnedUnitFromEntity(unit, out var spawnedUnit))
					{
						if (spawnedUnit.Unit.Team == 1)
						{
							icon = Prefabs.MapIcon_CharmedUnit;
						}
						else
						{
							icon = Prefabs.MapIcon_Crypt;
						}
						var buffer = buffEntity.ReadBuffer<AttachMapIconsToEntity>();
						for (var i = 0; i < buffer.Length; i++)
						{
							var attachMapIconsToEntity = buffer[i];
							attachMapIconsToEntity.Prefab = icon;
							buffer[i] = attachMapIconsToEntity;
						}
					}
				}*/
	}

	public override void HandleOnPlayerConnected(Player player)
	{
		if (!player.IsInCaptureThePancake()) return;


		Helper.DestroyEntity(player.Character);
		if (player.MatchmakingTeam == 1)
		{
			Helper.RespawnPlayer(player, CaptureThePancakeConfig.Config.Team1PlayerRespawn.ToFloat3());
		}
		else if (player.MatchmakingTeam == 2)
		{
			Helper.RespawnPlayer(player, CaptureThePancakeConfig.Config.Team2PlayerRespawn.ToFloat3());
		}
	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (!player.IsInCaptureThePancake()) return;

	}

	public void HandleOnPlayerWillLoseGallopBuff(Player player, Entity eventEntity)
	{
		if (!player.IsInCaptureThePancake()) return;

		if (!(shouldRemoveGallopBuff.TryGetValue(player, out var value) && value))
		{
			eventEntity.Remove<DestroyTag>(); //the game tries to remove the gallop buff whenever you stop moving, stop this so the horse stays slow
		}
	}

	public void HandleOnPlayerMounted(Player player, Entity eventEntity)
	{
		if (!player.IsInCaptureThePancake()) return;
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
					message = $"{colorizedName} is stealing the enemy's {"pancake".Emphasize()}! Help them!";
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
		if (!player.IsInCaptureThePancake()) return;

		shouldRemoveGallopBuff[player] = true; // when they re-mount, re-flag gallop buff to not be destroyed
	}

	public void HandleOnPlayerInvitedToClan(Player player, Entity eventEntity)
	{
		if (!player.IsInCaptureThePancake()) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not invite players to your clan while in Capture the Pancake".Error());
	}

	public void HandleOnPlayerKickedFromClan(Player player, Entity eventEntity)
	{
		if (!player.IsInCaptureThePancake()) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not kick players from your clan while in Capture the Pancake".Error());
	}

	public void HandleOnPlayerLeftClan(Player player, Entity eventEntity)
	{
		if (!player.IsInCaptureThePancake()) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage("You may not leave your clan while in Capture the Pancake".Error());
	}

	public void HandleOnUnitDeath(Entity unitEntity, OnKillCallResult killCallResult)
	{
		if (!MatchActive) return;

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
				foreach (var configUnitSpawn in CaptureThePancakeConfig.Config.UnitSpawns)
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



	public void HandleOnPlayerChatCommand(Player player, Entity eventEntity)
	{
		if (!player.IsInCaptureThePancake()) return;


	}

	public override void HandleOnItemWasThrown(Player closestPlayer, Entity eventEntity)
	{
		//we have no guarantee that this is the player that threw it, just that they are close to the item
		//this should be enough to at least verify the game mode
		if (!closestPlayer.IsInCaptureThePancake()) return;

		var dropItemAroundPositionEvent = eventEntity.Read<DropItemAroundPosition>();
		if (!dropItemAroundPositionEvent.ItemEntity.Has<Relic>())
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		}
	}

	public override void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
	{
		if (!player.IsInCaptureThePancake()) return;

		var damageDealtEvent = eventEntity.Read<DealDamageEvent>();
		var targetPrefab = damageDealtEvent.Target.Read<PrefabGUID>();

		bool isSpawnedByGameMode = false;
		var isStructure = damageDealtEvent.Target.Has<CastleHeartConnection>();
		if (UnitFactory.TryGetSpawnedUnitFromEntity(damageDealtEvent.Target, out var unit))
		{
			if (unit.Unit.Category == "pancake")
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
	}

	public void HandleOnPlayerDamageReceived(Player player, Entity eventEntity)
	{
		if (!player.IsInCaptureThePancake()) return;

		var damageDealtEvent = eventEntity.Read<DealDamageEvent>();
		var source = damageDealtEvent.SpellSource.Read<EntityOwner>().Owner;

		if (source.Read<PrefabGUID>() == Prefabs.CHAR_Gloomrot_Purifier_VBlood)
		{
			damageDealtEvent.MaterialModifiers.PlayerVampire *= 1.5f;
			eventEntity.Write(damageDealtEvent);
		}
	}

    public void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
    {
        if (!player.IsInCaptureThePancake()) return;


        var abilityCastStartedEvent = eventEntity.Read<AbilityCastStartedEvent>();
        if (abilityCastStartedEvent.AbilityGroup.Index <= 0) return;

        var abilityGuid = abilityCastStartedEvent.AbilityGroup.Read<PrefabGUID>();

        //prevent shapeshift spells from breaking players out of their shapeshift (due to the abnormal way we gave them shapeshift)
        if (AbilitiesToNotCauseBuffDestruction.TryGetValue(abilityGuid, out var buff))
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
        if (unit.Category != "pancake") return;

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


    public void HandleOnPlayerRespawn(Player player)
	{
        if (!player.IsInCaptureThePancake()) return;
		
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
						CaptureThePancakeHelper.EndMatch(player.MatchmakingTeam);
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

	private static void CheckForShardAndUpdate(Player player, ShardData shardData)
	{
		if (Helper.PlayerHasItemInInventories(player, shardData.ItemPrefabGUID))
		{
            Helper.RemoveItemFromInventory(player, shardData.ItemPrefabGUID);
			var friendlyTeam = GetFriendlyTeam(player);
			var enemyTeam = GetOpposingTeam(player);
			if (!TeamToShardBuffsMap[player.MatchmakingTeam].Contains(shardData.BuffPrefabGUID))
			{
				TeamToShardBuffsMap[player.MatchmakingTeam].Add(shardData.BuffPrefabGUID);
			}

			foreach (var teamPlayer in friendlyTeam)
			{
				Helper.BuffPlayer(teamPlayer, shardData.BuffPrefabGUID, out Entity buffEntity, Helper.NO_DURATION, true, true);
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

	private static bool IsInFriendlyEndZone(Player player)
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

	private static void BuffClanMembersOnConsume(Player player, InventoryBuffer item)
	{
		var itemToBuff = new Dictionary<PrefabGUID, PrefabGUID>
		{
			{ Prefabs.Item_Consumable_GlassBottle_SpellBrew_T02, Prefabs.AB_Consumable_SpellBrew_T02_Buff },
			{ Prefabs.Item_Consumable_GlassBottle_PhysicalBrew_T02, Prefabs.AB_Consumable_PhysicalBrew_T02_Buff },
		};

		if (item.ItemType == Prefabs.Item_Consumable_GlassBottle_SpellBrew_T02 || item.ItemType == Prefabs.Item_Consumable_GlassBottle_PhysicalBrew_T02)
		{
			var clanMembers = player.GetClanMembers();
			foreach (var member in clanMembers)
			{
				Helper.BuffEntity(member.Character, itemToBuff[item.ItemType], out Entity buffEntity);
				buffEntity.Remove<Buff_Persists_Through_Death>();
			}
		}
	}

	public static new Dictionary<string, bool> GetAllowedCommands()
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

	public override void ResetPlayer(Player player)
	{
		player.Reset(false, false, false);
	}
}

