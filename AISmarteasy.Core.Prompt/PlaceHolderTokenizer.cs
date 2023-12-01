using System.Diagnostics.CodeAnalysis;
using System.Text;
using AISmarteasy.Core.Prompt.Template;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISmarteasy.Core.Prompt;

internal sealed class PlaceHolderTokenizer
{
    private readonly ILoggerFactory _loggerFactory;

    public PlaceHolderTokenizer(ILoggerFactory? loggerFactory = null)
    {
        _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
    }

    public List<IBlock> Tokenize(string? text)
    {
        text = text?.Trim();

        if (string.IsNullOrEmpty(text)) { return new List<IBlock>(); }

        BlockTypeKind currentTokenType = BlockTypeKind.Undefined;

        var currentTokenContent = new StringBuilder();

        char textValueDelimiter = '\0';

        var blocks = new List<IBlock>();
        char nextChar = text[0];

        bool spaceSeparatorFound = false;

        bool namedArgSeparatorFound = false;
        char namedArgValuePrefix = '\0';


        if (text.Length == 1)
        {
            switch (nextChar)
            {
                case Symbol.VAR_PREFIX:
                    blocks.Add(new VariableBlock(text, _loggerFactory));
                    break;

                case Symbol.DBL_QUOTE:
                case Symbol.SGL_QUOTE:
                    blocks.Add(new ValueBlock(text, _loggerFactory));
                    break;

                default:
                    blocks.Add(new FunctionIdBlock(text, _loggerFactory));
                    break;
            }

            return blocks;
        }

        bool skipNextChar = false;
        for (int nextCharCursor = 1; nextCharCursor < text.Length; nextCharCursor++)
        {
            char currentChar = nextChar;
            nextChar = text[nextCharCursor];

            if (skipNextChar)
            {
                skipNextChar = false;
                continue;
            }

            if (nextCharCursor == 1)
            {
                if (IsVarPrefix(currentChar))
                {
                    currentTokenType = BlockTypeKind.Variable;
                }
                else if (IsQuote(currentChar))
                {
                    currentTokenType = BlockTypeKind.Value;
                    textValueDelimiter = currentChar;
                }
                else
                {
                    currentTokenType = BlockTypeKind.FunctionId;
                }

                currentTokenContent.Append(currentChar);
                continue;
            }

            if (currentTokenType == BlockTypeKind.Value || (currentTokenType == BlockTypeKind.NamedArg && IsQuote(namedArgValuePrefix)))
            {
                if (currentChar == Symbol.ESCAPE_CHAR && CanBeEscaped(nextChar))
                {
                    currentTokenContent.Append(nextChar);
                    skipNextChar = true;
                    continue;
                }

                currentTokenContent.Append(currentChar);

                if (currentChar == textValueDelimiter && currentTokenType == BlockTypeKind.Value)
                {
                    blocks.Add(new ValueBlock(currentTokenContent.ToString(), this._loggerFactory));
                    currentTokenContent.Clear();
                    currentTokenType = BlockTypeKind.Undefined;
                    spaceSeparatorFound = false;
                }
                else if (currentChar == namedArgValuePrefix && currentTokenType == BlockTypeKind.NamedArg)
                {
                    blocks.Add(new NamedArgBlock(currentTokenContent.ToString(), this._loggerFactory));
                    currentTokenContent.Clear();
                    currentTokenType = BlockTypeKind.Undefined;
                    spaceSeparatorFound = false;
                    namedArgSeparatorFound = false;
                    namedArgValuePrefix = '\0';
                }

                continue;
            }

            if (IsBlankSpace(currentChar))
            {
                if (currentTokenType == BlockTypeKind.Variable)
                {
                    blocks.Add(new VariableBlock(currentTokenContent.ToString(), this._loggerFactory));
                    currentTokenContent.Clear();
                    currentTokenType = BlockTypeKind.Undefined;
                }
                else if (currentTokenType == BlockTypeKind.FunctionId)
                {
                    var tokenContent = currentTokenContent.ToString();

                    if (PlaceHolderTokenizer.IsValidNamedArg(tokenContent))
                    {
                        blocks.Add(new NamedArgBlock(tokenContent, this._loggerFactory));
                    }
                    else
                    {
                        blocks.Add(new FunctionIdBlock(tokenContent, this._loggerFactory));
                    }
                    currentTokenContent.Clear();
                    currentTokenType = BlockTypeKind.Undefined;
                }
                else if (currentTokenType == BlockTypeKind.NamedArg && namedArgSeparatorFound && namedArgValuePrefix != 0)
                {
                    blocks.Add(new NamedArgBlock(currentTokenContent.ToString(), this._loggerFactory));
                    currentTokenContent.Clear();
                    namedArgSeparatorFound = false;
                    namedArgValuePrefix = '\0';
                    currentTokenType = BlockTypeKind.Undefined;
                }

                spaceSeparatorFound = true;

                continue;
            }

            if (currentTokenType == BlockTypeKind.NamedArg && (!namedArgSeparatorFound || namedArgValuePrefix == 0))
            {
                if (!namedArgSeparatorFound)
                {
                    if (currentChar == Symbol.NAMED_ARG_BLOCK_SEPARATOR)
                    {
                        namedArgSeparatorFound = true;
                    }
                }
                else
                {
                    namedArgValuePrefix = currentChar;
                    if (!IsQuote(namedArgValuePrefix) && namedArgValuePrefix != Symbol.VAR_PREFIX)
                    {
                        throw new CoreException($"Named argument values need to be prefixed with a quote or {Symbol.VAR_PREFIX}.");
                    }
                }
                currentTokenContent.Append(currentChar);
                continue;
            }

            currentTokenContent.Append(currentChar);

            if (currentTokenType == BlockTypeKind.Undefined)
            {
                if (!spaceSeparatorFound)
                {
                    throw new CoreException("Tokens must be separated by one space least");
                }

                if (IsQuote(currentChar))
                {
                    currentTokenType = BlockTypeKind.Value;
                    textValueDelimiter = currentChar;
                }
                else if (IsVarPrefix(currentChar))
                {
                    currentTokenType = BlockTypeKind.Variable;
                }
                else if (blocks.Count == 0)
                {
                    currentTokenType = BlockTypeKind.FunctionId;
                }
                else
                {
                    currentTokenType = BlockTypeKind.NamedArg;
                }
            }
        }

        currentTokenContent.Append(nextChar);
        switch (currentTokenType)
        {
            case BlockTypeKind.Value:
                blocks.Add(new ValueBlock(currentTokenContent.ToString(), _loggerFactory));
                break;
            case BlockTypeKind.Variable:
                blocks.Add(new VariableBlock(currentTokenContent.ToString(), _loggerFactory));
                break;
            case BlockTypeKind.FunctionId:
                var tokenContent = currentTokenContent.ToString();

                if (PlaceHolderTokenizer.IsValidNamedArg(tokenContent))
                {
                    blocks.Add(new NamedArgBlock(tokenContent, _loggerFactory));
                }
                else
                {
                    blocks.Add(new FunctionIdBlock(currentTokenContent.ToString(), _loggerFactory));
                }
                break;

            case BlockTypeKind.NamedArg:
                blocks.Add(new NamedArgBlock(currentTokenContent.ToString(), _loggerFactory));
                break;

            case BlockTypeKind.Undefined:
                throw new CoreException("Tokens must be separated by one space least");
        }

        return blocks;
    }

    private static bool IsVarPrefix(char c)
    {
        return (c == Symbol.VAR_PREFIX);
    }

    private static bool IsBlankSpace(char c)
    {
        return c is Symbol.SPACE or Symbol.NEW_LINE or Symbol.CARRIAGE_RETURN or Symbol.TAB;
    }

    private static bool IsQuote(char c)
    {
        return c is Symbol.DBL_QUOTE or Symbol.SGL_QUOTE;
    }

    private static bool CanBeEscaped(char c)
    {
        return c is Symbol.DBL_QUOTE or Symbol.SGL_QUOTE or Symbol.ESCAPE_CHAR;
    }

    [SuppressMessage("Design", "CA1031:Modify to catch a more specific allowed exception type, or rethrow exception",
    Justification = "Does not throw an exception by design.")]
    private static bool IsValidNamedArg(string tokenContent)
    {
        try
        {
            var tokenContentAsNamedArg = new NamedArgBlock(tokenContent);
            return tokenContentAsNamedArg.IsValid(out _);
        }
        catch
        {
            return false;
        }
    }
}
