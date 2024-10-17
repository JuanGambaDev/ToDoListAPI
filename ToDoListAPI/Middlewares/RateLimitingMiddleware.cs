using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ToDoListAPI.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, (int Count, DateTime Timestamp)> _requestCounts = new();
        private const int _limit = 5; // Límite de solicitudes
        private static readonly TimeSpan _duration = TimeSpan.FromSeconds(1); // Duración para el límite

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            if (ipAddress == null)
            {
                await _next(context);
                return;
            }

            var now = DateTime.UtcNow;

            // Obtiene el contador de solicitudes para la IP
            var requestInfo = _requestCounts.GetOrAdd(ipAddress, (0, now));

            // Resetea el contador si ha pasado el tiempo de duración
            if (now - requestInfo.Timestamp > _duration)
            {
                requestInfo = (0, now);
            }

            // Incrementa el contador
            requestInfo.Count++;

            // Verifica si se ha superado el límite
            if (requestInfo.Count > _limit)
            {
                _logger.LogWarning("Rate limit exceeded for {IpAddress} on {Path}", ipAddress, context.Request.Path);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;

                var response = new
                {
                    error = "Rate limit exceeded.",
                    message = "You have exceeded the number of allowed requests. Please try again later.",
                    statusCode = (int)HttpStatusCode.TooManyRequests
                };

                // Serializa la respuesta a JSON
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }

            // Actualiza el contador en el diccionario
            _requestCounts[ipAddress] = requestInfo;

            // Continúa con la siguiente parte del middleware
            await _next(context);
        }
    }
}

