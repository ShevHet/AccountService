using AccountService.Models;
using AccountService.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Handlers
{
    public sealed record GetStatementQuery(
        Guid AccountId,
        DateTime? From,
        DateTime? To,
        string Format = "json"
    ) : IRequest<IActionResult>;

    public class GetStatementValidator : AbstractValidator<GetStatementQuery>
    {
        public GetStatementValidator()
        {
            RuleFor(x => x.AccountId).NotEmpty();
            RuleFor(x => x.To)
                .GreaterThanOrEqualTo(x => x.From)
                .When(x => x.From.HasValue && x.To.HasValue)
                .WithMessage("Дата 'до' должна быть после даты 'с'");
            RuleFor(x => x.Format)
                .Must(f => f == "json" || f == "pdf")
                .WithMessage("Неподдерживаемый формат. Допустимые значения: json, pdf");
        }
    }

    public class GetStatementHandler : IRequestHandler<GetStatementQuery, IActionResult>
    {
        private readonly IAccountRepository _repository;
        private readonly IValidator<GetStatementQuery> _validator;

        public GetStatementHandler(
            IAccountRepository repository,
            IValidator<GetStatementQuery> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<IActionResult> Handle(GetStatementQuery request, CancellationToken ct)
        {
            var validationResult = await _validator.ValidateAsync(request, ct);
            if (!validationResult.IsValid)
            {
                return new BadRequestObjectResult(validationResult.Errors);
            }

            var account = await _repository.GetByIdAsync(request.AccountId);
            if (account == null)
            {
                return new NotFoundObjectResult("Счет не найден");
            }

            var transactions = account.Transactions.AsEnumerable();

            if (request.From.HasValue)
            {
                transactions = transactions.Where(t => t.DateTime >= request.From.Value);
            }

            if (request.To.HasValue)
            {
                transactions = transactions.Where(t => t.DateTime <= request.To.Value);
            }

            var result = transactions.ToList();

            if (request.Format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
            {
                return new FileContentResult(GeneratePdf(result), "application/pdf")
                {
                    FileDownloadName = $"statement_{request.AccountId}.pdf"
                };
            }

            return new OkObjectResult(result);
        }

        private byte[] GeneratePdf(List<Transaction> transactions)
        {
            return System.Text.Encoding.UTF8.GetBytes("PDF заглушка");
        }
    }
}
