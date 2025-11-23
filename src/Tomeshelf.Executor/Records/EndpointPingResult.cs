using System;

namespace Tomeshelf.Executor.Records;

public sealed record EndpointPingResult
{
    public EndpointPingResult(bool success, int statusCode, string message, string body, TimeSpan duration)
    {
        Success = success;
        StatusCode = statusCode;
        Message = message;
        Body = body;
        Duration = duration;
    }

    public bool Success { get; init; }

    public int StatusCode { get; init; }

    public string Message { get; init; }

    public string Body { get; init; }

    public TimeSpan Duration { get; init; }

    public void Deconstruct(out bool success, out int? statusCode, out string message, out string body, out TimeSpan duration)
    {
        success = Success;
        statusCode = StatusCode;
        message = Message;
        body = Body;
        duration = Duration;
    }
}