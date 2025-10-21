using Asp.Versioning;
using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Framework.Web.Api.Controllers;
using Framework.Web.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Application.Players.Commands.CreatePlayer;
using Web.Api.Application.Players.Commands.DeletePlayer;
using Web.Api.Application.Players.Commands.PatchPlayer;
using Web.Api.Application.Players.Commands.UpdatePlayer;
using Web.Api.Application.Players.DTOs;
using Web.Api.Application.Players.Queries.GetAllPlayersPaged;
using Web.Api.Application.Players.Queries.GetPlayerById;

namespace Web.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0", Deprecated = false)]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[Authorize]
    public class PlayersController : ApiBaseController
    {
        private readonly ISender _mediator;

        public PlayersController(ISender mediator, IMapper mapper)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PlayerResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPlayersPagedAsync([FromQuery] PaginationParameters pagination, CancellationToken cancellationToken, Guid? userId)
        {
            var query = new GetAllPlayersPagedQuery(pagination, userId);
            var result = await _mediator.Send(query, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errs => Problem(errs)
            );
        }

        [HttpGet("by-user", Name = nameof(GetAllPlayersPagedByUserAsync))]
        public async Task<IActionResult> GetAllPlayersPagedByUserAsync([FromQuery] PaginationParameters pagination, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized(new { message = "Invalid or missing user identifier in token." });
            }

            var query = new GetAllPlayersPagedQuery(pagination, userId);
            var result = await _mediator.Send(query, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errs => Problem(errs)
            );
        }

        [HttpGet("{id:guid}", Name = "GetPlayerByIdAsync")]
        [ProducesResponseType(typeof(ApiResponse<PlayerResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetPlayerByIdQuery(id), cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errs => Problem(errs)
            );
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PlayerResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreatePlayerCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                player => CreatedAtRoute(
                    "GetPlayerByIdAsync",
                    new { id = player.Data.Id },
                    player
                ),
                errors => Problem(errors)
            );
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlayerCommand command, CancellationToken cancellationToken)
        {
            if (command.Id != id)
            {
                List<Error> errors = [Error.Validation("Player.UpdateInvalid", "The request Id does not match with the url Id.")];
                return Problem(errors);
            }

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                _ => NoContent(),
                errors => Problem(errors)
            );
        }

        [HttpPatch("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PlayerResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchAsync(Guid id, [FromBody] PatchPlayerCommand command, CancellationToken cancellationToken)
        {
            if (command.Id != id)
            {
                var errors = new List<Error> { Error.Validation("Player.PatchInvalid", "The provided ID does not match the route parameter.") };
                return Problem(errors);
            }

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                player => Ok(player),
                errors => Problem(errors)
            );
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized(new { message = "Invalid or missing user identifier in token." });
            }

            var command = new DeletePlayerCommand(id, userId.Value);
            var deleteResult = await _mediator.Send(command, cancellationToken);

            return deleteResult.Match(
                _ => NoContent(),
                errors => Problem(errors)
            );
        }
    }
}
