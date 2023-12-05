using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Bloodstone.API;
using ProjectM.Network;
using PvpArena.Services;
using PvpArena.Models;
using System.Collections.Generic;
using System;

namespace PvpArena.Patches;

[HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
public static class BuildingPermissions
{
	public static Dictionary<Player, bool> AuthorizedBuilders = new Dictionary<Player, bool>();
	public static void Prefix(PlaceTileModelSystem __instance)
	{
		var entities = __instance._DismantleTileQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var fromCharacter = entity.Read<FromCharacter>();
				Player player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				if (!player.IsAdmin && !AuthorizedBuilders.ContainsKey(player))
				{
					player.ReceiveMessage($"You do not have building permissions".Error());
					VWorld.Server.EntityManager.DestroyEntity(entity);
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
				continue;
			}
		}
		entities.Dispose();

		entities = __instance._BuildTileQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var fromCharacter = entity.Read<FromCharacter>();
				Player player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				if (!player.IsAdmin && !AuthorizedBuilders.ContainsKey(player))
				{
					player.ReceiveMessage($"You do not have building permissions".Error());
					VWorld.Server.EntityManager.DestroyEntity(entity);
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
				continue;
			}
		}
		entities.Dispose();

		entities = __instance._MoveTileQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var fromCharacter = entity.Read<FromCharacter>();
				Player player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				if (!player.IsAdmin && !AuthorizedBuilders.ContainsKey(player))
				{
					player.ReceiveMessage($"You do not have building permissions".Error());
					VWorld.Server.EntityManager.DestroyEntity(entity);
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
				continue;
			}
		}
		entities.Dispose();

		entities = __instance._BuildWallpaperQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			try
			{
				var fromCharacter = entity.Read<FromCharacter>();
				Player player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				if (!player.IsAdmin && !AuthorizedBuilders.ContainsKey(player))
				{
					player.ReceiveMessage($"You do not have building permissions".Error());
					VWorld.Server.EntityManager.DestroyEntity(entity);
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
				continue;
			}
		}
		entities.Dispose();
	}
}
