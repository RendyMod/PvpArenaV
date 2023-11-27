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

namespace PvpArena.Helpers;

//this is horrible god help us all
public static partial class Helper
{
	public static void CreateClanForPlayer(Entity User)
	{
		var clanRequestEntity = CreateEntityWithComponents<CreateClan_Request, FromCharacter, NetworkEventType, ReceiveNetworkEventTag>();
        clanRequestEntity.Write(new CreateClan_Request
        {
            ClanMotto = "",
            ClanName = $"{User.Read<User>().CharacterName}"
        });
		clanRequestEntity.Write(new FromCharacter
		{
			User = User,
			Character = User.Read<User>().LocalCharacter._Entity
		});
	}

	public static void AddPlayerToPlayerClan(Entity UserJoiningClan, Entity UserInClan)
	{
		var user = UserJoiningClan.Read<User>();
		var clanEntity = UserInClan.Read<User>().ClanEntity._Entity;
		if (clanEntity.Index > 0)
		{
			Entity clanInviteRequestEntity = CreateEntityWithComponents<ClanInviteRequest_Server>();
			clanInviteRequestEntity.Write(new ClanInviteRequest_Server
			{
				FromUser = UserInClan,
				ToUser = UserJoiningClan,
				ClanEntity = clanEntity
			});

			Entity clanInviteResponseEntity = CreateEntityWithComponents<ClanInviteResponse, FromCharacter>();
			clanInviteResponseEntity.Write(new ClanInviteResponse
			{
				Response = InviteRequestResponse.Accept,
				ClanId = clanEntity.Read<NetworkId>()
			});
			clanInviteResponseEntity.Write(new FromCharacter
			{
				User = UserJoiningClan,
				Character = UserInClan.Read<User>().LocalCharacter._Entity
			});
			ClanUtility.SetCharacterClanName(VWorld.Server.EntityManager, UserJoiningClan, clanEntity.Read<ClanTeam>().Name);
			ClanUtility.SetCharacterClanName(VWorld.Server.EntityManager, UserInClan, clanEntity.Read<ClanTeam>().Name);
		}
	}

	public static void RemoveFromClan(this Player player)
	{
		var ecb = Core.entityCommandBufferSystem.CreateCommandBuffer();
		var clanSystem = VWorld.Server.GetExistingSystem<ClanSystem_Server>();
		var clanEntity = player.User.Read<User>().ClanEntity._Entity;
		if (clanEntity.Index > 0)
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
