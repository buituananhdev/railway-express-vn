using System.Text.Json;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using FluentValidation;
using Microsoft.Extensions.Hosting;

namespace Common.API.Middlewares;
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;
    private readonly ILoggerService _loggerService;
    public ExceptionMiddleware(RequestDelegate next, IWebHostEnvironment env, ILoggerService loggerService)
    {
        _next = next;
        _env = env;
        _loggerService = loggerService;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (CustomException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";
            var response = new { error_code = ex.ErrorCode, message = ex.Message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            _loggerService.LogError(ex.Message, ex);
        }
        catch (ValidationException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var response = new
            {
                error_code = ErrorCodeEnum.InvalidSyntax,
                message = "Invalid syntax",
                details = ex.Errors.Select(e => e.ErrorMessage)
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            _loggerService.LogError("[INVALID]", ex);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            if (_env.IsDevelopment())
            {
                await context.Response.WriteAsync(JsonSerializer.Serialize(
                  new
                  {
                      error_code = ErrorCodeEnum.ServerError,
                      message = "An unexpected error occurred on the server",
                      detail = ex.Message
                  }));
            }
            else await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error_code = ErrorCodeEnum.ServerError,
                message = "An unexpected error occurred on the server"
            }));
            _loggerService.LogError(ex.Message, ex);

        }
    }
}
