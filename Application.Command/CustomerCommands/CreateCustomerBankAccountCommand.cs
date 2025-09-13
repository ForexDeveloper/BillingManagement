#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Domain.Core.Entities;
using Domain.Core.Entities.BankAccountAggregate;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.UnitOfWorkContracts;
using MediatR;

namespace Application.Command.CustomerCommands;

public class CreateCustomerBankAccountCommand : IRequest<int>
{
    public CreateCustomerBankAccountCommand(int customerId, string iban, string shamsiBirthDate, int tenantId)
    {
        CustomerId = customerId;
        Iban = iban;
        ShamsiBirthDate = shamsiBirthDate;
        TenantId = tenantId;
    }

    public int CustomerId { get; set; }
    public string Iban { get; set; }
    public string ShamsiBirthDate { get; set; }
    public int TenantId { get; set; }
}

public class CreateCustomerBankAccountCommandHandler : IRequestHandler<CreateCustomerBankAccountCommand, int>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;

    public CreateCustomerBankAccountCommandHandler(ICustomerRepository customerRepository,
        IBankAccountRepository bankAccountRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _bankAccountRepository = bankAccountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateCustomerBankAccountCommand request, CancellationToken cancellationToken)
    {
        var customer = await GetCustomer(request.CustomerId, request.TenantId);

        await IsDuplicateIban(customer.Id, request.Iban);

        var bankAccount = await CreateBankAccount(request.Iban, customer.Id, customer.TenantId, cancellationToken);

        return bankAccount.Id;
    }

    private async Task<BankAccount> CreateBankAccount(string iban, int customerId, int tenantId, CancellationToken cancellationToken)
    {
        var bankAccount = new BankAccount(iban, tenantId, customerId);

        await _bankAccountRepository.AddAsync(bankAccount);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return bankAccount;
    }

    private async Task IsDuplicateIban(int customerId, string iban)
    {
        var hasCustomerIban = await _bankAccountRepository.BusinessIdentityHasIban(customerId, iban);

        if (hasCustomerIban)
            throw new ArgumentValidationException(nameof(iban), "شماره شبا تکراری می باشد");
    }

    private async Task<Customer> GetCustomer(int customerId, int? tenantId)
    {
        var customer = await _customerRepository.GetAsync(customerId);

        if (customer is null)
            throw new ArgumentValidationException(nameof(customerId), "شناسه نامعتبر است.");

        if (tenantId.HasValue && tenantId != customer.TenantId)
            throw new TenantForbiddenException();

        return customer;
    }
}
