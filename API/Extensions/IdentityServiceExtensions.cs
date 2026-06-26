using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddIdentityCore<AppUser>(opt =>
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
            }) .AddRoles<AppRole>()
               .AddRoleManager<RoleManager<AppRole>>()
               .AddSignInManager<SignInManager<AppUser>>()
               .AddRoleValidator<RoleValidator<AppRole>>()
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

            return services;
        }
    }
}
