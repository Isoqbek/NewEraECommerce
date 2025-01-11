using Serilog;
using System.Net;
using System.Text.Json;

namespace NewEra_Cash_Carry.Middlewares;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        Log.Error(ex, "An error occurred while processing the request.");

        var response = context.Response;
        response.ContentType = "application/json";
        response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var result = JsonSerializer.Serialize(new
        {
            Message = "anUnexpected error occurred! Try again later.",
            Details = ex.Message
        });

        return response.WriteAsync(result);
    }

}
