using Tomeshelf.Application.Shared.Abstractions.Messaging;
using Tomeshelf.Paissa.Application.Features.Housing.Dtos;

namespace Tomeshelf.Paissa.Application.Features.Housing.Queries;

public sealed record GetAcceptingEntriesQuery : IQuery<PaissaWorldSummaryDto>;