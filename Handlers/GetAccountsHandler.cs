﻿using AccountService.Models;
using AccountService.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Options;
using AccountService.Configuration;

namespace AccountService.Handlers
{
    public sealed record GetAccountsQuery(
        int Page = 1,
        int Size = 10,
        Guid? OwnerId = null,
        EAccountType? Type = null
    ) : IRequest<PagedResponse<Account>>;

    public class GetAccountsValidator : AbstractValidator<GetAccountsQuery>
    {
        public GetAccountsValidator(IOptions<AccountServiceOptions> options)
        {
            var settings = options.Value.Pagination;
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage($"Номер страницы должен быть больше 0");
            RuleFor(x => x.Size)
                .InclusiveBetween(settings.MinPageSize, settings.MaxPageSize)
                .WithMessage($"Размер страницы должен быть от {settings.MinPageSize} до {settings.MaxPageSize}");
        }
    }

    public class GetAccountsHandler : IRequestHandler<GetAccountsQuery, PagedResponse<Account>>
    {
        private readonly IAccountRepository _repo;

        public GetAccountsHandler(IAccountRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedResponse<Account>> Handle(GetAccountsQuery req, CancellationToken ct)
        {
            return await _repo.GetAccountsAsync(req.Page, req.Size, req.OwnerId, req.Type);
        }
    }
}
