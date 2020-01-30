using System;
using System.Threading.Tasks;
using Reusable.Teapot;
using Reusable.Utilities.Mailr;
using Reusable.Utilities.Mailr.Models;
using Reusable.Utilities.XUnit.Fixtures;
using Xunit;

namespace Reusable.Translucent.Controllers
{
    public class HttpControllerTest : IClassFixture<TeapotServerFixture>
    {
        private readonly TeapotServerFixture _teapotServerFixture;

        public HttpControllerTest(TeapotServerFixture teapotServerFixture) => _teapotServerFixture = teapotServerFixture;

        [Fact]
        public async Task Can_send_email_and_receive_html()
        {
            using var serverContext = _teapotServerFixture.GetServer("http://localhost:30002").BeginScope();

            serverContext
                .MockPost("/api/mailr/messages/test", request =>
                {
                    request
                        .AcceptsHtml()
                        .UserAgent("xunit", "1.0")
                        .ContentTypeJsonWhere(content =>
                        {
                            content
                                .HasProperty("$.To")
                                .HasProperty("$.Subject")
                                //.HasProperty("$.From") // Boom! This property does not exist.
                                .HasProperty("$.Body.Greeting");
                        })
                        .Occurs(1);
                })
                .ArrangeResponse(builder =>
                {
                    builder
                        .Once(200, "OK!");
                });

            var email = new Email.Html(new[] { "myemail@mail.com" }, "Test-mail")
            {
                Body = new { Greeting = "Hallo Mailr!" }
            };

            var resources = ResourceRepository.Create((c, _) => c.AddHttp("Mailr", "http://localhost:30002/api"));
            var response = await resources.SendEmailAsync("mailr/messages/test", email, http =>
            {
                http.HeaderActions.Add(headers => headers.UserAgent("xunit", "1.0"));
                http.ControllerName = "Mailr";
            });

            serverContext.Assert();
            Assert.Equal("OK!", response);
        }
    }

    internal class TeapotAssert : Assert
    {
        public static void ImATeapot(Exception ex) => Equal("Response status code does not indicate success: 418 (I'm a teapot).", ex.Message);
    }
}