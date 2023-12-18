using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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

    public virtual string Render(ContextVariableDictionary? variables)
    {
        return Content;
    }

    public virtual async Task<string> RenderAsync(ContextVariableDictionary variables, bool isNeedFunctionRun,
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(string.Empty);
    }

    public abstract bool IsValid(out string errorMsg);
}