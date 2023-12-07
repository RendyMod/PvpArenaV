using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bloodstone.API;
using Discord;
using Discord.WebSocket;
using ProjectM;
using PvpArena.Frameworks.CommandFramework;
using PvpArena.GameModes.BulletHell;
using PvpArena.Models;
using PvpArena.Services;

namespace PvpArena;

public static partial class DiscordBot
{
	public static DiscordSocketClient _client;
	public static SocketGuild _socketGuild;
	
	public static async Task InitializeAsync()
	{
		try
		{
			_client = new DiscordSocketClient(new DiscordSocketConfig
			{
				GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
			});

			await _client.LoginAsync(TokenType.Bot, DiscordBotConfig.Config.Token);
			await _client.StartAsync();

			_client.Ready += () =>
			{
				_client.MessageReceived += DiscordGlobalLinkSystem.HandleMessageReceivedAsync;
				_socketGuild = _client.GetGuild(DiscordBotConfig.Config.GuildID);
				return Task.CompletedTask;
			};
			PlayerService.OnOnlinePlayerAmountChanged += UpdatePlayerCountStatus;
		}
		catch (Exception ex)
		{
			Unity.Debug.Log(ex.ToString());
		}
	}

	public static async void Dispose()
	{
		if (_client != null)
		{
			PlayerService.OnOnlinePlayerAmountChanged -= UpdatePlayerCountStatus;
			_client.LogoutAsync();
			_client.StopAsync();
			_client.Dispose();
			_client.MessageReceived -= DiscordGlobalLinkSystem.HandleMessageReceivedAsync;
			_client = null;
		}
	}

	public static async void UpdatePlayerCountStatus ()
	{
		await _client.SetActivityAsync(new Game("Online: " + PlayerService.OnlinePlayers.Count + "/" + Core.serverBootstrapSystem.ServerHostData.ServerMaxConnectedUsers, ActivityType.Watching));
	}

	[CommandFramework.Command("test-bot-msg", description: "Used for debugging", adminOnly: true)]
	public static void BotMsgCommand (Player sender, string message)
	{
		SendMessageAsync(DiscordBotConfig.Config.JailChannel, message);
	}

	[CommandFramework.Command("test-bullet-embed", description: "Used for debugging", adminOnly: true)]
	public static void Blabla (Player sender)
	{
		DiscordBot.SendEmbedAsync(DiscordBotConfig.Config.JailChannel,
			BulletHellManager.EmbedBulletAnnouncement("Rendy", (15.6f).ToString("F2"), "Ash",
				(66.6f).ToString("F2")));
	}

	public static async Task SendEmbedAsync (ulong discordChannel, Embed _embed)
	{
		ulong channelId = discordChannel; // Replace with your channel ID
		var channel = _client.GetChannel(channelId) as IMessageChannel;
		if (channel != null)
		{
			// Sending the embed message
			await channel.SendMessageAsync(embed : _embed);
		}
	}

	public static async void SendMessageAsync(ulong channelId, string message)
	{
		var channel = _client.GetChannel(channelId) as IMessageChannel;
		if (channel != null)
		{
			await channel.SendMessageAsync(message);
		}
	}
}

