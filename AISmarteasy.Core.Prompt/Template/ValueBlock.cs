using Microsoft.Extensions.Logging;

namespace AISmarteasy.Core.Prompt.Template;

internal sealed class ValueBlock : Block
{
    private readonly char _first = '\0';
    private readonly char _last = '\0';

    public string Value { get; } = string.Empty;

    public ValueBlock(string text, ILoggerFactory? loggerFactory = null)
        : base(BlockTypeKind.Value, text.Trim(), loggerFactory)
    {
        if (Content.Length < 2)
        {
            Logger.LogError("A value must have single quotes or double quotes on both sides");
            return;
        }

        _first = Content[0];
        _last = Content[^1];
        Value = Content.Substring(1, Content.Length - 2);
    }

    public override bool IsValid(out string errorMsg)
    {
        errorMsg = string.Empty;

        if (Content.Length < 2)
        {
            errorMsg = "A value must have single quotes or double quotes on both sides";
            Logger.LogError(errorMsg);
            return false;
        }

        if (_first != _last)
        {
            errorMsg = "A value must be defined using either single quotes or double quotes, not both";
            Logger.LogError(errorMsg);
            return false;
        }

        return true;
    }

    public override string Render(ContextVariableDictionary? variables)
    {
        return Value;
    }

    public static bool HasPrefix(string text)
    {
        return text.Length > 0 && text[0] is Symbol.DBL_QUOTE or Symbol.SGL_QUOTE;
    }
}