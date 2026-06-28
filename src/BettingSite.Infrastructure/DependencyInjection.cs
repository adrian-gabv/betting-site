using BettingSite.Application.Abstractions;
using BettingSite.Infrastructure.Identity;
using BettingSite.Infrastructure.Mappings;
using BettingSite.Infrastructure.Persistence;
using BettingSite.Infrastructure.Persistence.Repositories;
using BettingSite.Infrastructure.Services;
using BettingSite.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BettingSite.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<DataContext>(options =>
        {
            options.UseNpgsql(config.GetConnectionString("DefaultConnection"))
            .UseSnakeCaseNamingConvention();
        });

        services.AddIdentityCore<ApplicationUser>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;
                opt.Password.RequireNonAlphanumeric = false;

                opt.Lockout.AllowedForNewUsers = true;
                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                opt.User.RequireUniqueEmail = true;
            }).AddRoles<ApplicationRole>()
               .AddRoleManager<RoleManager<ApplicationRole>>()
               .AddSignInManager<SignInManager<ApplicationUser>>()
               .AddRoleValidator<RoleValidator<ApplicationRole>>()
               .AddEntityFrameworkStores<DataContext>();

        var jwtSettings = config.GetSection("JwtSettings");
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        jwtSettings["TokenKey"] ?? throw new InvalidOperationException("JwtSettings:TokenKey is not configured"))),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JwtSettings:Issuer is not configured"),
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JwtSettings:Audience is not configured"),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminRole", policy => policy.RequireRole("Admin"));

        services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
        services.AddHttpContextAccessor();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPhotoService, LocalPhotoService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddAutoMapper(config => config.AddMaps(typeof(AutoMapperProfiles).Assembly));

        return services;
    }
}
