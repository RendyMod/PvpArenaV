using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using PvpArena.GameModes.BulletHell;
using PvpArena.GameModes.CaptureThePancake;
using PvpArena.GameModes.Dodgeball;
using PvpArena.GameModes.Domination;
using PvpArena.Models;
using PvpArena.Services;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

namespace PvpArena.Commands.GameModes;
internal class GameModeCommands
{
	[Command("start-pancake", description: "Starts capture the pancake", usage: ".start-pancake Ash Rendy", adminOnly: true)]
	public void StartPancakeCommand(Player sender, Player team2Leader, Player team1Leader = null)
	{
		if (team1Leader == null)
		{
			team1Leader = sender;
		}
		if (!Team.IsAllies(team1Leader.Character.Read<Team>(), team2Leader.Character.Read<Team>()))
		{
			CaptureThePancakeHelper.EndMatch();
			System.Action action = () =>
			{
				CaptureThePancakeHelper.StartMatch(team1Leader, team2Leader);
			};
			ActionScheduler.RunActionOnceAfterDelay(action, 1);

		}
		else
		{
			sender.ReceiveMessage("Cannot start a match against someone in the same clan!".Error());
		}
	}

	[Command("end-pancake", description: "Ends capture the pancake", adminOnly: true)]
	public void EndPancakeCommand(Player sender)
	{
		CaptureThePancakeHelper.EndMatch(1);
		sender.ReceiveMessage("Match ended".Success());
	}

	[Command("start-domination", description: "Starts a match of domination", usage: ".start-domination Ash", adminOnly: true)]
	public void StartDominationCommand(Player sender, Player team2Leader, Player team1Leader = null)
	{
		if (team1Leader == null)
		{
			team1Leader = sender;
		}
		if (!Team.IsAllies(team1Leader.Character.Read<Team>(), team2Leader.Character.Read<Team>()))
		{
			DodgeballHelper.EndMatch();
			DodgeballHelper.StartMatch(team1Leader, team2Leader);
			sender.ReceiveMessage("Match started".Success());
		}
		else
		{
			sender.ReceiveMessage("Cannot start a match against someone in your clan!".Error());
		}
	}

	[Command("end-domination", description: "Ends domination", adminOnly: true)]
	public void EndDominationCommand(Player sender)
	{
		DodgeballHelper.EndMatch();
		sender.ReceiveMessage("Match ended".Success());
	}

	[Command("start-bullet", description: "Starts bullet hell", usage: ".start-bullet", aliases: new string[] { "start bullet", "bullethell", "bullet", "strat-bullet" }, adminOnly: false)]
	public void StartBulletHellCommand(Player sender)
	{
		BulletHellManager.QueueForMatch(sender);
	}

	[Command("leave-bullet", description: "Starts bullet hell", usage: ".start-bullet", aliases: new string[] { "leave bullet", "endbullet", "end-bullet", "end bullet" }, adminOnly: false)]
	public void LeaveBulletHellCommand(Player sender)
	{
		BulletHellManager.LeaveQueue(sender);
	}

	[Command("start-dodgeball", description: "Starts dodgeball", usage: ".start-dodgeball Ash Rendy", aliases: new string[] { "start dodgeball", "dodgeball", "start dodgeball"}, adminOnly: true)]
	public void StartDodgeballCommand(Player sender, Player team2Leader, Player team1Leader = null)
	{
		if (team1Leader == null)
		{
			team1Leader = sender;
		}
		if (!Team.IsAllies(team1Leader.Character.Read<Team>(), team2Leader.Character.Read<Team>()))
		{
			DodgeballHelper.EndMatch();
			Action action = () =>
			{
				DodgeballHelper.StartMatch(team1Leader, team2Leader);
			};
			ActionScheduler.RunActionOnceAfterDelay(action, 1);
		}
		else
		{
			sender.ReceiveMessage("Cannot start a match against someone in the same clan!".Error());
		}
	}

	[Command("end-dodgeball", description: "Ends dodgeball", usage: ".end-dodgeball", aliases: new string[] { "end dodgeball", "end dodgeball" }, adminOnly: true)]
	public void EndDodgeballCommand(Player sender)
	{
		DodgeballHelper.EndMatch();
		sender.ReceiveMessage("Match ended".Success());
	}
}
