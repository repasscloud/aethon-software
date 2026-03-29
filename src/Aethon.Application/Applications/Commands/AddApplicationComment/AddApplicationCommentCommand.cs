namespace Aethon.Application.Applications.Commands.AddApplicationComment;

public sealed class AddApplicationCommentCommand
{
    public Guid ApplicationId { get; init; }
    public Guid? ParentCommentId { get; init; }
    public string Content { get; init; } = string.Empty;
}
