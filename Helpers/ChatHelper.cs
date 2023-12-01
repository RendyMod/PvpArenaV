using Bloodstone.API;
using ProjectM;

namespace PvpArena.Helpers;

public static partial class Helper
{
	public static void SendSystemMessageToAllClients (string message)
	{
		ServerChatUtils.SendSystemMessageToAllClients(
			VWorld.Server.EntityManager,
			message
		);
	}
}
