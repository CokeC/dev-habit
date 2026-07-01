namespace DevHabit.Api.DTOs.GitHub;

public class GitHubEventDto
{
    public string? Id { get; set; }
    public string? Type { get; set; }
    public required Actor Actor { get; set; }
    public required Payload Payload { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Actor
{
    public string? Login { get; set; }
}

public class Payload
{
    public List<Commit>? Commits { get; set; }
}

public class Commit
{
    public string? Message { get; set; }
}