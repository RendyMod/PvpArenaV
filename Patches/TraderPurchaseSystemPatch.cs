using PvpArena.Commands;
using HarmonyLib;
using Il2CppSystem;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Bloodstone.API;
using PvpArena.Data;
using ProjectM.Gameplay.Systems;
using System.Collections.Generic;
using ProjectM.Network;
using Stunlock.Network;
using PvpArena.Matchmaking;
using PvpArena.Services;
using UnityEngine;

namespace PvpArena.Patches;


[HarmonyPatch(typeof(TraderPurchaseSystem), nameof(TraderPurchaseSystem.OnUpdate))]
[HarmonyBefore("BloodyMerchant")]
public static class TraderPurchaseSystemPatch
{
	public static void Prefix(TraderPurchaseSystem __instance)
	{
		var entities = __instance._TraderPurchaseEventQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var fromCharacter = entity.Read<FromCharacter>();
			var Player = PlayerService.GetPlayerFromUser(fromCharacter.User);
			var purchaseEvent = entity.Read<TraderPurchaseEvent>();
			Entity trader = VWorld.Server.GetExistingSystem<NetworkIdSystem>()._NetworkIdToEntityMap[purchaseEvent.Trader];

			var costBuffer = trader.ReadBuffer<TradeCost>();
			var cost = -1*(costBuffer[purchaseEvent.ItemIndex].Amount);
			if (Player.PlayerPointsData.TotalPoints >= cost)
			{
				Player.PlayerPointsData.TotalPoints -= cost;
				Player.ReceiveMessage($"Purchased for {cost} VPoints. New total points: {Player.PlayerPointsData.TotalPoints}");
			}
			else
			{
				VWorld.Server.EntityManager.DestroyEntity(entity);
				Player.ReceiveMessage($"Not enough VPoints to purchase! {Player.PlayerPointsData.TotalPoints} / {cost}");
			}
		}
	}
}

