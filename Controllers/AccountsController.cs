using AccountService.Handlers;
using AccountService.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AccountService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/accounts")]
    public sealed class AccountsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Получение списка счетов с пагинацией
        /// </summary>
        /// <param name="page">Номер страницы (начиная с 1)</param>
        /// <param name="size">Размер страницы (количество элементов)</param>
        /// <param name="ownerId">Фильтр по идентификатору владельца</param>
        /// <param name="type">Фильтр по типу счета</param>
        /// <response code="200">Успешно возвращен список счетов</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet]
        public async Task<ActionResult<MbResult<PagedResponse<Account>>>> GetAccounts(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            [FromQuery] Guid? ownerId = null,
            [FromQuery] EAccountType? type = null)
        {
            try
            {
                var result = await _mediator.Send(new GetAccountsQuery(page, size, ownerId, type));
                return Ok(MbResult<PagedResponse<Account>>.Success(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status500InternalServerError)));
            }
        }

        // <summary>
        /// Поиск счетов по параметрам
        /// </summary>
        /// <param name="query">Параметры поиска</param>
        /// <response code="200">Успешно возвращен результат поиска</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpPost("search")]
        public async Task<ActionResult<MbResult<PagedResponse<Account>>>> SearchAccounts(
            [FromBody] GetAccountsQuery query)
        {
            try
            {
                var result = await _mediator.Send(query);
                return Ok(MbResult<PagedResponse<Account>>.Success(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status500InternalServerError)));
            }
        }

        /// <summary>
        /// Получение счета по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор счета</param>
        /// <response code="200">Успешно возвращен счет</response>
        /// <response code="404">Счет не найден</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<MbResult<Account>>> GetAccountById(Guid id)
        {
            try
            {
                var account = await _mediator.Send(new GetAccountByIdQuery(id));
                return Ok(MbResult<Account>.Success(account));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status404NotFound)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status500InternalServerError)));
            }
        }

        /// <summary>
        /// Проверка наличия счетов у клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <response code="200">Успешно выполнена проверка</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet("clients/{clientId:guid}/exists")]
        public async Task<ActionResult<MbResult<bool>>> ClientHasAccounts(Guid clientId)
        {
            try
            {
                var result = await _mediator.Send(new ClientHasAccountsQuery(clientId));
                return Ok(MbResult<bool>.Success(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status500InternalServerError)));
            }
        }

        // <summary>
        /// Получение баланса счета
        /// </summary>
        /// <param name="id">Идентификатор счета</param>
        /// <response code="200">Успешно возвращен баланс</response>
        /// <response code="404">Счет не найден</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet("{id:guid}/balance")]
        public async Task<ActionResult<MbResult<decimal>>> GetBalance(Guid id)
        {
            try
            {
                var account = await _mediator.Send(new GetAccountByIdQuery(id));
                return Ok(MbResult<decimal>.Success(account.Balance));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status404NotFound)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status500InternalServerError)));
            }
        }

        /// <summary>
        /// Создание нового счета
        /// </summary>
        /// <param name="command">Данные для создания счета</param>
        /// <response code="200">ID созданного счета</response>
        /// <response code="400">Ошибки валидации</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpPost]
        public async Task<ActionResult<MbResult<Guid>>> CreateAccount(
            [FromBody] CreateAccountCommand command)
        {
            try
            {
                var accountId = await _mediator.Send(command);
                return Ok(MbResult<Guid>.Success(accountId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status500InternalServerError)));
            }
        }

        /// <summary>
        /// Обновление счета
        /// </summary>
        /// <param name="id">Идентификатор счета</param>
        /// <param name="command">Данные для обновления</param>
        /// <response code="204">Счет успешно обновлен</response>
        /// <response code="400">Неверные параметры запроса</response>
        /// <response code="404">Счет не найден</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAccount(
            Guid id,
            [FromBody] UpdateAccountCommand command)
        {
            try
            {
                if (id != command.Id)
                {
                    return BadRequest(MbResult<object>.Failure(
                        new MbError("Идентификатор в URL не совпадает с телом запроса",
                        StatusCodes.Status400BadRequest)));
                }

                await _mediator.Send(command);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status404NotFound)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status500InternalServerError)));
            }
        }

        /// <summary>
        /// Частичное обновление счета
        /// </summary>
        /// <param name="id">Идентификатор счета</param>
        /// <param name="patchDoc">JSON Patch документ</param>
        /// <response code="204">Счет успешно обновлен</response>
        /// <response code="400">Неверный формат запроса</response>
        /// <response code="404">Счет не найден</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> PatchAccount(
            Guid id,
            [FromBody] JsonPatchDocument<Account> patchDoc)
        {
            try
            {
                await _mediator.Send(new PatchAccountCommand(id, patchDoc));
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status404NotFound)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status500InternalServerError)));
            }
        }

        /// <summary>
        /// Удаление счета
        /// </summary>
        /// <param name="id">Идентификатор счета</param>
        /// <response code="204">Счет успешно удален</response>
        /// <response code="400">Невозможно удалить счет</response>
        /// <response code="404">Счет не найден</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            try
            {
                await _mediator.Send(new DeleteAccountCommand(id));
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status404NotFound)));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status400BadRequest)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status500InternalServerError)));
            }
        }

        /// <summary>
        /// Получение выписки по счету
        /// </summary>
        /// <param name="id">Идентификатор счета</param>
        /// <param name="from">Начальная дата периода (опционально)</param>
        /// <param name="to">Конечная дата периода (опционально)</param>
        /// <response code="200">Успешно возвращена выписка</response>
        /// <response code="404">Счет не найден</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet("{id:guid}/statement")]
        public async Task<ActionResult<MbResult<List<Transaction>>>> GetStatement(
            Guid id,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var result = await _mediator.Send(new GetStatementQuery(id, from, to));
                return Ok(MbResult<List<Transaction>>.Success(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status404NotFound)));
            }
            catch (Exception ex)
            {
                return StatusCode(500, MbResult<object>.Failure(
                    new MbError(ex.Message, StatusCodes.Status500InternalServerError)));
            }
        }
    }
}