namespace DevHabit.Api.Settings;

public sealed class GitHubAutomationOptions
{
    public const string SectionName = "Quartz";

    public int ScanIntervalMinutes { get; set; }
}
