using System;
using PvpArena.Matchmaking;
using Unity.Mathematics;
using System.Text.Json.Serialization;
using static PvpArena.Services.PlayerService;
using UnityEngine;
using System.Collections;
using System.Threading;
using System.Text;
using PvpArena.Persistence;
using System.ComponentModel.DataAnnotations.Schema;
using PvpArena.Configs;
using PvpArena.Services.Moderation;

namespace PvpArena.Models;

public class PlayerBulletHellData : PlayerData
{
	public override ulong SteamID { get; set; }
	public string BestTime { get; set; } = "0";

	public PlayerBulletHellData()
	{

	}
}

public class PlayerMatchmaking1v1Data : PlayerData
{	
	public override ulong SteamID { get; set; }	
	public int Wins { get; set; } = 0;
	public int Losses { get; set; } = 0;
	public int MMR { get; set; } = MmrCalculator.StartingMmr;

	[JsonIgnore]
	public float3 ReturnLocation { get; set; } = PvpArenaConfig.Config.CustomSpawnLocation.ToFloat3();
	[JsonIgnore]
	public Arena CurrentArena { get; set; }
	[JsonIgnore]
	public bool AutoRequeue { get; set; } = false;

	public PlayerMatchmaking1v1Data()
	{

	}
}

public class PlayerConfigOptions : PlayerData
{
	public override ulong SteamID { get; set; }
	public bool SubscribeToKillFeed { get; set; } = true;
	public PlayerConfigOptions() { }
}

public class PlayerPoints : PlayerData
{
	public override ulong SteamID { get; set; }
	public int TotalPoints { get; set; }
	[JsonIgnore]
	public Timer OnlineTimer { get; set; }

	public PlayerPoints() { }
}

public class PlayerBanInfo : PlayerData
{
	public override ulong SteamID { get; set; }
	public DateTime BannedDate { get; set; }
	public int BanDurationDays { get; set; } // Use -1 for permanent bans
	public string Reason { get; set; } // Optional: The reason for the ban

	public PlayerBanInfo() { }

	public DateTime? GetBanExpirationDate()
	{
		if (BanDurationDays == -1)
		{
			return null; // No expiration date
		}
		return BannedDate.AddDays(BanDurationDays);
	}

	private bool IsBanExpired()
	{
		// If duration is -1, the mute never expires
		if (BanDurationDays == -1)
		{
			return false;
		}
		// Otherwise, compare with the current date to see if the mute has expired
		return DateTime.UtcNow >= GetBanExpirationDate();
	}

	public bool IsBanned()
	{
		if (SteamID == 0 || BannedDate == DateTime.MinValue)
		{
			return false;
		}
		if (IsBanExpired())
		{
			BanService.UnbanPlayer(SteamID);
			return false;
		}
		return true;
	}
}

public class PlayerImprisonInfo : PlayerData
{
	public override ulong SteamID { get; set; }
	public DateTime ImprisonedDate { get; set; }
	public int ImprisonDurationDays { get; set; } // Use -1 for permanent bans
	public string Reason { get; set; } // Optional: The reason for the ban
	public int PrisonCellNumber { get; set; }

	public PlayerImprisonInfo() { }

	public DateTime? GetImprisonExpirationDate()
	{
		if (ImprisonDurationDays == -1)
		{
			return null; // No expiration date
		}
		return ImprisonedDate.AddDays(ImprisonDurationDays);
	}

	private bool IsImprisonExpired()
	{
		// If duration is -1, the mute never expires
		if (ImprisonDurationDays == -1)
		{
			return false;
		}
		// Otherwise, compare with the current date to see if the mute has expired
		return DateTime.UtcNow >= GetImprisonExpirationDate();
	}

	public bool IsImprisoned()
	{
		if (SteamID == 0 || ImprisonedDate == DateTime.MinValue)
		{
			return false;
		}
		if (IsImprisonExpired())
		{
			ImprisonService.UnimprisonPlayer(SteamID);
			return false;
		}
		return true;
	}
}

public class PlayerMuteInfo : PlayerData
{
	public override ulong SteamID { get; set; }
	public DateTime MutedDate { get; set; }
	public int MuteDurationDays { get; set; }

	public PlayerMuteInfo()
	{
	}

	public PlayerMuteInfo(ulong steamID, DateTime mutedDate, int muteDurationDays)
	{
		SteamID = steamID;
		MutedDate = mutedDate;
		MuteDurationDays = muteDurationDays;
	}

	public DateTime? GetMuteExpirationDate()
	{
		if (MuteDurationDays == -1)
		{
			return null; // No expiration date
		}
		return MutedDate.AddDays(MuteDurationDays);
	}

	private bool IsMuteExpired()
	{
		// If duration is -1, the mute never expires
		if (MuteDurationDays == -1)
		{
			return false;
		}
		return DateTime.UtcNow >= GetMuteExpirationDate();
	}

	// Side effect of unmuting if expired
	public bool IsMuted()
	{
		if (SteamID == 0 || MutedDate == DateTime.MinValue)
		{
			return false;
		}
		if (IsMuteExpired())
		{
			MuteService.UnmutePlayer(SteamID);
			return false;
		}
		return true;
	}

	public TimeSpan? GetRemainingMuteTime()
	{
		if (MuteDurationDays == -1)
		{
			return null; // Mute never expires
		}

		DateTime? expirationDate = GetMuteExpirationDate();
		if (!expirationDate.HasValue)
		{
			return null; // No expiration date set
		}

		TimeSpan remainingTime = expirationDate.Value - DateTime.UtcNow;
		if (remainingTime < TimeSpan.Zero)
		{
			return TimeSpan.Zero; // Mute has already expired
		}

		return remainingTime;
	}

	public string GetFormattedRemainingMuteTime()
	{
		var remainingTime = GetRemainingMuteTime();

		StringBuilder formattedTime = new StringBuilder();
		if (remainingTime.Value.Days > 0)
		{
			formattedTime.Append($"{remainingTime.Value.Days} day(s) ");
		}
		if (remainingTime.Value.Hours > 0)
		{
			formattedTime.Append($"{remainingTime.Value.Hours} hour(s) ");
		}
		if (remainingTime.Value.Minutes > 0)
		{
			formattedTime.Append($"{remainingTime.Value.Minutes} minute(s)");
		}

		return formattedTime.ToString().Trim();
	}
}

