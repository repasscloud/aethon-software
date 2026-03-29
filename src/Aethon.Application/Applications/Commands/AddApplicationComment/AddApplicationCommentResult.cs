namespace Aethon.Application.Applications.Commands.AddApplicationComment;

public sealed class AddApplicationCommentResult
{
    public Guid Id { get; init; }
    public Guid ApplicationId { get; init; }
    public Guid? ParentCommentId { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedUtc { get; init; }
}
