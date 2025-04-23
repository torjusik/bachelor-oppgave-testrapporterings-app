using System;
using System.Text.Json.Serialization;

public class TestProcedure
{
    [JsonPropertyName("steps")]
    public List<Step> Steps { get; set; }

    [JsonPropertyName("safety_requirements")]
    public List<string> SafetyRequirements { get; set; }
}
