using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace AISmarteasy.Core.Prompt.Template;

internal sealed class TextBlock : Block//, INonCodeBlock, INonFunctionRenderer
{
    public TextBlock(string text, ILoggerFactory? loggerFactory = null)
        : base(BlockTypeKind.Text, text, loggerFactory)
    {
    }

    public TextBlock(string text, int startIndex, int stopIndex, ILoggerFactory? loggerFactory)
        : base(BlockTypeKind.Text, text.Substring(startIndex, stopIndex - startIndex), loggerFactory)
    {
    }

    public override bool IsValid(out string errorMsg)
    {
        errorMsg = "";
        return true;
    }
}
