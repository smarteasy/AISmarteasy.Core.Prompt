using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace AISmarteasy.Core.Prompt.Template;

internal sealed class VariableBlock : Block//, INonCodeBlock, INonFunctionRenderer
{
    public string Name { get; } = string.Empty;
    private static readonly Regex ValidNameRegex = new("^[a-zA-Z0-9_]*$");

    public VariableBlock(string text, ILoggerFactory? loggerFactory = null)
        : base(BlockTypeKind.Variable, text.Trim(), loggerFactory)
    {
        if (Content.Length < 2)
        {
            Logger.LogError("The variable name is empty");
            return;
        }

        Name = Content[1..];
    }

    public override bool IsValid(out string errorMsg)
    {
        errorMsg = string.Empty;

        if (string.IsNullOrEmpty(Content))
        {
            errorMsg = $"A variable must start with the symbol {Symbol.VAR_PREFIX} and have a name";
            Logger.LogError(errorMsg);
            return false;
        }

        if (Content[0] != Symbol.VAR_PREFIX)
        {
            errorMsg = $"A variable must start with the symbol {Symbol.VAR_PREFIX}";
            Logger.LogError(errorMsg);
            return false;
        }

        if (Content.Length < 2)
        {
            errorMsg = "The variable name is empty";
            Logger.LogError(errorMsg);
            return false;
        }

        if (!ValidNameRegex.IsMatch(Name))
        {
            errorMsg = $"The variable name '{Name}' contains invalid characters. " +
                       "Only alphanumeric chars and underscore are allowed.";
            Logger.LogError(errorMsg);
            return false;
        }

        return true;
    }

    public override string Render(ContextVariableDictionary? variables)
    {
        if (variables == null) { return string.Empty; }

        if (string.IsNullOrEmpty(Name))
        {
            const string ErrMsg = "Variable rendering failed, the variable name is empty";
            Logger.LogError(ErrMsg);
            throw new CoreException(ErrMsg);
        }

        if (variables.TryGetValue(Name, out string? value))
        {
            return value;
        }

        Logger.LogWarning("Variable `{0}{1}` not found", Symbol.VAR_PREFIX, Name);

        return string.Empty;
    }
}