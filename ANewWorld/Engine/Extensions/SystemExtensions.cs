using System;
using System.Collections.Generic;
using System.Text;

namespace ANewWorld.Engine.Extensions
{
    public static class SystemExtensions
    {
        extension(string? s)
        {
            public bool IsNullOrEmpty() => string.IsNullOrEmpty(s);
            public bool NotNullOrEmpty() => !string.IsNullOrEmpty(s);
            public bool IsNullOrWhiteSpace() => string.IsNullOrWhiteSpace(s);
        }
    }
}
