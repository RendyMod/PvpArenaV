using HarmonyLib;
using System;
using BepInEx.Core.Logging.Interpolation;
using BepInEx.Logging;
using System.Collections.Generic;

namespace PvpArena.Patches;

//bepinex log patch
[HarmonyPatch(typeof(ManualLogSource), nameof(ManualLogSource.Log), new Type[] { typeof(LogLevel), typeof(BepInExLogInterpolatedStringHandler) })]
public static class ManualLogSourcePatch
{

	public static bool Prefix(LogLevel level, BepInExLogInterpolatedStringHandler logHandler)
	{

		var message = logHandler.ToString();
		if (message.Contains("[Chat] [Team] ") || message.Contains("[Chat] [Whisper] "))
		{
			return false;
		}

		return true;
	}
}

[HarmonyPatch(typeof(UnityEngine.Debug), nameof(UnityEngine.Debug.Log), new Type[] { typeof(Il2CppSystem.Object) })]
public class LogPatch
{
	private static List<string> messagesToIgnores = new List<string>
	{
		"CommandBuffer error",
		"InvalidOperationException",
		"Tried to spawn Unity.Entities.Entity in Neutral team"
	};
	public static bool Prefix(Il2CppSystem.Object message)
	{
		try
		{
			foreach (var messageToIgnore in messagesToIgnores)
			{
				if (message.ToString().Contains(messageToIgnore))
				{
					return false;
				}
			}
		}
		catch
		{

		}

		return true;
	}
}

//this is only being triggered for some error messages, need to find another patch
[HarmonyPatch(typeof(UnityEngine.Debug), nameof(UnityEngine.Debug.LogError), new Type[] { typeof(Il2CppSystem.Object) })]
public class LogPatch2
{
	private static List<string> messagesToIgnores = new List<string>
	{
		"CommandBuffer error",
		"InvalidOperationException",
		"Tried to spawn Unity.Entities.Entity in Neutral team"
	};
	public static bool Prefix(Il2CppSystem.Object message)
	{
		try
		{
			foreach (var messageToIgnore in messagesToIgnores)
			{
				if (message.ToString().Contains(messageToIgnore))
				{
					return false;
				}
			}
		}
		catch
		{

		}

		return true;
	}
}
