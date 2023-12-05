using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Bloodstone.API;
using System.Collections.Generic;
using ProjectM.Network;
using PvpArena.Services;
using System.Linq;
using PvpArena.Models;
using System;

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
			try
			{
				var fromCharacter = entity.Read<FromCharacter>();
				var Player = PlayerService.GetPlayerFromUser(fromCharacter.User);
				var purchaseEvent = entity.Read<TraderPurchaseEvent>();
				Entity trader = VWorld.Server.GetExistingSystem<NetworkIdSystem>()._NetworkIdToEntityMap[purchaseEvent.Trader];

				var costBuffer = trader.ReadBuffer<TradeCost>();
				var cost = -1 * (costBuffer[purchaseEvent.ItemIndex].Amount);
				if (Player.PlayerPointsData.TotalPoints >= cost)
				{
					Player.PlayerPointsData.TotalPoints -= cost;
					Core.pointsDataRepository.SaveDataAsync(new List<PlayerPoints> { Player.PlayerPointsData });
					Player.ReceiveMessage($"Purchased for {cost.ToString().Emphasize()} {"VPoints".Warning()}. New total points: {Player.PlayerPointsData.TotalPoints.ToString().Warning()}".Success());

					RefillStock(purchaseEvent, trader);
				}
				else
				{
					VWorld.Server.EntityManager.DestroyEntity(entity);
					Player.ReceiveMessage($"Not enough {"VPoints".Warning()} to purchase! {Player.PlayerPointsData.TotalPoints.ToString().Warning()} / {cost}".Error());
				}
			}
			catch (Exception e)
			{
				Plugin.PluginLog.LogInfo(e.ToString());
			}
		}
	}

	private static void RefillStock(TraderPurchaseEvent purchaseEvent, Entity trader)
	{
		var _entryBuffer = trader.ReadBuffer<TraderEntry>();
		var _inputBuffer = trader.ReadBuffer<TradeCost>();
		var _outputBuffer = trader.ReadBuffer<TradeOutput>();

		for (int i = 0; i < _entryBuffer.Length; i++)
		{

			TraderEntry _newEntry = _entryBuffer[i];
			if (purchaseEvent.ItemIndex == _newEntry.OutputStartIndex)
			{
				PrefabGUID _outputItem = _outputBuffer[i].Item;
				PrefabGUID _inputItem = _inputBuffer[i].Item;


				foreach (var traderConfig in TradersConfig.Config.Traders)
				{
					var item = traderConfig.TraderItems.Where(x => x.InputItem == _inputItem && x.OutputItem == _outputItem).FirstOrDefault();
					if (item != null && item.AutoRefill)
					{
						_newEntry.StockAmount = item.StockAmount + 1;
						_entryBuffer[i] = _newEntry;
					}
				}
			}
		}
	}
}

