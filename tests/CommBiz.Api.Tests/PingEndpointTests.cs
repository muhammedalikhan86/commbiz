using System.Net;
using System.Net.Http.Json;
using CommBiz.Api.Features.Diagnostics;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CommBiz.Api.Tests;

public class PingEndpointTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Ping_command_is_dispatched_through_wolverine_and_echoed_back()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/diagnostics/ping", new PingCommand("hello"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PingResult>();
        Assert.Equal("hello", result?.Message);
    }
}
