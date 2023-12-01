using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISmarteasy.Core.Prompt.Template;

internal abstract class Block : IBlock
{
    public BlockTypeKind Type { get; }

    public string Content { get; }

    private protected ILogger Logger { get; }

    protected Block(BlockTypeKind type, string content, ILoggerFactory? loggerFactory)
    {
        Type = type;
        Content = content;
        Logger = loggerFactory is not null ? loggerFactory.CreateLogger(GetType()) : NullLogger.Instance;
    }

    public virtual string Render(ContextVariableDictionary? variables)
    {
        return Content;
    }

    public abstract bool IsValid(out string errorMsg);
}