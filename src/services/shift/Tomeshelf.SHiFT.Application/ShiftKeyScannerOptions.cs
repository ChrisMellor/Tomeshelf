using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tomeshelf.SHiFT.Application;

/// <summary>
///     Configuration options for scanning external sources for SHiFT codes.
/// </summary>
public sealed class ShiftKeyScannerOptions
{
    public const string SectionName = "ShiftKeyScanner";

    /// <summary>
    ///     Default lookback window, in hours, when none is provided at request time.
    /// </summary>
    [Range(1, 168)]
    public int LookbackHours { get; set; } = 24;

    /// <summary>
    ///     Options for scanning X (formerly Twitter).
    /// </summary>
    public XSourceOptions X { get; set; } = new();

    public sealed class XSourceOptions
    {
        public bool Enabled { get; set; } = true;

        /// <summary>
        ///     Base API endpoint for the X API v2.
        /// </summary>
        public string ApiBaseV2 { get; set; } = "https://api.x.com/2/";

        /// <summary>
        ///     Bearer token used to authenticate API requests.
        /// </summary>
        public string BearerToken { get; set; } = string.Empty;

        /// <summary>
        ///     API key used for app-only bearer token generation.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        ///     API secret used for app-only bearer token generation.
        /// </summary>
        public string ApiSecret { get; set; } = string.Empty;

        /// <summary>
        ///     OAuth token endpoint for app-only authentication.
        /// </summary>
        public string OAuthTokenEndpoint { get; set; } = "https://api.x.com/oauth2/token";


        [Range(1, 1440)]
        public int TokenCacheMinutes { get; set; } = 55;

        /// <summary>
        ///     Usernames to scan for SHiFT codes.
        /// </summary>
        public List<string> Usernames { get; set; } = new();

        [Range(1, 20)]
        public int MaxPages { get; set; } = 4;

        [Range(5, 200)]
        public int MaxResultsPerPage { get; set; } = 100;

        public bool ExcludeReplies { get; set; } = true;

        public bool ExcludeRetweets { get; set; } = false;

    }
}
