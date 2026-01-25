using System;
using System.Collections.Generic;

namespace Tomeshelf.Paissa.Application.Features.Housing.Dtos;

/// <summary>
///     Summary of the configured world with districts containing plots accepting entries.
/// </summary>
/// <param name="WorldId">Identifier of the world.</param>
/// <param name="WorldName">Display name of the world.</param>
/// <param name="RetrievedAtUtc">Timestamp when the data was retrieved from PaissaDB.</param>
/// <param name="Districts">Districts that currently have lots accepting entries.</param>
public sealed record PaissaWorldSummaryDto(int WorldId, string WorldName, DateTimeOffset RetrievedAtUtc, IReadOnlyList<PaissaDistrictSummaryDto> Districts);
