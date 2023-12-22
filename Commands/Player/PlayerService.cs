using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using PvpArena.Helpers;
using PvpArena.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
		public static readonly HashSet<Player> OnlinePlayers = new();
		public static Action OnOnlinePlayerAmountChanged;

		public abstract class PlayerData
		{
			public abstract ulong SteamID { get; set; }
		}

		public static void Initialize()
		{
			LoadAllPlayers();
		}

		public static void Dispose()
		{
			UserCache.Clear();
			CharacterCache.Clear();
			SteamIdCache.Clear();
			OnlinePlayers.Clear();
		}

		public static Player GetAnyPlayer()
		{
			if (OnlinePlayers.Count > 0)
			{
				return OnlinePlayers.FirstOrDefault();
			}
			else
			{
				return UserCache.Values.FirstOrDefault();
			}
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
					var userData = user.Read<User>();
					if (userData.LocalCharacter._Entity.Exists() && userData.CharacterName.ToString() != "")
					{
						var player = GetPlayerFromUser(user); //fill cache
						if (player.IsOnline)
						{
							OnlinePlayers.Add(player);
						}
					}
				}
				catch (Exception e)
				{

				}
			}
			OnOnlinePlayerAmountChanged?.Invoke();
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
				if (User.Exists())
				{
					player = new Player
					{
						User = User
					};
					UserCache[User] = player;
					if (player.Character.Exists())
					{
						CharacterCache[player.Character] = player;
					}
					SteamIdCache[player.SteamID] = player;
				}
				else
				{
					throw new Exception("Tried to create a player from a non-existent user");
				}
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
				if (Character.Exists())
				{
					CharacterCache[Character] = player;
				}
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
				if ((ulong.TryParse(input, out ulong platformID) && user.PlatformId == platformID) || user.CharacterName.ToString().ToLower() == input.ToLower())
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
						if (player.Character.Exists())
						{
							CharacterCache[player.Character] = player;
						}
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
