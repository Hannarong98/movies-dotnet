using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi;

namespace Movies.Application.Transformer;

public class OAuth2OpenApiTransformer(IConfiguration configuration) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "Movies API",

        };

        const string schemaKey = "OAuth2";
        var authorizationUrl = configuration["Jwt:AuthorizationURL"] ?? throw new InvalidOperationException("AuthorizationURL is missing");
        var tokenUrl =  configuration["Jwt:TokenURL"] ?? throw new InvalidOperationException("TokenURL is missing");
        var audience =  configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Audience is missing");
        
        Console.WriteLine(authorizationUrl);
        Console.WriteLine(tokenUrl);
        Console.WriteLine(audience);
        
        var scopes = new Dictionary<string, string>
        {
            {
                $"{audience}/.default",
                "All scopes"
            },
        };

        var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            [schemaKey] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Scheme = "OAuth2",
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(authorizationUrl),
                        TokenUrl = new Uri(tokenUrl),
                    }
                }
            }
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = securitySchemes;

        var securityRequirements = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(schemaKey, document)] = [..scopes.Keys]
        };
        
        document.Security = [securityRequirements];
        
        return Task.CompletedTask;
    }
}