using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.Json;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ISPL.NetCoreFramework.Middlewares
{
    public class ExceptionMiddleware(RequestDelegate _next, IWebHostEnvironment _env)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            string? errorMessage = null;
            context.Response.OnStarting(() =>
            {
                var duration = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2);
                context.Response.Headers["X-Response-Duration-ms"] = duration.ToString(CultureInfo.InvariantCulture);
                return Task.CompletedTask;
            });
            try
            {
                await _next(context);
            }
            catch(Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var response = new
                {
                    context.Response.StatusCode,
                    ex.Message,
                    InnerException = ex.InnerException?.Message
                };
                errorMessage = response.Message is null ? response.InnerException : response.Message;
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            finally
            {
                stopwatch.Stop();
                double duration = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2);
                string logEntry = $"""
            Timestamp        : {DateTime.UtcNow:O}
            Duration(ms)     : {duration}
            StatusCode       : {context.Response.StatusCode}
            Method           : {context.Request.Method}
            Path             : {context.Request.Path}
            Headers_Accept   : {context.Request.Headers.Accept}
            Headers_UserAgent: {context.Request.Headers.UserAgent}
            ErrorMessage     : {errorMessage}
            ------------------------------------------------

            """;
                if(duration > 1000)
                {
                    _ = Task.Run(() => AppendLogAsync(logEntry));
                }
            }
        }
        private async Task AppendLogAsync(string content)
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var logDir = Path.Combine(_env.WebRootPath, "logs");
            var logFilePath = Path.Combine(logDir, $"{date}.log");

            Directory.CreateDirectory(logDir);
            await File.AppendAllTextAsync(logFilePath, content);
        }
    }
}
