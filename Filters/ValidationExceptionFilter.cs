using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace AccountService.Filters
{
    public class ValidationExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
			if (context.Exception is ValidationException ex)
			{
				var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
				context.Result = new BadRequestObjectResult(new { Errors = errors });
				context.ExceptionHandled = true;
			}
		}
	}
}