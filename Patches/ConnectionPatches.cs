using HarmonyLib;
using ProjectM;
using Unity.Entities;
using Unity.Mathematics;
using Bloodstone.API;
using PvpArena.Data;
using ProjectM.Network;
using Stunlock.Network;
using PvpArena.Matchmaking;
using PvpArena.Services;
using System;
using PvpArena.Helpers;
using PvpArena.GameModes;
using PvpArena.Models;
using PvpArena.Configs;
using PvpArena.Frameworks.CommandFramework;

namespace PvpArena.Patches;

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserDisconnected))]
public static class OnUserDisconnectedPatch
{
	private static AdminAuthSystem adminAuthSystem = VWorld.Server.GetExistingSystem<AdminAuthSystem>();

	public static void Prefix (ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
	{
		try
		{
			if (__instance._NetEndPointToApprovedUserIndex.TryGetValue(netConnectionId, out int userIndex))
			{
				if (userIndex >= 0 && userIndex < __instance._ApprovedUsersLookup.Length)
				{
					var serverClient = __instance._ApprovedUsersLookup[userIndex];
					var User = serverClient.UserEntity;
					var Player = PlayerService.GetPlayerFromUser(User);
					PlayerService.OnlinePlayers.Remove(Player);
					GameEvents.RaisePlayerDisconnected(Player);
					if (Player.PlayerPointsData.OnlineTimer != null)
					{
						Player.PlayerPointsData.OnlineTimer.Dispose();
						Player.PlayerPointsData.OnlineTimer = null;
					}

					MatchmakingQueue.Leave(Player);

					Player.Teleport(new float3(0, 0, 0));
				}
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception in disconnect patch: {e.ToString()}");
		}
	}
}

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
public static class OnUserConnectedPatch
{
	private static AdminAuthSystem adminAuthSystem = VWorld.Server.GetExistingSystem<AdminAuthSystem>();
	public static bool HasLaunched = false;

	public static void Prefix (ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
	{
		try
		{
			if (__instance._NetEndPointToApprovedUserIndex.TryGetValue(netConnectionId, out int userIndex))
			{
				if (userIndex >= 0 && userIndex < __instance._ApprovedUsersLookup.Length)
				{
					var serverClient = __instance._ApprovedUsersLookup[userIndex];
					var User = serverClient.UserEntity;
					if (User.Index > 0)
					{
						var player = PlayerService.GetPlayerFromUser(User);
						PlayerService.OnlinePlayers.TryAdd(player, true);
						// TODO : Check everything is working and fix this
						//Action action = () =>
						//	LoginPointsService.AwardPoints(player, PvpArenaConfig.Config.PointsPerIntervalOnline);
						//player.PlayerPointsData.OnlineTimer = ActionScheduler.RunActionEveryInterval(action,
						//	60 * PvpArenaConfig.Config.IntervalDurationInMinutes);
						if (player.BanInfo.IsBanned())
						{
							Helper.KickPlayer(player.SteamID);
						}

						if (player.Character.Index > 0)
						{
							GameEvents.RaisePlayerConnected(player);
							Helper.Reset(player, false, true);
							if (player.IsAdmin)
							{
								//admins WILL be grandmas >:(
								Helper.UnlockAllContent(player.ToFromCharacter());
								Helper.RemoveBuff(player, Prefabs.Buff_Gloomrot_SentryOfficer_TurretCooldown);
							}
							else
							{
								AddBuffToPlayerToImpairBuilding(player);
							}
						}

						SendWelcomeMessageToPlayer(player);
					}
				}
			}
		}
		catch (Exception e)
		{
			Plugin.PluginLog.LogInfo($"Exception in connect patch: {e.ToString()}");
		}
	}

	[CommandFramework.Command("welcome-message", adminOnly: true)]
	public static void SendWelcomeMessageToPlayer (Player player)
	{
		player.ReceiveMessage(($"Welcome to " + "V Arena".Colorify(ExtendedColor.LightServerColor)).Colorify(ExtendedColor.ServerColor));
		player.ReceiveMessage(("Join us on Discord" + ": " +
		                      $"{PvpArenaConfig.Config.DiscordLink}".Colorify(ExtendedColor.LightServerColor)).Emphasize());
		player.ReceiveMessage(("Jewels:".Emphasize()+" Use " +
		                      ".j spellName ?".Colorify(ExtendedColor.LightServerColor) +
		                      " to see the " + "mods".Emphasize()).White());
		player.ReceiveMessage(("Legendaries:".Emphasize()+" Use " + ".lw ?".Colorify(ExtendedColor.LightServerColor) +
		                      " to see the available "+"effects".Emphasize()).White());
		player.ReceiveMessage(("Blood:".Emphasize()+" Use " + ".bp bloodName".Colorify(ExtendedColor.LightServerColor) +
		                      " to get "+"blood".Emphasize()).White());
		player.ReceiveMessage(("Buffs:".Emphasize()+" Use " + ".buffs".Colorify(ExtendedColor.LightServerColor) + " or " + "Blood Hunger".Colorify(ExtendedColor.LightServerColor) +
		                      " to toggle "+"buffs".Emphasize()).White());
		player.ReceiveMessage(("Reset:".Emphasize()+" Use " + ".r".Colorify(ExtendedColor.LightServerColor) + " or " + "Blood Mend".Colorify(ExtendedColor.LightServerColor) +
		                       " to reset "+ "CD".Emphasize()+" and "+"HP".Emphasize()).White());
		player.ReceiveMessage(("TP:".Emphasize()+" Use " + ".tp-list".Colorify(ExtendedColor.LightServerColor) +
		                      " to check the list of TPs").White());
		player.ReceiveMessage(("Type " + ".help".Colorify(ExtendedColor.LightServerColor) +
		                      " in chat to see "+"all available commands".Emphasize()).White());
	}

	public static void AddBuffToPlayerToImpairBuilding (Player player)
	{
		if (!Helper.BuffPlayer(player, Prefabs.Buff_Gloomrot_SentryOfficer_TurretCooldown, out Entity buffEntity,
			    Helper.NO_DURATION, true))
		{
			var action = new ScheduledAction(AddBuffToPlayerToImpairBuilding, new object[] { player });
			ActionScheduler.ScheduleAction(action, 50);
		}
		else
		{
			Helper.ModifyBuff(buffEntity, BuffModificationTypes.BuildMenuImpair);
		}
	}
}
