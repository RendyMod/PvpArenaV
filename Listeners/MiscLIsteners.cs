using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM.Network;
using PvpArena.Services;
using Unity.Entities;

namespace PvpArena.Listeners;

public class FromCharacterListener : EntityQueryListener
{
	public void OnNewMatchFound(Entity entity)
	{
		if (entity.Exists())
		{
			var user = entity.Read<FromCharacter>().User;
			if (user.Exists())
			{
				var player = PlayerService.GetPlayerFromUser(user);
				entity.LogComponentTypes();
			}
		}
	}

	public void OnNewMatchRemoved(Entity entity)
	{

	}

	public void OnUpdate(Entity entity)
	{

	}
}
