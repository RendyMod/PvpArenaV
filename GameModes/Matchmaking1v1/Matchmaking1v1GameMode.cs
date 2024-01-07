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
using PvpArena.Models;
using PvpArena.Services;
using Unity.Entities;
using static ProjectM.DeathEventListenerSystem;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

namespace PvpArena.GameModes.Matchmaking1v1;

public class Matchmaking1v1GameMode : BaseGameMode
{
	public override Player.PlayerState PlayerGameModeType => Player.PlayerState.In1v1Matchmaking;
	public override string UnitGameModeType => "1v1";
	public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
	{
		ResetCooldowns = true,
		RemoveConsumables = true,
		RemoveShapeshifts = true
	};

	private static HashSet<string> AllowedCommands = new HashSet<string>
	{ 
		"ping",
		"help" ,
		"legendary" ,
		"jewel" ,
		"forfeit" ,
		"points" ,
		"lb ranked" ,
	};

	public Matchmaking1v1GameMode()
	{
		ResetOptions = new Helper.ResetOptions();
	}

	public override void Initialize()
	{
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
		GameEvents.OnPlayerUsedConsumable += HandleOnConsumableUse;
		GameEvents.OnItemWasDropped += HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt += HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected += HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected += HandleOnPlayerConnected;
		GameEvents.OnPlayerPlacedStructure += HandleOnPlayerPlacedStructure;
	}
	public override void Dispose()
	{
		GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
		GameEvents.OnPlayerUsedConsumable -= HandleOnConsumableUse;
		GameEvents.OnItemWasDropped -= HandleOnItemWasDropped;
		GameEvents.OnPlayerDamageDealt -= HandleOnPlayerDamageDealt;
		GameEvents.OnPlayerDisconnected -= HandleOnPlayerDisconnected;
		GameEvents.OnPlayerConnected -= HandleOnPlayerConnected;
		GameEvents.OnPlayerPlacedStructure -= HandleOnPlayerPlacedStructure;
	}
	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (player.CurrentState != PlayerGameModeType) return;

		player.Reset(Helper.ResetOptions.FreshMatch);
		if (Helper.BuffPlayer(player, Prefabs.Witch_PigTransformation_Buff, out var buffEntity, 3))
		{
			Helper.ModifyBuff(buffEntity, BuffModifiers.PigModifications, true);

			var buffer = VWorld.Server.EntityManager.AddBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
			buffer.Clear();

			buffer.Add(BuffModifiers.ShapeshiftFastMoveSpeed);
			var buffer2 = buffEntity.ReadBuffer<CreateGameplayEventsOnDestroy>();
			buffer2.Clear();
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

	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != PlayerGameModeType) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
		player.ReceiveMessage($"Shapeshifting is disabled while in a match.".Error());
	}
	public void HandleOnConsumableUse(Player player, Entity eventEntity, InventoryBuffer item)
	{
		if (player.CurrentState != PlayerGameModeType) return;

		VWorld.Server.EntityManager.DestroyEntity(eventEntity);
	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (player.CurrentState != PlayerGameModeType) return;
		base.HandleOnPlayerDisconnected(player);

		MatchmakingQueue.MatchManager.EndMatch(MatchmakingHelper.GetOpponentForPlayer(player), player, false);
	}

	public static new HashSet<string> GetAllowedCommands()
	{
		return AllowedCommands;
	}
}
