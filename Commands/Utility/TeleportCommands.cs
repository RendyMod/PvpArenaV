using System;
using PvpArena.Models;
using ProjectM;
using Unity.Transforms;
using Bloodstone.API;
using ProjectM.Network;
using Unity.Entities;
using PvpArena.Services;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Helpers;
using PvpArena.Data;
using PvpArena.Configs;
using PvpArena.GameModes;

namespace PvpArena.Commands;

internal class TeleportCommands
{
	[Command("tp", description: "Teleports you to a set waypoint", usage:".tp 1", aliases: new string[] { "teleport" }, adminOnly: false, includeInHelp: true, category: "Teleport")]
	public void TeleportCommand(Player sender, string nameOrId)
	{
		if (sender.IsInMatch())
		{
			sender.ReceiveMessage("Can't teleport while in a match!".Error());
			return;
		}

		if (WaypointManager.TryFindWaypoint(nameOrId, out var Waypoint))
		{
			if (sender.IsAdmin || !Waypoint.IsAdminOnly)
			{
				try
				{
					sender.Teleport( Waypoint.Position);
					sender.ReceiveMessage(("Teleported to waypoint: " + Waypoint.Name.Emphasize()).White());
				}
				catch (Exception e)
				{
					sender.ReceiveMessage(e.ToString());
					return;
				}
			}
			else
			{
				sender.ReceiveMessage("Could not find waypoint!".Error());
				return;
			}
		}
		else
		{
			sender.ReceiveMessage("Could not find waypoint!".Error());
			return;
		}
	}

	[Command("ttp", description: "Requests to teleport you to player", usage:".ttp Ash", adminOnly: false, includeInHelp: true, category: "Teleport")]
	public void TeleportToPlayerCommand (Player sender, Player Recipient)
	{
		if (!Recipient.IsOnline)
		{
			sender.ReceiveMessage("Cannot send a teleport request to an offline player".Error());
			return;
		}
		Player RequesterPlayer = sender;
		Player RecipientPlayer = Recipient;
		var RequesterUserData = RequesterPlayer.User.Read<User>();

		if (RequesterPlayer.IsInMatch())
		{
			sender.ReceiveMessage("Cannot teleport while in a match!".Error());
			return;
		}

		if (RecipientPlayer.CurrentState == Player.PlayerState.In1v1Matchmaking)
		{
			sender.ReceiveMessage("Cannot teleport to player -- Player is currently in a match!".Error());
			return;
		}

		var request = new TeleportRequest
		{
			Requester = RequesterPlayer,
			Recipient = RecipientPlayer,
			Timestamp = DateTime.Now,
		};

		TeleportRequestManager.AddRequest(request);
		sender.ReceiveMessage($"Requested teleportation to {RecipientPlayer.User.Read<User>().CharacterName.ToString().Emphasize()}".White());
		RecipientPlayer.ReceiveMessage($"{RequesterUserData.CharacterName.ToString().Emphasize()} has requested to teleport to you. Accept with .tpa.".White());
	}

	[Command("tpa", description: "Approves teleportation request", usage: ".tpa", adminOnly: false, includeInHelp: false, category: "Teleport")]
	public void TeleportAcceptCommand(Player sender, Player RequesterPlayer = null)
	{
		if (RequesterPlayer != null && !RequesterPlayer.IsOnline)
		{
			sender.ReceiveMessage("Cannot approve a teleport request from an offline player".Error());
			return;
		}
		var RecipientPlayer = sender;
		TeleportRequest request;
		if (RequesterPlayer != null)
		{
			request = TeleportRequestManager.GetRequest(RequesterPlayer, RecipientPlayer);
		}
		else
		{
			request = TeleportRequestManager.GetRequest();
			if (RequesterPlayer == null)
			{
				RequesterPlayer = request.Requester;
			}
		}
		if (request != null && !request.IsExpired())
		{
			if (RequesterPlayer.IsInMatch())
			{
				sender.ReceiveMessage($"{RequesterPlayer.Name.Emphasize()} cannot be teleported because they have joined a match!".Error());
				return;
			}

			if (RecipientPlayer.IsInMatch())
			{
				sender.ReceiveMessage("Cannot accept teleport request while in a match!".Error());
				return;
			}
			TeleportRequestManager.RemoveRequest(request);
			var targetPosition = request.Recipient.Position;
			request.Requester.Teleport( targetPosition);
			RequesterPlayer.ReceiveMessage($"{RecipientPlayer.Name.Emphasize()} has approved your request".White());
			RecipientPlayer.ReceiveMessage("Teleportation ".White() + "approved!".Emphasize());
		}
		else
		{
			RecipientPlayer.ReceiveMessage("No active teleportation requests to approve!".Error());
		}
	}

	[Command("spectate", description: "Go invisible and teleport to a player", usage: ".spectate Ash", aliases: new string[] { "spec" }, adminOnly: false, includeInHelp: true, category: "Teleport")]
	public void SpectateCommand(Player sender, Player player = null)
	{
		if (sender.CurrentState == Player.PlayerState.Spectating && player == null)
		{
			sender.Reset(BaseGameMode.ResetOptions);
			sender.Teleport( PvpArenaConfig.Config.CustomSpawnLocation.ToFloat3());
			sender.CurrentState = Player.PlayerState.Normal;
			sender.ReceiveMessage("Stopped spectating".White());
		}
		else
		{
			sender.CurrentState = Player.PlayerState.Spectating;
			Helper.BuffPlayer(sender, Prefabs.Admin_Observe_Invisible_Buff, out var buffEntity, Helper.NO_DURATION);
			Helper.RemoveBuffModifications(buffEntity, BuffModificationTypes.DisableMapCollision);
			
			if (player != null)
			{
				sender.Teleport( player.Position);
				sender.ReceiveMessage($"Now spectating {player.Name.Colorify(ExtendedColor.ClanNameColor)}. Do .spectate again to undo.".White());
			}
			else
			{
				sender.ReceiveMessage($"Entered spectate mode. Do .spectate again to undo.".White());
			}
		}
	}
}
