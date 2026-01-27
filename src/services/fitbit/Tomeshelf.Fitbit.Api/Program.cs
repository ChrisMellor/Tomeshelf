using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Tomeshelf.Fitbit.Application;
using Tomeshelf.Fitbit.Infrastructure;
using Tomeshelf.ServiceDefaults;

namespace Tomeshelf.Fitbit.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.AddServiceDefaults();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddHttpLogging(o =>
            {
                o.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.RequestMethod | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration | HttpLoggingFields.RequestHeaders | HttpLoggingFields.ResponseHeaders;
                o.RequestHeaders.Add("User-Agent");
                o.MediaTypeOptions.AddText("application/json");
            });
        }

        builder.Services
               .AddProblemDetails()
               .AddOpenApi()
               .AddControllers()
               .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "tomeshelf.fitbit.oauth";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = false;
        })
        .AddOAuth(FitbitOAuthDefaults.AuthenticationScheme, options =>
        {
            var fitbit = builder.Configuration.GetSection("Fitbit");
            options.ClientId = fitbit["ClientId"] ?? string.Empty;
            options.ClientSecret = fitbit["ClientSecret"] ?? string.Empty;
            options.CallbackPath = fitbit["CallbackPath"] ?? "/api/fitbit/auth/callback";
            options.AuthorizationEndpoint = "https://www.fitbit.com/oauth2/authorize";
            options.TokenEndpoint = "https://api.fitbit.com/oauth2/token";
            options.Scope.Clear();
            var scopeText = fitbit["Scope"] ?? string.Empty;
            foreach (var scope in scopeText.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                options.Scope.Add(scope);
            }

            options.SaveTokens = false;
            options.UsePkce = true;
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Events = new OAuthEvents
            {
                OnRedirectToAuthorizationEndpoint = context =>
                {
                    var parameters = new Dictionary<string, string?>
                    {
                        ["prompt"] = "login"
                    };

                    var redirectUri = QueryHelpers.AddQueryString(context.RedirectUri, parameters);
                    context.RedirectUri = redirectUri;
                    return Task.CompletedTask;
                },
                OnCreatingTicket = context =>
                {
                    if (string.IsNullOrWhiteSpace(context.AccessToken))
                    {
                        context.Fail("Missing access token.");
                        return Task.CompletedTask;
                    }

                    var tokenCache = context.HttpContext.RequestServices.GetRequiredService<FitbitTokenCache>();
                    var expiresAt = context.ExpiresIn.HasValue
                        ? DateTimeOffset.UtcNow.Add(context.ExpiresIn.Value)
                        : (DateTimeOffset?)null;
                    tokenCache.Update(context.AccessToken, context.RefreshToken, expiresAt);

                    return Task.CompletedTask;
                },
                OnRemoteFailure = context =>
                {
                    var message = context.Failure?.Message ?? "Fitbit authorisation failed.";
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning(context.Failure, "Fitbit OAuth failed: {Message}", message);
                    context.Response.Redirect($"/api/fitbit/auth/failure?message={Uri.EscapeDataString(message)}");
                    context.HandleResponse();
                    return Task.CompletedTask;
                }
            };
        });
        builder.Services.AddAuthorization();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.Cookie.Name = "tomeshelf.fitbit.session";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.IdleTimeout = TimeSpan.FromHours(8);
        });

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        var config = builder.Configuration.GetSection("Fitbit");

        builder.Services
               .AddOptions<FitbitOptions>()
               .Bind(config)
               .ValidateDataAnnotations();

        builder.Services.AddApplicationServices();
        builder.AddInfrastructureServices();

        var app = builder.Build();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = string.Empty;
                options.SwaggerEndpoint("/openapi/v1.json", "Tomeshelf Fitbit API v1");
            });
        }

        app.UseHttpsRedirection();
        if (app.Environment.IsDevelopment())
        {
            app.UseHttpLogging();
        }

        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapExecutorDiscoveryEndpoint();
        app.MapDefaultEndpoints();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TomeshelfFitbitDbContext>();
            await db.Database.MigrateAsync();
        }

        await app.RunAsync();
    }
}
