using System.Collections.Generic;
using ProjectM;
using PvpArena.Models;
using Unity.Entities;
using static ProjectM.DeathEventListenerSystem;
using ProjectM.Network;
using PvpArena.Data;
using Bloodstone.API;
using PvpArena.Configs;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Helpers;
using ProjectM.Gameplay.Systems;
using ProjectM.CastleBuilding;
using PvpArena.Services;
using System.Threading;
using System.Diagnostics;
using static PvpArena.Configs.ConfigDtos;
using PvpArena.Factories;

namespace PvpArena.GameModes.BulletHell;

public class BulletHellGameMode : BaseGameMode
{
	public override Player.PlayerState GameModeType => Player.PlayerState.BulletHell;
    public Player player = null;
	public bool HasStarted = false;
	public List<Timer> Timers = new List<Timer>();
	public Stopwatch stopwatch = new Stopwatch();
	public int ArenaNumber = 0;
	public CircleZone FightZone;
	public TemplateUnitSpawn UnitSpawns = new TemplateUnitSpawn();
	private long lastReportedInterval = 0;
	public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
    {
        RemoveConsumables = true,
        RemoveShapeshifts = true,
    };

    public BulletHellGameMode(CircleZone fightZone, TemplateUnitSpawn unitSpawns, int arenaNumber)
	{
		FightZone = fightZone;
		UnitSpawns = unitSpawns;
		ArenaNumber = arenaNumber;
	}

	public override void Initialize()
	{
		/*GameEvents.OnPlayerRespawn += HandleOnPlayerRespawn;*/
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
		GameEvents.OnPlayerStartedCasting += HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerUsedConsumable += HandleOnConsumableUse;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnItemWasDropped += HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
		GameEvents.OnGameFrameUpdate += HandleOnGameFrameUpdate;
	}
	public override void Dispose()
	{
		/*GameEvents.OnPlayerRespawn -= HandleOnPlayerRespawn;*/
		GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
		GameEvents.OnPlayerStartedCasting -= HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerUsedConsumable -= HandleOnConsumableUse;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnItemWasDropped -= HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
		GameEvents.OnGameFrameUpdate -= HandleOnGameFrameUpdate;
		HasStarted = false;
		stopwatch.Reset();
		foreach (var timer in Timers)
		{
			if (timer != null)
			{
				timer.Dispose();
			}
		}
		Timers.Clear();
		player = null;
		lastReportedInterval = 0;
	}

	private static HashSet<string> AllowedCommands = new HashSet<string>
    {

	};

	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (player.CurrentState != GameModeType) return;

		player.Reset(ResetOptions);
		if (Helper.BuffPlayer(player, Prefabs.Witch_PigTransformation_Buff, out var buffEntity, 3))
		{
			buffEntity.Add<BuffModificationFlagData>();
			buffEntity.Write(BuffModifiers.PigModifications);
		}
		BulletHellManager.EndMatch(this);
		//end match and tp to training + report time and record score
	}
	public override void HandleOnPlayerDeath(Player player, OnKillCallResult killCallResult)
	{
		if (player.CurrentState != GameModeType) return;

		var pos = player.Position;
		Helper.RespawnPlayer(player, pos);
        player.Reset(ResetOptions);
        var blood = player.Character.Read<Blood>();
		Helper.SetPlayerBlood(player, blood.BloodType, blood.Quality);
		BulletHellManager.EndMatch(this);
		//end match and tp to training + report time and record score
	}
	/*public override void HandleOnPlayerRespawn(Player player)
	{
		if (!player.IsInDefaultMode()) return;

	}*/
	public override void HandleOnPlayerChatCommand(Player player, CommandAttribute command)
	{
		if (player.CurrentState != GameModeType) return;

	}
	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

	}
	public override void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (player.CurrentState != GameModeType) return;

	}

	public void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

	}
	public override void HandleOnPlayerConnected(Player player)
	{
		if (player.CurrentState != GameModeType) return;

		if (PvpArenaConfig.Config.UseCustomSpawnLocation)
		{
			player.Teleport(PvpArenaConfig.Config.CustomSpawnLocation.ToFloat3()); //replace this with training tp
		}
	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (player.CurrentState != GameModeType) return;

        BulletHellManager.EndMatch(this);
	}

	public override void HandleOnItemWasDropped(Player player, Entity eventEntity, PrefabGUID itemType)
	{
		if (player.CurrentState != GameModeType) return;

        Helper.RemoveItemAtSlotFromInventory(player, itemType, eventEntity.Read<DropInventoryItemEvent>().SlotIndex);
        VWorld.Server.EntityManager.DestroyEntity(eventEntity);
    }

	public override void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

        if (eventEntity.Exists())
        {
            var damageDealtEvent = eventEntity.Read<DealDamageEvent>();
            var isStructure = damageDealtEvent.Target.Has<CastleHeartConnection>();
            if (isStructure)
            {
                VWorld.Server.EntityManager.DestroyEntity(eventEntity);
            }
        }
	}

	private bool IsOutOfBounds()
	{
		return !FightZone.Contains(player);
	}

	public void HandleOnGameFrameUpdate()
	{
		if (HasStarted)
		{
			if (!IsOutOfBounds())
			{
				long elapsedTimeInSeconds = stopwatch.ElapsedMilliseconds / 1000;
				long nextInterval = lastReportedInterval + 5;

				if (elapsedTimeInSeconds >= nextInterval)
				{
					var pos = player.Position;
					pos.y += 2;
					Helper.MakeSCT(player, Prefabs.SCT_Type_InfoMessage, nextInterval, pos);
					lastReportedInterval = nextInterval;
				}
				
			}
			else
			{
				player.ReceiveMessage("You have gone out of bounds!".Error());
				BulletHellManager.EndMatch(this);
			}
			
		}

	}

	public static new HashSet<string> GetAllowedCommands()
	{
		return AllowedCommands;
	}
}

