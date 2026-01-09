using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Movies.Application.Auth;
using Movies.Application.Database;
using Movies.Application.Repositories;
using Movies.Application.Services;

namespace Movies.Application.Extensions;

public static class ServiceCollectionExtensions
{

    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            services.AddSingleton<IMovieRepository, MovieRepository>();
            services.AddSingleton<IMovieService, MovieService>();
            services.AddSingleton<IRatingRepository, RatingRepository>();
            services.AddSingleton<IRatingService, RatingService>();
            services.AddValidatorsFromAssemblyContaining<IApplicationMarker>(ServiceLifetime.Singleton);
            return services;
        }

        public IServiceCollection AddDatabase(IConfiguration configuration)
        {
            services.AddSingleton<IDbConnectionFactory>(_ =>
                new NpgsqlConnectionFactory(configuration["Database:ConnectionString"]!));
            services.AddSingleton<DbInitializer>();
            return services;
        }
        
        public IServiceCollection AddAuth(IConfiguration configuration)
        {
            services.AddAuthentication().AddJwtBearer(options =>
            {
                options.Authority = "http://localhost:8080/realms/movies";
                options.Audience = configuration["Jwt:ClientId"];
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    RoleClaimType = ClaimTypes.Role
                };
                options.MapInboundClaims = false;
            });

            services.AddScoped<IClaimsTransformation, KeycloakRolesClaimsTransformation>();

            services.AddAuthorizationBuilder()
                .AddPolicy(Roles.Write, policy => { policy.RequireRole(Roles.Write); });
            return services;
        }


        public IServiceCollection AddOpenApiWithSecuritySchemes(IConfiguration configuration)
        {
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, _, _) =>
                {
                    
                    const string schemaKey = "OAuth2";
                    var authorizationUrl = configuration["Jwt:AuthorizationURL"] ?? throw new InvalidOperationException("AuthorizationURL is missing");
                    var tokenUrl =  configuration["Jwt:AccessTokenURL"] ?? throw new InvalidOperationException("AccessTokenURL is missing");
                    
                    var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
                    {
                        [schemaKey] = new OpenApiSecurityScheme
                        {
                            BearerFormat = "JWT",
                            Description = "OAuth2 authentication using JWT bearer tokens.",
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
                    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                    document.Components.SecuritySchemes = securitySchemes;
                    
                    document.Tags = new SortedSet<OpenApiTag>
                    {
                        new()
                        {
                            Name = "Health",
                            Description = "Performs API health check",
                        }
                    };
                    
                    var operation = new OpenApiOperation
                    {
                        OperationId = "HealthCheck",
                        Description = "Performs API health check",
                        Tags = new HashSet<OpenApiTagReference>
                        {
                            new ("Health", document)
                        },
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse { Description = "Healthy" },
                            ["503"] = new OpenApiResponse { Description = "Service Unavailable" }
                        }
                    };

                    var pathItem = new OpenApiPathItem();
                    pathItem.AddOperation(HttpMethod.Get, operation);
                    document.Paths.Add("/healthz", pathItem);
                    
                    return Task.CompletedTask;
                });
            });
            
            return services;
        }
    }
}