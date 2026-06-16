namespace DevHabit.Api.Entities;

public sealed class User
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    //用于存储Identity Provider的标识，例如AzureAD，Okta，Auth0
    public string? IdentityId { get; set; }
}