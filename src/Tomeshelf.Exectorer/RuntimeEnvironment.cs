namespace Tomeshelf.Executor;

/// <summary>
///     Runtime environment helper utilities.
/// </summary>
internal static class RuntimeEnvironment
{
    /// <summary>
    ///     Determines whether the current process is running inside a Docker container.
    /// </summary>
    /// <returns><c>true</c> when the /.dockerenv sentinel file is present; otherwise false.</returns>
    public static bool IsRunningInDocker()
    {
        return File.Exists("/.dockerenv");
    }
}