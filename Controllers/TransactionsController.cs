using AccountService.Handlers;
using AccountService.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TransactionsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Зарегистрировать операцию по счету
        /// </summary>
        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RegisterTransaction([FromBody] RegisterTransactionCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Получить транзакции по фильтру
        /// </summary>
        /// <param name="accountId">ID счета (опционально)</param>
        /// <param name="page">Номер страницы</param>
        /// <param name="size">Размер страницы</param>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(PagedResponse<Transaction>))]
        public async Task<ActionResult> GetTransactions(
            [FromQuery] Guid? accountId = null,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
            => Ok(await _mediator.Send(new GetTransactionsQuery(accountId, page, size)));
    }
}
