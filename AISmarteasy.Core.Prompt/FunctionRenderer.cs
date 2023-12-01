using AISmarteasy.Core.Prompt.Template;
using Microsoft.Extensions.Logging;

namespace AISmarteasy.Core.Prompt;

public class FunctionRenderer
{
    //private readonly IKernel _kernel;
    private readonly ILogger _logger;

    public FunctionRenderer()
    {
        //_kernel = KernelProvider.Kernel;
        _logger = LoggerFactoryProvider.Default.CreateLogger(typeof(FunctionRenderer));
    }

    public async Task<string> RenderAsync(IList<IBlock> blocks, CancellationToken cancellationToken = default)
    {
        //    var fBlock = (FunctionIdBlock)blocks[0];
        //    var function = GetFunctionFromPlugins(fBlock);
        //    if (function == null)
        //    {
        //        var errorMsg = $"Function `{fBlock.Content}` not found";
        //        _logger.LogError(errorMsg);
        //        throw new CoreException(errorMsg);
        //    }

        //if (blocks.Count > 1)
        //{
        //    _kernel.Context = PopulateContextWithFunctionArguments(blocks);
        //}

        //try
        //{
        //    var requestSetting = AIRequestSettingProvider.ProvideFromCompletionConfig(new PromptTemplateConfig().Completion);
        //    await function.RunAsync(requestSetting, cancellationToken).ConfigureAwait(false);
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, "Function {Plugin}.{Function} execution failed with error {Error}", function.PluginName, function.Name, ex.Message);
        //    throw;
        //}

        //return _kernel.Context.Result;

        return string.Empty;
    }

    //private IFunction GetFunctionFromPlugins(FunctionIdBlock functionBlock)
    //{
    //    var plugin = _kernel.Plugins[functionBlock.PluginName];
    //    return plugin.GetFunction(functionBlock.FunctionName);
    //}

    //public IContext PopulateContextWithFunctionArguments(IList<IBlock> blocks)
    //{
    //    var contextClone = _kernel.Context.Clone();
    //    var firstArg = blocks[1];

    //    _logger.LogTrace("Passing variable/value: `{Content}`", firstArg.Content);

    //    var namedArgsStartIndex = 1;
    //    if (firstArg.Type is not BlockTypeKind.NamedArg)
    //    {
    //        string input = ((INonFunctionRenderer)blocks[1]).Render(contextClone.Variables);
    //        contextClone.Variables.Update(input);
    //        namedArgsStartIndex++;
    //    }

    //    for (int i = namedArgsStartIndex; i < blocks.Count; i++)
    //    {
    //        var arg = blocks[i] as NamedArgBlock;

    //        if (arg == null)
    //        {
    //            var errorMsg = "Functions support up to one positional argument";
    //            _logger.LogError(errorMsg);
    //            throw new CoreException($"Unexpected first token type: {blocks[i].Type:G}");
    //        }

    //        _logger.LogTrace("Passing variable/value: `{Content}`", arg.Content);

    //        contextClone.Variables.Set(arg.Name, arg.GetValue(_kernel.Context.Variables));
    //    }

    //    return contextClone;
    //}
}
