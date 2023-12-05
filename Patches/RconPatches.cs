using System;
using System.Collections.Generic;
using System.Linq;
using Bloodstone.API;
using HarmonyLib;
using ProjectM;

namespace PvpArena.Patches;

[HarmonyPatch(typeof(RconListenerSystem), nameof(RconListenerSystem.ExecuteCommand))]
public static class RconListenerSystemPatch
{
	public static Dictionary<string, Action<string[]>> CommandMethods = new Dictionary<string, Action<string[]>>();

	static RconListenerSystemPatch()
	{
		CommandMethods.Add("announce", Announce);
		CommandMethods.Add("announcerestart", AnnounceServerRestart);
	}


	public static bool Prefix(RconListenerSystem __instance, string command)
	{
		try
		{
			string input = command;
			Plugin.PluginLog.LogInfo($"rcon command: {input}");

			// Split the input into parts
			string[] parts = input.Split(' ');

			// The first part is the command
			string commandPart = parts[0];

			// The rest are the arguments
			string[] arguments = parts.Skip(1).ToArray();

			// Check if the command is in the dictionary
			if (CommandMethods.TryGetValue(commandPart, out Action<string[]> method))
			{
				// Run the method and pass the arguments
				method(arguments);
			}
			else
			{
				// Handle the case where the command is not found
				Plugin.PluginLog.LogInfo($"RCON Command not found: {commandPart}");
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo(e.ToString());
		}

		return false;
	}

	

	public static void Announce(string[] args)
	{
		if (args.Length > 0)
		{
			ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager, args[0].White());
		}
	}

	public static void AnnounceServerRestart(string[] args)
	{
		if (args.Length > 0)
		{
			if (int.TryParse(args[0], out var minutes)) 
			{
				var message = $"There will be an automated {"Server Restart".Warning()} in {minutes.ToString().Emphasize()} minutes".White();
				ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager, message);
				return;
			}
		}
		
		Plugin.PluginLog.LogInfo($"Invalid arguments for announcerestart command. Should be something like: runcommand announcerestart 5");
	}
}
