using Bachelor_Testing_V1;
using System;
using System.Text.Json.Serialization;

public class Step
{
    [JsonPropertyName("step_id")]
    public int StepId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("requirements")]
    public List<Requirement> Requirements { get; set; }

    [JsonPropertyName("equipment_needed")]
    public List<string> EquipmentNeeded { get; set; }
}
