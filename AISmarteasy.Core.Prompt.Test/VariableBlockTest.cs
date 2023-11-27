using AISmarteasy.Core.Prompt.Template;

namespace AISmarteasy.Core.Prompt.Test;

public class VariableBlockTest
{
    [Test]
    public void ItHasTheCorrectType()
    {
        var target = new VariableBlock("");
        Assert.That(target.Type, Is.EqualTo(BlockTypeKind.Variable));
    }

    [Test]
    public void ItTrimsSpaces()
    {
        Assert.That(new VariableBlock("  $  ").Content, Is.EqualTo("$"));
    }

    [Test]
    public void ItIgnoresSpacesAround()
    {
        Assert.That(new VariableBlock("  $var \n ").Content, Is.EqualTo("$var"));
    }

    [Test]
    public void ItRendersToEmptyStringIfVariableIsMissing()
    {
        var target = new VariableBlock("  $var \n ");
        var variables = new ContextVariableDictionary
        {
            ["foo"] = "bar"
        };

        var result = target.Render(variables);
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void ItRendersToVariableValueWhenAvailable()
    {
        var target = new VariableBlock("  $var \n ");
        var variables = new ContextVariableDictionary
        {
            ["foo"] = "bar",
            ["var"] = "able",
        };

        var result = target.Render(variables);
        Assert.That(result, Is.EqualTo("able"));
    }

    [Test]
    public void ItThrowsIfTheVarNameIsEmpty()
    {
        var variables = new ContextVariableDictionary
        {
            ["foo"] = "bar",
            ["var"] = "able",
        };
        var target = new VariableBlock(" $ ");

        Assert.Throws<CoreException>(() => target.Render(variables));
    }
}
