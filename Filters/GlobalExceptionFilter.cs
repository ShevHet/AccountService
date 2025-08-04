using AccountService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;

namespace AccountService.Filters
{
    public sealed class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            int statusCode = StatusCodes.Status500InternalServerError;
            string message = "¬нутренн€€ ошибка сервера";

            if (context.Exception is KeyNotFoundException)
            {
                statusCode = StatusCodes.Status404NotFound;
                message = "–есурс не найден";
            }
            else if (context.Exception is ValidationException)
            {
                return;
            }
            else if (context.Exception is InvalidOperationException)
            {
                statusCode = StatusCodes.Status400BadRequest;
                message = context.Exception.Message;
            }

            var error = new MbError(message, statusCode);
            context.Result = new ObjectResult(MbResult<object>.Failure(error))
            {
                StatusCode = statusCode
            };
            context.ExceptionHandled = true;
        }
    }
} 