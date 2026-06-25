using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;
using API.Helpers;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
            services.AddHttpContextAccessor();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPhotoService, LocalPhotoService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddAutoMapper(config => config.AddMaps(typeof(AutoMapperProfiles).Assembly));
            services.AddDbContext<DataContext>(options => {
                options.UseNpgsql(config.GetConnectionString("DefaultConnection"))
                .UseSnakeCaseNamingConvention();
            });

            return services;
        }
    }
}
