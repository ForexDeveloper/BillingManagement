#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Domain.Core.Entities;
using Domain.Core.Entities.BankAccountAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.UnitOfWorkContracts;
using MediatR;

namespace Application.Command.CustomerCommands;

public class DeleteCustomerBankAccountCommand : IRequest
{
    public DeleteCustomerBankAccountCommand(int bankAccountId, int customerId, int? tenantId)
    {
        BankAccountId = bankAccountId;
        CustomerId = customerId;
        TenantId = tenantId;
    }

    public int BankAccountId { get; set; }
    public int CustomerId { get; set; }
    public int? TenantId { get; set; }
}

public class DeleteCustomerBankAccountCommandHandler : IRequestHandler<DeleteCustomerBankAccountCommand>
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public DeleteCustomerBankAccountCommandHandler(IBankAccountRepository bankAccountRepository, IApplicationDbContextUnitOfWork unitOfWork)
    {
        _bankAccountRepository = bankAccountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCustomerBankAccountCommand request, CancellationToken cancellationToken)
    {
        var bankAccount = await GetBankAccount(request);

        bankAccount.Delete();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<BankAccount> GetBankAccount(DeleteCustomerBankAccountCommand request)
    {
        var bankAccount =
            await _bankAccountRepository.GetByIdAsync(request.BankAccountId);

        if (bankAccount == null)
            throw new ArgumentValidationException(nameof(request.BankAccountId), "شناسه بانکی نامعتبر می باشد");

        if (request.TenantId.HasValue && request.TenantId != bankAccount.TenantId)
            throw new TenantForbiddenException();

        if (bankAccount.BusinessIdentityId != request.CustomerId)
            throw new ArgumentValidationException(nameof(request.CustomerId), "شناسه مشتری نامعتبر می باشد");
        return bankAccount;
    }
}