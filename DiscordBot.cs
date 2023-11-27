using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace PvpArena;

/*public class DiscordBot
{
	private DiscordSocketClient _client;

	public async void InitializeAsync()
	{
		try
		{
			_client = new DiscordSocketClient();
			_client.LoginAsync(TokenType.Bot, "");
			_client.StartAsync();
		}
		catch (Exception ex)
		{
			Unity.Debug.Log(ex.ToString());
		}
	}

	public async void SendMessageAsync(string message)
	{
		ulong channelId = 0; // Replace with your channel ID
		var channel = _client.GetChannel(channelId) as IMessageChannel;
		if (channel != null)
		{
			await channel.SendMessageAsync(message);
		}
	}
}
*/
