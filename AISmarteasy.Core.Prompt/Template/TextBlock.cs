using Microsoft.Extensions.Logging;

namespace AISmarteasy.Core.Prompt.Template;

internal sealed class TextBlock : Block
{
    public TextBlock(string text, ILogger logger)
        : base(BlockTypeKind.Text, text, logger)
    {
    }

    public TextBlock(string text, int startIndex, int stopIndex, ILogger logger)
        : base(BlockTypeKind.Text, text.Substring(startIndex, stopIndex - startIndex), logger)
    {
    }

    public override bool IsValid(out string errorMsg)
    {
        errorMsg = "";
        return true;
    }
}
