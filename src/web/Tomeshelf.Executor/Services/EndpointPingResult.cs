using System;

namespace Tomeshelf.Executor.Services;

public sealed record EndpointPingResult(bool Success, int? StatusCode, string Message, string? Body, TimeSpan Duration);
