using ANewWorld.Engine.Serialization;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;

namespace ANewWorld.Engine.Extensions
{
    public static class MonoGameExtensions
    {
        private static readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new Vector2JsonConverter() }
        };

        extension(ContentManager content)
        {
            /// <summary>
            /// Loads and deserializes a JSON file from the specified asset subpath into an object of type <typeparamref name="T"/>.
            /// </summary>
            /// <remarks>The method expects the JSON file to be well-formed and compatible with the
            /// target type <typeparamref name="T"/>. The deserialization uses the configured JSON serializer options,
            /// if any.</remarks>
            /// <typeparam name="T">The type to which the JSON content will be deserialized.</typeparam>
            /// <param name="assetSubpath">The relative path to the JSON asset file within the content root directory. Cannot be null or empty.</param>
            /// <returns>An instance of type <typeparamref name="T"/> containing the deserialized data from the JSON file.</returns>
            /// <exception cref="FileNotFoundException">Thrown if the file at the specified <paramref name="assetSubpath"/> does not exist.</exception>
            /// <exception cref="Exception">Thrown if the JSON content cannot be deserialized into the specified type <typeparamref name="T"/>.</exception>
            public T LoadJson<T>(string assetSubpath)
            {
                var path = Path.Combine(content.RootDirectory, assetSubpath);
                if(!File.Exists(path)) throw new FileNotFoundException($"JSON file not found at path: {path}");
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<T>(json, options) ?? throw new JsonException($"Failed to deserialize JSON from {path} to type {typeof(T).FullName}");
            }
        }
    }
}
