using AccountService.Handlers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AccountService.Controllers
{
	[ApiController]
    [Authorize] 
	[Route("api/transfers")]
	public class TransfersController : ControllerBase
	{
		private readonly IMediator _mediator;

		public TransfersController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Выполнение перевода между счетами
        /// </summary>
        /// <param name="command">Данные для перевода</param>
        /// <response code="204">Перевод успешно выполнен</response>
        /// <response code="400">Ошибка валидации или бизнес-правила</response>
        [HttpPost]
        [ProducesResponseType(204)]
		[ProducesResponseType(400)]
		public async Task<IActionResult> Transfer([FromBody] TransferCommand command)
		{
			await _mediator.Send(command);
			return NoContent();
		}
	}
}
