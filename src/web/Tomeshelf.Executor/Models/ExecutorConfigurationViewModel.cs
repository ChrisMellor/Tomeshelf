using System.Collections.Generic;

namespace Tomeshelf.Executor.Models;

public sealed class ExecutorConfigurationViewModel
{
    public bool Enabled { get; set; }

    public List<EndpointSummaryViewModel> Endpoints { get; set; } = [];

    public EndpointEditorModel Editor { get; set; } = new();

    public EndpointPingModel Ping { get; set; } = new();

    public EndpointPingResultViewModel? PingResult { get; set; }

    public List<ApiServiceOptionViewModel> ApiServices { get; set; } = [];
}