using AccountService.Models;
using AccountService.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AccountService.Handlers
{
    /// <summary>
    /// Запрос получения выписки по счету
    /// </summary>
    public sealed record GetStatementQuery(
        /// <summary>
        /// Идентификатор счета
        /// </summary>
        Guid AccountId,

        /// <summary>
        /// Начальная дата периода (опционально)
        /// </summary>
        DateTime? From,

        /// <summary>
        /// Конечная дата периода (опционально)
        /// </summary>
        DateTime? To
    ) : IRequest<List<Transaction>>;

    public sealed class GetStatementHandler : IRequestHandler<GetStatementQuery, List<Transaction>>
    {
        private readonly IAccountRepository _repository;

        public GetStatementHandler(IAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Transaction>> Handle(GetStatementQuery request, CancellationToken cancellationToken)
        {
            // Простая валидация
            if (request.AccountId == Guid.Empty)
            {
                throw new ArgumentException("Неверный ID счета");
            }

            var account = await _repository.GetByIdAsync(request.AccountId, cancellationToken);
            if (account == null)
            {
                throw new KeyNotFoundException("Счет не найден");
            }

            var transactions = account.Transactions.AsQueryable();

            if (request.From.HasValue)
            {
                transactions = transactions.Where(t => t.DateTime >= request.From.Value);
            }

            if (request.To.HasValue)
            {
                transactions = transactions.Where(t => t.DateTime <= request.To.Value);
            }

            return transactions.ToList();
        }
    }
}