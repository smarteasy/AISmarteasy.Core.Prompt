using AISmarteasy.Core.Prompt.Template;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISmarteasy.Core.Prompt.Test;

public class PlaceHolderBlockTest
{
    private readonly ILogger _logger = NullLogger.Instance;

    [Test]
    public void ItRequiresAValidFunctionCall()
    {
        var funcId = new FunctionIdBlock("funcName", _logger);
        var valBlock = new ValueBlock("'value'", _logger);
        var varBlock = new VariableBlock("$var", _logger);
        var namedArgBlock = new NamedArgBlock("varName='foo'", _logger);

        var placeHolderBlock1 = new PlaceHolderBlock(new List<IBlock> { funcId, valBlock }, _logger);
        var placeHolderBlock2 = new PlaceHolderBlock(new List<IBlock> { funcId, varBlock }, _logger);
        var placeHolderBlock3 = new PlaceHolderBlock(new List<IBlock> { funcId, funcId }, _logger);
        var placeHolderBlock5 = new PlaceHolderBlock(new List<IBlock> { funcId, varBlock, namedArgBlock }, _logger);
        var placeHolderBlock6 = new PlaceHolderBlock(new List<IBlock> { varBlock, valBlock }, _logger);
        var placeHolderBlock7 = new PlaceHolderBlock(new List<IBlock> { namedArgBlock }, _logger);

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
        var placeHolderTokenizer = new PlaceHolderTokenizer(_logger);
        var blocks = placeHolderTokenizer.Tokenize("$varName");
        var placeHolderBlock = new PlaceHolderBlock(blocks, _logger);
        var result = await placeHolderBlock.RenderAsync(variables, false);

        Assert.That(result, Is.EqualTo("foo"));
    }

    [Test]
    public async Task ItRendersCodeBlockConsistingOfJustAVarBlock2Async()
    {
        var variables = new ContextVariableDictionary { ["varName"] = "bar" };
        var varBlock = new VariableBlock("$varName", _logger);

        var placeHolderBlock = new PlaceHolderBlock(new List<IBlock> { varBlock }, _logger);
        var result = await placeHolderBlock.RenderAsync(variables, false);

        Assert.That(result, Is.EqualTo("bar"));
    }
}