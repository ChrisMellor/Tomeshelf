namespace Tomeshelf.Web.Models.Home;

public sealed class HomeIndexViewModel
{
    public string EventsSummary { get; init; } = "Events unavailable";

    public string EducationSummary { get; init; } = "Bundles unavailable";

    public string HealthSummary { get; init; } = "Fitbit unavailable";

    public string GamingSummary { get; init; } = "Gaming unavailable";
}