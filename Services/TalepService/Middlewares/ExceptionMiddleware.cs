using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TalepService.Exceptions;
using TalepService.Wrappers;

namespace TalepService.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = new BaseResponse<string>(exception.Message);

            switch (exception)
            {
                case CustomException e:
                    // Custom application error
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.ErrorCode = "CUSTOM_ERROR";
                    break;
                case BadRequestException e:
                    // Bad request error
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.ErrorCode = "BAD_REQUEST";
                    break;
                case NotFoundException e:
                    // Not found error
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.ErrorCode = "NOT_FOUND";
                    break;
                case KeyNotFoundException e:
                    // Not found error
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.ErrorCode = "KEY_NOT_FOUND";
                    break;
                default:
                    // Unhandled error
                    _logger.LogError(exception, exception.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "Internal Server Error"; // Prod ortamında detay gizlenebilir
                    response.ErrorCode = "INTERNAL_SERVER_ERROR";
                    break;
            }

            var result = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(result);
        }
    }
}
