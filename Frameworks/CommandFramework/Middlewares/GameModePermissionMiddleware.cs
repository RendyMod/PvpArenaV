using System.Collections.Generic;
using System.Reflection;
using PvpArena;
using PvpArena.GameModes;
using PvpArena.GameModes.BulletHell;
using PvpArena.GameModes.CaptureThePancake;
using PvpArena.GameModes.Domination;
using PvpArena.GameModes.Matchmaking1v1;
using PvpArena.GameModes.Prison;
using PvpArena.Models;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

public class GameModePermissionMiddleware : IMiddleware
{
	public bool CanExecute(Player sender, CommandAttribute command, MethodInfo method)
	{
		try
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
			else if (sender.IsInBulletHell())
			{
				allowedCommands = BulletHellGameMode.GetAllowedCommands();
			}
			else if (sender.IsImprisoned())
			{
				allowedCommands = PrisonGameMode.GetAllowedCommands();
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
		catch
		{
			sender.ReceiveMessage("Command settings for this game mode have not been configured yet.".Error());
			return false;
		}
	}
}
