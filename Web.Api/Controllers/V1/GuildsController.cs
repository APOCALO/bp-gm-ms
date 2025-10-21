using Asp.Versioning;
using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Framework.Web.Api.Controllers;
using Framework.Web.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Application.Guilds.Commands.CreateGuild;
using Web.Api.Application.Guilds.Commands.DeleteGuild;
using Web.Api.Application.Guilds.Commands.PatchGuild;
using Web.Api.Application.Guilds.Commands.UpdateGuild;
using Web.Api.Application.Guilds.DTOs;
using Web.Api.Application.Guilds.Queries.GetAllGuildsPaged;
using Web.Api.Application.Guilds.Queries.GetGuildById;

namespace Web.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0", Deprecated = false)]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[Authorize]
    public class GuildsController : ApiBaseController
    {
        private readonly ISender _mediator;

        public GuildsController(ISender mediator, IMapper mapper)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GuildResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllGuildsPagedAsync([FromQuery] PaginationParameters pagination, CancellationToken cancellationToken, Guid? userId)
        {
            var query = new GetAllGuildsPagedQuery(pagination, userId);
            var result = await _mediator.Send(query, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errs => Problem(errs)
            );
        }

        [HttpGet("by-user", Name = nameof(GetAllGuildsPagedByUserAsync))]
        public async Task<IActionResult> GetAllGuildsPagedByUserAsync([FromQuery] PaginationParameters pagination, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized(new { message = "Invalid or missing user identifier in token." });
            }

            var query = new GetAllGuildsPagedQuery(pagination, userId);
            var result = await _mediator.Send(query, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errs => Problem(errs)
            );
        }

        [HttpGet("{id:guid}", Name = "GetGuildByIdAsync")]
        [ProducesResponseType(typeof(ApiResponse<GuildResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetGuildByIdQuery(id), cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errs => Problem(errs)
            );
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<GuildResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateGuildCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                guild => CreatedAtRoute(
                    "GetGuildByIdAsync",
                    new { id = guild.Data.Id },
                    guild
                ),
                errors => Problem(errors)
            );
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGuildCommand command, CancellationToken cancellationToken)
        {
            if (command.Id != id)
            {
                List<Error> errors = [Error.Validation("Guild.UpdateInvalid", "The request Id does not match with the url Id.")];
                return Problem(errors);
            }

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                _ => NoContent(),
                errors => Problem(errors)
            );
        }

        [HttpPatch("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<GuildResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchAsync(Guid id, [FromBody] PatchGuildCommand command, CancellationToken cancellationToken)
        {
            if (command.Id != id)
            {
                var errors = new List<Error> { Error.Validation("Guild.PatchInvalid", "The provided ID does not match the route parameter.") };
                return Problem(errors);
            }

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                guild => Ok(guild),
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

            var command = new DeleteGuildCommand(id, userId.Value);
            var deleteResult = await _mediator.Send(command, cancellationToken);

            return deleteResult.Match(
                _ => NoContent(),
                errors => Problem(errors)
            );
        }
    }
}
