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
        /// ���������������� �������� �� �����
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
        /// �������� ���������� �� �������
        /// </summary>
        /// <param name="accountId">ID ����� (�����������)</param>
        /// <param name="page">����� ��������</param>
        /// <param name="size">������ ��������</param>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(PagedResponse<Transaction>))]
        public async Task<ActionResult> GetTransactions(
            [FromQuery] Guid? accountId = null,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
            => Ok(await _mediator.Send(new GetTransactionsQuery(accountId, page, size)));
    }
}