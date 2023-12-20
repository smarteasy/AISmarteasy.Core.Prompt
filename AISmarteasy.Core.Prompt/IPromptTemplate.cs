namespace AISmarteasy.Core.Prompt;

public interface IPromptTemplate
{
    string Content { get; }

    Task<string> RenderAsync(IAIServiceConnector serviceConnector, CancellationToken cancellationToken = default);
}
