namespace AISmarteasy.Core.Prompt;

public interface IPromptTemplate
{
    string Content { get; }

    Task<string> RenderAsync(ITextCompletionConnector serviceConnector, CancellationToken cancellationToken = default);
}
