using AISmarteasy.Core.Prompt.Template;
using Microsoft.Extensions.Logging;

namespace AISmarteasy.Core.Prompt;

public sealed class PromptTemplateTokenizer
{
    public PromptTemplateTokenizer()
    {
        _loggerFactory = LoggerFactoryProvider.Default;
        _codeTokenizer = new PlaceHolderTokenizer(_loggerFactory);
    }

    public IList<IBlock> Tokenize(string text)
    {
        const int Empty_CodeBlock_Length = 4;
        const int Min_CodeBlock_Length = Empty_CodeBlock_Length + 1;

        if (string.IsNullOrEmpty(text))
        {
            return new List<IBlock> { new TextBlock(string.Empty, _loggerFactory) };
        }

        if (text.Length < Min_CodeBlock_Length)
        {
            return new List<IBlock> { new TextBlock(text, _loggerFactory) };
        }

        var blocks = new List<IBlock>();

        var endOfLastBlock = 0;

        var blockStartPos = 0;
        var blockStartFound = false;

        var insideTextValue = false;
        var textValueDelimiter = '\0';

        var skipNextChar = false;
        var nextChar = text[0];

        for (var nextCharCursor = 1; nextCharCursor < text.Length; nextCharCursor++)
        {
            var currentCharPos = nextCharCursor - 1;
            var currentChar = nextChar;
            nextChar = text[nextCharCursor];

            if (skipNextChar)
            {
                skipNextChar = false;
                continue;
            }

            if (!insideTextValue && currentChar == Symbol.BLOCK_STARTER && nextChar == Symbol.BLOCK_STARTER)
            {
                blockStartPos = currentCharPos;
                blockStartFound = true;
            }

            if (blockStartFound)
            {
                if (insideTextValue)
                {
                    if (currentChar == Symbol.ESCAPE_CHAR && CanBeEscaped(nextChar))
                    {
                        skipNextChar = true;
                        continue;
                    }

                    if (currentChar == textValueDelimiter)
                    {
                        insideTextValue = false;
                    }
                }
                else
                {
                    if (IsQuote(currentChar))
                    {
                        insideTextValue = true;
                        textValueDelimiter = currentChar;
                    }
                    else if (currentChar == Symbol.BLOCK_ENDER && nextChar == Symbol.BLOCK_ENDER)
                    {
                        if (blockStartPos > endOfLastBlock)
                        {
                            blocks.Add(new TextBlock(text, endOfLastBlock, blockStartPos, _loggerFactory));
                        }

                        var contentWithDelimiters = SubStr(text, blockStartPos, nextCharCursor + 1);

                        var contentWithoutDelimiters = contentWithDelimiters
                            .Substring(2, contentWithDelimiters.Length - Empty_CodeBlock_Length)
                            .Trim();

                        if (contentWithoutDelimiters.Length == 0)
                        {
                            blocks.Add(new TextBlock(contentWithDelimiters, _loggerFactory));
                        }
                        else
                        {
                            var placeHolderBlocks = _codeTokenizer.Tokenize(contentWithoutDelimiters);

                            switch (placeHolderBlocks[0].Type)
                            {
                                case BlockTypeKind.Variable:
                                    if (placeHolderBlocks.Count > 1)
                                    {
                                        throw new CoreException($"Invalid token detected after the variable: {contentWithoutDelimiters}");
                                    }

                                    blocks.Add(placeHolderBlocks[0]);
                                    break;

                                case BlockTypeKind.Value:
                                    if (placeHolderBlocks.Count > 1)
                                    {
                                        throw new CoreException($"Invalid token detected after the value: {contentWithoutDelimiters}");
                                    }

                                    blocks.Add(placeHolderBlocks[0]);
                                    break;

                                case BlockTypeKind.FunctionId:
                                    blocks.Add(new PlaceHolderBlock(placeHolderBlocks, contentWithoutDelimiters, _loggerFactory));
                                    break;
                                case BlockTypeKind.Undefined:
                                case BlockTypeKind.Text:
                                case BlockTypeKind.PlaceHolder:
                                default:
                                    throw new CoreException($"Code tokenizer returned an incorrect first token type {placeHolderBlocks[0].Type:G}");
                            }
                        }

                        endOfLastBlock = nextCharCursor + 1;
                        blockStartFound = false;
                    }
                }
            }
        }

        if (endOfLastBlock < text.Length)
        {
            blocks.Add(new TextBlock(text, endOfLastBlock, text.Length, _loggerFactory));
        }

        return blocks;
    }

    private readonly ILoggerFactory _loggerFactory;
    private readonly PlaceHolderTokenizer _codeTokenizer;

    private static string SubStr(string text, int startIndex, int stopIndex)
    {
        return text.Substring(startIndex, stopIndex - startIndex);
    }

    private static bool IsQuote(char c)
    {
        return c is Symbol.DBL_QUOTE or Symbol.SGL_QUOTE;
    }

    private static bool CanBeEscaped(char c)
    {
        return c is Symbol.DBL_QUOTE or Symbol.SGL_QUOTE or Symbol.ESCAPE_CHAR;
    }
}
