using FluentValidation;

namespace Shopizy.Application.Users.Commands.UpdateUserRole;

public class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ModifiedById).NotEmpty();
        RuleFor(x => x.Role).NotEmpty();
    }
}
