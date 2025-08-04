using AccountService.Models;
using AccountService.Services;
using AccountService.Configuration;
using MediatR;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;

namespace AccountService.Handlers
{
    public class GetAccountsQuery : IRequest<PagedResponse<Account>>
    {
        /// <summary>
        /// Номер страницы (начиная с 1)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Размер страницы (количество элементов)
        /// </summary>
        public int Size { get; set; } = 10;

        /// <summary>
        /// Фильтр по идентификатору владельца
        /// </summary>
        public Guid? OwnerId { get; set; } = null;

        /// <summary>
        /// Фильтр по типу счета
        /// </summary>
        public EAccountType? Type { get; set; } = null;

        public GetAccountsQuery() { }

        /// <summary>
        /// Конструктор запроса
        /// </summary>
        /// <param name="page">Номер страницы</param>
        /// <param name="size">Размер страницы</param>
        /// <param name="ownerId">Фильтр по владельцу</param>
        /// <param name="type">Фильтр по типу счета</param>
        public GetAccountsQuery(int page, int size, Guid? ownerId, EAccountType? type)
        {
            Page = page;
            Size = size;
            OwnerId = ownerId;
            Type = type;
        }
    }

    public sealed class GetAccountsHandler : IRequestHandler<GetAccountsQuery, PagedResponse<Account>>
    {
        private readonly IAccountRepository _repo;
        private readonly IOptions<AccountServiceOptions> _options;

        public GetAccountsHandler(IAccountRepository repo, IOptions<AccountServiceOptions> options)
        {
            _repo = repo;
            _options = options;
        }

        public async Task<PagedResponse<Account>> Handle(GetAccountsQuery req, CancellationToken ct)
        {
            if (req.Page < 1) req.Page = 1;
            if (req.Size < 1 || req.Size > _options.Value.Pagination.MaxPageSize)
                req.Size = _options.Value.Pagination.MaxPageSize;

            var result = await _repo.GetAccountsAsync(req.Page, req.Size, req.OwnerId, req.Type, ct);
            return new PagedResponse<Account>(
                result.Items,
                req.Page,
                req.Size,
                result.TotalCount
            );
        }
    }
}