using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Bloodstone.API;
using ProjectM.Network;
using PvpArena.Services;
using System;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using System.Collections.Generic;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;
using PvpArena.GameModes;
using Bloodstone;

namespace PvpArena.Patches;


[HarmonyPatch(typeof(ChatMessageSystem), nameof(ChatMessageSystem.OnUpdate))]
[HarmonyBefore(new string[] { "gg.deca.Bloodstone" })]
public static class ChatMessageSystemPatch
{
	public static void Prefix(ChatMessageSystem __instance)
	{
		var entities = __instance._ChatMessageQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var fromCharacter = entity.Read<FromCharacter>();
				var chatEvent = entity.Read<ChatMessageEvent>();

				var player = PlayerService.GetPlayerFromUser(fromCharacter.User);

				if (CommandHandler.ExecuteCommand(player, chatEvent.MessageText.ToString()))
				{
					entity.Destroy();
				}
				if (player.MuteInfo.IsMuted())
				{
					if (player.MuteInfo.MuteDurationDays == -1)
					{
						player.ReceiveMessage($"You are muted indefinitely. If you feel there is a mistake, you can open a ticket on discord to appeal".Error());
					}
					else
					{
						player.ReceiveMessage($"You are muted for {player.MuteInfo.GetFormattedRemainingMuteTime()}. If you feel there is a mistake, you can open a ticket on discord to appeal".Error());
					}

					entity.Destroy();
				}
				if (entity.Exists())
				{
					GameEvents.RaisePlayerChatMessage(player, entity);
					if (chatEvent.MessageType == ChatMessageType.Global && DiscordBotConfig.Config.GlobalChannel > 0)
					{
						if (chatEvent.MessageText.ToString() != "!reload")
						{
							DiscordBot.SendMessageAsync($"{player.Name}: {chatEvent.MessageText}", DiscordBotConfig.Config.GlobalChannel);
						}
					}
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
				continue;
			}
		}
	}
}

[HarmonyPatch(typeof(VivoxConnectionSystem), nameof(VivoxConnectionSystem._HandleClientLoginMessages_b__13_0))]
public static class VivoxConnectionSystemPatch
{
	public static bool Prefix(VivoxConnectionSystem __instance, Entity entity, ref FromCharacter fromCharacter)
	{
		try
		{
			if (PlayerService.GetPlayerFromUser(fromCharacter.User).MuteInfo.IsMuted())
			{
				VWorld.Server.EntityManager.DestroyEntity(entity);
				return false;
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo(e.ToString());
		}

		return true;
	}
}
