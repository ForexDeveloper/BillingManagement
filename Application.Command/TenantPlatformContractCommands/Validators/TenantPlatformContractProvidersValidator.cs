using Application.Service.Dtos.TenantPlatformContract;
using FluentValidation;

namespace Application.Command.TenantPlatformContractCommands.Validators;

public class TenantPlatformContractProvidersValidator : AbstractValidator<TenantPlatformContractProviderDto>
{
    public TenantPlatformContractProvidersValidator()
    {
        RuleFor(c => c.ProviderId).
            GreaterThan(0).WithMessage("شناسه سرویس معتبر نمی باشد.");

        RuleFor(c => c.Amount).
            GreaterThan(0).WithMessage("مبلغ قسط اجباریست.");
    }
}
