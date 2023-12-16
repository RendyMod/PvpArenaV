using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Bloodstone.API;
using HarmonyLib;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using PvpArena.Commands.Debug;
using PvpArena.Data;
using PvpArena.GameModes;
using PvpArena.Helpers;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace PvpArena.Services;

[HarmonyPatch(typeof(ServerTimeSystem_Server), nameof(ServerTimeSystem_Server.OnUpdate))]
public static class ActionScheduler
{
	public static int CurrentFrameCount = 0;
	public static ConcurrentQueue<Action> actionsToExecuteOnMainThread = new ConcurrentQueue<Action>();
	private static List<Timer> activeTimers = new List<Timer>();
	
	public static void Postfix()
	{
		GameEvents.RaiseGameFrameUpdate();
		
		if (CurrentFrameCount % 30 == 0) //move this into game mode actions later
		{
			foreach (var player in PlayerService.OnlinePlayers.Keys)
			{
				if (!player.HasControlledEntity())
				{
					GameEvents.RaisePlayerHasNoControlledEntity(player);
					Helper.ControlOriginalCharacter(player);
					Helper.RemoveBuff(player, Prefabs.Admin_Observe_Invisible_Buff);
				}
			}
		}
		
		CurrentFrameCount++;

		while (actionsToExecuteOnMainThread.TryDequeue(out Action action))
		{
			action?.Invoke();
		}
	}

	public static Timer RunActionEveryInterval(Action action, double intervalInSeconds)
	{
		return new Timer(_ =>
		{
			actionsToExecuteOnMainThread.Enqueue(action);
		}, null, TimeSpan.FromSeconds(intervalInSeconds), TimeSpan.FromSeconds(intervalInSeconds));
	}

	public static Timer RunActionOnceAfterDelay(Action action, double delayInSeconds)
	{
		Timer timer = null;

		timer = new Timer(_ =>
		{
			// Enqueue the action to be executed on the main thread
			actionsToExecuteOnMainThread.Enqueue(() =>
			{
				action.Invoke();  // Execute the action
				timer?.Dispose(); // Dispose of the timer after the action is executed
			});
		}, null, TimeSpan.FromSeconds(delayInSeconds), Timeout.InfiniteTimeSpan); // Prevent periodic signaling

		return timer;
	}

	public static Timer RunActionOnceAfterFrames(Action action, int frameDelay)
	{
		int startFrame = CurrentFrameCount;
		Timer timer = null;

		timer = new Timer(_ =>
		{
			if (CurrentFrameCount - startFrame >= frameDelay)
			{
				// Enqueue the action to be executed on the main thread
				actionsToExecuteOnMainThread.Enqueue(() =>
				{
					action.Invoke();  // Execute the action
				});
				timer?.Dispose();
			}
		}, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(8));

		return timer;
	}

	public static void RunActionOnMainThread(Action action)
	{
		// Enqueue the action to be executed on the main thread
		actionsToExecuteOnMainThread.Enqueue(() =>
		{
			action.Invoke();  // Execute the action
		});
	}
}
