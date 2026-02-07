using System.Text.Json.Serialization;

namespace Tomeshelf.MCM.Application.Mcm;

public sealed record ScheduleDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("start_time")]
    public string? StartTime { get; init; }

    [JsonPropertyName("end_time")]
    public string? EndTime { get; init; }

    [JsonPropertyName("no_end_time")]
    public bool NoEndTime { get; init; }

    [JsonPropertyName("location")]
    public string? Location { get; init; }

    [JsonPropertyName("venue_location")]
    public VenueLocationDto? VenueLocation { get; init; }
}