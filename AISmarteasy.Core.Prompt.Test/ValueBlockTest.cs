using AISmarteasy.Core.Prompt.Template;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISmarteasy.Core.Prompt.Test;

public class ValueBlockTest
{
    private readonly ILogger _logger = NullLogger.Instance;

    [Test]
    public void ItHasTheCorrectType()
    {
        var target = new ValueBlock("", _logger);
        Assert.That(target.Type, Is.EqualTo(BlockTypeKind.Value));
    }

    [Test]
    public void ItTrimsSpaces()
    {
        Assert.That(new ValueBlock("  ' '  ", _logger).Content, Is.EqualTo("' '"));
        Assert.That(new ValueBlock("  \"  \"  ", _logger).Content, Is.EqualTo("\"  \""));
    }

    [Test]
    public void ItChecksIfAValueStartsWithQuote()
    {
        Assert.True(ValueBlock.HasPrefix("'"));
        Assert.True(ValueBlock.HasPrefix("'a"));
        Assert.True(ValueBlock.HasPrefix("\""));
        Assert.True(ValueBlock.HasPrefix("\"b"));

        Assert.False(ValueBlock.HasPrefix("d'"));
        Assert.False(ValueBlock.HasPrefix("e\""));
        Assert.False(ValueBlock.HasPrefix(""));
        Assert.False(ValueBlock.HasPrefix("v"));
        Assert.False(ValueBlock.HasPrefix("_"));
    }

    [Test]
    public void ItRequiresConsistentQuotes()
    {
        var validBlock1 = new ValueBlock("'ciao'", _logger);
        var validBlock2 = new ValueBlock("\"hello\"", _logger);
        var badBlock1 = new ValueBlock("'nope\"", _logger);
        var badBlock2 = new ValueBlock("'no\"", _logger);

        Assert.True(validBlock1.IsValid(out _));
        Assert.True(validBlock2.IsValid(out _));
        Assert.False(badBlock1.IsValid(out _));
        Assert.False(badBlock2.IsValid(out _));
    }
}
