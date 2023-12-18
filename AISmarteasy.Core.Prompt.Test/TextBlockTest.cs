using AISmarteasy.Core.Prompt.Template;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISmarteasy.Core.Prompt.Test;

public class TextBlockTest
{
    private readonly ILogger _logger = NullLogger.Instance;

    [Test]
    public void ItHasTheCorrectType()
    {
        var target = new TextBlock("", _logger);
        Assert.That(target.Type, Is.EqualTo(BlockTypeKind.Text));
    }

    [Test]
    public void ItPreservesEmptyValues()
    {
        Assert.That(new TextBlock("", _logger).Content, Is.EqualTo(""));
        Assert.That(new TextBlock(" ", _logger).Content, Is.EqualTo(" "));
        Assert.That(new TextBlock("  ", _logger).Content, Is.EqualTo("  "));
        Assert.That(new TextBlock(" \n", _logger).Content, Is.EqualTo(" \n"));
    }

    [Test]
    public void ItIsAlwaysValid()
    {
        Assert.That(new TextBlock("", _logger).IsValid(out _));
        Assert.That(new TextBlock(" ", _logger).IsValid(out _));
        Assert.That(new TextBlock("  ", _logger).IsValid(out _));
        Assert.That(new TextBlock(" \n", _logger).IsValid(out _));
        Assert.That(new TextBlock(" \nabc", _logger).IsValid(out _));
    }

    [Test]
    public void ItRendersTheContentAsIs()
    {
        Assert.That(new TextBlock("", _logger).Render(null), Is.EqualTo(""));
        Assert.That(new TextBlock(" ", _logger).Render(null), Is.EqualTo(" "));
        Assert.That(new TextBlock("  ", _logger).Render(null), Is.EqualTo("  "));
        Assert.That(new TextBlock(" \n", _logger).Render(null), Is.EqualTo(" \n"));
        Assert.That(new TextBlock("'x'", _logger).Render(null), Is.EqualTo("'x'"));

    }
}
