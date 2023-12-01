using AISmarteasy.Core.Prompt.Template;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISmarteasy.Core.Prompt.Test;

public class PlaceHolderBlockTest
{
    //    private readonly ILoggerFactory _logger = NullLoggerFactory.Instance;
    //    private readonly Kernel _kernel = new(new Mock<IAIServiceProvider>().Object);

    //[Test]
    //public async Task ItThrowsIfAFunctionDoesntExistAsync()
    //{
    //    // Arrange
    //    var context = new SKContext();
    //    var target = new CodeBlock("functionName", this._logger);

    //    // Act & Assert
    //    await Assert.ThrowsAsync<KeyNotFoundException>(() => target.RenderCodeAsync(this._kernel, context));
    //}

    //    [Fact]
    //    public async Task ItThrowsIfAFunctionCallThrowsAsync()
    //    {
    //        // Arrange
    //        var context = new SKContext();

    //        static void method() => throw new FormatException("error");
    //        var function = SKFunctionFactory.CreateFromMethod(method, "function", "description");

    //        this._kernel.Plugins.Add(new SKPlugin("plugin", new[] { function }));

    //        var target = new CodeBlock("plugin.function", this._logger);

    //        // Act & Assert
    //        await Assert.ThrowsAsync<FormatException>(() => target.RenderCodeAsync(this._kernel, context));
    //    }

    //[Test]
    //public void ItHasTheCorrectType()
    //{
    //    // Act
    //    var target = new PlaceHolderBlock("");

    //    // Assert
    //    Assert.Equal(BlockTypes.Code, target.Type);
    //}

    //[Test]
    //public void ItTrimsSpaces()
    //{
    //    Assert.That(target.Type, Is.EqualTo(BlockTypeKind.Value));
    //    Assert.Equal("aa", new PlaceHolderBlock("  aa  ");
    //}

    //    [Fact]
    //    public void ItChecksValidityOfInternalBlocks()
    //    {
    //        // Arrange
    //        var validBlock1 = new FunctionIdBlock("x");
    //        var validBlock2 = new ValBlock("''");
    //        var invalidBlock = new VarBlock("");

    //        // Act
    //        var codeBlock1 = new CodeBlock(new List<Block> { validBlock1, validBlock2 }, "", NullLoggerFactory.Instance);
    //        var codeBlock2 = new CodeBlock(new List<Block> { validBlock1, invalidBlock }, "", NullLoggerFactory.Instance);

    //        // Assert
    //        Assert.True(codeBlock1.IsValid(out _));
    //        Assert.False(codeBlock2.IsValid(out _));
    //    }

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
        Assert.That(errorMessage3, Is.EqualTo("The first arg of a function must be a quoted string, variable or named argument"));

        Assert.False(placeHolderBlock3.IsValid(out var errorMessage4));
        Assert.That(errorMessage4, Is.EqualTo("The first arg of a function must be a quoted string, variable or named argument"));


        Assert.True(placeHolderBlock5.IsValid(out var errorMessage5));
        Assert.IsEmpty(errorMessage5);

        Assert.False(placeHolderBlock6.IsValid(out var errorMessage6));
        Assert.That(errorMessage6, Is.EqualTo("Unexpected second token found: 'value'"));

        Assert.False(placeHolderBlock7.IsValid(out var errorMessage7));
        Assert.That(errorMessage7, Is.EqualTo("Unexpected named argument found. Expected function name first."));
    }

    //[Test]
    //public async Task ItRendersCodeBlockConsistingOfJustAVarBlock1Async()
    //{
    //    // Arrange
    //    var variables = new ContextVariables { ["varName"] = "foo" };
    //    var context = new SKContext(variables);

    //    // Act
    //    var codeBlock = new CodeBlock("$varName", NullLoggerFactory.Instance);
    //    var result = await codeBlock.RenderCodeAsync(this._kernel, context);

    //    // Assert
    //    Assert.Equal("foo", result);
    //}

    //    [Fact]
    //    public async Task ItRendersCodeBlockConsistingOfJustAVarBlock2Async()
    //    {
    //        // Arrange
    //        var variables = new ContextVariables { ["varName"] = "bar" };
    //        var context = new SKContext(variables);
    //        var varBlock = new VarBlock("$varName");

    //        // Act
    //        var codeBlock = new CodeBlock(new List<Block> { varBlock }, "", NullLoggerFactory.Instance);
    //        var result = await codeBlock.RenderCodeAsync(this._kernel, context);

    //        // Assert
    //        Assert.Equal("bar", result);
    //    }

    //    [Fact]
    //    public async Task ItRendersCodeBlockConsistingOfJustAValBlock1Async()
    //    {
    //        // Arrange
    //        var context = new SKContext();

    //        // Act
    //        var codeBlock = new CodeBlock("'ciao'", NullLoggerFactory.Instance);
    //        var result = await codeBlock.RenderCodeAsync(this._kernel, context);

    //        // Assert
    //        Assert.Equal("ciao", result);
    //    }

    //    [Fact]
    //    public async Task ItRendersCodeBlockConsistingOfJustAValBlock2Async()
    //    {
    //        // Arrange
    //        var context = new SKContext();
    //        var valBlock = new ValBlock("'arrivederci'");

    //        // Act
    //        var codeBlock = new CodeBlock(new List<Block> { valBlock }, "", NullLoggerFactory.Instance);
    //        var result = await codeBlock.RenderCodeAsync(this._kernel, context);

    //        // Assert
    //        Assert.Equal("arrivederci", result);
    //    }

    //    [Fact]
    //    public async Task ItInvokesFunctionCloningAllVariablesAsync()
    //    {
    //        // Arrange
    //        var variables = new ContextVariables { ["input"] = "zero", ["var1"] = "uno", ["var2"] = "due" };
    //        var context = new SKContext(variables);
    //        var funcBlock = new FunctionIdBlock("plugin.function");

    //        var canary0 = string.Empty;
    //        var canary1 = string.Empty;
    //        var canary2 = string.Empty;

    //        var function = SKFunctionFactory.CreateFromMethod((SKContext context) =>
    //        {
    //            canary0 = context!.Variables["input"];
    //            canary1 = context.Variables["var1"];
    //            canary2 = context.Variables["var2"];

    //            context.Variables["input"] = "overridden";
    //            context.Variables["var1"] = "overridden";
    //            context.Variables["var2"] = "overridden";
    //        },
    //        "function");

    //        this._kernel.Plugins.Add(new SKPlugin("plugin", new[] { function }));

    //        // Act
    //        var codeBlock = new CodeBlock(new List<Block> { funcBlock }, "", NullLoggerFactory.Instance);
    //        string result = await codeBlock.RenderCodeAsync(this._kernel, context);

    //        // Assert - Values are received
    //        Assert.Equal("zero", canary0);
    //        Assert.Equal("uno", canary1);
    //        Assert.Equal("due", canary2);

    //        // Assert - Original context is intact
    //        Assert.Equal("zero", variables["input"]);
    //        Assert.Equal("uno", variables["var1"]);
    //        Assert.Equal("due", variables["var2"]);
    //    }

    //    [Fact]
    //    public async Task ItInvokesFunctionWithCustomVariableAsync()
    //    {
    //        // Arrange
    //        const string Var = "varName";
    //        const string VarValue = "varValue";

    //        var variables = new ContextVariables { [Var] = VarValue };
    //        var context = new SKContext(variables);
    //        var funcId = new FunctionIdBlock("plugin.function");
    //        var varBlock = new VarBlock($"${Var}");

    //        var canary = string.Empty;

    //        var function = SKFunctionFactory.CreateFromMethod((SKContext context) =>
    //        {
    //            canary = context!.Variables["input"];
    //        },
    //        "function");

    //        this._kernel.Plugins.Add(new SKPlugin("plugin", new[] { function }));

    //        // Act
    //        var codeBlock = new CodeBlock(new List<Block> { funcId, varBlock }, "", NullLoggerFactory.Instance);
    //        string result = await codeBlock.RenderCodeAsync(this._kernel, context);

    //        // Assert
    //        Assert.Equal(VarValue, result);
    //        Assert.Equal(VarValue, canary);
    //    }

    //    [Fact]
    //    public async Task ItInvokesFunctionWithCustomValueAsync()
    //    {
    //        // Arrange
    //        const string Value = "value";

    //        var context = new SKContext(variables: null);
    //        var funcBlock = new FunctionIdBlock("plugin.function");
    //        var valBlock = new ValBlock($"'{Value}'");

    //        var canary = string.Empty;

    //        var function = SKFunctionFactory.CreateFromMethod((SKContext context) =>
    //        {
    //            canary = context!.Variables["input"];
    //        },
    //        "function");

    //        this._kernel.Plugins.Add(new SKPlugin("plugin", new[] { function }));

    //        // Act
    //        var codeBlock = new CodeBlock(new List<Block> { funcBlock, valBlock }, "", NullLoggerFactory.Instance);
    //        string result = await codeBlock.RenderCodeAsync(this._kernel, context);

    //        // Assert
    //        Assert.Equal(Value, result);
    //        Assert.Equal(Value, canary);
    //    }

    //    [Fact]
    //    public async Task ItInvokesFunctionWithNamedArgsAsync()
    //    {
    //        // Arrange
    //        const string Value = "value";
    //        const string FooValue = "bar";
    //        const string BobValue = "bob's value";

    //        var variables = new ContextVariables();
    //        variables.Set("bob", BobValue);
    //        variables.Set("input", Value);
    //        var context = new SKContext(variables);
    //        var funcId = new FunctionIdBlock("plugin.function");
    //        var namedArgBlock1 = new NamedArgBlock($"foo='{FooValue}'");
    //        var namedArgBlock2 = new NamedArgBlock("baz=$bob");

    //        var foo = string.Empty;
    //        var baz = string.Empty;

    //        var function = SKFunctionFactory.CreateFromMethod((SKContext context) =>
    //        {
    //            foo = context!.Variables["foo"];
    //            baz = context!.Variables["baz"];
    //        },
    //        "function");

    //        this._kernel.Plugins.Add(new SKPlugin("plugin", new[] { function }));

    //        // Act
    //        var codeBlock = new CodeBlock(new List<Block> { funcId, namedArgBlock1, namedArgBlock2 }, "", NullLoggerFactory.Instance);
    //        string result = await codeBlock.RenderCodeAsync(this._kernel, context);

    //        // Assert
    //        Assert.Equal(FooValue, foo);
    //        Assert.Equal(BobValue, baz);
    //        Assert.Equal(Value, result);
    //    }
}
