using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;

namespace PvpArena.Factories;
public static class ModificationIdFactory
{
	public static BurstableIdGenerator modificationIdGenerator = BurstableIdGenerator.Create();
	public static ModificationId NewId()
	{
		return new ModificationId(modificationIdGenerator.NewId());
	}
}
