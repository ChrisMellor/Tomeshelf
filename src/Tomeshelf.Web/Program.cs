using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tomeshelf.ServiceDefaults;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web;

/// <summary>
///     Web application entry point and configuration.
/// </summary>
public class Program
{
    /// <summary>
    ///     Application entry point for the MVC web host.
    ///     Configures services and starts the web server.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddHttpLogging(o =>
            {
                o.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.RequestMethod | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration | HttpLoggingFields.RequestHeaders | HttpLoggingFields.ResponseHeaders;
                o.RequestHeaders.Add("User-Agent");
                o.MediaTypeOptions.AddText("text/html");
            });
        }

        builder.Services.AddControllersWithViews();
        builder.Services.AddAuthorization();
        builder.Services.AddLocalization();

        builder.Services.AddHttpClient<IGuestsApi, GuestsApi>(client =>
                {
                    var configured = builder.Configuration["Services:ApiBase"];

                    if (!string.IsNullOrWhiteSpace(configured) && !builder.Environment.IsDevelopment())
                    {
                        if (!Uri.TryCreate(configured, UriKind.Absolute, out var configuredUri))
                        {
                            throw new InvalidOperationException("Invalid URI in configuration setting 'Services:ApiBase'.");
                        }

                        client.BaseAddress = configuredUri;
                    }
                    else
                    {
                        client.BaseAddress = new Uri("https://ComicConApi");
                    }

                    client.DefaultRequestVersion = HttpVersion.Version11;
                    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                    client.Timeout = TimeSpan.FromSeconds(100);
                })
               .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AutomaticDecompression = DecompressionMethods.None });

        builder.Services.AddHttpClient<IBundlesApi, BundlesApi>(client =>
                {
                    var configured = builder.Configuration["Services:HumbleBundleApiBase"];

                    if (!string.IsNullOrWhiteSpace(configured) && !builder.Environment.IsDevelopment())
                    {
                        if (!Uri.TryCreate(configured, UriKind.Absolute, out var configuredUri))
                        {
                            throw new InvalidOperationException("Invalid URI in configuration setting 'Services:HumbleBundleApiBase'.");
                        }

                        client.BaseAddress = configuredUri;
                    }
                    else
                    {
                        client.BaseAddress = new Uri("https://HumbleBundleApi");
                    }

                    client.DefaultRequestVersion = HttpVersion.Version11;
                    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                    client.Timeout = TimeSpan.FromSeconds(100);
                })
               .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AutomaticDecompression = DecompressionMethods.None });

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        var supportedCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
        var locOptions = new RequestLocalizationOptions().SetDefaultCulture("en-GB")
                                                         .AddSupportedCultures(Array.ConvertAll(supportedCultures, c => c.Name))
                                                         .AddSupportedUICultures(Array.ConvertAll(supportedCultures, c => c.Name));
        app.UseRequestLocalization(locOptions);

        app.UseHttpsRedirection();
        if (app.Environment.IsDevelopment())
        {
            app.UseHttpLogging();
        }

        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}")
           .WithStaticAssets();

        app.MapDefaultEndpoints();

        app.Run();
    }
}