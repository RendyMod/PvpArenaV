using System;
using System.Collections.Generic;

namespace PvpArena.Models;

public class Request
{
	public enum RequestType
	{
		TeleportRequest,
		ClanInviteRequest,
		ClanJoinRequest,
	}

	public RequestType Type { get; set; }
	public Player Requester { get; set; }
	public Player Recipient { get; set; }
	public DateTime Timestamp { get; set; }

	public bool IsExpired()
	{
		return (DateTime.Now - Timestamp > TimeSpan.FromSeconds(15));
	}
}

public class RequestManager
{
	// Mapping from Recipient ID to a dictionary of Requester to Request.
	private Dictionary<Player, Dictionary<Player, Request>> activeRequests =
		new Dictionary<Player, Dictionary<Player, Request>>();

	private List<Request> requestOrder = new List<Request>();

	public void AddRequest(Request request)
	{
		var existingRequest = GetRequest(request.Recipient, request.Requester);
		if (existingRequest != null && !existingRequest.IsExpired())
		{
			request.Recipient.ReceiveMessage($"{request.Recipient.Name.Emphasize()} already has a pending invite".Warning());
		}

		if (!activeRequests.TryGetValue(request.Recipient, out var requesterToRequestMap))
		{
			requesterToRequestMap = new Dictionary<Player, Request>();
			activeRequests[request.Recipient] = requesterToRequestMap;
		}

		// Overwrites or adds the request for that specific requester.
		requesterToRequestMap[request.Requester] = request;

		requestOrder.Add(request);
	}

	public Request GetRequest(Player recipient, Player requester)
	{
		if (activeRequests.TryGetValue(recipient, out var requesterToRequestMap) &&
			requesterToRequestMap.TryGetValue(requester, out var request))
		{
			return request;
		}

		return null;
	}

	public Request GetRequest()
	{
		if (requestOrder.Count == 0) return null;

		return requestOrder[requestOrder.Count - 1];
	}

	public void RemoveRequest(Request request)
	{
		if (activeRequests.TryGetValue(request.Recipient, out var requesterToRequestMap))
		{
			requesterToRequestMap.Remove(request.Requester);

			// Remove the recipient's entry entirely if they have no active requests.
			if (requesterToRequestMap.Count == 0)
			{
				activeRequests.Remove(request.Recipient);
			}
		}

		requestOrder.Remove(request);
	}
}
