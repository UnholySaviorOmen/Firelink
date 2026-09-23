using System.Net;
using FluentAssertions;
using Firelink.Core.Models.Hashing;
using Firelink.Core.Models.Manifest.Sources;
using Firelink.Platform.Nexus;
using Microsoft.Extensions.Logging.Abstractions;

namespace Firelink.Platform.Nexus.Tests;

public class NexusDownloaderTests
{
    private const string TestKey = "test-api-key-12345";

    private sealed class RoutingHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage>? ApiHandler { get; set; }
        public Func<HttpRequestMessage, HttpResponseMessage>? CdnHandler { get; set; }

        public List<HttpRequestMessage> ApiRequests { get; } = new();
        public List<HttpRequestMessage> CdnRequests { get; } = new();

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var host = request.RequestUri!.Host;
            if (host == "api.nexusmods.com")
            {
                ApiRequests.Add(request);
                if (ApiHandler is null)
                    throw new InvalidOperationException("Unexpected API request");
                return Task.FromResult(ApiHandler(request));
            }
            else
            {
                CdnRequests.Add(request);
                if (CdnHandler is null)
                    throw new InvalidOperationException("Unexpected CDN request");
                return Task.FromResult(CdnHandler(request));
            }
        }
    }

    private static (NexusDownloader downloader, RoutingHandler handler) MakeDownloader(
        Func<HttpRequestMessage, HttpResponseMessage>? api = null,
        Func<HttpRequestMessage, HttpResponseMessage>? cdn = null,
        string? key = TestKey)
    {
        var handler = new RoutingHandler { ApiHandler = api, CdnHandler = cdn };
        var http = new HttpClient(handler);

        var factory = new FakeHttpClientFactory();
        factory.Register("nexus-api", handler);
        factory.Register("nexus", handler);

        var provider = new FakeNexusApiKeyProvider(key);
        var client = new NexusClient(http, provider, NullLogger<NexusClient>.Instance);
        var downloader = new NexusDownloader(
            factory, client, NullLogger<NexusDownloader>.Instance);

        return (downloader, handler);
    }

    // ... остальные тесты без изменений ...
}
