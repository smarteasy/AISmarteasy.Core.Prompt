using Microsoft.Extensions.Logging;

namespace AISmarteasy.Core.Prompt.Template;

internal sealed class PlaceHolderBlock : Block
{
    private bool _validated;
    private readonly FunctionRenderer _functionRenderer;
    private readonly List<IBlock> _blocks;

    public PlaceHolderBlock(List<IBlock> blocks, ILoggerFactory? loggerFactory)
        : this(blocks, string.Empty, loggerFactory)
    {
    }

    public PlaceHolderBlock(List<IBlock> blocks, string text, ILoggerFactory? loggerFactory)
        : base(BlockTypeKind.PlaceHolder, text, loggerFactory)
    {
        _blocks = blocks;
        _functionRenderer = new FunctionRenderer();
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

    public override string Render(ContextVariableDictionary? variables)
    {
        return string.Empty;
    }

    public async Task<string> RenderAsync(ContextVariableDictionary variables, bool isNeedFunctionRun, CancellationToken cancellationToken = default)
    {
        if (!_validated && !IsValid(out var error))
        {
            throw new CoreException(error);
        }

        Logger.LogTrace("Rendering code: `{Content}`", Content);

        if (isNeedFunctionRun)
        {
            return _blocks[0].Type switch
            {
                BlockTypeKind.Value => _blocks[0].Render(variables),
                BlockTypeKind.Variable => _blocks[0].Render(variables),
                BlockTypeKind.FunctionId => await _functionRenderer.RenderAsync(_blocks, cancellationToken).ConfigureAwait(false),
                _ => throw new CoreException($"Unexpected first token type: {_blocks[0].Type:G}")
            };
        }
        else
        {
            return "{{" + Content + "}}";
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
