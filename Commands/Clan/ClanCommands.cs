using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using Bloodstone.API;
using PvpArena.Models;
using PvpArena.Services;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.Helpers;
using Unity.Collections;

namespace PvpArena.Commands;

internal class ClanCommands
{
	[Command("clan invite", description: "Invite a player to your clan, regardless of either person's clan state", usage: ".inv Ash", aliases: new string[] { "inv", "ci", "invite", "clan-invite" }, adminOnly: false, category:"Clan", includeInHelp: true)]
	public void ClanInviteCommand (Player sender, Player player)
	{
		if (sender.User.Read<Team>().Equals(player.User.Read<Team>()))
		{
			sender.ReceiveMessage("You're already in the same clan!".Error());
			return;
		}

		var RequesterPlayer = sender;
		var RecipientPlayer = player;

		var request = new Request
		{
			Type = Request.RequestType.ClanInviteRequest,
			Requester = RequesterPlayer,
			Recipient = RecipientPlayer,
			Timestamp = System.DateTime.Now
		};
		ClanRequestManager.AddRequest(request);
		sender.ReceiveMessage($"Invited {RecipientPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} to join clan..");
		RecipientPlayer.ReceiveMessage($"{RequesterPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} invited you to their clan - Use {".ca".Emphasize()} to accept.");
	}

	[Command("request join clan", description: "Request to join a player's clan", usage: ".rjc Ash", aliases: new string[] { "rjc", "req", "request-join-clan" }, adminOnly: false, category: "Clan", includeInHelp: true)]
	public void RequestJoinClanCommand (Player sender, Player player)
	{
		if (sender.User.Read<Team>().Equals(player.User.Read<Team>()))
		{
			sender.ReceiveMessage("You're already in the same clan!".Error());
			return;
		}
		
		var RequesterPlayer = sender;
		var RecipientPlayer = player;

		var request = new Request
		{
			Type = Request.RequestType.ClanJoinRequest,
			Requester = RequesterPlayer,
			Recipient = RecipientPlayer,
			Timestamp = System.DateTime.Now
		};
		var existingRequest = ClanRequestManager.GetRequest(RequesterPlayer, RecipientPlayer);
		if (existingRequest != null && !existingRequest.IsExpired())
		{
			RequesterPlayer.ReceiveMessage($"{RecipientPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} already has a pending request".Warning());
			return;
		}

		ClanRequestManager.AddRequest(request);
		sender.ReceiveMessage($"Requested to join {RecipientPlayer.Name.Colorify(ExtendedColor.ClanNameColor)}'s clan..");
		RecipientPlayer.ReceiveMessage($"{RequesterPlayer.Name.Colorify(ExtendedColor.ClanNameColor)} wants to join your clan - {".ca".Emphasize()} to accept");
	}

	[Command("clan accept", description: "Approves a clan request", usage: ".ca Ash", aliases: new string[] { "clanaccept", "clan-accept", "ca" }, adminOnly: false, category: "Clan", includeInHelp: false)]
	public void ClanAcceptCommand (Player sender, Player player = null)
	{
		if (player != null)
		{
			HandleClanAccept(sender, player);
		}
		else
		{
			HandleClanAccept(sender);
		}
	}

	public static void HandleClanAccept(Player RecipientPlayer, Player RequesterPlayer = default)
	{
		Request request;
		if (RequesterPlayer != default)
		{
			request = ClanRequestManager.GetRequest(RequesterPlayer, RecipientPlayer);
		}
		else
		{
			request = ClanRequestManager.GetRequest();
		}

		if (request != null && !request.IsExpired())
		{
			RequesterPlayer = request.Requester;

			var RequesterUserData = RequesterPlayer.User.Read<User>();
			ClanRequestManager.RemoveRequest(request);

			if (request.Type == Request.RequestType.ClanJoinRequest)
			{
				Helper.RemoveFromClan(RequesterPlayer);
				ScheduledAction action;

				var clanEntity = RecipientPlayer.User.Read<User>().ClanEntity._Entity;
				if (!clanEntity.Exists())
				{
					action = new ScheduledAction(Helper.CreateClanForPlayer, new object[] { RecipientPlayer.User });
					ActionScheduler.ScheduleAction(action, 2);
				}

				action = new ScheduledAction(Helper.AddPlayerToPlayerClanForce, new object[] { RequesterPlayer.User, RecipientPlayer.User });
				ActionScheduler.ScheduleAction(action, 3);
			}
			else if (request.Type == Request.RequestType.ClanInviteRequest)
			{
				RecipientPlayer.RemoveFromClan();
				ScheduledAction action;

				var clanEntity = RequesterPlayer.User.Read<User>().ClanEntity._Entity;
				if (!clanEntity.Exists())
				{
					action = new ScheduledAction(Helper.CreateClanForPlayer, new object[] { RequesterPlayer.User });
					ActionScheduler.ScheduleAction(action, 2);
				}
				
				action = new ScheduledAction(Helper.AddPlayerToPlayerClanForce, new object[] { RecipientPlayer.User, RequesterPlayer.User });
				ActionScheduler.ScheduleAction(action, 3);
				RequesterPlayer.ReceiveMessage("You have joined the clan.".White());
			}
		}
		else
		{
			RecipientPlayer.ReceiveMessage($"No active clan requests to approve!".Error());
		}
	}

	[Command("leave clan", description: "Leave your clan", usage: ".lc", adminOnly: false, aliases: new string[] { "lc", "leaveclan", "leave-clan" }, category: "Clan", includeInHelp: true)]
	public void LeaveClanCommand (Player sender)
	{
		var clanEntity = sender.User.Read<User>().ClanEntity._Entity;
		if (!clanEntity.Exists())
			sender.ReceiveMessage("You aren't in a clan.".Error());
		else
		{
			sender.RemoveFromClan();
			sender.ReceiveMessage("You left the clan successfully!");
		}
	}

	[Command("rename clan", description: "Rename your clan", usage: ".rnc", adminOnly: false, aliases: new string[] { "rnc", "renameclan", "rename-clan" }, category: "Clan", includeInHelp: true)]
	public void RenameClanCommand(Player sender, string name)
	{
		var clanEntity = sender.User.Read<User>().ClanEntity._Entity;
		if (clanEntity.Exists())
		{
			var clanTeam = clanEntity.Read<ClanTeam>();
			clanTeam.Name = new FixedString64(name);
			clanEntity.Write(clanTeam);
			var clanMembers = sender.GetClanMembers();
			foreach (var clanMember in clanMembers)
			{
				ClanUtility.SetCharacterClanName(VWorld.Server.EntityManager, clanMember.User, clanTeam.Name);
			}
			sender.ReceiveMessage("You have renamed your clan to " + name.Colorify(ExtendedColor.ClanNameColor) +".");
		}
		else
		{
			sender.ReceiveMessage("You aren't in a clan.".Error());
		}
	}

	[Command("create-clan", description: "Used for debugging", adminOnly: true)]
	public static void CreateClanCommand(Player sender, string name = "")
	{
		Helper.CreateClanForPlayer(sender.User);
		sender.ReceiveMessage("You successfully created the clan!");
	}
}
