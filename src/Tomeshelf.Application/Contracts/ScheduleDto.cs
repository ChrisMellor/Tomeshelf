using System.Text.Json.Serialization;

namespace Tomeshelf.Application.Contracts;

public sealed record ScheduleDto
{
    [JsonPropertyName("id")] public string Id { get; init; } = null!;

    [JsonPropertyName("title")] public string Title { get; init; } = null!;

    [JsonPropertyName("description")] public string Description { get; init; }

    [JsonPropertyName("start_time")] public string StartTime { get; init; } = null!;

    [JsonPropertyName("end_time")] public string EndTime { get; init; }

    [JsonPropertyName("no_end_time")] public bool NoEndTime { get; init; }

    [JsonPropertyName("location")] public string Location { get; init; }

    [JsonPropertyName("venue_location")] public VenueLocationDto VenueLocation { get; init; }
}