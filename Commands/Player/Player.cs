using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using PvpArena.Data;
using PvpArena.Helpers;
using PvpArena.Services;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.TextCore;

namespace PvpArena.Models;

public class Player
{
	public enum PlayerState
	{
		Normal,
		Imprisoned,
		In1v1Matchmaking,
		InTeamMatchmaking,
		CaptureThePancake,
		Spectating,
		Domination,
		BulletHell,
		Dodgeball,
		Troll,
		PrisonBreak,
		NoHealingLimit,
		Moba,
		Pacified
	}

	private Entity _user;
	private Entity _character;
	private ulong _steamID;

	public Entity User
	{
		get => _user;
		set { SetUser(value); }
	}
	public Entity Character
	{
		get
		{
			if (!_character.Exists())
			{
				RetrieveCharacterFromUser();
			}
			return _character;
		}
		set { SetCharacter(value); }
	}
	public ulong SteamID
	{
		get => _steamID == default && _user != default ? _user.Read<User>().PlatformId : _steamID;
		set => _steamID = value;
	}
	public Entity Clan => GetClan();

	public string Name => GetName();
	public PlayerState CurrentState { get; set; } = PlayerState.Normal;
	public bool IsAdmin => GetIsAdmin();
	public bool IsOnline => GetIsOnline();
	public bool IsAlive => GetIsAlive();
	public Entity ControlledEntity => GetControlledEntity();

	public int MatchmakingTeam { get; set; }

	public float3 Position => GetPosition();
	public PlayerMatchmaking1v1Data MatchmakingData1v1 { get; set; } = new PlayerMatchmaking1v1Data();
	public PlayerPoints PlayerPointsData { get; set; } = new PlayerPoints();
	public PlayerBanInfo BanInfo { get; set; } = new PlayerBanInfo();
	public PlayerMuteInfo MuteInfo { get; set; } = new PlayerMuteInfo();
	public PlayerImprisonInfo ImprisonInfo { get; set; } = new PlayerImprisonInfo();
	public PlayerConfigOptions ConfigOptions { get; set; } = new PlayerConfigOptions();
	public PlayerBulletHellData PlayerBulletHellData { get; set; } = new PlayerBulletHellData();

	private void SetUser(Entity user)
	{
		if (_user != user)
		{
			if (!user.Exists())
			{
				throw new Exception("Invalid User");
			}
			_user = user;

			_steamID = _user.Read<User>().PlatformId;
			AddSteamIdsToPlayerSubData();
			if (!_character.Exists())
			{
				_character = _user.Read<User>().LocalCharacter._Entity;
			}
		}
	}

    private void SetCharacter(Entity character)
    {
        if (character.Exists())
        {
            _character = character;
			if (!_user.Exists())
			{
				if (_character.Read<PlayerCharacter>().UserEntity.Exists())
				{
					_user = _character.Read<PlayerCharacter>().UserEntity;
					_steamID = _user.Read<User>().PlatformId;
					AddSteamIdsToPlayerSubData();
				}
				else
				{
					throw new Exception("Tried to load a player without a valid user");
				}
			}
        }
    }

	private void RetrieveCharacterFromUser()
	{
		if (_user.Exists())
		{
			var userComponent = _user.Read<User>();
			if (userComponent.LocalCharacter._Entity.Exists())
			{
				_character = userComponent.LocalCharacter._Entity;
				PlayerService.CharacterCache[_character] = this;
			}
			else
			{
				_character = default;
			}
		}
	}

	private void AddSteamIdsToPlayerSubData()
	{
		BanInfo.SteamID = SteamID;
		MuteInfo.SteamID = SteamID;
		PlayerPointsData.SteamID = SteamID;
		MatchmakingData1v1.SteamID = SteamID;
		ConfigOptions.SteamID = SteamID;
		PlayerBulletHellData.SteamID = SteamID;
	}

	private string GetName()
	{
		return User.Read<User>().CharacterName.ToString();
	}

	private Entity GetClan()
	{
		return User.Read<User>().ClanEntity._Entity;
	}

	private bool GetIsAdmin()
	{
		return User.Read<User>().IsAdmin || Core.adminAuthSystem._LocalAdminList.Contains(SteamID);
	}

	private bool GetIsOnline()
	{
		if (User.Exists())
		{
			return User.Read<User>().IsConnected;
		}
		else
		{
			return false;
		}
	}

	private float3 GetPosition()
	{
		if (Character.Has<LocalToWorld>())
		{
			return Character.Read<LocalToWorld>().Position;
		}
		else
		{
			return new float3(0, 0, 0);
		}
	}

	private bool GetIsAlive()
	{
		return !Character.Read<Health>().IsDead && !Helper.HasBuff(this, Prefabs.Buff_General_Vampire_Wounded_Buff);
	}

	public Entity GetControlledEntity()
	{
		return User.Read<Controller>().Controlled._Entity;
	}

	public bool IsEligibleForMatchmaking()
	{
		return CurrentState == PlayerState.Normal;
	}

	public bool IsIn1v1()
	{
		return CurrentState == PlayerState.In1v1Matchmaking;
	}

	public bool IsInDomination()
	{
		return CurrentState == PlayerState.Domination;
	}

	public bool IsInMatch()
	{
		return CurrentState == PlayerState.In1v1Matchmaking || CurrentState == PlayerState.InTeamMatchmaking || CurrentState == PlayerState.CaptureThePancake;
	}

	public bool IsInTeamMatch()
	{
		return CurrentState == PlayerState.InTeamMatchmaking;
	}

	public bool IsInCaptureThePancake()
	{
		return CurrentState == PlayerState.CaptureThePancake;
	}

	public bool IsInDefaultMode()
	{
		return CurrentState == PlayerState.Normal;
	}

	public bool IsSpectating()
	{
		return CurrentState == PlayerState.Spectating;
	}

	public bool IsInBulletHell()
	{
		return CurrentState == PlayerState.BulletHell;
	}

	public bool IsImprisoned()
	{
		return CurrentState == PlayerState.Imprisoned;
	}

	public bool IsInDodgeball()
	{
		return CurrentState == PlayerState.Dodgeball;
	}

	public bool IsTroll()
	{
		return CurrentState == PlayerState.Troll;
	}

	public void ReceiveMessage(string message)
	{
		ServerChatUtils.SendSystemMessageToClient(
			VWorld.Server.EntityManager,
			User.Read<User>(),
			message
		);
	}

	public FromCharacter ToFromCharacter()
	{
		return new FromCharacter
		{
			Character = this.Character,
			User = this.User
		};
	}

	public List<Player> GetClanMembers()
	{
		List<Player> clanPlayers = new List<Player>();
		if (Clan.Exists())
		{
			NativeList<Entity> entities = new NativeList<Entity>(Allocator.Temp);
			TeamUtility.GetClanMembers(VWorld.Server.EntityManager, Clan, entities);
			foreach (var entity in entities)
			{
				if (entity.Has<User>())
				{
					Player player = PlayerService.GetPlayerFromUser(entity);
					clanPlayers.Add(player);
				}
			}
		}
		else
		{
			clanPlayers.Add(this);
		}

		return clanPlayers;
	}

	public bool IsAlliedWith(Player player)
	{
		return Team.IsAllies(Character.Read<Team>(), player.Character.Read<Team>());
	}

	public bool HasControlledEntity()
	{
		if (ControlledEntity == Character)
		{
			return true;
		}
		else
		{
			bool isDead;
			if (ControlledEntity.Has<Health>())
			{
				isDead = ControlledEntity.Read<Health>().IsDead;
			} 
			else
			{
				isDead = true;
			}
			if (isDead)
			{
				return false;
			}
			return ((ControlledEntity.Exists() && ControlledEntity.Has<PrefabGUID>()));
		}
	}

	public override int GetHashCode()
	{
		return SteamID.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		Player other = (Player)obj;
		return SteamID == other.SteamID;
	}

}
