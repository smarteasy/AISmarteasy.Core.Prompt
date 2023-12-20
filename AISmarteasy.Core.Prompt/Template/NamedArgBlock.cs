using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace AISmarteasy.Core.Prompt.Template;

internal sealed class NamedArgBlock : Block
{
    internal string Name { get; }

    public NamedArgBlock(string text, ILogger logger)
        : base(BlockTypeKind.NamedArg, TrimWhitespace(text), logger)
    {
        var argParts = Content.Split(Symbol.NAMED_ARG_BLOCK_SEPARATOR);
        if (argParts.Length != 2)
        {
            Logger.LogError("Invalid named argument `{Text}`", text);
            throw new CoreException($"A function named argument must contain a name and value separated by a '{Symbol.NAMED_ARG_BLOCK_SEPARATOR}' character.");
        }

        Name = argParts[0];
        _argNameAsVarBlock = new VariableBlock($"{Symbol.VAR_PREFIX}{argParts[0]}", logger);
        var argValue = argParts[1];
        if (argValue.Length == 0)
        {
            Logger.LogError("Invalid named argument `{Text}`", text);
            throw new CoreException($"A function named argument must contain a quoted value or variable after the '{Symbol.NAMED_ARG_BLOCK_SEPARATOR}' character.");
        }

        if (argValue[0] == Symbol.VAR_PREFIX)
        {
            _argValueAsVarBlock = new VariableBlock(argValue, logger);
        }
        else
        {
            _valBlock = new ValueBlock(argValue, logger);
        }
    }

    public override bool IsValid(out string errorMsg)
    {
        errorMsg = string.Empty;
        if (string.IsNullOrEmpty(Name))
        {
            errorMsg = "A named argument must have a name";
            Logger.LogError(errorMsg);
            return false;
        }

        if (_valBlock != null && !_valBlock.IsValid(out var valErrorMsg))
        {
            errorMsg = $"There was an issue with the named argument value for '{Name}': {valErrorMsg}";
            Logger.LogError(errorMsg);
            return false;
        }
        else if (_argValueAsVarBlock != null && !_argValueAsVarBlock.IsValid(out var variableErrorMsg))
        {
            errorMsg = $"There was an issue with the named argument value for '{Name}': {variableErrorMsg}";
            Logger.LogError(errorMsg);
            return false;
        }
        else if (_valBlock == null && _argValueAsVarBlock == null)
        {
            errorMsg = "A named argument must have a value";
            Logger.LogError(errorMsg);
            return false;
        }

        if (!_argNameAsVarBlock.IsValid(out var argNameErrorMsg))
        {
            errorMsg = Regex.Replace(argNameErrorMsg, "a variable", "An argument", RegexOptions.IgnoreCase);
            errorMsg = Regex.Replace(errorMsg, "the variable", "The argument", RegexOptions.IgnoreCase);
            return false;
        }

        return true;
    }

    private readonly VariableBlock _argNameAsVarBlock;
    private readonly ValueBlock? _valBlock;
    private readonly VariableBlock? _argValueAsVarBlock;

    private static string TrimWhitespace(string text)
    {
        string[] trimmedParts = GetTrimmedParts(text);
        switch (trimmedParts.Length)
        {
            case 2:
                return $"{trimmedParts[0]}{Symbol.NAMED_ARG_BLOCK_SEPARATOR}{trimmedParts[1]}";
            case 1:
                return trimmedParts[0];
            default:
                throw new CoreException("Poorly formatted.");
        }
    }

    private static string[] GetTrimmedParts(string text)
    {
        var parts = text.Split(new[] { Symbol.NAMED_ARG_BLOCK_SEPARATOR }, 2);
        var result = new string[parts.Length];
        if (parts.Length > 0)
        {
            result[0] = parts[0].Trim();
        }

        if (parts.Length > 1)
        {
            result[1] = parts[1].Trim();
        }

        return result;
    }

    internal string GetValue(VariableDictionary? variables)
    {
        var valueIsValidValBlock = _valBlock != null && _valBlock.IsValid(out _);
        if (valueIsValidValBlock)
        {
            return _valBlock!.Render(variables);
        }

        var valueIsValidVarBlock = _argValueAsVarBlock != null && _argValueAsVarBlock.IsValid(out _);
        if (valueIsValidVarBlock)
        {
            return _argValueAsVarBlock!.Render(variables);
        }

        return string.Empty;
    }
}