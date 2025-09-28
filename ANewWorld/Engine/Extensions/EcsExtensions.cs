using ANewWorld.Engine.Components;
using DefaultEcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANewWorld.Engine.Extensions
{
    public static class EcsExtensions
    {
        public static Entity GetPlayer(this World world)
        {
            return world.GetEntities().With<Tag>().AsEnumerable().First(e => e.Get<Tag>().Value == "Player");
        }

        public static string GetName(this Entity entity)
        {
            if (entity.Has<Name>())
            {
                return entity.Get<Name>().Value;
            }
            return "";
        }
    }
}
