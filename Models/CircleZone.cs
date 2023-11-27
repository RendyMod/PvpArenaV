using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjectM;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace PvpArena.Models
{
	public class CircleZone
	{
		public float3 Center { get; set; }
		public float Radius { get; set; }

		public CircleZone(float3 center, float radius)
		{
			Center = center;
			Radius = radius;
		}

		public static CircleZone FromEntity(Entity entity, float radius)
		{
			var position = entity.Read<LocalToWorld>().Position;

			return new CircleZone(position, radius);
		}

		public bool Contains(Player player)
		{
			return Contains(player.Position);
		}

		public bool Contains(float3 position)
		{
			float dx = position.x - Center.x;
			float dz = position.z - Center.z;
			return (dx * dx + dz * dz) <= (Radius * Radius);
		}

		// Additional functionality as needed

		public override string ToString()
		{
			return $"\"Center\": ({Center.x:F2}, {Center.y:F2}, {Center.z:F2}),\n\"Radius\": {Radius:F2}";
		}
	}
}


