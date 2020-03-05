using Xunit;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace LanguageExt.Tests.Transformer.Traverse.OptionUnsafe.Sync
{
    public class OptionUnsafeOptionUnsafe
    {
        [Fact]
        public void NoneIsNone()
        {
            var ma = OptionUnsafe<OptionUnsafe<int>>.None;
            var mb = ma.Sequence();
            var mc = OptionUnsafe<OptionUnsafe<int>>.None;

            var mr = mb == mc;
            
            Assert.True(mr);
        }
        
        [Fact]
        public void SomeNoneIsNone()
        {
            var ma = SomeUnsafe<OptionUnsafe<int>>(None);
            var mb = ma.Sequence();
            var mc = OptionUnsafe<OptionUnsafe<int>>.None;

            var mr = mb == mc;
            
            Assert.True(mr);
        }
        
        [Fact]
        public void SomeSomeIsSomeSome()
        {
            var ma = SomeUnsafe(SomeUnsafe(1234));
            var mb = ma.Sequence();
            var mc = SomeUnsafe(SomeUnsafe(1234));

            var mr = mb == mc;
            
            Assert.True(mr);
        }
    }
}
