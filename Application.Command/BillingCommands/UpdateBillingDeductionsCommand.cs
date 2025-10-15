using System;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Domain.Core.UnitOfWorkContracts;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.MerchantBillingAggregate;

namespace Application.Command.BillingCommands;

public sealed record UpdateBillingDeductionsCommand(int TenantId, long Id, decimal Amount, string Description) : IRequest<long>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;

    public decimal Amount { get; set; } = Amount;

    public string Description { get; set; } = Description;

}

public sealed class UpdateBillingDeductionsCommandHandler(
    IMerchantBillingRepository repository,
    IApplicationDbContextUnitOfWork unitOfWork) : IRequestHandler<UpdateBillingDeductionsCommand, long>
{
    public async Task<long> Handle(UpdateBillingDeductionsCommand command, CancellationToken cancellationToken)
    {
        var billing = await repository.GetAsync(command.Id);

        if (billing == null)
        {
            throw new NullReferenceException("صورتحساب یافت نشد");
        }

        if (billing.TenantId != command.TenantId)
        {
            throw new ArgumentValidationException(nameof(command.TenantId), "شناسه زیر ساخت معتبر نمی باشد");
        }

        billing.SetDeductions(command.Amount, command.Description);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return billing.Id;
    }
}