using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tomeshelf.Application.Contracts.Mcm;

public sealed record PersonDto
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = null!;

    [JsonPropertyName("uid")]
    public string Uid { get; init; }

    [JsonPropertyName("publicly_visible")]
    public bool PubliclyVisible { get; init; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; init; } = "";

    [JsonPropertyName("last_name")]
    public string LastName { get; init; } = "";

    [JsonPropertyName("alt_name")]
    public string AltName { get; init; }

    [JsonPropertyName("bio")]
    public string Bio { get; init; }

    [JsonPropertyName("known_for")]
    public string KnownFor { get; init; }

    [JsonPropertyName("profile_url")]
    public string ProfileUrl { get; init; }

    [JsonPropertyName("profile_url_label")]
    public string ProfileUrlLabel { get; init; }

    [JsonPropertyName("video_link")]
    public string VideoLink { get; init; }

    [JsonPropertyName("twitter")]
    public string Twitter { get; init; }

    [JsonPropertyName("facebook")]
    public string Facebook { get; init; }

    [JsonPropertyName("instagram")]
    public string Instagram { get; init; }

    [JsonPropertyName("youtube")]
    public string YouTube { get; init; }

    [JsonPropertyName("twitch")]
    public string Twitch { get; init; }

    [JsonPropertyName("snapchat")]
    public string Snapchat { get; init; }

    [JsonPropertyName("deviantart")]
    public string DeviantArt { get; init; }

    [JsonPropertyName("tumblr")]
    public string Tumblr { get; init; }

    [JsonPropertyName("category")]
    public string Category { get; init; }

    [JsonPropertyName("days_at_show")]
    public string DaysAtShow { get; init; }

    [JsonPropertyName("booth_number")]
    public string BoothNumber { get; init; }

    [JsonConverter(typeof(NullableFlexibleDecimalConverter))]
    [JsonPropertyName("autograph_amount")]
    public decimal? AutographAmount { get; init; }

    [JsonConverter(typeof(NullableFlexibleDecimalConverter))]
    [JsonPropertyName("photo_op_amount")]
    public decimal? PhotoOpAmount { get; init; }

    [JsonConverter(typeof(NullableFlexibleDecimalConverter))]
    [JsonPropertyName("photo_op_table_amount")]
    public decimal? PhotoOpTableAmount { get; init; }

    [JsonPropertyName("people_categories")]
    public List<object> PeopleCategories { get; init; }

    [JsonPropertyName("global_categories")]
    public List<CategoryDto> GlobalCategories { get; init; } = [];

    [JsonPropertyName("images")]
    public List<ImageSetDto> Images { get; init; } = [];

    [JsonPropertyName("schedules")]
    public List<ScheduleDto> Schedules { get; init; } = [];

    [JsonPropertyName("removed_at")]
    public string RemovedAt { get; init; }
}