using AISmarteasy.Core.Prompt.Template;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISmarteasy.Core.Prompt.Test
{
    public class PromptTemplateTokenizerTest
    {
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly PromptTemplateTokenizer _target = new PromptTemplateTokenizer(NullLogger.Instance);

        [Test]
        public void CanBePromptTemplate()
        {
            Assert.False(Verifier.CanBePromptTemplate(""));
            Assert.False(Verifier.CanBePromptTemplate(" "));
            Assert.False(Verifier.CanBePromptTemplate(" {}  "));
            Assert.False(Verifier.CanBePromptTemplate(" {{}  "));
            Assert.False(Verifier.CanBePromptTemplate(" { }}  "));
            Assert.False(Verifier.CanBePromptTemplate(" {{ } }  "));

            Assert.True(Verifier.CanBePromptTemplate(" {{ }}  "));
            Assert.True(Verifier.CanBePromptTemplate(" {{$a}}"));
        }

        [Test]
        public void TextBlockCase1()
        {
            var blocks = _target.Tokenize("{{  \" }}");
            Assert.That(blocks.Count, Is.EqualTo(1));

            var block = (TextBlock)blocks[0];
            Assert.That(block.Type, Is.EqualTo(BlockTypeKind.Text));
            Assert.That(block.Content, Is.EqualTo("{{  \" }}"));
        }

        [Test]
        public void TextBlockCase2()
        {
            var blocks = _target.Tokenize("{{}}");
            Assert.That(blocks.Count, Is.EqualTo(1));

            var block = (TextBlock)blocks[0];
            Assert.That(block.Type, Is.EqualTo(BlockTypeKind.Text));
            Assert.That(block.Content, Is.EqualTo("{{}}"));
        }

        [Test]
        public void VariableCase1()
        {
            var blocks = _target.Tokenize("{{$}}");
            Assert.That(blocks.Count, Is.EqualTo(1));

            var block1 = (VariableBlock)blocks[0];
            Assert.That(block1.Type, Is.EqualTo(BlockTypeKind.Variable));
            Assert.That(block1.Content, Is.EqualTo("$"));
            Assert.That(block1.Name, Is.EqualTo(""));
        }

        [Test]
        public void VariableCase2()
        {
            var blocks = _target.Tokenize("{{$a}}");

            Assert.That(blocks.Count, Is.EqualTo(1));

            var block1 = (VariableBlock)blocks[0];
            Assert.That(block1.Type, Is.EqualTo(BlockTypeKind.Variable));
            Assert.That(block1.Content, Is.EqualTo("$a"));
            Assert.That(block1.Name, Is.EqualTo("a"));
        }

        [Test]
        public void VariableCase3()
        {
            var blocks = _target.Tokenize("{{  $a  }}");
            Assert.That(blocks.Count, Is.EqualTo(1));

            var block1 = (VariableBlock)blocks[0];
            Assert.That(block1.Type, Is.EqualTo(BlockTypeKind.Variable));
            Assert.That(block1.Content, Is.EqualTo("$a"));
            Assert.That(block1.Name, Is.EqualTo("a"));
        }

        [Test]
        public void TextWithVariable3()
        {
            var blocks = _target.Tokenize("{{ $a}}");
            Assert.That(blocks.Count, Is.EqualTo(1));

            var block1 = (VariableBlock)blocks[0];
            Assert.That(block1.Type, Is.EqualTo(BlockTypeKind.Variable));
            Assert.That(block1.Content, Is.EqualTo("$a"));
            Assert.That(block1.Name, Is.EqualTo("a"));
        }

        [Test]
        public void TextWithValueCase1()
        {
            var blocks = _target.Tokenize("{{ ' ' }}");
            Assert.That(blocks.Count, Is.EqualTo(1));

            var block1 = (ValueBlock)blocks[0];
            Assert.That(block1.Type, Is.EqualTo(BlockTypeKind.Value));
            Assert.That(block1.Content, Is.EqualTo("' '"));
        }

        [Test]
        public void TextWithValueCase2()
        {
            var blocks = _target.Tokenize("{{ 'good' }}");
            Assert.That(blocks.Count, Is.EqualTo(1));

            var block1 = (ValueBlock)blocks[0];
            Assert.That(block1.Type, Is.EqualTo(BlockTypeKind.Value));
            Assert.That(block1.Content, Is.EqualTo("'good'"));
        }

        [Test]
        public void TextWithPlaceHolderCase1()
        {
            var blocks = _target.Tokenize("{{ code }}");
            Assert.That(blocks.Count, Is.EqualTo(1));

            var block1 = (PlaceHolderBlock)blocks[0];
            Assert.That(block1.Type, Is.EqualTo(BlockTypeKind.PlaceHolder));
            Assert.That(block1.Content, Is.EqualTo("code"));
        }

        [Test]
        public void TextWithPlaceHolderCase2()
        {
            var blocks = _target.Tokenize("}}{{{ {$a}}}}");
            Assert.That(blocks.Count, Is.EqualTo(3));

            Assert.That(blocks[0].Content, Is.EqualTo("}}{"));
            Assert.That(blocks[1].Content, Is.EqualTo("{$a"));
            Assert.That(blocks[2].Content, Is.EqualTo("}}"));
        }

        [Test]
        public void TextWithPlaceHolderCase3()
        {
            var blocks = _target.Tokenize("//}}{{{ {$a}}}} {{b}}x}}");
            Assert.That(blocks.Count, Is.EqualTo(5));

            Assert.That(blocks[0].Content, Is.EqualTo("//}}{"));
            Assert.That(blocks[1].Content, Is.EqualTo("{$a"));
            Assert.That(blocks[2].Content, Is.EqualTo("}} "));
            Assert.That(blocks[3].Content, Is.EqualTo("b"));
            Assert.That(blocks[4].Content, Is.EqualTo("x}}"));
        }

        [Test]
        public void TextWithPlaceHolderCase4()
        {
            var ex = Assert.Throws<CoreException>(() =>
            {
                _target.Tokenize("{{ not='valid' }}");
            });

            Assert.That(ex!.Message, Is.EqualTo("Placeholder tokenizer returned an incorrect first token type NamedArg"));
        }
    }
}
