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
using Unity.Entities;
using UnityEngine;

namespace PvpArena.Services;

[HarmonyPatch(typeof(ServerTimeSystem_Server), nameof(ServerTimeSystem_Server.OnUpdate))]
public static class ActionScheduler
{
	public static int CurrentFrameCount = 0;
	public static ConcurrentQueue<Action> actionsToExecuteOnMainThread = new ConcurrentQueue<Action>();
	private static List<ScheduledAction> scheduledActions = new List<ScheduledAction>();
	private static List<Timer> activeTimers = new List<Timer>();
	
	public static void Postfix()
	{
		CurrentFrameCount++;
		// Execute scheduled actions for the current frame
		for (int i = scheduledActions.Count - 1; i >= 0; i--)
		{
			if (scheduledActions[i].ScheduledFrame <= CurrentFrameCount)
			{
				scheduledActions[i].Execute();
				scheduledActions.RemoveAt(i);
			}
		}

		while (actionsToExecuteOnMainThread.TryDequeue(out Action action))
		{
			action?.Invoke();
		}
	}

	//deprecate this
	public static void ScheduleAction(ScheduledAction action, int frameDelay)
	{
		action.ScheduledFrame = CurrentFrameCount + frameDelay;
		scheduledActions.Add(action);
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
					timer?.Dispose(); // Dispose of the timer after the action is executed
				});
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

	public static List<ScheduledAction> GetScheduledActions()
	{
		return scheduledActions;
	}
}

public class ScheduledAction
{
	public int ScheduledFrame { get; set; }
	public Delegate Callback { get; set; }
	public object[] CallbackArgs { get; set; }


	public ScheduledAction(Delegate callback, params object[] callbackArgs)
	{
		Callback = callback;
		CallbackArgs = callbackArgs;
	}

	public void Execute()
	{
		try
        {
			Callback?.DynamicInvoke(CallbackArgs);
		}
		catch (Exception e)
        {
			Unity.Debug.Log(e.ToString());
        }
	}
}
