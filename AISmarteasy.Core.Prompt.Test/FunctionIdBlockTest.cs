using AISmarteasy.Core.Prompt.Template;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISmarteasy.Core.Prompt.Test;

public class FunctionIdBlockTest
{
    private readonly ILogger _logger = NullLogger.Instance;

    [Test]
    public void ItHasTheCorrectType()
    {
        var target = new FunctionIdBlock("", _logger);
        Assert.That(target.Type, Is.EqualTo(BlockTypeKind.FunctionId));
    }

    [Test]
    public void ItTrimsSpaces()
    {
        Assert.That(new FunctionIdBlock("  aa  ", _logger).Content, Is.EqualTo("aa"));
    }

    [Test]
    public void ItAllowsUnderscoreDotsLettersAndDigits()
    {
        Assert.True(new FunctionIdBlock("0", _logger).IsValid(out _));
        Assert.False(new FunctionIdBlock("-", _logger).IsValid(out _));
        Assert.False(new FunctionIdBlock("a b", _logger).IsValid(out _));
        Assert.False(new FunctionIdBlock("a\nb", _logger).IsValid(out _));
    }

    [Test]
    public void ItAllowsOnlyOneDot()
    {
        var target1 = new FunctionIdBlock("functionName", _logger);
        var target2 = new FunctionIdBlock("pluginName.functionName", _logger);
        Assert.Throws<CoreException>(() =>
        {
            var _ = new FunctionIdBlock("foo.pluginName.functionName", _logger);
        });

        Assert.True(target1.IsValid(out _));
        Assert.True(target2.IsValid(out _));
    }
}