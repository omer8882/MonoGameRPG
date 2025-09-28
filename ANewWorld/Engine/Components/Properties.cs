using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANewWorld.Engine.Components
{
    public class Properties: Dictionary<string, string>
    {
    }

    public record Name(string Value);
    public record Tag(string Value);
}
