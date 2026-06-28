using System.Net;
using System.Text.Json;
using BettingSite.API.Errors;

namespace BettingSite.API.Middleware
{
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionMiddleware> _logger = logger;
        private readonly IHostEnvironment _env = env;

        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, "An unhandled exception has occurred: {Message}", exc.Message);
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("The response has already started, the error handling middleware will not be executed.");
                    throw;
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = _env.IsDevelopment()
                    ? new ApiException(context.Response.StatusCode, exc.Message, exc.StackTrace ?? string.Empty)
                    : new ApiException(context.Response.StatusCode, "Internal Server Error");

                await context.Response.WriteAsJsonAsync(response, _jsonOptions);
            }
        }
    }
}
