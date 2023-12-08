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
using PvpArena.GameModes.BulletHell;
using PvpArena.Services.Moderation;

namespace PvpArena.GameModes.Prison;

public class PrisonGameMode : BaseGameMode
{
	public override Player.PlayerState GameModeType => Player.PlayerState.Imprisoned;
	public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
	{
		ResetCooldowns = true,
		RemoveShapeshifts = false,
		RemoveConsumables = false
	};

	static HashSet<string> AllowedCommands = new HashSet<string>
	{
		"ping",
		"help",
		"legendary",
		"jewel",
		"points",
		"lb ranked",
		"bp",
		"recount"
	};

	public override void Initialize()
	{
		BaseInitialize();
		GameEvents.OnPlayerDowned += HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath += HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift += HandleOnShapeshift;
		GameEvents.OnPlayerStartedCasting += HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerChatMessage += HandleOnPlayerChatMessage;
	}
	public override void Dispose()
	{
		BaseDispose();
		GameEvents.OnPlayerDowned -= HandleOnPlayerDowned;
		GameEvents.OnPlayerDeath -= HandleOnPlayerDeath;
		GameEvents.OnPlayerShapeshift -= HandleOnShapeshift;
		GameEvents.OnPlayerStartedCasting -= HandleOnPlayerStartedCasting;
		GameEvents.OnPlayerChatMessage -= HandleOnPlayerChatMessage;
	}

	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (player.CurrentState != GameModeType) return;

		player.Reset(ResetOptions);
		Helper.MakeGhostlySpectator(player);

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
	public override void HandleOnPlayerDeath(Player player, DeathEvent deathEvent)
	{
		if (player.CurrentState != GameModeType) return;
		var pos = ImprisonService.GetPlayerCellCoordinates(player);
		Helper.RespawnPlayer(player, pos);
		player.Reset(ResetOptions);
		var blood = player.Character.Read<Blood>();
		Helper.SetPlayerBlood(player, blood.BloodType, blood.Quality);
	}

	public override void HandleOnPlayerChatCommand(Player player, CommandAttribute command)
	{
		if (player.CurrentState != GameModeType) return;

	}
	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

		var enterShapeshiftEvent = eventEntity.Read<EnterShapeshiftEvent>();
		if (enterShapeshiftEvent.Shapeshift == Prefabs.AB_Shapeshift_BloodMend_Group)
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
			player.Reset(ResetOptions);
		}
		else if (enterShapeshiftEvent.Shapeshift == Prefabs.AB_Shapeshift_ShareBlood_ExposeVein_Group)
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
			Helper.ToggleBloodOnPlayer(player);
		}
		else if (enterShapeshiftEvent.Shapeshift == Prefabs.AB_Shapeshift_BloodHunger_BloodSight_Group)
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
			Helper.ToggleBuffsOnPlayer(player);
		}
		else
		{
			VWorld.Server.EntityManager.DestroyEntity(eventEntity);
			player.ReceiveMessage("You can't feel your vampire essence here...".Error());
		}
	}

	public void HandleOnPlayerStartedCasting(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

	}

	public override void HandleOnPlayerConnected(Player player)
	{
		if (!player.ImprisonInfo.IsImprisoned()) return;

		player.CurrentState = Player.PlayerState.Imprisoned;
		player.Teleport(PrisonConfig.Config.CellCoordinateList[player.ImprisonInfo.PrisonCellNumber].ToFloat3());
	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (player.CurrentState != GameModeType) return;

		player.Teleport(PrisonConfig.Config.CellCoordinateList[player.ImprisonInfo.PrisonCellNumber].ToFloat3());
	}

	public void HandleOnPlayerChatMessage(Player player, Entity eventEntity)
	{
		if (player.CurrentState != GameModeType) return;

		var chatEvent = eventEntity.Read<ChatMessageEvent>();
		if (chatEvent.MessageType == ChatMessageType.Global)
		{
			eventEntity.Destroy();
			player.ReceiveMessage("The prison walls are too thick for anyone to hear you.".Error());
		}
	}

	public static new HashSet<string> GetAllowedCommands()
	{
		return AllowedCommands;
	}
}

