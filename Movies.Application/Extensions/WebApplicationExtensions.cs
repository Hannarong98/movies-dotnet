using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Scalar.AspNetCore;

namespace Movies.Application.Extensions;

public static class WebApplicationExtensions
{

    extension(WebApplication app)
    {
        public WebApplication MapOpenApiSpec(IConfiguration configuration)
        {
            app.MapOpenApi();
            app.MapScalarApiReference("/docs", options =>
            {
                options.WithTitle("Movies API")
                    .SortTagsAlphabetically()
                    .SortOperationsByMethod()
                    .AddPreferredSecuritySchemes("OAuth2")
                    .AddAuthorizationCodeFlow("OAuth2", flow =>
                    {
                        flow.ClientId = configuration["Scalar:ClientId"]!;
                        flow.ClientSecret = configuration["Scalar:ClientSecret"]!;
                        flow.Pkce = Pkce.Sha256;
                        flow.AuthorizationUrl = configuration["Jwt:AuthorizationURL"]!;
                        flow.TokenUrl = configuration["Jwt:AccessTokenURL"]!;
                        flow.WithCredentialsLocation(CredentialsLocation.Body);
                    })
                    .AddDefaultScopes("OAuth2", ["movies.write"]);
            });

            return app;
        }
    }
}