using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PvpArena;
using PvpArena.GameModes;
using PvpArena.GameModes.BulletHell;
using PvpArena.GameModes.CaptureThePancake;
using PvpArena.GameModes.Dodgeball;
using PvpArena.GameModes.Domination;
using PvpArena.GameModes.Matchmaking1v1;
using PvpArena.GameModes.Moba;
using PvpArena.GameModes.Pacified;
using PvpArena.GameModes.Prison;
using PvpArena.GameModes.PrisonBreak;
using PvpArena.GameModes.Troll;
using PvpArena.Models;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

public class GameModePermissionMiddleware : IMiddleware
{
	private static Dictionary<Player.PlayerState, HashSet<string>> allowedCommandsByGameMode;

	static GameModePermissionMiddleware()
	{
		InitializeAllowedCommands();
	}

	private static void InitializeAllowedCommands()
	{
		allowedCommandsByGameMode = new Dictionary<Player.PlayerState, HashSet<string>>
		{
			{ Player.PlayerState.Normal, DefaultGameMode.GetAllowedCommands() },
			{ Player.PlayerState.In1v1Matchmaking, Matchmaking1v1GameMode.GetAllowedCommands() },
			{ Player.PlayerState.CaptureThePancake, CaptureThePancakeGameMode.GetAllowedCommands() },
			{ Player.PlayerState.Domination, DominationGameMode.GetAllowedCommands() },
			{ Player.PlayerState.Spectating, SpectatingGameMode.GetAllowedCommands() },
			{ Player.PlayerState.BulletHell, BulletHellGameMode.GetAllowedCommands() },
			{ Player.PlayerState.Imprisoned, PrisonGameMode.GetAllowedCommands() },
			{ Player.PlayerState.Dodgeball, DodgeballGameMode.GetAllowedCommands() },
			{ Player.PlayerState.Troll, TrollGameMode.GetAllowedCommands() },
			{ Player.PlayerState.PrisonBreak, PrisonBreakGameMode.GetAllowedCommands() },
			{ Player.PlayerState.NoHealingLimit, NoHealingLimitGameMode.GetAllowedCommands() },
			{ Player.PlayerState.Moba, MobaGameMode.GetAllowedCommands() },
			{ Player.PlayerState.Pacified, PacifiedGameMode.GetAllowedCommands() }
			// Add more game modes and their corresponding allowed commands here
		};
	}
	public bool CanExecute(Player sender, CommandAttribute command, MethodInfo method)
	{
		try
		{
			if (command.AdminOnly) return true;

			if (allowedCommandsByGameMode.TryGetValue(sender.CurrentState, out var allowedCommands))
			{
				if (allowedCommands.Contains(command.Name) || allowedCommands.Contains("all"))
				{
					return true;
				}
				else
				{
					sender.ReceiveMessage("That command is not allowed while in this game mode".Error());
				}
			}
			else
			{
				sender.ReceiveMessage("This game mode is not properly configured for commands yet");
			}

			
			return false;
		}
		catch (Exception ex)
		{
			sender.ReceiveMessage("An error occurred while processing the command.".Error());
			return false;
		}
	}

}
