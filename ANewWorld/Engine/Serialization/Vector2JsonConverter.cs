using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;

namespace ANewWorld.Engine.Serialization
{
    public class Vector2JsonConverter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return Vector2.Zero;
            
            // Handle object format: {"x": 100, "y": 200}
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                float x = 0, y = 0;
                
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;
                        
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString()!.ToLowerInvariant();
                        reader.Read();
                        
                        if (propertyName == "x")
                            x = reader.GetSingle();
                        else if (propertyName == "y")
                            y = reader.GetSingle();
                    }
                }
                
                return new Vector2(x, y);
            }
            
            // Handle array format: [100, 200]
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                float x = reader.GetSingle();
                
                reader.Read();
                float y = reader.GetSingle();
                
                reader.Read();
                if (reader.TokenType != JsonTokenType.EndArray)
                    throw new JsonException("Expected end of array for Vector2");

                return new Vector2(x, y);
            }
            
            throw new JsonException($"Unexpected token type for Vector2: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteEndArray();
        }
    }
}
