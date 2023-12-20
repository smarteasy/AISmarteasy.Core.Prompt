﻿using AISmarteasy.Core.Prompt.Template;
using Microsoft.Extensions.Logging;

namespace AISmarteasy.Core.Prompt;

public class FunctionRenderer(ILogger? logger)
{
    private readonly ILogger _logger = logger?? LoggerFactoryProvider.Default.CreateLogger(typeof(FunctionRenderer));

    public async Task<string> RenderAsync(IAIServiceConnector serviceConnector, IList<IBlock> blocks, LLMServiceSetting serviceSetting, CancellationToken cancellationToken = default)
    {
        var functionBlock = (FunctionIdBlock)blocks[0];
        var function = GetFunctionFromPlugins(functionBlock);
        if (function == null)
        {
            var errorMsg = $"Function `{functionBlock.Content}` not found";
            _logger.LogError(errorMsg);
            throw new CoreException(errorMsg);
        }

        if (blocks.Count > 1)
        {
            LLMWorkEnv.WorkerContext = PopulateContextWithFunctionArguments(blocks);
        }

        try
        {
            await function.RunAsync(serviceConnector, serviceSetting, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Function {Plugin}.{Function} execution failed with error {Error}", function.PluginName, function.Name, ex.Message);
            throw;
        }

        return LLMWorkEnv.WorkerContext.Result;
    }

    private IPluginFunction? GetFunctionFromPlugins(FunctionIdBlock functionBlock)
    {
        var plugin = LLMWorkEnv.PluginStore?.FindPlugin(functionBlock.PluginName);
        return plugin?.GetFunction(functionBlock.FunctionName);
    }

    public IWorkerContext PopulateContextWithFunctionArguments(IList<IBlock> blocks)
    {
        var contextClone = LLMWorkEnv.WorkerContext.Clone();
        var firstArg = blocks[1];

        _logger.LogTrace("Passing variable/value: `{Content}`", firstArg.Content);

        var namedArgsStartIndex = 1;
        if (firstArg.Type is not BlockTypeKind.NamedArg)
        {
            string input = blocks[1].Render(contextClone.Variables);
            contextClone.Variables.Update(input);
            namedArgsStartIndex++;
        }

        for (int i = namedArgsStartIndex; i < blocks.Count; i++)
        {
            var arg = blocks[i] as NamedArgBlock;

            if (arg == null)
            {
                var errorMsg = "Functions support up to one positional argument";
                _logger.LogError(errorMsg);
                throw new CoreException($"Unexpected first token type: {blocks[i].Type:G}");
            }

            _logger.LogTrace("Passing variable/value: `{Content}`", arg.Content);

            contextClone.Variables.Set(arg.Name, arg.GetValue(LLMWorkEnv.WorkerContext.Variables));
        }

        return contextClone;
    }
}
