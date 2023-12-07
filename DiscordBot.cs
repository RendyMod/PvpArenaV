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
using PvpArena.Models;
using PvpArena.Services;

namespace PvpArena;

public static class DiscordBot
{
	public static DiscordSocketClient _client;

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
				_client.MessageReceived += HandleMessageReceivedAsync;
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
			_client.MessageReceived -= HandleMessageReceivedAsync;
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
		SendMessageAsync(message);
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

	public static async void SendMessageAsync(string message, ulong channelId)
	{
		var channel = _client.GetChannel(channelId) as IMessageChannel;
		if (channel != null)
		{
			await channel.SendMessageAsync($"In-Game: {message}");
		}
	}

	private static async Task HandleMessageReceivedAsync(SocketMessage message)
	{
		// Filter out messages from bots or from non-targeted channels
		if (message.Author.IsBot || message.Channel.Id != DiscordBotConfig.Config.GlobalChannel)
			return;

		string username = message.Author.Username;

		string messageContent = $"{message.Author.Username}: {message.Content}";

		// Send a new message with the same content
		await message.Channel.SendMessageAsync(FormatMessage($"Discord: {messageContent}"));

		// Delete the original message
		await message.DeleteAsync();

		var action = () => ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager, $"{username.Colorify(ExtendedColor.ClanNameColor)} ({"Discord".Emphasize()}) - {TruncateString(message.Content, 60)}".White());
		ActionScheduler.RunActionOnMainThread(action);
	}

	private static string FormatMessage(string content)
	{
		string formattedMessage = TruncateString(content, 30);
		return formattedMessage;
	}


	public static string TruncateString(string input, int maxLength)
	{
		// Check if the string is null or if truncation is not needed
		if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
			return input;

		// Truncate the string to the maximum length minus the length of the ellipsis
		return input.Substring(0, maxLength - 3) + "...";
	}
}

