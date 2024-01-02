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
using PvpArena.GameModes.Moba;
using PvpArena.GameModes.PrisonBreak;
using PvpArena.GameModes.Troll;
using PvpArena.Models;
using PvpArena.Services;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

namespace PvpArena.Commands.GameModes;
internal class GameModeCommands
{
	[Command("start-pancake", description: "Starts capture the pancake", usage: ".start-pancake 1 Ash Rendy", adminOnly: true)]
	public void StartPancakeCommand(Player sender, int arenaNumber, Player team2Leader, Player team1Leader = null)
	{
		arenaNumber--;
		if (team1Leader == null)
		{
			team1Leader = sender;
		}
		if (!Team.IsAllies(team1Leader.Character.Read<Team>(), team2Leader.Character.Read<Team>()))
		{
			if (arenaNumber < 0 || arenaNumber >= CaptureThePancakeManager.captureThePancakeGameModes.Count)
			{
				sender.ReceiveMessage("You have specified an invalid arena number.".Error());
				return;
			}
			if (team1Leader.IsInCaptureThePancake())
			{
				sender.ReceiveMessage($"{team1Leader.Name} is already in a pancake match!".Error());
				return;
			} 
			else if (team2Leader.IsInCaptureThePancake())
			{
				sender.ReceiveMessage($"{team2Leader.Name} is already in a pancake match!".Error());
				return;
			}
			if (!CaptureThePancakeManager.StartMatchAtArenaIfAvailable(arenaNumber, team1Leader, team2Leader))
			{
				sender.ReceiveMessage("That arena is currently in a match!".Error());
			}
		}
		else
		{
			sender.ReceiveMessage("Cannot start a match against someone in the same clan!".Error());
		}
	}

	[Command("end-pancake", usage: ".end-pancake {arenaNumber}", description: "Ends capture the pancake", adminOnly: true)]
	public void EndPancakeCommand(Player sender, int arenaNumber)
	{
		CaptureThePancakeManager.EndMatch(arenaNumber - 1, 0);
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
			DominationHelper.EndMatch();
			DominationHelper.StartMatch(team1Leader, team2Leader);
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
		DominationHelper.EndMatch();
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

	[Command("start-prisonbreak", description: "Starts prison break", usage: ".start-prisonbreak", aliases: new string[] { "start prisonbreak", "start prison-break", "start-prison-break" }, adminOnly: true)]
	public void StartPrisonBreakCommand(Player sender, Player target)
	{
		PrisonBreakHelper.EndMatch();
		Action action = () =>
		{
			PrisonBreakHelper.StartMatch(target.GetClanMembers());
		};
		ActionScheduler.RunActionOnceAfterDelay(action, 1);
		sender.ReceiveMessage("Starting".White());
	}

	[Command("end-prisonbreak", description: "Ends prison break", usage: ".end-prisonbreak", aliases: new string[] { "end prisonbreak", "end-prison-break" }, adminOnly: true)]
	public void EndPrisonBreakCommand(Player sender)
	{
		PrisonBreakHelper.EndMatch();
		sender.ReceiveMessage("Match ended".Success());
	}

	[Command("troll", description: "Used for debugging", adminOnly: true)]
	public void TrollCommand(Player sender, Player player = null)
	{
		if (sender.CurrentState == Player.PlayerState.Troll)
		{
			TrollModeManager.RemoveTroll(player ?? sender);
		}
		else
		{
			TrollModeManager.AddTroll(player ?? sender);
		}
	}


	[Command("no-healing-limit", description: "Used for debugging", adminOnly: true)]
	public void NoHealingLimitCommand(Player sender, Player player = null)
	{
		if (sender.CurrentState == Player.PlayerState.NoHealingLimit)
		{
			NoHealingLimitManager.RemovePlayer(player ?? sender);
		}
		else
		{
			NoHealingLimitManager.AddPlayer(player ?? sender);
		}
	}

	[Command("start-moba", description: "Used for debugging", adminOnly: true)]
	public void StartMobaCommand(Player sender, Player team2Leader, Player team1Leader = null)
	{
		if (team1Leader == null)
		{
			team1Leader = sender;
		}
		if (!Team.IsAllies(team1Leader.Character.Read<Team>(), team2Leader.Character.Read<Team>()))
		{
			MobaHelper.EndMatch();
			Action action = () =>
			{
				MobaHelper.StartMatch(team1Leader, team2Leader);
			};
			ActionScheduler.RunActionOnceAfterDelay(action, 1);
		}
		else
		{
			sender.ReceiveMessage("Cannot start a match against someone in the same clan!".Error());
		}
	}


	[Command("end-moba", description: "Used for debugging", adminOnly: true)]
	public void EndMobaCommand(Player sender)
	{
		MobaHelper.EndMatch();
		sender.ReceiveMessage("Match ended".Success());
	}
}
