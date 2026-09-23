using System.Net;
using System.Net.Http;
using FluentAssertions;
using Firelink.Platform.Nexus;
using Microsoft.Extensions.Logging.Abstractions;

namespace Firelink.Platform.Nexus.Tests;

public class NexusClientTests
{
    private const string TestKey = "test-api-key-12345";

    private static NexusClient MakeClient(
        FakeHttpMessageHandler handler,
        string? key = TestKey)
    {
        var http = new HttpClient(handler);
        var provider = new FakeNexusApiKeyProvider(key);
        return new NexusClient(http, provider, NullLogger<NexusClient>.Instance);
    }

    // ------------------------------------------------------------------
    //  Validate / IsPremiumAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task IsPremiumAsync_PremiumAccount_ReturnsTrue()
    {
        var handler = new FakeHttpMessageHandler(
            HttpStatusCode.OK,
            """{"is_premium":true,"name":"testuser"}""");

        var client = MakeClient(handler);

        var result = await client.IsPremiumAsync(CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPremiumAsync_FreeAccount_ReturnsFalse()
    {
        var handler = new FakeHttpMessageHandler(
            HttpStatusCode.OK,
            """{"is_premium":false,"name":"testuser"}""");

        var client = MakeClient(handler);

        var result = await client.IsPremiumAsync(CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsPremiumAsync_SendsApiKeyHeader()
    {
        var handler = new FakeHttpMessageHandler(
            HttpStatusCode.OK,
            """{"is_premium":true,"name":"testuser"}""");

        var client = MakeClient(handler);

        await client.IsPremiumAsync(CancellationToken.None);

        handler.Requests.Should().HaveCount(1);
        var request = handler.Requests[0];
        request.Headers.GetValues("apikey").Should().ContainSingle().Which.Should().Be(TestKey);
    }

    [Fact]
    public async Task IsPremiumAsync_SendsApplicationHeaders()
    {
        var handler = new FakeHttpMessageHandler(
            HttpStatusCode.OK,
            """{"is_premium":true,"name":"testuser"}""");

        var client = MakeClient(handler);

        await client.IsPremiumAsync(CancellationToken.None);

        var request = handler.Requests[0];
        request.Headers.GetValues("Application-Name").Should().ContainSingle().Which.Should().Be("Firelink");
        request.Headers.GetValues("Application-Version").Should().ContainSingle().Which.Should().Be("0.1.0");
        request.Headers.GetValues("User-Agent").Should().ContainSingle().Which.Should().Be("Firelink/0.1.0");
    }

    [Fact]
    public async Task IsPremiumAsync_RequestsCorrectUrl()
    {
        var handler = new FakeHttpMessageHandler(
            HttpStatusCode.OK,
            """{"is_premium":true,"name":"testuser"}""");

        var client = MakeClient(handler);

        await client.IsPremiumAsync(CancellationToken.None);

        handler.Requests[0].RequestUri!.ToString()
            .Should().Be("https://api.nexusmods.com/v1/users/validate.json");
    }

    [Fact]
    public async Task IsPremiumAsync_NoKey_Throws()
    {
        var handler = new FakeHttpMessageHandler(
            HttpStatusCode.OK,
            """{"is_premium":true,"name":"testuser"}""");

        var client = MakeClient(handler, key: null);

        var act = async () => await client.IsPremiumAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*nexus.key*");
    }

    [Fact]
    public async Task IsPremiumAsync_EmptyKey_Throws()
    {
        var handler = new FakeHttpMessageHandler(
            HttpStatusCode.OK,
            """{"is_premium":true,"name":"testuser"}""");

        var client = MakeClient(handler, key: "   ");

        var act = async () => await client.IsPremiumAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*nexus.key*");
    }

    [Fact]
    public async Task IsPremiumAsync_401_Throws()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Unauthorized);
        var client = MakeClient(handler);

        var act = async () => await client.IsPremiumAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*invalid or revoked*");
    }

    [Fact]
    public async Task IsPremiumAsync_429_Throws()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.TooManyRequests);
        var client = MakeClient(handler);

        var act = async () => await client.IsPremiumAsync(CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*rate limit*");
    }

    [Fact]
    public async Task IsPremiumAsync_500_ThrowsHttpRequestException()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError);
        var client = MakeClient(handler);

        var act = async () => await client.IsPremiumAsync(CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("*500*");
    }

    // ------------------------------------------------------------------
    //  GetDownloadLinksAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetDownloadLinksAsync_ReturnsAllLinks()
    {
        var handler = new FakeHttpMessageHandler(
            HttpStatusCode.OK,
            """
            [
              {"name":"CDN","short_name":"cd","URI":"https://cdn1.example.com/file.7z"},
              {"name":"CDN2","short_name":"cd2","URI":"https://cdn2.example.com/file.7z"}
            ]
            """);

        var client = MakeClient(handler);

        var links = await client.GetDownloadLinksAsync(
            "skyrimspecialedition", 3863, 1000172397, CancellationToken.None);

        links.Should().HaveCount(2);
        links[0].Name.Should().Be("CDN");
        links[0].Uri!.ToString().Should().Be("https://cdn1.example.com/file.7z");
        links[1].Uri!.ToString().Should().Be("https://cdn2.example.com/file.7z");
    }

    [Fact]
    public async Task GetDownloadLinksAsync_FiltersLinksWithoutUri()
    {
        var handler = new FakeHttpMessageHandler(
            HttpStatusCode.OK,
            """
            [
              {"name":"CDN","URI":"https://cdn1.example.com/file.7z"},
              {"name":"Broken","URI":null},
              {"name":"AlsoBroken"}
            ]
            """);

        var client = MakeClient(handler);

        var links = await client.GetDownloadLinksAsync(
            "skyrimspecialedition", 3863, 1000172397, CancellationToken.None);

        links.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetDownloadLinksAsync_EmptyArray_ReturnsEmpty()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "[]");
        var client = MakeClient(handler);

        var links = await client.GetDownloadLinksAsync(
            "skyrimspecialedition", 3863, 1000172397, CancellationToken.None);

        links.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDownloadLinksAsync_RequestsCorrectUrl()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "[]");
        var client = MakeClient(handler);

        await client.GetDownloadLinksAsync(
            "skyrimspecialedition", 3863, 1000172397, CancellationToken.None);

        handler.Requests[0].RequestUri!.ToString().Should().Be(
            "https://api.nexusmods.com/v1/games/skyrimspecialedition/mods/3863/files/1000172397/download_link.json");
    }

    [Fact]
    public async Task GetDownloadLinksAsync_SendsApiKeyHeader()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "[]");
        var client = MakeClient(handler);

        await client.GetDownloadLinksAsync(
            "skyrimspecialedition", 3863, 1000172397, CancellationToken.None);

        handler.Requests[0].Headers.GetValues("apikey").Should().ContainSingle().Which.Should().Be(TestKey);
    }

    [Fact]
    public async Task GetDownloadLinksAsync_403_ThrowsPremiumRequired()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Forbidden);
        var client = MakeClient(handler);

        var act = async () => await client.GetDownloadLinksAsync(
            "skyrimspecialedition", 3863, 1000172397, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Premium*");
    }

    [Fact]
    public async Task GetDownloadLinksAsync_404_ThrowsNotFound()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound);
        var client = MakeClient(handler);

        var act = async () => await client.GetDownloadLinksAsync(
            "skyrimspecialedition", 3863, 1000172397, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*")
            .WithMessage("*3863*");
    }

    [Fact]
    public async Task GetDownloadLinksAsync_401_ThrowsInvalidKey()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Unauthorized);
        var client = MakeClient(handler);

        var act = async () => await client.GetDownloadLinksAsync(
            "skyrimspecialedition", 3863, 1000172397, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*invalid or revoked*");
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public async Task GetDownloadLinksAsync_NonPositiveIds_Throws(int modId, int fileId)
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "[]");
        var client = MakeClient(handler);

        var act = async () => await client.GetDownloadLinksAsync(
            "skyrimspecialedition", modId, fileId, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetDownloadLinksAsync_EmptyGame_Throws()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "[]");
        var client = MakeClient(handler);

        var act = async () => await client.GetDownloadLinksAsync(
            "", 3863, 1000172397, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
