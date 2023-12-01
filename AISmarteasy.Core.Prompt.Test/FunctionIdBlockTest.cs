using AISmarteasy.Core.Prompt.Template;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISmarteasy.Core.Prompt.Test;

public class FunctionIdBlockTest
{
    [Test]
    public void ItHasTheCorrectType()
    {
        var target = new FunctionIdBlock("");
        Assert.That(target.Type, Is.EqualTo(BlockTypeKind.FunctionId));
    }

    [Test]
    public void ItTrimsSpaces()
    {
        Assert.That(new FunctionIdBlock("  aa  ", NullLoggerFactory.Instance).Content, Is.EqualTo("aa"));
    }

    [Test]
    public void ItAllowsUnderscoreDotsLettersAndDigits()
    {
        Assert.True(new FunctionIdBlock("0").IsValid(out _));
        Assert.False(new FunctionIdBlock("-").IsValid(out _));
        Assert.False(new FunctionIdBlock("a b").IsValid(out _));
        Assert.False(new FunctionIdBlock("a\nb").IsValid(out _));
    }

    [Test]
    public void ItAllowsOnlyOneDot()
    {
        var target1 = new FunctionIdBlock("functionName");
        var target2 = new FunctionIdBlock("pluginName.functionName");
        Assert.Throws<CoreException>(() =>
        {
            var _ = new FunctionIdBlock("foo.pluginName.functionName");
        });

        Assert.True(target1.IsValid(out _));
        Assert.True(target2.IsValid(out _));
    }
}