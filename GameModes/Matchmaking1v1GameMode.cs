using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bloodstone.API;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using PvpArena.Data;
using PvpArena.Helpers;
using PvpArena.Matchmaking;
using PvpArena.Models;
using PvpArena.Services;
using Unity.Entities;
using static ProjectM.DeathEventListenerSystem;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

namespace PvpArena.GameModes;

public class Matchmaking1v1GameMode : BaseGameMode
{
	public static Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
	{
		ResetCooldowns = true,
		RemoveConsumables = true,
		RemoveShapeshifts = true
	};

	private static Dictionary<string, bool> AllowedCommands = new Dictionary<string, bool>
	{
		{ "ping", true },
		{ "help", true },
		{ "legendary", true },
		{ "jewel", true },
		{ "forfeit", true },
		{ "points", true },
		{ "lb ranked", true },
	};

	public Matchmaking1v1GameMode()
	{
		ResetOptions = new Helper.ResetOptions();
	}

	public override void Initialize()
	{
		/*GameEvents.OnPlayerRespawn += HandleOnPlayerRespawn;*/
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
		GameEvents.OnPlayerUsedConsumable += HandleOnConsumableUse;
		GameEvents.OnPlayerBuffed += HandleOnPlayerBuffed;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnItemWasThrown += HandleOnItemWasThrown;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
	}
	public override void Dispose()
	{
		/*GameEvents.OnPlayerRespawn -= HandleOnPlayerRespawn;*/
		GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
		GameEvents.OnPlayerUsedConsumable -= HandleOnConsumableUse;
		GameEvents.OnPlayerBuffed -= HandleOnPlayerBuffed;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnItemWasThrown -= HandleOnItemWasThrown;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
	}
	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (!player.IsIn1v1()) return;

		Helper.Reset(player, ResetOptions);
		if (Helper.BuffPlayer(player, Prefabs.Witch_PigTransformation_Buff, out var buffEntity, 3))
		{
			buffEntity.Add<BuffModificationFlagData>();
			buffEntity.Write(BuffModifiers.PigModifications);
		}
		
		var loser = player;
		var winner = MatchmakingHelper.GetOpponentForPlayer(player);
		MatchmakingQueue.MatchManager.EndMatch(winner, loser);
		if (killer.Has<PlayerCharacter>())
		{
			var KillerPlayer = PlayerService.GetPlayerFromCharacter(killer);

			if (player.ConfigOptions.SubscribeToKillFeed)
			{
				player.ReceiveMessage($"You were killed by {KillerPlayer.Name.Error()}.".White());
			}
			if (KillerPlayer.ConfigOptions.SubscribeToKillFeed)
			{
				KillerPlayer.ReceiveMessage($"You killed {player.Name.Success()}!".White());
			}
		}
	}
	public override void HandleOnPlayerDeath(Player player, OnKillCallResult killCallResult)
	{
		if (!player.IsIn1v1()) return;

		var winner = MatchmakingHelper.GetOpponentForPlayer(player);
		MatchmakingQueue.MatchManager.EndMatch(winner, player, false);
		Helper.Reset(player);

		var blood = player.Character.Read<Blood>();
		Helper.SetPlayerBlood(player, blood.BloodType, blood.Quality);
	}
/*	public override void HandleOnPlayerRespawn(Player player)
	{
		if (!player.IsIn1v1()) return;

		
	}*/
	public void HandleOnGameModeBegin(Player player)
	{
		if (!player.IsIn1v1()) return;
	}
	public void HandleOnGameModeEnd(Player player)
	{
		if (!player.IsIn1v1()) return;
	}
	public override void HandleOnPlayerChatCommand(Player player, CommandAttribute command)
	{
		if (!player.IsIn1v1()) return;
	}
	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (!player.IsIn1v1()) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage($"Shapeshifting is disabled while in a match.".Error());
	}
	public override void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (!player.IsIn1v1()) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
	}
	public override void HandleOnPlayerBuffed(Player player, Entity buffEntity)
	{
		if (!player.IsIn1v1()) return;
	}

	public override void HandleOnPlayerConnected(Player player)
	{
		if (!player.IsIn1v1()) return;

	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (!player.IsIn1v1()) return;

		
		MatchmakingQueue.MatchManager.EndMatch(MatchmakingHelper.GetOpponentForPlayer(player), player, false);
	}

	public override void HandleOnItemWasThrown(Player player, Entity eventEntity)
	{
		if (!player.IsIn1v1()) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
	}

	public override void HandleOnPlayerDamageDealt(Player player, Entity eventEntity)
	{
		if (!player.IsIn1v1()) return;

		var damageDealtEvent = eventEntity.Read<DealDamageEvent>();
		var isStructure = (damageDealtEvent.Target.Has<CastleHeartConnection>());
		if (isStructure)
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		}
	}

	public static new Dictionary<string, bool> GetAllowedCommands()
	{
		return AllowedCommands;
	}
}
