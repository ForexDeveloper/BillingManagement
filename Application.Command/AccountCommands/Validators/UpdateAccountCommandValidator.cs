using FluentValidation;

namespace Application.Command.AccountCommands.Validators;

public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
    }
}
