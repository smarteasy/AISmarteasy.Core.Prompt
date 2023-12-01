using AISmarteasy.Core.Prompt.Template;
using Microsoft.Extensions.Logging.Abstractions;
using static System.Reflection.Metadata.BlobBuilder;

namespace AISmarteasy.Core.Prompt.Test;

public class PlaceHolderBlockTest
{
    [Test]
    public void ItRequiresAValidFunctionCall()
    {
        var funcId = new FunctionIdBlock("funcName");
        var valBlock = new ValueBlock("'value'");
        var varBlock = new VariableBlock("$var");
        var namedArgBlock = new NamedArgBlock("varName='foo'");

        var placeHolderBlock1 = new PlaceHolderBlock(new List<IBlock> { funcId, valBlock }, null);
        var placeHolderBlock2 = new PlaceHolderBlock(new List<IBlock> { funcId, varBlock }, null);
        var placeHolderBlock3 = new PlaceHolderBlock(new List<IBlock> { funcId, funcId }, null);
        var placeHolderBlock5 = new PlaceHolderBlock(new List<IBlock> { funcId, varBlock, namedArgBlock }, null);
        var placeHolderBlock6 = new PlaceHolderBlock(new List<IBlock> { varBlock, valBlock }, null);
        var placeHolderBlock7 = new PlaceHolderBlock(new List<IBlock> { namedArgBlock }, null);

        Assert.True(placeHolderBlock1.IsValid(out _));
        Assert.True(placeHolderBlock2.IsValid(out _));

        Assert.False(placeHolderBlock3.IsValid(out var errorMessage3));
        Assert.That(errorMessage3,
            Is.EqualTo("The first arg of a function must be a quoted string, variable or named argument"));

        Assert.False(placeHolderBlock3.IsValid(out var errorMessage4));
        Assert.That(errorMessage4,
            Is.EqualTo("The first arg of a function must be a quoted string, variable or named argument"));


        Assert.True(placeHolderBlock5.IsValid(out var errorMessage5));
        Assert.IsEmpty(errorMessage5);

        Assert.False(placeHolderBlock6.IsValid(out var errorMessage6));
        Assert.That(errorMessage6, Is.EqualTo("Unexpected second token found: 'value'"));

        Assert.False(placeHolderBlock7.IsValid(out var errorMessage7));
        Assert.That(errorMessage7, Is.EqualTo("Unexpected named argument found. Expected function name first."));
    }

    [Test]
    public async Task ItRendersCodeBlockConsistingOfJustAVarBlock1Async()
    {
        var variables = new ContextVariableDictionary { ["varName"] = "foo" };
        var placeHolderTokenizer = new PlaceHolderTokenizer();
        var blocks = placeHolderTokenizer.Tokenize("$varName");
        var placeHolderBlock = new PlaceHolderBlock(blocks, NullLoggerFactory.Instance);
        var result = await placeHolderBlock.RenderAsync(variables, false);

        Assert.That(result, Is.EqualTo("foo"));
    }

    [Test]
    public async Task ItRendersCodeBlockConsistingOfJustAVarBlock2Async()
    {
        var variables = new ContextVariableDictionary { ["varName"] = "bar" };
        var varBlock = new VariableBlock("$varName");

        var placeHolderBlock = new PlaceHolderBlock(new List<IBlock> { varBlock }, NullLoggerFactory.Instance);
        var result = await placeHolderBlock.RenderAsync(variables, false);

        Assert.That(result, Is.EqualTo("bar"));
    }
}