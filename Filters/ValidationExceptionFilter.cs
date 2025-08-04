using AccountService.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AccountService.Filters
{
    public sealed class ValidationExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is ValidationException ex)
            {
                var errors = new Dictionary<string, List<string>>();

                foreach (var error in ex.Errors)
                {
                    if (!errors.ContainsKey(error.PropertyName))
                    {
                        errors[error.PropertyName] = new List<string>();
                    }
                    errors[error.PropertyName].Add(error.ErrorMessage);
                }

                var formattedErrors = new Dictionary<string, string[]>();
                foreach (var error in errors)
                {
                    formattedErrors[error.Key] = error.Value.ToArray();
                }

                var mbError = new MbError("Ошибки валидации", StatusCodes.Status400BadRequest, formattedErrors);
                var result = MbResult<object>.Failure(mbError);

                context.Result = new BadRequestObjectResult(result);
                context.ExceptionHandled = true;
            }
        }
    }
}