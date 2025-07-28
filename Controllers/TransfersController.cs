using AccountService.Handlers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Controllers
{
	[ApiController]
	[Route("api/transfers")]
	public class TransfersController : ControllerBase
	{
		private readonly IMediator _mediator;

		public TransfersController(IMediator mediator) => _mediator = mediator;

		/// <summary>
		/// Выполнить перевод между счетами
		/// </summary>
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