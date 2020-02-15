using System.Threading.Tasks;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Data;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace Reusable.Translucent.Middleware
{
    public class ResourceMiddlewareTest : IClassFixture<TestHelperFixture>
    {
        private readonly TestHelperFixture _testHelper;

        public ResourceMiddlewareTest(TestHelperFixture testHelper)
        {
            _testHelper = testHelper;
        }

        [Fact]
        public async Task Gets_first_matching_resource_by_default()
        {
            var c1 = Mock.Create<TestFileController>(Behavior.CallOriginal, ControllerName.Empty);
            var c2 = Mock.Create<TestFileController>(Behavior.CallOriginal, ControllerName.Empty);
            var c3 = Mock.Create<TestFileController>(Behavior.CallOriginal, ControllerName.Empty);

            Mock.Arrange(() => c1.Get(Arg.IsAny<Request>())).Returns(new Response { StatusCode = ResourceStatusCode.NotFound }.ToTask()).OccursOnce();
            Mock.Arrange(() => c2.Get(Arg.IsAny<Request>())).Returns(new Response { StatusCode = ResourceStatusCode.OK }.ToTask()).OccursOnce();
            Mock.Arrange(() => c3.Get(Arg.IsAny<Request>())).OccursNever();

            var resources = 
                Resource
                    .Builder()
                    .UseController(c1)
                    .UseController(c2)
                    .UseController(c3)
                    .Build(ImmutableServiceProvider.Empty.Add(TestHelper.CreateCache()).Add(TestHelper.CreateLoggerFactory()));

            await resources.InvokeAsync(Request.CreateGet<FileRequest>("file:///foo"));

            c1.Assert();
            c2.Assert();
            c3.Assert();
        }

        [Fact]
        public async Task Can_filter_controllers_by_controller_id()
        {
            var c1 = Mock.Create<TestFileController>(Behavior.CallOriginal, new ControllerName("a"));
            var c2 = Mock.Create<TestFileController>(Behavior.CallOriginal, new ControllerName("b"));
            var c3 = Mock.Create<TestFileController>(Behavior.CallOriginal, new ControllerName("d"));

            Mock.Arrange(() => c1.Get(Arg.IsAny<Request>())).OccursNever();
            Mock.Arrange(() => c2.Get(Arg.IsAny<Request>())).Returns(new Response { StatusCode = ResourceStatusCode.OK }.ToTask()).OccursOnce();
            Mock.Arrange(() => c3.Get(Arg.IsAny<Request>())).OccursNever();

            var resources = 
                Resource
                    .Builder()
                    .UseController(c1)
                    .UseController(c2)
                    .UseController(c3)
                    .Build(ImmutableServiceProvider.Empty.Add(_testHelper.Cache).Add(_testHelper.LoggerFactory));

            await resources.InvokeAsync(Request.CreateGet<FileRequest>("file:///foo").Pipe(r => { r.ControllerName = new ControllerName("b"); }));

            c1.Assert();
            c2.Assert();
            c3.Assert();
        }

        [Fact]
        public async Task Can_filter_controllers_by_request()
        {
            var c1 = Mock.Create<TestFileController>(Behavior.CallOriginal, new ControllerName("a"));
            var c2 = Mock.Create<TestHttpController>(Behavior.CallOriginal, new ControllerName("b"));
            var c3 = Mock.Create<TestFileController>(Behavior.CallOriginal, new ControllerName("d"));

            Mock.Arrange(() => c1.Get(Arg.IsAny<Request>())).OccursNever();
            Mock.Arrange(() => c2.Get(Arg.IsAny<Request>())).Returns(new Response { StatusCode = ResourceStatusCode.OK }.ToTask()).OccursOnce();
            Mock.Arrange(() => c3.Get(Arg.IsAny<Request>())).OccursNever();

            var resources = 
                Resource
                    .Builder()
                    .UseController(c1)
                    .UseController(c2)
                    .UseController(c3)
                    .Build(ImmutableServiceProvider.Empty.Add(_testHelper.Cache).Add(_testHelper.LoggerFactory));

            await resources.InvokeAsync(Request.CreateGet<HttpRequest>("///foo"));

            c1.Assert();
            c2.Assert();
            c3.Assert();
        }

        [Fact]
        public async Task Can_filter_controllers_by_tag()
        {
            var c1 = Mock.Create<TestFileController>(Behavior.CallOriginal, new ControllerName("a"));
            var c2 = Mock.Create<TestFileController>(Behavior.CallOriginal, new ControllerName("b", "bb"));

            Mock.Arrange(() => c1.Get(Arg.IsAny<Request>())).CallOriginal().OccursNever();
            Mock.Arrange(() => c2.Get(Arg.IsAny<Request>())).CallOriginal().OccursOnce();

            var resources = 
                Resource
                    .Builder()
                    .UseController(c1)
                    .UseController(c2)
                    .Build(ImmutableServiceProvider.Empty.Add(TestHelper.CreateCache()).Add(TestHelper.CreateLoggerFactory()));

            await resources.InvokeAsync(Request.CreateGet<FileRequest>("file:///foo").Pipe(r => { r.ControllerName = new ControllerName(string.Empty, "bb"); }));

            c1.Assert();
            c2.Assert();
        }

        [Fact]
        public async Task Throws_when_PUT_matches_multiple_controllers()
        {
            var c1 = Mock.Create<TestFileController>(Behavior.CallOriginal, ControllerName.Empty);
            var c2 = Mock.Create<TestFileController>(Behavior.CallOriginal, ControllerName.Empty);
            var c3 = Mock.Create<TestFileController>(Behavior.CallOriginal, ControllerName.Empty);

            Mock.Arrange(() => c1.Get(Arg.IsAny<Request>())).OccursNever();
            Mock.Arrange(() => c2.Get(Arg.IsAny<Request>())).OccursNever();
            Mock.Arrange(() => c3.Get(Arg.IsAny<Request>())).OccursNever();

            var resources = 
                Resource
                    .Builder()
                    .UseController(c1)
                    .UseController(c2)
                    .UseController(c3)
                    .Build(ImmutableServiceProvider.Empty.Add(TestHelper.CreateCache()).Add(TestHelper.CreateLoggerFactory()));

            await Assert.ThrowsAnyAsync<DynamicException>(async () => await resources.InvokeAsync(Request.CreatePut<FileRequest>("file:///foo")));

            c1.Assert();
            c2.Assert();
            c3.Assert();
        }
    }

    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    [Handles(typeof(FileRequest))]
    public class TestFileController : Controller
    {
        public TestFileController(ControllerName controllerName) : base(controllerName, "file") { }

        [ResourceGet]
        public virtual Task<Response> Get(Request request) => new Response().ToTask();

        [ResourcePost]
        public virtual Task<Response> Post(Request request) => new Response().ToTask();

        [ResourcePut]
        public virtual Task<Response> Put(Request request) => new Response().ToTask();

        [ResourceDelete]
        public virtual Task<Response> Delete(Request request) => new Response().ToTask();
    }

    [Handles(typeof(HttpRequest))]
    public class TestHttpController : Controller
    {
        public TestHttpController(ControllerName controllerName) : base(controllerName, "http") { }

        [ResourceGet]
        public virtual Task<Response> Get(Request request) => new Response().ToTask();

        [ResourcePost]
        public virtual Task<Response> Post(Request request) => new Response().ToTask();

        [ResourcePut]
        public virtual Task<Response> Put(Request request) => new Response().ToTask();

        [ResourceDelete]
        public virtual Task<Response> Delete(Request request) => new Response().ToTask();
    }
}