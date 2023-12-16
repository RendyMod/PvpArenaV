using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;

namespace PvpArena.Factories;
public static class ModificationIdFactory
{
	public static ModificationId NewId()
	{
		return Core.modificationSystem.ModificationIDs.NewModificationId();
	}
}
