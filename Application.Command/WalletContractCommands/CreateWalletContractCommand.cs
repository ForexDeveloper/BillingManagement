using Application.Service.Contracts;
using Application.Service.Dtos.WalletContract;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using Shared.IdentityServerProvider.Contracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.WalletContractCommands
{
    public class CreateWalletContractCommand : IRequest<int>
    {
        public int TenantId { get; set; }
        public List<int> PlanIds { get; set; }
        public int OrganizationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? TenantIpgSettingId { get; set; }
        public bool AssignWalletToOrganizationCustomers { get; set; }
        public List<int>? Customers { get; set; }
        public WalletContractGuarantorDto Guarantor { get; set; }
        public WalletContractFinancierDto? Financier { get; set; }
        public List<WalletContractFacilitatorDto>? Facilitators { get; set; }

        public CreateWalletContractCommand(int tenantId, List<int> planIds, int organizationId, DateTime startDate, DateTime endDate, int? tenantIpgSettingId,
            bool assignWalletToOrganizationCustomers,
            WalletContractGuarantorDto guarantor, List<int>? customers, WalletContractFinancierDto? financier,
            List<WalletContractFacilitatorDto>? facilitators)
        {
            TenantId = tenantId;
            PlanIds = planIds;
            OrganizationId = organizationId;
            StartDate = startDate;
            EndDate = endDate;
            TenantIpgSettingId = tenantIpgSettingId;
            Customers = customers;
            Guarantor = guarantor;
            Financier = financier;
            Facilitators = facilitators;
            AssignWalletToOrganizationCustomers = assignWalletToOrganizationCustomers;
        }
    }

    public class CreateWalletContractCommandHandler : IRequestHandler<CreateWalletContractCommand, int>
    {
        private readonly IWalletContractRepository _walletContractRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IWalletContractService _walletContractService;

        public CreateWalletContractCommandHandler(
            IApplicationDbContextUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IWalletContractRepository walletContractRepository, ICustomerRepository customerRepository,
            IWalletContractService walletContractService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _walletContractRepository = walletContractRepository;
            _customerRepository = customerRepository;
            _walletContractService = walletContractService;
        }

        public async Task<int> Handle(CreateWalletContractCommand request, CancellationToken cancellationToken)
        {
            await _walletContractService.ValidateInputData(request.TenantId, request.OrganizationId, request.PlanIds, request.Guarantor, request.Financier, request.Facilitators, request.Customers, request.TenantIpgSettingId);

            var existsContractByorganizationIdAndPlanIdsAsync = await _walletContractRepository.ExistsContractByorganizationIdAndPlanIdsAsync(request.TenantId, request.OrganizationId, request.PlanIds);
            if (existsContractByorganizationIdAndPlanIdsAsync)
                throw new ArgumentValidationException(nameof(request.PlanIds), "یک قرارداد برای سازمان و طرح های انتخابی وجود دارد.");

            var contract = new WalletContract(
                request.TenantId,
                request.TenantIpgSettingId,
                request.OrganizationId,
                request.StartDate,
                request.EndDate
            );
            contract.SetCreatorUserId(_currentUserService.UserId);
            contract.SetClientId(_currentUserService.ClientId);

            _walletContractService.SetWalletContractPlans(contract, request.PlanIds);

            if (request.AssignWalletToOrganizationCustomers)
            {
                var customerOrganization = await _customerRepository.GetCustomersIdByOrganizationId(request.TenantId, request.OrganizationId);
                _walletContractService.SetWalletContractCustomers(contract, customerOrganization);
            }
            _walletContractService.SetWalletContractGuarantor(contract, request.Guarantor);
            _walletContractService.SetWalletContractFinancier(contract, request.Financier);
            _walletContractService.SetWalletContractFacilitators(contract, request.Facilitators);

            await _walletContractRepository.AddAsync(contract);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return contract.Id;
        }
    }
}
