using Asp.Versioning;
using AutoMapper;
using ErrorOr;
using Framework.Application.Common;
using Framework.Web.Api.Controllers;
using Framework.Web.Api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Application.Companies.Commands.CreateCompany;
using Web.Api.Application.Companies.Commands.DeleteCompany;
using Web.Api.Application.Companies.Commands.PatchCompany;
using Web.Api.Application.Companies.Commands.UpdateCompany;
using Web.Api.Application.Companies.DTOs;
using Web.Api.Application.Companies.Queries.GetAllCompaniesPaged;
using Web.Api.Application.Companies.Queries.GetCompanyById;

namespace Web.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0", Deprecated = false)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class CompaniesController : ApiBaseController
    {
        private readonly ISender _mediator;

        public CompaniesController(ISender mediator, IMapper mapper)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Retrieve companies paginated.
        /// </summary>
        /// <param name="pagination">
        /// Pagination parameters.  
        /// Expected query fields: <c>pageNumber</c> (int, 1..N) y <c>pageSize</c> (int, 1..N).
        /// </param>
        /// <param name="userId">Optional user filter (only companies created by this user).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// <para><b>Examples</b></para>
        /// <para>GET <c>/api/v1/companies?pageNumber=1&amp;pageSize=20</c></para>
        /// </remarks>
        /// <response code="200">A paginated list of companies.</response>
        /// <response code="400">Invalid pagination parameters.</response>
        /// <response code="500">Unexpected server error.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CompanyResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCompaniesPagedAsync(
            [FromQuery] PaginationParameters pagination,
            CancellationToken cancellationToken,
            Guid? userId)
        {
            var query = new GetAllCompaniesPagedQuery(pagination, userId);
            var result = await _mediator.Send(query, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errs => Problem(errs)
            );
        }

        /// <summary>
        /// Retrieve companies paginated by a specific user (only companies created by this user).
        /// </summary>
        /// <param name="pagination">
        /// Pagination parameters.  
        /// Expected query fields: <c>pageNumber</c> (int) y <c>pageSize</c> (int).
        /// </param>
        /// <param name="userId">
        /// The user's identifier used to filter companies (GUID).  
        /// Only companies created by this user will be returned.
        /// </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// <para><b>Examples</b></para>
        /// <para>GET <c>/api/v1/companies/by-user?userId=11111111-2222-3333-4444-555555555555&amp;pageNumber=1&amp;pageSize=20</c></para>
        /// </remarks>
        /// <response code="200">A paginated list of companies for the given user.</response>
        /// <response code="400">Invalid <c>userId</c> or pagination parameters.</response>
        /// <response code="500">Unexpected server error.</response>
        [HttpGet("by-user", Name = nameof(GetAllCompaniesPagedByUserAsync))]
        public async Task<IActionResult> GetAllCompaniesPagedByUserAsync([FromQuery] PaginationParameters pagination, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized(new { message = "Invalid or missing user identifier in token." });
            }

            var query = new GetAllCompaniesPagedQuery(pagination, userId);
            var result = await _mediator.Send(query, cancellationToken);

            return result.Match(
                ok => Ok(ok),
                errs => Problem(errs)
            );
        }

        /// <summary>Retrieves a company by its unique identifier.</summary>
        /// <param name="id">The unique identifier of the company.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpGet("{id:guid}", Name = "GetByIdAsync")]
        [ProducesResponseType(typeof(ApiResponse<CompanyResponseDTO>), StatusCodes.Status200OK)] //  alineado
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCompanyByIdQuery(id), cancellationToken);

            return result.Match(
                company => Ok(company),
                errors => Problem(errors)
            );
        }

        /// <summary>Create a new company.</summary>
        /// <param name="command">Information required to register the company.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpPost]
        [Consumes("multipart/form-data")] // Se reciben files
        [ProducesResponseType(typeof(ApiResponse<CompanyResponseDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromForm] CreateCompanyCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                company => CreatedAtRoute(
                    nameof(GetByIdAsync),
                    new { id = company.Data.Id },
                    company
                ),
                errors => Problem(errors)
            );
        }

        /// <summary>Fully updates an existing company by its unique identifier.</summary>
        /// <param name="id">The unique identifier of the company to update.</param>
        /// <param name="command">The complete set of updated company information.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpPut("{id:guid}")] // constrain
        [Consumes("multipart/form-data")] // Se reciben files
        [ProducesResponseType(StatusCodes.Status204NoContent)] // sin tipo
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateCompanyCommand command, CancellationToken cancellationToken)
        {
            if (command.Id != id)
            {
                List<Error> errors = [Error.Validation("Company.UpdateInvalid", "The request Id does not match with the url Id.")];
                return Problem(errors);
            }

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                _ => NoContent(),
                errors => Problem(errors)
            );
        }

        /// <summary>Partially updates a company.</summary>
        /// <param name="id">The ID of the company to update.</param>
        /// <param name="command">The partial data to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpPatch("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<CompanyResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PatchAsync(Guid id, [FromForm] PatchCompanyCommand command, CancellationToken cancellationToken)
        {
            if (command.Id != id)
            {
                var errors = new List<Error> { Error.Validation("Company.PatchInvalid", "The provided ID does not match the route parameter.") };
                return Problem(errors);
            }

            var result = await _mediator.Send(command, cancellationToken);

            return result.Match(
                company => Ok(company),
                errors => Problem(errors)
            );
        }

        /// <summary>Delete an existing company by its identifier.</summary>
        /// <param name="id">Unique identifier of the company to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] // sin tipo
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

            var command = new DeleteCompanyCommand(id, userId.Value);

            var deleteResult = await _mediator.Send(command, cancellationToken);

            return deleteResult.Match(
                _ => NoContent(),
                errors => Problem(errors)
            );
        }
    }
}
