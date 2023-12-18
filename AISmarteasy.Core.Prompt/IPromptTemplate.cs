namespace AISmarteasy.Core.Prompt;

public interface IPromptTemplate
{
    string Content { get; }

    Task<string> RenderAsync(CancellationToken cancellationToken = default);
}
