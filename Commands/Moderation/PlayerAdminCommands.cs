using ProjectM.Network;
using PvpArena;
using PvpArena.Data;
using PvpArena.Helpers;
using PvpArena.Models;
using PvpArena.Services;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

internal static class PlayerAdminCommands
{
	[Command("rename", description:"Rename a character", adminOnly: true)]
	public static void RenamePlayer (Player sender, Player _foundPlayer = null, string newName = "")
	{
		if (_foundPlayer == null)
			sender.ReceiveMessage("Player not found!".Error());
		else if (newName == "")
			sender.ReceiveMessage("New Name is empty!".Error());
		else if (PlayerService.TryGetPlayerFromString(newName, out Player otherPlayer))
			sender.ReceiveMessage("Name already taken!".Error());
		else
		{
			Helper.RenamePlayer(new FromCharacter
			{
				Character = _foundPlayer.Character,
				User = _foundPlayer.User,
			}, newName);
			sender.ReceiveMessage("Renaming done!".Success());
		}
	}

	[Command("log-steamids", description:"Logs steam ids of all connected players", adminOnly:true)]
	public static void LogSteamIds(Player sender)
	{
		Plugin.PluginLog.LogInfo($"Logging online players:");
		foreach (var Player in PlayerService.UserCache.Values)
		{
			if (Player.IsOnline)
			{
				Plugin.PluginLog.LogInfo($"{Player.Name} {Player.SteamID}");
			}
		}
		sender.ReceiveMessage("Logged Steam IDs to Console !".Success());
	}

	[Command("down", adminOnly: true)]
	public static void DownCommand(Player sender, Player player = null)
	{
		var Player = player ?? sender;

		Helper.BuffPlayer(Player, Prefabs.Buff_General_Vampire_Wounded_Buff, out var buffEntity);

		sender.ReceiveMessage("Downed.".Success());
	}

	[Command("kill", adminOnly: true)]
	public static void KillCommand(Player sender, Player player = null)
	{
		var Player = player ?? sender;

		Helper.DestroyEntity(Player.Character);

		sender.ReceiveMessage("Killed.".Success());
	}
}
