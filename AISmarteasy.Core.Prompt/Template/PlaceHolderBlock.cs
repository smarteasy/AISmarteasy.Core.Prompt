using Microsoft.Extensions.Logging;

namespace AISmarteasy.Core.Prompt.Template;

internal sealed class PlaceHolderBlock : Block
{
    private bool _validated;
    private readonly FunctionRenderer _functionRenderer;
    private readonly List<IBlock> _blocks;

    public PlaceHolderBlock(List<IBlock> blocks, ILogger logger)
        : this(blocks, string.Empty, logger)
    {
    }

    public PlaceHolderBlock(List<IBlock> blocks, string text, ILogger logger)
        : base(BlockTypeKind.PlaceHolder, text, logger)
    {
        _blocks = blocks;
        _functionRenderer = new FunctionRenderer(Logger);
    }

    public override bool IsValid(out string errorMsg)
    {
        errorMsg = "";

        foreach (var block in _blocks)
        {
            if (!block.IsValid(out errorMsg))
            {
                Logger.LogError(errorMsg);
                return false;
            }
        }

        if (_blocks.Count > 0 && _blocks[0].Type == BlockTypeKind.NamedArg)
        {
            errorMsg = "Unexpected named argument found. Expected function name first.";
            Logger.LogError(errorMsg);
            return false;
        }

        if (_blocks.Count > 1 && !IsValidFunctionCall(out errorMsg))
        {
            return false;
        }

        _validated = true;

        return true;
    }

    public override async Task<string> RenderAsync(ContextVariableDictionary variables, bool isNeedFunctionRun, CancellationToken cancellationToken = default)
    {
        if (!_validated && !IsValid(out var error))
        {
            throw new CoreException(error);
        }

        Logger.LogTrace("Rendering code: `{Content}`", Content);

        switch (_blocks[0].Type)
        {
            case BlockTypeKind.NamedArg:
            case BlockTypeKind.Variable:
                return _blocks[0].Render(variables);
            case BlockTypeKind.FunctionId:
            {
                if (isNeedFunctionRun)
                {
                    var requestSetting = new LLMRequestSetting();
                    return await _functionRenderer.RenderAsync(_blocks, requestSetting, cancellationToken).ConfigureAwait(false);
                }
                return "{{" + Content + "}}";
            }
            default:
                throw new CoreException($"Unexpected first token type: {_blocks[0].Type:G}");
        }
    }

    private bool IsValidFunctionCall(out string errorMsg)
    {
        errorMsg = "";
        if (_blocks[0].Type != BlockTypeKind.FunctionId)
        {
            errorMsg = $"Unexpected second token found: {_blocks[1].Content}";
            Logger.LogError(errorMsg);
            return false;
        }

        if (_blocks[1].Type is not BlockTypeKind.Value and not BlockTypeKind.Variable and not BlockTypeKind.NamedArg)
        {
            errorMsg = "The first arg of a function must be a quoted string, variable or named argument";
            Logger.LogError(errorMsg);
            return false;
        }

        for (int i = 2; i < _blocks.Count; i++)
        {
            if (_blocks[i].Type is not BlockTypeKind.NamedArg)
            {
                errorMsg = $"Functions only support named arguments after the first argument. Argument {i} is not named.";
                Logger.LogError(errorMsg);
                return false;
            }
        }

        return true;
    }
}
