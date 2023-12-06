using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using ProjectM.Network;
using PvpArena.Helpers;
using PvpArena.Models;
using Unity.Entities;
using static ProjectM.DeathEventListenerSystem;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

namespace PvpArena.GameModes;
public abstract class BaseGameMode
{
	public abstract void Initialize();
	public abstract void Dispose();
	public abstract void HandleOnPlayerDowned(Player player, Entity killer);
	public abstract void HandleOnPlayerDeath(Player player, OnKillCallResult killCallResult);
	//public abstract void HandleOnPlayerRespawn(Player entity); //currently unneeded..tricky to find a way to call it only once on respawn
	public abstract void HandleOnShapeshift(Player player, Entity eventEntity);
	public abstract void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item);
	public abstract void HandleOnPlayerConnected(Player player);
	public abstract void HandleOnPlayerDisconnected(Player player);
	public abstract void HandleOnItemWasDropped(Player player, Entity eventEntity, PrefabGUID itemType, int slotIndex);
	public abstract void HandleOnPlayerDamageDealt(Player player, Entity eventEntity);
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

