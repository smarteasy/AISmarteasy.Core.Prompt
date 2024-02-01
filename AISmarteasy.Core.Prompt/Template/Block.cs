using Microsoft.Extensions.Logging;

namespace AISmarteasy.Core.Prompt.Template;

internal abstract class Block(BlockTypeKind type, string content, ILogger logger) : IBlock
{
    public BlockTypeKind Type { get; } = type;

    public string Content { get; } = content;

    private protected ILogger Logger { get; } = logger;

    public virtual string Render(VariableDictionary? variables)
    {
        return Content;
    }

    public virtual async Task<string> RenderAsync(ITextCompletionConnector serviceConnector, VariableDictionary variables, bool isNeedFunctionRun,
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(string.Empty);
    }

    public abstract bool IsValid(out string errorMsg);
}