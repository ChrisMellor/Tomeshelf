using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Tomeshelf.ServiceDefaults;
using Tomeshelf.Web.Controllers;
using Tomeshelf.Web.Infrastructure;
using Tomeshelf.Web.Services;

namespace Tomeshelf.Web;

/// <summary>
///     Web application entry point and configuration.
/// </summary>
public class Program
{
    public static WebApplication BuildApp(string[] args, Action<WebApplicationBuilder>? configureBuilder = null)
    {
        var builder = WebApplication.CreateBuilder(args);

        configureBuilder?.Invoke(builder);

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
        builder.Services
               .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
               .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.Name = "tomeshelf.oauth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = false;
                })
               .AddOAuth(DriveAuthController.AuthenticationScheme, options =>
                {
                    var drive = builder.Configuration.GetSection("GoogleDrive");
                    options.ClientId = drive["ClientId"] ?? string.Empty;
                    options.ClientSecret = drive["ClientSecret"] ?? string.Empty;
                    options.CallbackPath = "/drive-auth/callback";
                    options.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
                    options.TokenEndpoint = "https://oauth2.googleapis.com/token";
                    options.Scope.Clear();
                    options.Scope.Add("https://www.googleapis.com/auth/drive");
                    options.SaveTokens = false;
                    options.UsePkce = true;
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.Events = new OAuthEvents
                    {
                        OnRedirectToAuthorizationEndpoint = context =>
                        {
                            var hint = context.Properties.Items.TryGetValue("login_hint", out var value)
                                ? value
                                : drive["UserEmail"];
                            var parameters = new Dictionary<string, string?>
                            {
                                ["access_type"] = "offline",
                                ["prompt"] = "consent",
                                ["include_granted_scopes"] = "true",
                                ["login_hint"] = hint
                            };

                            var redirectUri = QueryHelpers.AddQueryString(context.RedirectUri, parameters);
                            context.RedirectUri = redirectUri;

                            return Task.CompletedTask;
                        },
                        OnCreatingTicket = context =>
                        {
                            if (string.IsNullOrWhiteSpace(context.RefreshToken))
                            {
                                context.HttpContext.Session.SetString(GoogleDriveSessionKeys.Error, "Google did not return a refresh token. Re-run the flow and accept offline access.");
                                context.Fail("Missing refresh token.");

                                return Task.CompletedTask;
                            }

                            context.HttpContext.Session.SetString(GoogleDriveSessionKeys.ClientId, context.Options.ClientId ?? string.Empty);
                            context.HttpContext.Session.SetString(GoogleDriveSessionKeys.ClientSecret, context.Options.ClientSecret ?? string.Empty);
                            context.HttpContext.Session.SetString(GoogleDriveSessionKeys.RefreshToken, context.RefreshToken);

                            if (context.Properties.Items.TryGetValue("login_hint", out var loginHint) && !string.IsNullOrWhiteSpace(loginHint))
                            {
                                context.HttpContext.Session.SetString(GoogleDriveSessionKeys.UserEmail, loginHint);
                            }
                            else if (!string.IsNullOrWhiteSpace(drive["UserEmail"]))
                            {
                                context.HttpContext.Session.SetString(GoogleDriveSessionKeys.UserEmail, drive["UserEmail"]);
                            }

                            return Task.CompletedTask;
                        },
                        OnRemoteFailure = context =>
                        {
                            var message = context.Failure?.Message ?? "Google authorisation failed.";
                            context.HttpContext.Session.SetString(GoogleDriveSessionKeys.Error, message);
                            context.Response.Redirect("/drive-auth/result");
                            context.HandleResponse();

                            return Task.CompletedTask;
                        }
                    };
                });
        builder.Services.AddAuthorization();
        builder.Services.AddLocalization();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.Cookie.Name = "tomeshelf.web.session";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.IdleTimeout = TimeSpan.FromHours(8);
        });
        builder.Services.AddTransient<FitbitSessionCookieHandler>();
        builder.Services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 1_073_741_824;
        });

        var gatewayBaseUri = TryResolveGatewayBaseUri(builder);

        builder.Services
               .AddHttpClient(GuestsApi.HttpClientName, client =>
                 {
                     if (gatewayBaseUri is not null)
                     {
                         client.BaseAddress = new Uri(gatewayBaseUri, "api/mcm/");
                     }
                     else
                     {
                         var configured = builder.Configuration["Services:McmApiBase"] ?? builder.Configuration["Services:ApiBase"];

                         if (!string.IsNullOrWhiteSpace(configured) && !builder.Environment.IsDevelopment())
                         {
                             if (!Uri.TryCreate(configured, UriKind.Absolute, out var configuredUri))
                             {
                                 throw new InvalidOperationException("Invalid URI in configuration setting 'Services:McmApiBase'.");
                             }

                             client.BaseAddress = configuredUri;
                         }
                         else
                         {
                             var protocol = "https";
                             if (IsRunningInDocker())
                             {
                                 protocol = "http";
                             }

                             client.BaseAddress = new Uri($"{protocol}://mcmapi");
                         }
                     }

                     client.DefaultRequestVersion = HttpVersion.Version11;
                     client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                     client.Timeout = TimeSpan.FromSeconds(100);
                 })
               .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AutomaticDecompression = DecompressionMethods.None });
        builder.Services.AddScoped<IGuestsApi, GuestsApi>();

        builder.Services
               .AddHttpClient(BundlesApi.HttpClientName, client =>
                {
                     if (gatewayBaseUri is not null)
                     {
                         client.BaseAddress = new Uri(gatewayBaseUri, "api/humblebundle/");
                     }
                     else
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
                             var protocol = "https";
                             if (IsRunningInDocker())
                             {
                                 protocol = "http";
                             }

                             client.BaseAddress = new Uri($"{protocol}://humblebundleapi");
                         }
                     }

                     client.DefaultRequestVersion = HttpVersion.Version11;
                     client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                     client.Timeout = TimeSpan.FromSeconds(100);
                 })
               .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AutomaticDecompression = DecompressionMethods.None });
        builder.Services.AddScoped<IBundlesApi, BundlesApi>();

        builder.Services
               .AddHttpClient(FitbitApi.HttpClientName, client =>
                 {
                     if (gatewayBaseUri is not null)
                     {
                         client.BaseAddress = gatewayBaseUri;
                     }
                     else
                     {
                         var configured = builder.Configuration["Services:FitbitApiBase"];

                         if (!string.IsNullOrWhiteSpace(configured) && !builder.Environment.IsDevelopment())
                         {
                             if (!Uri.TryCreate(configured, UriKind.Absolute, out var configuredUri))
                             {
                                 throw new InvalidOperationException("Invalid URI in configuration setting 'Services:FitbitApiBase'.");
                             }

                             client.BaseAddress = configuredUri;
                         }
                         else
                         {
                             var protocol = "https";
                             if (IsRunningInDocker())
                             {
                                 protocol = "http";
                             }

                             client.BaseAddress = new Uri($"{protocol}://fitbitapi");
                         }
                     }

                     client.DefaultRequestVersion = HttpVersion.Version11;
                     client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                     client.Timeout = TimeSpan.FromSeconds(100);
                 })
               .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
               {
                   AutomaticDecompression = DecompressionMethods.None,
                   AllowAutoRedirect = false
               })
               .AddHttpMessageHandler<FitbitSessionCookieHandler>();
        builder.Services.AddScoped<IFitbitApi, FitbitApi>();

        builder.Services
               .AddHttpClient(PaissaApi.HttpClientName, client =>
                 {
                      if (gatewayBaseUri is not null)
                      {
                          client.BaseAddress = new Uri(gatewayBaseUri, "api/");
                      }
                      else
                      {
                          var configured = builder.Configuration["Services:PaissaApiBase"];

                          if (!string.IsNullOrWhiteSpace(configured) && !builder.Environment.IsDevelopment())
                          {
                              if (!Uri.TryCreate(configured, UriKind.Absolute, out var configuredUri))
                              {
                                  throw new InvalidOperationException("Invalid URI in configuration setting 'Services:PaissaApiBase'.");
                              }

                              client.BaseAddress = configuredUri;
                          }
                          else
                          {
                              var protocol = "https";
                              if (IsRunningInDocker())
                              {
                                  protocol = "http";
                              }

                              client.BaseAddress = new Uri($"{protocol}://paissaapi");
                          }
                      }

                     client.DefaultRequestVersion = HttpVersion.Version11;
                     client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                     client.Timeout = TimeSpan.FromSeconds(30);
                 })
               .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AutomaticDecompression = DecompressionMethods.None });
        builder.Services.AddScoped<IPaissaApi, PaissaApi>();

        builder.Services
               .AddHttpClient(ShiftApi.HttpClientName, client =>
                 {
                     if (gatewayBaseUri is not null)
                     {
                         client.BaseAddress = new Uri(gatewayBaseUri, "api/shift/");
                     }
                     else
                     {
                         var configured = builder.Configuration["Services:ShiftApiBase"];

                         if (!string.IsNullOrWhiteSpace(configured) && !builder.Environment.IsDevelopment())
                         {
                             if (!Uri.TryCreate(configured, UriKind.Absolute, out var configuredUri))
                             {
                                 throw new InvalidOperationException("Invalid URI in configuration setting 'Services:ShiftApiBase'.");
                             }

                             client.BaseAddress = configuredUri;
                         }
                         else
                         {
                             var protocol = "https";
                             if (IsRunningInDocker())
                             {
                                 protocol = "http";
                             }

                             client.BaseAddress = new Uri($"{protocol}://shiftapi");
                         }
                     }

                     client.DefaultRequestVersion = HttpVersion.Version11;
                     client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                     client.Timeout = TimeSpan.FromSeconds(30);
                 })
               .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AutomaticDecompression = DecompressionMethods.None });
        builder.Services.AddScoped<IShiftApi, ShiftApi>();

        builder.Services
               .AddHttpClient(FileUploadsApi.HttpClientName, client =>
                 {
                     if (gatewayBaseUri is not null)
                     {
                         client.BaseAddress = new Uri(gatewayBaseUri, "api/fileuploader/");
                     }
                     else
                     {
                         var configured = builder.Configuration["Services:FileUploaderApiBase"];

                         if (!string.IsNullOrWhiteSpace(configured))
                         {
                             if (!Uri.TryCreate(configured, UriKind.Absolute, out var configuredUri))
                             {
                                 throw new InvalidOperationException("Invalid URI in configuration setting 'Services:FileUploaderApiBase'.");
                             }

                             client.BaseAddress = configuredUri;
                         }
                         else
                         {
                             if (IsRunningInDocker())
                             {
                                 client.BaseAddress = new Uri("http://fileuploaderapi");
                             }
                             else
                             {
                                 client.BaseAddress = new Uri("https://localhost:49960");
                             }
                         }
                     }

                     client.DefaultRequestVersion = HttpVersion.Version11;
                     client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                     client.Timeout = TimeSpan.FromMinutes(30);
                 })
               .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AutomaticDecompression = DecompressionMethods.None })
               .ConfigureHttpMessageHandlerBuilder(b => b.AdditionalHandlers.Clear());
        builder.Services.AddScoped<IFileUploadsApi, FileUploadsApi>();

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
        app.UseSession();

        app.UseAuthentication();
        app.UseAuthorization();

        var staticAssetsManifestPath = app.Configuration["StaticAssets:ManifestPath"];
        if (!string.IsNullOrWhiteSpace(staticAssetsManifestPath))
        {
            app.MapStaticAssets(staticAssetsManifestPath);
        }
        else
        {
            app.MapStaticAssets();
        }

        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}")
           .WithStaticAssets();

        app.MapDefaultEndpoints();

        return app;
    }

    /// <summary>
    ///     Application entry point for the MVC web host.
    ///     Configures services and starts the web server.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        var app = BuildApp(args);
        app.Run();
    }

    private static bool IsRunningInDocker()
    {
        return File.Exists("/.dockerenv");
    }

    private static Uri? TryResolveGatewayBaseUri(WebApplicationBuilder builder)
    {
        var configured = builder.Configuration["Services:GatewayBase"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            if (!Uri.TryCreate(configured, UriKind.Absolute, out var configuredUri))
            {
                throw new InvalidOperationException("Invalid URI in configuration setting 'Services:GatewayBase'.");
            }

            return EnsureTrailingSlash(configuredUri);
        }

        var discovered = TryGetAspireServiceEndpointUri(builder.Configuration, "gateway");
        if (discovered is not null)
        {
            return EnsureTrailingSlash(discovered);
        }

        return null;
    }

    private static Uri? TryGetAspireServiceEndpointUri(IConfiguration configuration, string serviceName)
    {
        var serviceSection = configuration.GetSection("services")
                                          .GetSection(serviceName);
        if (!serviceSection.Exists())
        {
            return null;
        }

        // Prioritize well-known transports first ("http" then "https"), followed by any others (e.g. "grpc").
        var transports = new List<IConfigurationSection>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var transportName in new[] { "http", "https" })
        {
            var preferred = serviceSection.GetSection(transportName);
            if (preferred.Exists())
            {
                transports.Add(preferred);
                seen.Add(transportName);
            }
        }

        foreach (var transport in serviceSection.GetChildren())
        {
            if (!seen.Contains(transport.Key))
            {
                transports.Add(transport);
            }
        }

        foreach (var transport in transports)
        {
            foreach (var endpointSection in transport.GetChildren())
            {
                var rawAddress = endpointSection.Value?.Trim();
                if (string.IsNullOrWhiteSpace(rawAddress) || !Uri.TryCreate(rawAddress, UriKind.Absolute, out var parsed))
                {
                    continue;
                }

                return parsed;
            }
        }

        return null;
    }

    private static Uri EnsureTrailingSlash(Uri uri)
    {
        var left = uri.GetLeftPart(UriPartial.Path);
        if (!left.EndsWith("/", StringComparison.Ordinal))
        {
            left += "/";
        }

        return new Uri(left, UriKind.Absolute);
    }
}
