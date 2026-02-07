using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using System.Net;
using System.Text;
using Tomeshelf.Paissa.Domain.ValueObjects;
using Tomeshelf.Paissa.Infrastructure.Services.External;

namespace Tomeshelf.Paissa.Infrastructure.Tests.PaissaClientTests;

public class GetWorldAsync
{
    [Fact]
    public async Task ReturnsMappedWorld()
    {
        var json = """
                   {
                     "id": 33,
                     "name": "TestWorld",
                     "districts": [
                       {
                         "id": 4,
                         "name": "Lavender Beds",
                         "open_plots": [
                           {
                             "world_id": 33,
                             "district_id": 4,
                             "ward_number": 0,
                             "plot_number": 1,
                             "size": 1,
                             "price": 123456,
                             "last_updated_time": 1700000000,
                             "purchase_system": 6,
                             "lotto_entries": 2,
                             "lotto_phase": 2
                           }
                         ]
                       }
                     ]
                   }
                   """;

        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://paissa.test/") };
        var factory = new StubHttpClientFactory(client);
        var subject = new PaissaClient(factory, NullLogger<PaissaClient>.Instance);

        var world = await subject.GetWorldAsync(33, CancellationToken.None);

        world.Id.ShouldBe(33);
        world.Name.ShouldBe("TestWorld");
        world.Districts.ShouldHaveSingleItem();

        var district = world.Districts[0];
        district.Id.ShouldBe(4);
        district.Name.ShouldBe("Lavender Beds");
        district.OpenPlots.ShouldHaveSingleItem();

        var plot = district.OpenPlots[0];
        plot.WardNumber.ShouldBe(1);
        plot.PlotNumber.ShouldBe(2);
        plot.Size.ShouldBe(HousingPlotSize.Medium);
        plot.Price.ShouldBe(123456);
        plot.LotteryEntries.ShouldBe(2);
        plot.LotteryPhase.ShouldBe(LotteryPhase.ResultsProcessing);
        plot.PurchaseSystem
            .HasFlag(PurchaseSystem.FreeCompany)
            .ShouldBeTrue();
        plot.PurchaseSystem
            .HasFlag(PurchaseSystem.Personal)
            .ShouldBeTrue();
        plot.LastUpdatedUtc.ShouldBe(DateTimeOffset.FromUnixTimeSeconds(1700000000));
    }

    [Fact]
    public async Task Throws_WhenNotFound()
    {
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.NotFound));
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://paissa.test/") };
        var subject = new PaissaClient(new StubHttpClientFactory(client), NullLogger<PaissaClient>.Instance);

        var exception = await Should.ThrowAsync<HttpRequestException>(() => subject.GetWorldAsync(404, CancellationToken.None));

        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Throws_WhenPayloadIsNull()
    {
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("null", Encoding.UTF8, "application/json") });
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://paissa.test/") };
        var subject = new PaissaClient(new StubHttpClientFactory(client), NullLogger<PaissaClient>.Instance);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => subject.GetWorldAsync(1, CancellationToken.None));

        exception.Message.ShouldBe("PaissaDB returned an empty payload.");
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public StubHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient CreateClient(string name)
        {
            return _client;
        }
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public StubHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}