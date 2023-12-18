using AISmarteasy.Core.Prompt.Template;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISmarteasy.Core.Prompt.Test;

public class NamedArgBlockTest
{
    private readonly ILogger _logger = NullLogger.Instance;

    [Test]
    public void ItHasTheCorrectType()
    {
        var target = new NamedArgBlock("a=$b", _logger);
        Assert.That(target.Type, Is.EqualTo(BlockTypeKind.NamedArg));
    }


    [Test]
    public void ItTrimsSpaces()
    {
        Assert.That(new NamedArgBlock("  a=$b  ", _logger).Content, Is.EqualTo("a=$b"));
        Assert.That(new NamedArgBlock(" a =  $b ", _logger).Content, Is.EqualTo("a=$b"));
        Assert.That(new NamedArgBlock(" a =  \"b\" ", _logger).Content, Is.EqualTo("a=\"b\""));
    }

    [Test]
    public void ArgNameAllowsUnderscoreLettersAndDigits()
    {
        Assert.True(new NamedArgBlock("0='val'", _logger).IsValid(out _));
        Assert.True(new NamedArgBlock("01a='val'", _logger).IsValid(out _));
        Assert.True(new NamedArgBlock("_0='val'", _logger).IsValid(out _));
        Assert.True(new NamedArgBlock("a01_='val'", _logger).IsValid(out _));
        Assert.True(new NamedArgBlock("_a01='val'", _logger).IsValid(out _));

        Assert.False(new NamedArgBlock(".='val'", _logger).IsValid(out _));
        Assert.False(new NamedArgBlock("a{b='val'", _logger).IsValid(out _));
    }

    [Test]
    public void AllowsAnyNumberOfSpacesBeforeAndAfterEqualSign()
    {
        Assert.True(new NamedArgBlock("name   ='value'", _logger).IsValid(out _));
        Assert.True(new NamedArgBlock("name=   'value'", _logger).IsValid(out _));
    }


    [Test]
    public void ArgValueNeedsQuoteOrDollarSignPrefix()
    {
        var target = new NamedArgBlock("a=b", _logger);

        Assert.False(target.IsValid(out var error));
        Assert.That(error,
            Is.EqualTo(
                "There was an issue with the named argument value for 'a': A value must have single quotes or double quotes on both sides"));
    }


    [Test]
    public void ArgNameShouldBeNonEmpty()
    {
        var target = new NamedArgBlock("='b'", _logger);

        Assert.False(target.IsValid(out var error));
        Assert.That(error, Is.EqualTo("A named argument must have a name"));
    }


    [Test]
    public void ArgValueShouldBeNonEmpty()
    {
        Assert.Throws<CoreException>(() =>
        {
            var namedArgBlock = new NamedArgBlock("a=", _logger);
        });
    }

    [Test]
    public void ArgNameAndVariableShouldBeAValidVariableName()
    {
        var target = new NamedArgBlock("!@#^='b'", _logger);
        Assert.False(target.IsValid(out var error1));
        Assert.That(error1,
            Is.EqualTo(
                "The argument name '!@#^' contains invalid characters. Only alphanumeric chars and underscore are allowed."));

        target = new NamedArgBlock("a=$!@#^", _logger);
        Assert.False(target.IsValid(out var error2));
        Assert.That(error2,
            Is.EqualTo(
                "There was an issue with the named argument value for 'a': The variable name '!@#^' contains invalid characters. Only alphanumeric chars and underscore are allowed."));
    }

    [Test]
    public void ArgValueAllowsConsistentlyQuotedValues()
    {

        Assert.True(new NamedArgBlock("0='val'", _logger).IsValid(out _));
        Assert.True(new NamedArgBlock("0=\"val\"", _logger).IsValid(out _));

        Assert.False(new NamedArgBlock("0='val\"", _logger).IsValid(out _));
    }

    [Test]
    public void ItRequiresOneEquals()
    {
        var target1 = new NamedArgBlock("a='b'", _logger);
        var target2 = new NamedArgBlock("a=$b", _logger);
        var target3 = new NamedArgBlock("a=\"b\"", _logger);
        Assert.Throws<CoreException>(() =>
        {
            var namedArgBlock = new NamedArgBlock("foo", _logger);
        });
        Assert.Throws<CoreException>(() =>
        {
            var namedArgBlock = new NamedArgBlock("foo=$bar=$baz", _logger);
        });

        Assert.True(target1.IsValid(out _));
        Assert.True(target2.IsValid(out _));
        Assert.True(target3.IsValid(out _));
    }
}