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
using static ProjectM.Debugging.DealDamageEventCommand;
using UnityEngine.UI;

namespace PvpArena.GameModes.BulletHell;

public class DodgeballGameMode : BaseGameMode
{
    
	public bool HasStarted = false;
	public static Dictionary<int, List<Player>> Teams = new Dictionary<int, List<Player>>();
	public List<Timer> Timers = new List<Timer>();
	public Stopwatch stopwatch = new Stopwatch();
	public Dictionary<int, RectangleZone> FightZones;
    public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
    {
        RemoveConsumables = true,
        RemoveShapeshifts = true,
    };

    public DodgeballGameMode()
	{
		
	}

	public override void Initialize()
	{
		/*GameEvents.OnPlayerRespawn += HandleOnPlayerRespawn;*/
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
		GameEvents.OnPlayerStartedCasting += HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerUsedConsumable += HandleOnConsumableUse;
		GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnItemWasThrown += HandleOnItemWasThrown;
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
		GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnItemWasThrown -= HandleOnItemWasThrown;
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
	}

	private static Dictionary<string, bool> AllowedCommands = new Dictionary<string, bool>
	{

	};

	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (!player.IsInDodgeball()) return;

	}
	public override void HandleOnPlayerDeath(Player player, OnKillCallResult killCallResult)
	{
		if (!player.IsInDodgeball()) return;

		var pos = player.Position;
		Helper.RespawnPlayer(player, pos);
        player.Reset(ResetOptions);
        var blood = player.Character.Read<Blood>();
		Helper.SetPlayerBlood(player, blood.BloodType, blood.Quality);
		//end match and tp to training + report time and record score
	}
	/*public override void HandleOnPlayerRespawn(Player player)
	{
		if (!player.IsInDefaultMode()) return;

	}*/
	public override void HandleOnPlayerChatCommand(Player player, CommandAttribute command)
	{
		if (!player.IsInDodgeball()) return;

	}
	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (!player.IsInDodgeball()) return;

	}
	public override void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (!player.IsInDodgeball()) return;

	}

	public void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
	{
		if (!player.IsInDodgeball()) return;

	}

	public override void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (!player.IsInDodgeball()) return;

		//if buff is bloodrite, revive the teammate that died
	}

	public override void HandleOnPlayerConnected(Player player)
	{
		if (!player.IsInDodgeball()) return;

		if (PvpArenaConfig.Config.UseCustomSpawnLocation)
		{
			player.Teleport(PvpArenaConfig.Config.CustomSpawnLocation.ToFloat3()); //replace this with training tp
		}
	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (!player.IsInDodgeball()) return;

        //kill them
	}

	public override void HandleOnItemWasThrown(Player player, Entity eventEntity)
	{
		if (!player.IsInDodgeball()) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
	}

	public override void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
	{
		if (!player.IsInDodgeball()) return;

		var damageDealtEvent = eventEntity.Read<DealDamageEvent>();
		var isStructure = damageDealtEvent.Target.Has<CastleHeartConnection>();
		if (isStructure)
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		}
		else
		{
			var damageDealtEventNew = new DealDamageEvent(damageDealtEvent.Target, damageDealtEvent.MainType, damageDealtEvent.MainFactor, damageDealtEvent.ResourceModifier, damageDealtEvent.MaterialModifiers, damageDealtEvent.SpellSource, 0, .34f, damageDealtEvent.Modifier, damageDealtEvent.DealDamageFlags);
			eventEntity.Write(damageDealtEventNew);
		}
	}
	private bool IsOutOfBounds(Player player)
	{
		return !FightZones[player.MatchmakingTeam].Contains(player);
	}

	private static void MakeSpectator(Player player)
	{
		player.Reset(ResetOptions);
		Helper.BuffPlayer(player, Prefabs.AB_Shapeshift_Mist_Buff, out var buffEntity, Helper.NO_DURATION);
		Helper.CompletelyRemoveAbilityBarFromBuff(buffEntity);
		Helper.FixIconForShapeshiftBuff(player, buffEntity, Prefabs.AB_Shapeshift_Mist_Group);
		Helper.ModifyBuff(buffEntity, BuffModificationTypes.Invulnerable | BuffModificationTypes.Immaterial | BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.AbilityCastImpair | BuffModificationTypes.PickupItemImpaired | BuffModificationTypes.TargetSpellImpaired, true);

		Helper.BuffPlayer(player, Prefabs.Admin_Observe_Invisible_Buff, out var invisibleBuff, Helper.NO_DURATION);
		Helper.ModifyBuff(invisibleBuff, BuffModificationTypes.None, true);
		Helper.RespawnPlayer(player, player.Position);
	}

	public void HandleOnGameFrameUpdate()
	{
		if (HasStarted)
		{
			foreach (var team in Teams.Values)
			{
				foreach (var player in team)
				{
					if (IsOutOfBounds(player))
					{
						player.ReceiveMessage("You have gone out of bounds!".Error());
						//kill them
					}
				}
			}
		}
	}

	public static new Dictionary<string, bool> GetAllowedCommands()
	{
		return AllowedCommands;
	}
}

