using System.Threading.Tasks;
using Bloodstone.API;
using Discord;
using Discord.WebSocket;
using ProjectM;
using PvpArena;
using PvpArena.Services;
using UnityEngine;

public static partial class DiscordGlobalLinkSystem
{
	#region MessageHandling
	public static async Task HandleMessageReceivedAsync(SocketMessage message)
	{
		// Filter out messages from bots or from non-targeted channels
		if (message.Author.IsBot || message.Channel.Id != DiscordBotConfig.Config.GlobalChannel)
			return;
		
		string username = GetUsernameOnCurrentGuild(message.Author);
		
		// Send a new message with the same content
		await DiscordBot.SendEmbedAsync(DiscordBotConfig.Config.GlobalChannel, EmbedToGlobalChat(message.Author, message.Content));

		// Delete the original message
		await message.DeleteAsync();

		var action = () => ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager, "[Discord] ".Colorify(ExtendedColor.DiscordColor) + $"[Global] {username.Colorify(ExtendedColor.ClassicUsernameColor)} {TruncateString(message.Content, 60)}".Colorify(ExtendedColor.GlobalColor));
		ActionScheduler.RunActionOnMainThread(action);
	}
	
	private static string GetUsernameOnCurrentGuild (SocketUser _socketUser)
	{
		var user = DiscordBot._socketGuild?.GetUser(_socketUser.Id);

		if (user != null)
		{
			return user.DisplayName; 
		}

		return "";
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
	#endregion
	
	public static Embed EmbedFromGlobalChat (string _playerName, string _playerMessage)
	{
		var embedAuthorBuilder = new EmbedAuthorBuilder
		{
			Name = _playerName,
			IconUrl = DiscordBot._client.CurrentUser.GetAvatarUrl(),
		};
		
		var embedBuilder = new EmbedBuilder
		{
			Author = embedAuthorBuilder,
			Description = _playerMessage,
			Color = Discord.Color.Blue,
		};
		
		return embedBuilder.Build();
	}
	
	public static Embed EmbedToGlobalChat (SocketUser _user, string _playerMessage)
	{
		string username = GetUsernameOnCurrentGuild(_user);
		
		var embedAuthorBuilder = new EmbedAuthorBuilder
		{
			Name = username,
			IconUrl = _user.GetAvatarUrl(),
		};
		
		var embedBuilder = new EmbedBuilder
		{
			Author = embedAuthorBuilder,
			Description = _playerMessage,
			Color = Discord.Color.Purple,
		};
		
		return embedBuilder.Build();
	}
}
