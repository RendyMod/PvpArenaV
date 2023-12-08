using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bloodstone.API;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using PvpArena.Configs;
using PvpArena.Helpers;
using PvpArena.Models;
using Unity.Entities;
using Unity.Mathematics;
using static ProjectM.DeathEventListenerSystem;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

namespace PvpArena.GameModes;
public abstract class BaseGameMode
{
	public abstract void Initialize();
	public abstract void Dispose();
	protected virtual void BaseInitialize()
	{
		GameEvents.OnItemWasDropped += HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
	}
	protected virtual void BaseDispose()
	{
		GameEvents.OnItemWasDropped -= HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
	}
	public abstract void HandleOnPlayerDowned(Player player, Entity killer);
	public abstract void HandleOnPlayerDeath(Player player, DeathEvent deathEvent);
	//public abstract void HandleOnPlayerRespawn(Player entity); 
	public abstract void HandleOnShapeshift(Player player, Entity eventEntity);
	public virtual void HandleOnPlayerConnected(Player player)
	{
		if (player.CurrentState != this.GameModeType) return;

		if (PvpArenaConfig.Config.UseCustomSpawnLocation)
		{
			player.Teleport(PvpArenaConfig.Config.CustomSpawnLocation.ToFloat3());
		}
	}
	public virtual void HandleOnPlayerDisconnected(Player player)
	{
		if (player.CurrentState != GameModeType) return;

		player.Teleport(new float3(0, 0, 0));
	}
	public virtual void HandleOnItemWasDropped(Player player, Entity eventEntity, PrefabGUID itemType, int slotIndex)
    {
        if (player.CurrentState != GameModeType) return;

        Helper.RemoveItemAtSlotFromInventory(player, itemType, slotIndex);
        VWorld.Server.EntityManager.DestroyEntity(eventEntity);
    }
	public virtual void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
    {
		if (player.CurrentState != GameModeType) return;

		if (!eventEntity.Exists()) return;

		var damageDealtEvent = eventEntity.Read<DealDamageEvent>();

		var isStructure = damageDealtEvent.Target.Has<CastleHeartConnection>();
		if (isStructure)
		{
			eventEntity.Destroy();
		}
	}
	public abstract void HandleOnPlayerChatCommand(Player player, CommandAttribute command);
    public virtual Player.PlayerState GameModeType { get; }

    private static HashSet<string> AllowedCommands = new HashSet<string>
	{
		{ "all" }
	};
	public static HashSet<string> GetAllowedCommands()
	{
		return AllowedCommands;
	}

	//use this for joining / leaving any match, but use the actual game mode's reset for resetting from within a match itself
	public static Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
	{
		RemoveConsumables = true,
		RemoveShapeshifts = true
	};
}

