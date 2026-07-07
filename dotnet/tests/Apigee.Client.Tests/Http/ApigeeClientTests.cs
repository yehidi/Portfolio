using System.Net;
using System.Text;
using Apigee.Client.Exceptions;
using Apigee.Client.Tests.TestSupport;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using SystemUnderTest = Apigee.Client.Http.ApigeeClient;

namespace Apigee.Client.Tests.Http;

public sealed class ApigeeClientTests
{
    private sealed record Widget(string Id, string Name);

    [Fact]
    public async Task GetAsync_SuccessResponse_DeserializesBody()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":"1","name":"Gadget"}""", Encoding.UTF8, "application/json"),
        });
        var client = CreateClient(handler);

        var widget = await client.GetAsync<Widget>("/widgets/1");

        widget.Should().Be(new Widget("1", "Gadget"));
    }

    [Fact]
    public async Task PostAsync_SerializesRequestBodyAsJson()
    {
        string? sentBody = null;
        var handler = new StubHttpMessageHandler(async request =>
        {
            sentBody = await request.Content!.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"id":"2","name":"Widget"}""", Encoding.UTF8, "application/json"),
            };
        });
        var client = CreateClient(handler);

        await client.PostAsync<Widget, Widget>("/widgets", new Widget("2", "Widget"));

        sentBody.Should().Contain("\"id\":\"2\"").And.Contain("\"name\":\"Widget\"");
    }

    [Fact]
    public async Task GetAsync_NonSuccessResponse_ThrowsApigeeRequestExceptionWithBody()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("""{"error":"not found"}""", Encoding.UTF8, "application/json"),
        });
        var client = CreateClient(handler);

        var act = async () => await client.GetAsync<Widget>("/widgets/missing");

        var exception = await act.Should().ThrowAsync<ApigeeRequestException>();
        exception.Which.StatusCode.Should().Be(404);
        exception.Which.ResponseBody.Should().Contain("not found");
    }

    [Fact]
    public async Task DeleteAsync_NoContentResponse_CompletesWithoutError()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var client = CreateClient(handler);

        var act = async () => await client.DeleteAsync("/widgets/1");

        await act.Should().NotThrowAsync();
    }

    private static SystemUnderTest CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.com") };
        return new SystemUnderTest(httpClient, NullLogger<SystemUnderTest>.Instance);
    }
}
