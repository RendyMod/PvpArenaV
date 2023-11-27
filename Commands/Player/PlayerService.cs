using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using PvpArena.Helpers;
using PvpArena.Models;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Jobs;

namespace PvpArena.Services
{
	public static class PlayerService
    {
		public static readonly Dictionary<Entity, Player> UserCache = new Dictionary<Entity, Player>();
		public static readonly Dictionary<Entity, Player> CharacterCache = new Dictionary<Entity, Player>();
		public static readonly Dictionary<ulong, Player> SteamIdCache = new Dictionary<ulong, Player>();
		public static readonly Dictionary<Player, bool> OnlinePlayers = new Dictionary<Player, bool>();

		public abstract class PlayerData
		{
			public abstract ulong SteamID { get; set; }
		}
		public static void UpdatePlayerCache<T>(List<T> dataList) where T : PlayerData
		{
			foreach (var data in dataList)
			{
				var player = GetPlayerFromSteamId(data.SteamID);
				if (player == null)
					continue; // Or handle the case where the player is not found

				// Directly set the properties without using 'as T'
				if (typeof(T) == typeof(PlayerBanInfo))
				{
					player.BanInfo = (PlayerBanInfo)(object)data;
				}
				else if (typeof(T) == typeof(PlayerMuteInfo))
				{
					player.MuteInfo = (PlayerMuteInfo)(object)data;
				}
				else if (typeof(T) == typeof(PlayerPoints))
				{
					player.PlayerPointsData = (PlayerPoints)(object)data;
				}
				else if (typeof(T) == typeof(PlayerMatchmaking1v1Data))
				{
					player.MatchmakingData1v1 = (PlayerMatchmaking1v1Data)(object)data;
				}
				else if (typeof(T) == typeof(PlayerConfigOptions))
				{
					player.ConfigOptions = (PlayerConfigOptions)(object)data;
				}
				// Add more conditions for other types as necessary
			}
		}

		public static List<T> GetPlayerSubData<T>() where T : PlayerData
		{
			var dataList = new List<T>();

			foreach (var player in UserCache.Values)
			{
				if (typeof(T) == typeof(PlayerBanInfo) && player.BanInfo is T banInfo)
				{
					if (player.BanInfo.IsBanned())
						dataList.Add(banInfo);
				}
				else if (typeof(T) == typeof(PlayerMuteInfo) && player.MuteInfo is T muteInfo)
				{
					if (player.MuteInfo.IsMuted())
						dataList.Add(muteInfo);
				}
				else if (typeof(T) == typeof(PlayerPoints) && player.PlayerPointsData is T loginPoints)
				{
					dataList.Add(loginPoints);
				}
				else if (typeof(T) == typeof(PlayerMatchmaking1v1Data) && player.MatchmakingData1v1 is T matchmakingData1v1)
				{
					dataList.Add(matchmakingData1v1);
				}
				else if (typeof(T) == typeof(PlayerConfigOptions) && player.ConfigOptions is T configOptions)
				{
					dataList.Add(configOptions);
				}
			}

			return dataList;
		}

		public static Player GetAnyEnemyPlayer(Player player)
		{
			foreach (var enemyPlayer in UserCache.Values)
			{
				if (!Team.IsAllies(player.User.Read<Team>(), enemyPlayer.User.Read<Team>()))
				{
					return enemyPlayer;
				}
			}
			return default;
		}

		public static void LoadAllPlayers()
		{
			var users = Helper.GetEntitiesByComponentTypes<User>(true);
			foreach (var user in users)
			{
				try
				{
					var player = GetPlayerFromUser(user); //fill cache
					if (player.IsOnline)
					{
						OnlinePlayers.TryAdd(player, true);
					}
				}
				catch (Exception e)
				{

				}
			}
		}

		public static Player GetPlayerFromUser(Entity User)
		{
			Player player;
			if (UserCache.ContainsKey(User))
			{
				player = UserCache[User];
			}
			else
			{
				player = new Player
				{
					User = User
				};
				UserCache[User] = player;
				CharacterCache[player.Character] = player;
				SteamIdCache[player.SteamID] = player;
			}
			return player;
		}

		public static Player GetPlayerFromCharacter(Entity Character)
		{
			Player player;
			if (CharacterCache.ContainsKey(Character))
			{
				player = CharacterCache[Character];
			}
			else
			{
				player = new Player
				{
					Character = Character
				};
				CharacterCache[Character] = player;
				UserCache[player.User] = player;
				SteamIdCache[player.SteamID] = player;
			}
			
			return player;
		}

		public static Player GetPlayerFromSteamId(ulong SteamId)
		{
			Player player;
			if (SteamIdCache.ContainsKey(SteamId))
			{
				player = SteamIdCache[SteamId];
			}
			else
			{
				TryGetPlayerFromString(SteamId.ToString(), out player);
			}
			if (player == null)
			{
				player = new Player();
			}
			return player;
		}

		public static bool TryGetPlayerFromString(string input, out Player player)
		{
			var userEntities = Helper.GetEntitiesByComponentTypes<User>(true);
			foreach (var userEntity in userEntities)
			{
				var user = userEntity.Read<User>();
				if (user.CharacterName.ToString().ToLower() == input.ToLower() || (ulong.TryParse(input, out ulong platformID) && user.PlatformId == platformID))
				{
					if (UserCache.TryGetValue(userEntity, out player))
					{
						return true;
					}
					else
					{
						player = new Player
						{
							User = userEntity,
						};
						UserCache[userEntity] = player;
						CharacterCache[player.Character] = player;
						SteamIdCache[player.SteamID] = player;
						return true;
					}
				}
			}
			player = default;
			return false;
		}

	}
	
}
