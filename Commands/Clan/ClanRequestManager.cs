using Unity.Entities;

namespace PvpArena.Models;

public static class ClanRequestManager
{
	private static RequestManager requestManager = new RequestManager();

	public static void AddRequest(Request request)
	{
		requestManager.AddRequest(request);
	}

	public static Request GetRequest(Player recipient, Player requester)
	{
		return requestManager.GetRequest(recipient, requester);
	}
	public static Request GetRequest(Player recipient)
	{
		return requestManager.GetRequest(recipient);
	}

	public static void RemoveRequest(Request request)
	{
		requestManager.RemoveRequest(request);
	}
}
