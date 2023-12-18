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
using AsmResolver;

namespace PvpArena.GameModes.Prison;

public class PrisonGameMode : DefaultGameMode
{
	public override Player.PlayerState PlayerGameModeType => Player.PlayerState.Imprisoned;
	public override string UnitGameModeType => "prison";
	public static new Helper.ResetOptions ResetOptions { get; set; } = new Helper.ResetOptions
	{
		ResetCooldowns = true,
		RemoveShapeshifts = false,
		RemoveConsumables = false
	};

	public static List<Timer> Timers = new List<Timer>();

	static new HashSet<string> AllowedCommands = new HashSet<string>
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
        base.Initialize();
		GameEvents.OnPlayerChatMessage += HandleOnPlayerChatMessage;
	}
	public override void Dispose()
	{
        base.Dispose();
		GameEvents.OnPlayerChatMessage -= HandleOnPlayerChatMessage;

		foreach (var timer in Timers)
		{
			if (timer != null)
			{
				timer.Dispose();
			}
		}
		Timers.Clear();
	}

	public override void HandleOnShapeshift(Player player, Entity eventEntity)
	{
		if (player.CurrentState != PlayerGameModeType) return;

		var enterShapeshiftEvent = eventEntity.Read<EnterShapeshiftEvent>();
		
		if (enterShapeshiftEvent.Shapeshift == Prefabs.AB_Shapeshift_ShareBlood_ExposeVein_Group)
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

	public override void HandleOnPlayerConnected(Player player)
	{
		if (!player.ImprisonInfo.IsImprisoned()) return;

		player.CurrentState = Player.PlayerState.Imprisoned;
		player.Teleport(PrisonConfig.Config.CellCoordinateList[player.ImprisonInfo.PrisonCellNumber].ToFloat3());
	}

	public override void HandleOnPlayerDisconnected(Player player)
	{
		if (player.CurrentState != PlayerGameModeType) return;

		player.Teleport(PrisonConfig.Config.CellCoordinateList[player.ImprisonInfo.PrisonCellNumber].ToFloat3());
	}

	
	public static BuffModificationTypes PrisonDeathModifications = BuffModificationTypes.Invulnerable | BuffModificationTypes.Immaterial | BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.AbilityCastImpair | BuffModificationTypes.MovementImpair | BuffModificationTypes.RotationImpair;
	public override void HandleOnPlayerDowned(Player player, Entity killer)
	{
		if (player.CurrentState != this.PlayerGameModeType) return;
		
		if (player.Clan.Exists()) //if they might be in a teamfight then we don't want to remove summons just because they died
		{
			player.Reset(TeamfightResetOptions);
		}
		else
		{
			player.Reset(ResetOptions);
		}
		
		if (Helper.BuffPlayer(player, Prefabs.VampireDeathBuff, out var buffEntity, 2.2f))
		{
			Helper.ModifyBuff(buffEntity, PrisonDeathModifications, true);
		}
		
		System.Action teleportBackToCellAction = () =>
		{
			Helper.RemoveBuff(player, Prefabs.VampireDeathBuff);
			player.Teleport(PrisonConfig.Config.CellCoordinateList[player.ImprisonInfo.PrisonCellNumber].ToFloat3());
		};
		

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
		
		var timer = ActionScheduler.RunActionOnceAfterDelay(teleportBackToCellAction,  2.2f);
		Timers.Add(timer);
	}

	public void HandleOnPlayerChatMessage(Player player, Entity eventEntity)
	{
		if (player.CurrentState != PlayerGameModeType) return;

		var chatEvent = eventEntity.Read<ChatMessageEvent>();
		if (chatEvent.MessageType == ChatMessageType.Global || chatEvent.MessageType == ChatMessageType.Whisper)
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

