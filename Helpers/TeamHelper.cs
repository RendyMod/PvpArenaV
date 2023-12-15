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
		var user = playerJoiningClan.User.Read<User>();
		var clanEntity = playerInClan.User.Read<User>().ClanEntity._Entity;
		if (clanEntity.Exists())
		{
			Entity clanInviteRequestEntity = CreateEntityWithComponents<ClanInviteRequest_Server>();
			clanInviteRequestEntity.Write(new ClanInviteRequest_Server
			{
				FromUser = playerInClan.User,
				ToUser = playerJoiningClan.User,
				ClanEntity = clanEntity
			});

			Entity clanInviteResponseEntity = CreateEntityWithComponents<ClanInviteResponse, FromCharacter>();
			clanInviteResponseEntity.Write(new ClanInviteResponse
			{
				Response = InviteRequestResponse.Accept,
				ClanId = clanEntity.Read<NetworkId>()
			});
			clanInviteResponseEntity.Write(playerInClan.ToFromCharacter());
			ClanUtility.SetCharacterClanName(VWorld.Server.EntityManager, playerJoiningClan.User, clanEntity.Read<ClanTeam>().Name);
			ClanUtility.SetCharacterClanName(VWorld.Server.EntityManager, playerInClan.User, clanEntity.Read<ClanTeam>().Name);
		}
	}

	public static void AddPlayerToPlayerClanForce(Player playerJoiningClan, Player playerInClan)
	{
		RemoveFromClan(playerJoiningClan);
		if (!playerInClan.User.Read<User>().ClanEntity._Entity.Exists())
		{
			var action = () => CreateClanForPlayer(playerInClan);
			ActionScheduler.RunActionOnceAfterFrames(action, 2);
		}
		TryJoinClanArgs tryJoinClanArgs = new TryJoinClanArgs
		{
			Ecb = Core.entityCommandBufferSystem.CreateCommandBuffer(),
			EntityManager = VWorld.Server.EntityManager,
			FromUser = playerJoiningClan.User,
			TargetUser = playerInClan.User
		};
		try
		{
			var forceJoinClanAction = () => { ForceJoinClanEventSystem_Server.TryJoinClan(ref tryJoinClanArgs); };
			ActionScheduler.RunActionOnceAfterFrames(forceJoinClanAction, 3);
		}
		catch
		{

		}
	}

	public static void RemoveFromClan(this Player player)
	{
		var ecb = Core.entityCommandBufferSystem.CreateCommandBuffer();
		var clanSystem = VWorld.Server.GetExistingSystem<ClanSystem_Server>();
		var clanEntity = player.User.Read<User>().ClanEntity._Entity;
		if (clanEntity.Exists())
		{
			clanSystem.LeaveClan(ecb, clanEntity, player.User, ClanSystem_Server.LeaveReason.Leave);
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
