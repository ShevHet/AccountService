using AccountService.Handlers;
using AccountService.Models;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountsController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Получить список счетов
        /// </summary>
        /// <param name="page">Номер страницы (начиная с 1)</param>
        /// <param name="size">Количество на странице (1-100)</param>
        /// <param name="ownerId">ID владельца счета</param>
        /// <param name="type">Тип счета</param>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(PagedResponse<Account>))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> GetAccounts(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] Guid? ownerId = null,
            [FromQuery] EAccountType? type = null)
            => Ok(await _mediator.Send(new GetAccountsQuery(page, size, ownerId, type)));

        /// <summary>
        /// Поиск счетов (для длинных запросов)
        /// </summary>
        [HttpPost("search")]
        [ProducesResponseType(200, Type = typeof(PagedResponse<Account>))]
        public async Task<ActionResult> SearchAccounts([FromBody] GetAccountsQuery query)
            => Ok(await _mediator.Send(query));

        /// <summary>
        /// Получить информацию о счете
        /// </summary>
        /// <param name="id">Идентификатор счета</param>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(200, Type = typeof(Account))]
        [ProducesResponseType(404)]
        public async Task<ActionResult> GetAccountById(Guid id)
            => Ok(await _mediator.Send(new GetAccountByIdQuery(id)));

        /// <summary>
        /// Проверить наличие счетов у клиента
        /// </summary>
        /// <param name="clientId">ID клиента</param>
        [HttpGet("clients/{clientId:guid}/exists")]
        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> ClientHasAccounts(Guid clientId)
            => Ok(await _mediator.Send(new ClientHasAccountsQuery(clientId)));

        /// <summary>
        /// Получить баланс счета
        /// </summary>
        /// <param name="id">ID счета</param>
        [HttpGet("{id:guid}/balance")]
        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        public async Task<ActionResult> GetBalance(Guid id)
        {
            var account = await _mediator.Send(new GetAccountByIdQuery(id));
            return Ok(account.Balance);
        }

        /// <summary>
        /// Создать новый счет
        /// </summary>
        [HttpPost]
        [ProducesResponseType(200, Type = typeof(Guid))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> CreateAccount([FromBody] CreateAccountCommand command)
            => Ok(await _mediator.Send(command));

        /// <summary>
        /// Обновить информацию о счете
        /// </summary>
        /// <param name="id">Идентификатор счета</param>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("Идентификатор в URL не совпадает с телом запроса");
            }

            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Частичное обновление счета
        /// </summary>
        /// <param name="id">ID счета</param>
        /// <param name="patchDoc">JSON Patch документ</param>
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PatchAccount(
            Guid id,
            [FromBody] JsonPatchDocument<Account> patchDoc)
        {
            await _mediator.Send(new PatchAccountCommand(id, patchDoc));
            return NoContent();
        }

        /// <summary>
        /// Закрыть счет
        /// </summary>
        /// <param name="id">Идентификатор счета</param>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            await _mediator.Send(new DeleteAccountCommand(id));
            return NoContent();
        }

        /// <summary>
        /// Получить выписку по счету
        /// </summary>
        /// <param name="id">Идентификатор счета</param>
        /// <param name="from">Начало периода (опционально)</param>
        /// <param name="to">Конец периода (опционально)</param>
        /// <param name="format">Формат выписки (json/pdf)</param>
        [HttpGet("{id:guid}/statement")]
        [ProducesResponseType(200, Type = typeof(List<Transaction>))]
        [ProducesResponseType(200, Type = typeof(FileResult))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetStatement(
            Guid id,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string format = "json")
            => await _mediator.Send(new GetStatementQuery(id, from, to, format));
    }
}
