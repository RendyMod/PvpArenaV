using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using ProjectM;
using PvpArena.Frameworks.CommandFramework;
using PvpArena.GameModes.BulletHell;
using PvpArena.Models;
using PvpArena.Services;

namespace PvpArena;

public static class DiscordBot
{
	public static DiscordSocketClient _client;

	public static async void InitializeAsync()
	{
		try
		{
			_client = new DiscordSocketClient();
			_client.LoginAsync(TokenType.Bot, DiscordBotConfig.Config.Token);
			_client.StartAsync();
			PlayerService.OnOnlinePlayerAmountChanged += UpdatePlayerCountStatus;
		}
		catch (Exception ex)
		{
			Unity.Debug.Log(ex.ToString());
		}
	}

	public static async void UpdatePlayerCountStatus ()
	{
		await _client.SetActivityAsync(new Game("Online: " + PlayerService.OnlinePlayers.Count + "/" + Core.serverBootstrapSystem.ServerHostData.ServerMaxConnectedUsers, ActivityType.Watching));
	}

	[CommandFramework.Command("test-bot-msg", description: "Used for debugging", adminOnly: true)]
	public static void BotMsgCommand (Player sender, string message)
	{
		SendMessageAsync(message);
	}

	[CommandFramework.Command("test-bullet-embed", description: "Used for debugging", adminOnly: true)]
	public static void Blabla (Player sender)
	{
		DiscordBot.SendEmbedAsync(DiscordBotConfig.Config.JailChannel,
			BulletHellManager.EmbedBulletAnnouncement("Rendy", (15.6f).ToString("F2"), "Ash le noob",
				(66.6f).ToString("F2")));
	}
	
	[CommandFramework.Command("test-bot-embed", description: "Used for debugging", adminOnly: true)]
	public static void BotEmbedMsgCommand (Player sender, string message, string description)
	{
		SendEmbedAsync(message, description);
	}
	
	public static async void SendMessageAsync(string message)
	{
		ulong channelId = DiscordBotConfig.Config.JailChannel; // Replace with your channel ID
		var channel = _client.GetChannel(channelId) as IMessageChannel;
		if (channel != null)
		{
			await channel.SendMessageAsync(message);
		}
	}
	
	public static async void SendEmbedAsync (ulong discordChannel, Embed _embed)
	{
		ulong channelId = DiscordBotConfig.Config.JailChannel; // Replace with your channel ID
		var channel = _client.GetChannel(channelId) as IMessageChannel;
		if (channel != null)
		{
			// Sending the embed message
			await channel.SendMessageAsync(embed : _embed);
		}
	}
	
	public static async void SendEmbedAsync (string title, string message)
	{
		ulong channelId = DiscordBotConfig.Config.JailChannel; // Replace with your channel ID
		var channel = _client.GetChannel(channelId) as IMessageChannel;
		if (channel != null)
		{
			var embedBuilder = new EmbedBuilder
			{
				Title = title,
				Description = message,
				Color = Color.DarkRed // You can set the color of the embed
			};

			// Sending the embed message
			await channel.SendMessageAsync(embed: embedBuilder.Build());
		}
	}
}

