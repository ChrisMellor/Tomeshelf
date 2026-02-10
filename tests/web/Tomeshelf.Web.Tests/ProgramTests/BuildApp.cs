using Microsoft.Extensions.Hosting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Tomeshelf.Web.Services;
using Tomeshelf.Web.Tests.TestUtilities;

namespace Tomeshelf.Web.Tests.ProgramTests;

public class BuildApp
{
    /// <summary>
    ///     Uses default service addresses when the value is a development.
    /// </summary>
    [Fact]
    public void Development_UsesDefaultServiceAddresses()
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            ["Services:McmApiBase"] = "https://config.test/",
            ["Services:HumbleBundleApiBase"] = "https://config.test/",
            ["Services:FitbitApiBase"] = "https://config.test/",
            ["Services:PaissaApiBase"] = "https://config.test/",
            ["Services:ShiftApiBase"] = "https://config.test/"
        };

        using var app = ProgramTestHarness.BuildApp(Environments.Development, config);
        var factory = app.Services.GetRequiredService<IHttpClientFactory>();

        // Act
        var guests = factory.CreateClient(GuestsApi.HttpClientName);
        var bundles = factory.CreateClient(BundlesApi.HttpClientName);
        var fitbit = factory.CreateClient(FitbitApi.HttpClientName);
        var paissa = factory.CreateClient(PaissaApi.HttpClientName);
        var shift = factory.CreateClient(ShiftApi.HttpClientName);
        var uploads = factory.CreateClient(FileUploadsApi.HttpClientName);

        // Assert
        guests.BaseAddress.ShouldBe(new Uri("https://mcmapi"));
        guests.Timeout.ShouldBe(TimeSpan.FromSeconds(100));
        guests.DefaultRequestVersion.ShouldBe(HttpVersion.Version11);
        guests.DefaultVersionPolicy.ShouldBe(HttpVersionPolicy.RequestVersionExact);

        bundles.BaseAddress.ShouldBe(new Uri("https://humblebundleapi"));
        bundles.Timeout.ShouldBe(TimeSpan.FromSeconds(100));

        fitbit.BaseAddress.ShouldBe(new Uri("https://fitbitapi"));
        fitbit.Timeout.ShouldBe(TimeSpan.FromSeconds(100));

        paissa.BaseAddress.ShouldBe(new Uri("https://paissaapi"));
        paissa.Timeout.ShouldBe(TimeSpan.FromSeconds(30));

        shift.BaseAddress.ShouldBe(new Uri("https://shiftapi"));
        shift.Timeout.ShouldBe(TimeSpan.FromSeconds(30));

        uploads.BaseAddress.ShouldBe(new Uri("https://localhost:49960"));
        uploads.Timeout.ShouldBe(TimeSpan.FromMinutes(30));
    }

    /// <summary>
    ///     Invalids the service uris.
    /// </summary>
    /// <returns>The result of the operation.</returns>
    public static IEnumerable<object[]> InvalidServiceUris()
    {
        yield return new object[] { "Services:McmApiBase", GuestsApi.HttpClientName, "Invalid URI in configuration setting 'Services:McmApiBase'." };
        yield return new object[] { "Services:HumbleBundleApiBase", BundlesApi.HttpClientName, "Invalid URI in configuration setting 'Services:HumbleBundleApiBase'." };
        yield return new object[] { "Services:FitbitApiBase", FitbitApi.HttpClientName, "Invalid URI in configuration setting 'Services:FitbitApiBase'." };
        yield return new object[] { "Services:PaissaApiBase", PaissaApi.HttpClientName, "Invalid URI in configuration setting 'Services:PaissaApiBase'." };
        yield return new object[] { "Services:ShiftApiBase", ShiftApi.HttpClientName, "Invalid URI in configuration setting 'Services:ShiftApiBase'." };
        yield return new object[] { "Services:FileUploaderApiBase", FileUploadsApi.HttpClientName, "Invalid URI in configuration setting 'Services:FileUploaderApiBase'." };
    }

    /// <summary>
    ///     Uses API base fallback for guests when the value is a production.
    /// </summary>
    [Fact]
    public void Production_UsesApiBaseFallbackForGuests()
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            ["Services:McmApiBase"] = null,
            ["Services:ApiBase"] = "https://fallback.example.test/"
        };

        using var app = ProgramTestHarness.BuildApp(Environments.Production, config);

        // Act
        var client = app.Services
                        .GetRequiredService<IHttpClientFactory>()
                        .CreateClient(GuestsApi.HttpClientName);

        // Assert
        client.BaseAddress.ShouldBe(new Uri("https://fallback.example.test/"));
    }

    /// <summary>
    ///     Uses configured service addresses when the value is a production.
    /// </summary>
    [Fact]
    public void Production_UsesConfiguredServiceAddresses()
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            ["Services:McmApiBase"] = "https://mcm.example.test/",
            ["Services:HumbleBundleApiBase"] = "https://humble.example.test/",
            ["Services:FitbitApiBase"] = "https://fitbit.example.test/",
            ["Services:PaissaApiBase"] = "https://paissa.example.test/",
            ["Services:ShiftApiBase"] = "https://shift.example.test/",
            ["Services:FileUploaderApiBase"] = "https://uploads.example.test/"
        };

        using var app = ProgramTestHarness.BuildApp(Environments.Production, config);
        var factory = app.Services.GetRequiredService<IHttpClientFactory>();

        // Act
        var guests = factory.CreateClient(GuestsApi.HttpClientName);
        var bundles = factory.CreateClient(BundlesApi.HttpClientName);
        var fitbit = factory.CreateClient(FitbitApi.HttpClientName);
        var paissa = factory.CreateClient(PaissaApi.HttpClientName);
        var shift = factory.CreateClient(ShiftApi.HttpClientName);
        var uploads = factory.CreateClient(FileUploadsApi.HttpClientName);

        // Assert
        guests.BaseAddress.ShouldBe(new Uri("https://mcm.example.test/"));
        bundles.BaseAddress.ShouldBe(new Uri("https://humble.example.test/"));
        fitbit.BaseAddress.ShouldBe(new Uri("https://fitbit.example.test/"));
        paissa.BaseAddress.ShouldBe(new Uri("https://paissa.example.test/"));
        shift.BaseAddress.ShouldBe(new Uri("https://shift.example.test/"));
        uploads.BaseAddress.ShouldBe(new Uri("https://uploads.example.test/"));
    }

    /// <summary>
    ///     Uses gateway addresses when the gateway configured.
    /// </summary>
    [Fact]
    public void WhenGatewayConfigured_UsesGatewayAddresses()
    {
        // Arrange
        var config = new Dictionary<string, string?>
        {
            ["Services:GatewayBase"] = "https://gateway.example.test/"
        };

        using var app = ProgramTestHarness.BuildApp(Environments.Production, config);
        var factory = app.Services.GetRequiredService<IHttpClientFactory>();

        // Act
        var guests = factory.CreateClient(GuestsApi.HttpClientName);
        var bundles = factory.CreateClient(BundlesApi.HttpClientName);
        var fitbit = factory.CreateClient(FitbitApi.HttpClientName);
        var paissa = factory.CreateClient(PaissaApi.HttpClientName);
        var shift = factory.CreateClient(ShiftApi.HttpClientName);
        var uploads = factory.CreateClient(FileUploadsApi.HttpClientName);

        // Assert
        guests.BaseAddress.ShouldBe(new Uri("https://gateway.example.test/api/mcm/"));
        bundles.BaseAddress.ShouldBe(new Uri("https://gateway.example.test/api/humblebundle/"));
        fitbit.BaseAddress.ShouldBe(new Uri("https://gateway.example.test/"));
        paissa.BaseAddress.ShouldBe(new Uri("https://gateway.example.test/api/"));
        shift.BaseAddress.ShouldBe(new Uri("https://gateway.example.test/api/shift/"));
        uploads.BaseAddress.ShouldBe(new Uri("https://gateway.example.test/api/fileuploader/"));
    }

    /// <summary>
    ///     Throws when the URI configured is invalid.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="clientName">The client name.</param>
    /// <param name="message">The message.</param>
    [Theory]
    [MemberData(nameof(InvalidServiceUris))]
    public void WhenInvalidUriConfigured_Throws(string key, string clientName, string message)
    {
        // Arrange
        var config = new Dictionary<string, string?> { [key] = "not-a-uri" };
        using var app = ProgramTestHarness.BuildApp(Environments.Production, config);
        var factory = app.Services.GetRequiredService<IHttpClientFactory>();

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => factory.CreateClient(clientName));

        // Assert
        exception.Message.ShouldBe(message);
    }
}
