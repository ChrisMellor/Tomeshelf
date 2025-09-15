using System.Text.Json.Serialization;

namespace Tomeshelf.Web.Models.ComicCon;

public sealed record ScheduleModel
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = null!;

    [JsonPropertyName("title")]
    public string Title { get; init; } = null!;

    [JsonPropertyName("description")]
    public string Description { get; init; }

    [JsonPropertyName("start_time")]
    public string StartTime { get; init; } = null!;

    [JsonPropertyName("end_time")]
    public string EndTime { get; init; }

    [JsonPropertyName("no_end_time")]
    public bool NoEndTime { get; init; }

    [JsonPropertyName("location")]
    public string Location { get; init; }

    [JsonPropertyName("venue_location")]
    public VenueLocationModel VenueLocation { get; init; }
}

