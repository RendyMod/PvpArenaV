using System.Collections.Generic;
using System.Reflection;
using PvpArena.GameModes;
using PvpArena.GameModes.CaptureThePancake;
using PvpArena.GameModes.Domination;
using PvpArena.Models;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

public class GameModePermissionMiddleware : IMiddleware
{
	public bool CanExecute(Player sender, CommandAttribute command, MethodInfo method)
	{
		if (sender.IsAdmin && command.AdminOnly) return true; //don't let admins run basic commands accidentally
		Dictionary<string, bool> allowedCommands = default;
		if (sender.IsInDefaultMode())
		{
			allowedCommands = DefaultGameMode.GetAllowedCommands();
		}
		else if (sender.IsIn1v1())
		{
			allowedCommands = Matchmaking1v1GameMode.GetAllowedCommands();
		}
		else if (sender.IsInCaptureThePancake())
		{
			allowedCommands = CaptureThePancakeGameMode.GetAllowedCommands();
		}
		else if (sender.IsInDomination())
		{
			allowedCommands = DominationGameMode.GetAllowedCommands();
		}
		else if (sender.IsSpectating())
		{
			allowedCommands = SpectatingGameMode.GetAllowedCommands();
		}
		if (allowedCommands.ContainsKey(command.Name) || allowedCommands.ContainsKey("all"))
		{
			return true;
		}
		else
		{
			sender.ReceiveMessage("That command is not allowed while in this game mode".Error());
			return false;
		}
	}
}
