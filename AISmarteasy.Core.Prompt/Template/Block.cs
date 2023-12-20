using Microsoft.Extensions.Logging;

namespace AISmarteasy.Core.Prompt.Template;

internal abstract class Block : IBlock
{
    public BlockTypeKind Type { get; }

    public string Content { get; }

    private protected ILogger Logger { get; }

    protected Block(BlockTypeKind type, string content, ILogger logger)
    {
        Type = type;
        Content = content;
        Logger = logger;
    }

    public virtual string Render(VariableDictionary? variables)
    {
        return Content;
    }

    public virtual async Task<string> RenderAsync(IAIServiceConnector serviceConnector, VariableDictionary variables, bool isNeedFunctionRun,
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(string.Empty);
    }

    public abstract bool IsValid(out string errorMsg);
}