using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CoreAdvanceConcepts.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await LogRequest(httpContext.Request);
                var originalBodyStream = httpContext.Response.Body;
                using (var responseBody = new MemoryStream())
                {
                    httpContext.Response.Body = responseBody;
                    await _next(httpContext);
                    LogResponse(httpContext.Response);
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An exception occurred: {ex.Message}\nStackTrace: {ex.StackTrace}\nLocation: {ex.TargetSite?.Name} in {ex.TargetSite?.DeclaringType?.FullName}");
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsync("An unexpected error occurred.");
                httpContext.Response.Redirect($"/Home/Error/{httpContext.Response.StatusCode}");                
            }
        }

        private async Task LogRequest(HttpRequest request)
        {            
            var requestBody = await ReadRequestBodyAsync(request);
            _logger.LogInformation($"Request: {request.Method} {request.Path} {requestBody}");
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            request.EnableBuffering();
            using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, false, 1024, true))
            {
                var requestBody = await reader.ReadToEndAsync();
                request.Body.Seek(0, SeekOrigin.Begin); // Rewind the stream
                return requestBody;
            }
        }        

        private void LogResponse(HttpResponse response)
        {
            _logger.LogInformation($"Response: {response.StatusCode}");
            response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = new StreamReader(response.Body).ReadToEnd();
            _logger.LogInformation($"Response Body: {responseBody}");
        }
    }
}
