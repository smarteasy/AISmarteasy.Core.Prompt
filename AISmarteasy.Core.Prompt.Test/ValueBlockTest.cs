using AISmarteasy.Core.Prompt.Template;

namespace AISmarteasy.Core.Prompt.Test;

public class ValueBlockTest
{
    [Test]
    public void ItHasTheCorrectType()
    {
        var target = new ValueBlock("");
        Assert.That(target.Type, Is.EqualTo(BlockTypeKind.Value));
    }

    [Test]
    public void ItTrimsSpaces()
    {
        Assert.That(new ValueBlock("  ' '  ").Content, Is.EqualTo("' '"));
        Assert.That(new ValueBlock("  \"  \"  ").Content, Is.EqualTo("\"  \""));
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
        var validBlock1 = new ValueBlock("'ciao'");
        var validBlock2 = new ValueBlock("\"hello\"");
        var badBlock1 = new ValueBlock("'nope\"");
        var badBlock2 = new ValueBlock("'no\"");

        Assert.True(validBlock1.IsValid(out _));
        Assert.True(validBlock2.IsValid(out _));
        Assert.False(badBlock1.IsValid(out _));
        Assert.False(badBlock2.IsValid(out _));
    }
}
