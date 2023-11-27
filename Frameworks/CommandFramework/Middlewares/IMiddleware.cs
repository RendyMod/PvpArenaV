using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using PvpArena;
using PvpArena.GameModes;
using PvpArena.Models;
using static PvpArena.Frameworks.CommandFramework.CommandFramework;

public interface IMiddleware
{
	public bool CanExecute(Player sender, CommandAttribute command, MethodInfo method);
}
