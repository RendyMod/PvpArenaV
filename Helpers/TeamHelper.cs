using System.Collections.Generic;
using System.Linq;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ProjectM.Gameplay.Clan;
using static ProjectM.Network.ClanEvents_Client;
using PvpArena.Services;
using PvpArena.Models;
using PvpArena.Data;
using PvpArena.Configs;
using Il2CppSystem;
using Unity.Physics;
using Unity.Jobs;
using UnityEngine.Jobs;
using static ProjectM.ForceJoinClanEventSystem_Server;

namespace PvpArena.Helpers;

//this is horrible god help us all
public static partial class Helper
{
	public static void CreateClanForPlayer(Player player, string name = "")
	{
		var clanRequestEntity = CreateEntityWithComponents<CreateClan_Request, FromCharacter, NetworkEventType, ReceiveNetworkEventTag>();
		if (name == "")
		{
			name = player.Name;
		}
		clanRequestEntity.Write(new CreateClan_Request
		{
			ClanMotto = "",
			ClanName = $"{name}"
		});
		clanRequestEntity.Write(new FromCharacter
		{
			User = player.User,
			Character = player.Character
		});
	}

	public static void AddPlayerToPlayerClanNatural(Player playerJoiningClan, Player playerInClan)
	{
		if (playerInClan.Clan.Exists())
		{
			Entity clanInviteRequestEntity = CreateEntityWithComponents<ClanInviteRequest_Server>();
			clanInviteRequestEntity.Write(new ClanInviteRequest_Server
			{
				FromUser = playerInClan.User,
				ToUser = playerJoiningClan.User,
				ClanEntity = playerInClan.Clan
			});

			Entity clanInviteResponseEntity = CreateEntityWithComponents<ClanInviteResponse, FromCharacter>();
			clanInviteResponseEntity.Write(new ClanInviteResponse
			{
				Response = InviteRequestResponse.Accept,
				ClanId = playerInClan.Clan.Read<NetworkId>()
			});
			clanInviteResponseEntity.Write(playerInClan.ToFromCharacter());
			ClanUtility.SetCharacterClanName(VWorld.Server.EntityManager, playerJoiningClan.User, playerInClan.Clan.Read<ClanTeam>().Name);
			ClanUtility.SetCharacterClanName(VWorld.Server.EntityManager, playerInClan.User, playerInClan.Clan.Read<ClanTeam>().Name);
		}
	}

	public static void AddPlayerToPlayerClanForce(Player playerJoiningClan, Player playerInClan)
	{
		CreateClanForPlayer(playerInClan);
		RemoveFromClan(playerJoiningClan);
		var action = () =>
		{
			try
			{
				var entity = Helper.CreateEntityWithComponents<ForceJoinClanEvents.Request_ByPlayerName, FromCharacter>();
				entity.Write(new ForceJoinClanEvents.Request_ByPlayerName
				{
					Name = playerInClan.Name
				});
				entity.Write(playerJoiningClan.ToFromCharacter());
				if (playerInClan.Clan.Exists())
				{
					ClanUtility.SetCharacterClanName(VWorld.Server.EntityManager, playerJoiningClan.User, playerInClan.Clan.Read<ClanTeam>().Name);
					ClanUtility.SetCharacterClanName(VWorld.Server.EntityManager, playerInClan.User, playerInClan.Clan.Read<ClanTeam>().Name);
				}
			}
			catch (System.Exception e)
			{
				Plugin.PluginLog.LogInfo(e.Message);
			}
		};
		ActionScheduler.RunActionOnceAfterFrames(action, 2);
	}

	public static void RemoveFromClan(this Player player)
	{
		if (player.Clan.Exists())
		{
			var ecb = Core.entityCommandBufferSystem.CreateCommandBuffer();
			Core.clanSystem.LeaveClan(ecb, player.Clan, player.User, ClanSystem_Server.LeaveReason.Leave);
		}
	}


	public static NativeList<Entity> GetPlayerClanMembers(this Player player, bool includeStartingUser = true)
	{
		NativeList<Entity> clanMembers = new NativeList<Entity>(Allocator.Temp);
		TeamUtility.GetAlliedUsers(VWorld.Server.EntityManager, player.User.Read<TeamReference>(), clanMembers);

		return clanMembers;
	}

	//used to spawn npcs in an enemy team
	public static Player GetEnemyPlayer(this Player player)
	{
		foreach (var player2 in PlayerService.UserCache.Values)
		{
			if (!Team.IsAllies(player.Character.Read<Team>(), player2.Character.Read<Team>()) )
			{
				return player2;
			}
		}
		return default;
	}
}
