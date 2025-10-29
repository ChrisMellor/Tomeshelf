using System;
using System.Collections.Generic;

namespace Tomeshelf.Paissa.Api.Models;

/// <summary>
///     Response payload representing housing plots grouped by district and size.
/// </summary>
/// <param name="WorldId">Identifier of the world.</param>
/// <param name="WorldName">Display name of the world.</param>
/// <param name="RetrievedAtUtc">Timestamp when the data was retrieved from PaissaDB.</param>
/// <param name="Districts">Districts that currently have lots accepting entries.</param>
public sealed record PaissaWorldResponse(int WorldId, string WorldName, DateTimeOffset RetrievedAtUtc, IReadOnlyList<PaissaDistrictResponse> Districts);