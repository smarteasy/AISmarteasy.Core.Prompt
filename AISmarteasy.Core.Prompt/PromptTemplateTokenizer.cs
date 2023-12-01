using AISmarteasy.Core.Prompt.Template;
using Microsoft.Extensions.Logging;

namespace AISmarteasy.Core.Prompt;

public sealed class PromptTemplateTokenizer
{
    public PromptTemplateTokenizer(ILoggerFactory? loggerFactory)
    {
        _loggerFactory = loggerFactory?? LoggerFactoryProvider.Default;
        _placeHolderTokenizer = new PlaceHolderTokenizer(_loggerFactory);
    }

    public IList<IBlock> Tokenize(string promptTemplate)
    {
        const int Empty_CodeBlock_Length = 4;

        var blocks = new List<IBlock>();

        var endOfLastBlock = 0;
        var blockStartPos = 0;
        var blockStartFound = false;

        var insideTextValue = false;
        var textValueDelimiter = '\0';

        var skipNextChar = false;
        var nextChar = promptTemplate[0];

        for (var nextCharCursor = 1; nextCharCursor < promptTemplate.Length; nextCharCursor++)
        {
            var currentCharPos = nextCharCursor - 1;
            var currentChar = nextChar;
            nextChar = promptTemplate[nextCharCursor];

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
                            blocks.Add(new TextBlock(promptTemplate, endOfLastBlock, blockStartPos, _loggerFactory));
                        }

                        var contentWithDelimiters = SubStr(promptTemplate, blockStartPos, nextCharCursor + 1);

                        var contentWithoutDelimiters = contentWithDelimiters
                            .Substring(2, contentWithDelimiters.Length - Empty_CodeBlock_Length)
                            .Trim();

                        if (contentWithoutDelimiters.Length == 0)
                        {
                            blocks.Add(new TextBlock(contentWithDelimiters, _loggerFactory));
                        }
                        else
                        {
                            var placeHolderBlocks = _placeHolderTokenizer.Tokenize(contentWithoutDelimiters);

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
                                    throw new CoreException($"Placeholder tokenizer returned an incorrect first token type {placeHolderBlocks[0].Type:G}");
                            }
                        }

                        endOfLastBlock = nextCharCursor + 1;
                        blockStartFound = false;
                    }
                }
            }
        }

        if (endOfLastBlock < promptTemplate.Length)
        {
            blocks.Add(new TextBlock(promptTemplate, endOfLastBlock, promptTemplate.Length, _loggerFactory));
        }

        return blocks;
    }

    private readonly ILoggerFactory _loggerFactory;
    private readonly PlaceHolderTokenizer _placeHolderTokenizer;

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
