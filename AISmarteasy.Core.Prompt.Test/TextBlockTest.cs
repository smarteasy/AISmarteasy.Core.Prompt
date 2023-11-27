using AISmarteasy.Core.Prompt.Template;

namespace AISmarteasy.Core.Prompt.Test;

public class TextBlockTest
{
    [Test]
    public void ItHasTheCorrectType()
    {
        var target = new TextBlock("");
        Assert.That(target.Type, Is.EqualTo(BlockTypeKind.Text));
    }

    [Test]
    public void ItPreservesEmptyValues()
    {
        Assert.That(new TextBlock("").Content, Is.EqualTo(""));
        Assert.That(new TextBlock(" ").Content, Is.EqualTo(" "));
        Assert.That(new TextBlock("  ").Content, Is.EqualTo("  "));
        Assert.That(new TextBlock(" \n").Content, Is.EqualTo(" \n"));
    }

    [Test]
    public void ItIsAlwaysValid()
    {
        Assert.That(new TextBlock("").IsValid(out _));
        Assert.That(new TextBlock(" ").IsValid(out _));
        Assert.That(new TextBlock("  ").IsValid(out _));
        Assert.That(new TextBlock(" \n").IsValid(out _));
        Assert.That(new TextBlock(" \nabc").IsValid(out _));
    }

    [Test]
    public void ItRendersTheContentAsIs()
    {
        Assert.That(new TextBlock("").Render(null), Is.EqualTo(""));
        Assert.That(new TextBlock(" ").Render(null), Is.EqualTo(" "));
        Assert.That(new TextBlock("  ").Render(null), Is.EqualTo("  "));
        Assert.That(new TextBlock(" \n").Render(null), Is.EqualTo(" \n"));
        Assert.That(new TextBlock("'x'").Render(null), Is.EqualTo("'x'"));

    }
}
