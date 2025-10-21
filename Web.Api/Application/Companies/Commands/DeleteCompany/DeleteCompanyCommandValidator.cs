using FluentValidation;

namespace Web.Api.Application.Companies.Commands.DeleteCompany
{
    public class DeleteCompanyCommandValidator : AbstractValidator<DeleteCompanyCommand>
    {
        public DeleteCompanyCommandValidator()
        {
            RuleFor(r => r.Id)
                .NotEmpty();
        }
    }
}
