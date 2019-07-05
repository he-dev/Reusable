using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.IOnymous;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace Reusable.Tests.IOnymous
{
    public class RelativeProviderTest
    {
//        [Fact]
//        public void Throws_when_base_uri_not_absolute()
//        {
//            Assert.Throws<ArgumentException>(() => new InMemoryResourceProvider().DecorateWith(RelativeResourceProvider.Factory("blub")));
//        }

        [Fact]
        public async Task Creates_new_absolute_uri_when_relative_one_specified()
        {
            var mockProvider = Mock.Create<IResourceProvider>();

            mockProvider
                .Arrange(x => x.Properties)
                .Returns(ImmutableSession.Empty.SetScheme("blub"));
            
//            mockProvider
//                .Arrange(x => x.Schemes)
//                .Returns(new SoftString[] { "blub" }.ToImmutableHashSet());

            mockProvider
                .Arrange(x => x.GetAsync(Arg.Matches<UriString>(uri => uri == new UriString("blub:base/relative")), Arg.IsAny<ImmutableSession>()))
                .Returns<UriString, ImmutableSession>((uri, metadata) => Task.FromResult<IResource>(new InMemoryResource(ImmutableSession.Empty.SetUri(uri), Stream.Null)));


            var relativeProvider = mockProvider.DecorateWith(RelativeProvider.Factory("blub:base"));
            var resource = await relativeProvider.GetAsync("relative", ImmutableSession.Empty);

            Assert.False(resource.Exists);
            Assert.Equal("blub:base/relative", resource.Uri);
        }
    }
}