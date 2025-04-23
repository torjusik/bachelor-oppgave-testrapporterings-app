using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bachelor_Testing_V1;

public class RequirementConverter : JsonConverter<List<Requirement>>
{
    public override List<Requirement> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string requirementString = reader.GetString();
            return new List<Requirement> { new Requirement(requirementString) { Completed = false } };
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            var requirements = new List<Requirement>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var requirement = JsonSerializer.Deserialize<Requirement>(ref reader, options);
                    requirements.Add(requirement);
                }
                else if(reader.TokenType == JsonTokenType.String)
                {
                    requirements.Add(new Requirement(reader.GetString()){Completed = false});
                }
            }
            return requirements;
        }
        
        return null;
    }

    public override void Write(Utf8JsonWriter writer, List<Requirement> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, options);
    }
}