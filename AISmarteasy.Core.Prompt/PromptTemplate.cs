using System.Text;
using AISmarteasy.Core.Prompt.Template;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISmarteasy.Core.Prompt;

public class PromptTemplate : IPromptTemplate
{
    public string Content { get; }

    private protected ILogger Logger { get; }

    public PromptTemplate(string content, ILoggerFactory? loggerFactory = null)
    {
        Content = content;
        Logger = loggerFactory is not null ? loggerFactory.CreateLogger(GetType()) : NullLogger.Instance;
    }

    public async Task<string> RenderAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogTrace("Rendering string template: {0}", Content);

        var blocks = ExtractBlocks(Content);
        return await RenderAsync(blocks, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> RenderAsync(IList<IBlock> blocks, CancellationToken cancellationToken = default)
    {
        Logger.LogTrace("Rendering list of {0} blocks", blocks.Count);

        var context = KernelProvider.Kernel.Context;
        var tasks = new List<Task<string>>(blocks.Count);
        foreach (var block in blocks)
        {
            tasks.Add(block.Type == BlockTypeKind.PlaceHolder
                ? ((PlaceHolderBlock)block).RenderAsync(context.Variables, true, cancellationToken)
                : Task.FromResult(block.Render(context.Variables)));
        }

        var result = new StringBuilder();
        foreach (Task<string> task in tasks)
        {
            result.Append(await task.ConfigureAwait(false));
        }

        Logger.LogTrace("Rendered prompt: {0}", result);

        return result.ToString();
    }

    private IList<IBlock> ExtractBlocks(string promptTemplate, bool validate = true)
    {
        Logger.LogTrace("Extracting blocks from template: {0}", promptTemplate);

        var tokenizer = new PromptTemplateTokenizer(Logger);
        var blocks = tokenizer.Tokenize(promptTemplate);

        if (validate)
        {
            foreach (var block in blocks)
            {
                if (!block.IsValid(out var error))
                {
                    throw new CoreException(error);
                }
            }
        }

        return blocks;
    }
}