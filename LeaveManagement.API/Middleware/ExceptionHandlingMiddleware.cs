using LeaveManagement.API.Models;
using LeaveManagement.Application.Exceptions;
using LeaveManagement.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LeaveManagement.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context) {
            try
            {
                await _next(context);
            }
            catch (Exception ex) 
            {
                await HandleExceptions(context, ex);
            }
        }

        private async Task HandleExceptions(HttpContext context, Exception ex) 
        {
            var errorResponse = new ApiErrorResponse();
            if (ex is ValidationException validationEx)
            {
                errorResponse.Title = "Validation Failed";
                errorResponse.Status = 400;
                errorResponse.Detail = ex.Message;
                errorResponse.Errors = validationEx.Errors;
            }
            else if (ex is ArgumentException)
            {
                errorResponse.Title = "Invalid Argument";
                errorResponse.Status = 400;
                errorResponse.Detail = ex.Message;
            }
            else if (ex is UnauthorizedAccessException)
            {
                errorResponse.Title = "Unauthorized Access";
                errorResponse.Status = 401;
                errorResponse.Detail = ex.Message;
            }
            else if (ex is DbUpdateConcurrencyException)
            {
                errorResponse.Title = "Unable to update resource";
                errorResponse.Status = 409;
                errorResponse.Detail = "The resource was modified by another user. Please retrieve the latest version and try again.";
            }
            else if (ex is DomainException)
            {
                errorResponse.Title = "Validation Failed";
                errorResponse.Status = 400;
                errorResponse.Detail = ex.Message;
            }
            else
            {
                errorResponse.Title = "Internal Server Error";
                errorResponse.Status = 500;
                errorResponse.Detail = "Internal server error";
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = errorResponse.Status;
            await JsonSerializer.SerializeAsync(context.Response.Body, errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }
}
