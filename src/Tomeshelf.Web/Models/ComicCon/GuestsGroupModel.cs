using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Tomeshelf.Web.Models.ComicCon;

public sealed record GuestsGroupModel
{
    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; init; }

    [JsonPropertyName("items")]
    public List<PersonModel> Items { get; init; } =
        [];
}