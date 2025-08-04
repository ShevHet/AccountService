using AccountService.Handlers;
using AccountService.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AccountService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TransactionsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Регистрация операции по счету
        /// </summary>
        /// <param name="command">Данные операции</param>
        /// <response code="204">Операция успешно зарегистрирована</response>
        /// <response code="400">Ошибка валидации или бизнес-правила</response>
        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RegisterTransaction([FromBody] RegisterTransactionCommand command)
        {
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Получение списка транзакций
        /// </summary>
        /// <param name="accountId">Фильтр по идентификатору счета (опционально)</param>
        /// <param name="page">Номер страницы</param>
        /// <param name="size">Размер страницы</param>
        /// <response code="200">Успешно возвращен список транзакций</response>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(PagedResponse<Transaction>))]
        public async Task<ActionResult> GetTransactions(
            [FromQuery] Guid? accountId = null,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
            => Ok(await _mediator.Send(new GetTransactionsQuery(accountId, page, size)));
    }
}
