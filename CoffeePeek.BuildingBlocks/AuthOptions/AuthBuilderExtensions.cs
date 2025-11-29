using System.Text;
using CoffeePeek.Domain.Databases;
using CoffeePeek.Domain.Entities.Users;
using CoffeePeek.Shared.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CoffeePeek.BuildingBlocks.AuthOptions;

public static class AuthBuilderExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddBearerAuthentication()
        {
            var jwtOptions = services.AddValidateOptions<JWTOptions>();
            services.AddValidateOptions<AuthenticationOptions>();
            services.AddScoped<RoleManager<IdentityRole<int>>>();
        
            services.AddAuthorization(options =>
            {
                options.AddPolicy(RoleConsts.Admin, policy => policy.RequireRole(RoleConsts.Admin));
                options.AddPolicy(RoleConsts.Merchant, policy => policy.RequireRole(RoleConsts.Merchant));
                options.AddPolicy(RoleConsts.User, policy => policy.RequireRole(RoleConsts.User));
            });

            services.AddUserIdentity();
        
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    #if DEBUG
                    x.RequireHttpsMetadata = false;
                    #elif RELEASE
                    x.RequireHttpsMetadata = true;
                    #endif
                    x.RequireHttpsMetadata = true;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero
                    };
                
                    x.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                            logger.LogDebug($"Token received: {context.Token}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                            logger.LogInformation("Token validated successfully");
            
                            var claims = context.Principal?.Claims;
                            if (claims != null)
                            {
                                foreach (var claim in claims)
                                {
                                    logger.LogInformation($"Validated Claim: {claim.Type} - {claim.Value}");
                                }
                            }

                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                            logger.LogError($"Authentication failed: {context.Exception.Message}");
                            if (context.Exception is SecurityTokenExpiredException)
                            {
                                logger.LogError("Token expired");
                            }
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                            logger.LogWarning($"Challenge requested: {context.Error}, {context.ErrorDescription}");
                            return Task.CompletedTask;
                        }

                    };

                });
        
            return services;
        }

        private void AddUserIdentity()
        {
            services.AddIdentity<User, IdentityRole<int>>(options =>
                {
                    options.User.RequireUniqueEmail = true;

                    options.Password.RequireDigit = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;

                    options.SignIn.RequireConfirmedEmail = false;
                })
                .AddEntityFrameworkStores<CoffeePeekDbContext>()
                .AddDefaultTokenProviders();
        }
    }
}