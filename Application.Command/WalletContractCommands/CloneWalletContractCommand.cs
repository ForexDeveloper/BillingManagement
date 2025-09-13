using Application.Query.ReadOnlyRepositoryContracts;
using Application.Service.Contracts;
using Application.Service.Dtos.WalletContract;
using Domain.Core.Entities.CustomerAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Entities.WalletContractAggregate.Exceptions;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using Shared.IdentityServerProvider.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.WalletContractCommands
{
    public class CloneWalletContractCommand : IRequest<int>
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? TenantIpgSettingId { get; set; }
        public bool AssignWalletToOrganizationCustomers { get; set; }
        public List<int>? Customers { get; set; }
        public WalletContractGuarantorDto Guarantor { get; set; }
        public WalletContractFinancierDto? Financier { get; set; }
        public List<WalletContractFacilitatorDto>? Facilitators { get; set; }

        public CloneWalletContractCommand(int id, int tenantId, DateTime startDate, DateTime endDate, int? tenantIpgSettingId,
            bool assignWalletToOrganizationCustomers,
            WalletContractGuarantorDto guarantor, List<int>? customers, WalletContractFinancierDto? financier,
            List<WalletContractFacilitatorDto>? facilitators)
        {
            Id = id;
            TenantId = tenantId;
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

    public class CloneWalletContractCommandHandler : IRequestHandler<CloneWalletContractCommand, int>
    {
        private readonly IWalletContractRepository _walletContractRepository;
        private readonly IWalletContractReadOnlyRepository _walletContractReadOnlyRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICustomerRepository _customerRepository;
        private readonly IWalletContractService _walletContractService;

        public CloneWalletContractCommandHandler(
            IApplicationDbContextUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IWalletContractRepository walletContractRepository, ICustomerRepository customerRepository,
            IWalletContractService walletContractService,
            IWalletContractReadOnlyRepository walletContractReadOnlyRepository)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _walletContractRepository = walletContractRepository;
            _customerRepository = customerRepository;
            _walletContractService = walletContractService;
            _walletContractReadOnlyRepository = walletContractReadOnlyRepository;
        }

        public async Task<int> Handle(CloneWalletContractCommand request, CancellationToken cancellationToken)
        {
            var currentContract = await _walletContractReadOnlyRepository.GetForCloneByContractIdAsync(request.Id, request.TenantId)
                           ?? throw new WalletContractNotFoundException("قرارداد پیدا نشد.");

            if (currentContract.Status == Domain.Core.Enums.WalletContractStatus.Reviewing)
            {
                throw new ArgumentValidationException(nameof(request.Id), "وضعیت قرارداد در حال بررسی می باشد امکان ایجاد الحاقیه وجود ندارد.");
            }
            else if (currentContract.Status == Domain.Core.Enums.WalletContractStatus.Reject)
            {
                throw new ArgumentValidationException(nameof(request.Id), "وضعیت قرارداد رد شده می باشد امکان ایجاد الحاقیه وجود ندارد.");
            }

            var contracts = await _walletContractReadOnlyRepository.GetNotRejectedEndorsementsListAsync(currentContract.Id, currentContract.RootParentId, currentContract.TenantId);
            if (contracts != null && contracts.Count > 0)
            {
                var sortedContracts = contracts.OrderBy(x => x.Id).ToList();
                if (contracts.Any(x => x.Status == Domain.Core.Enums.WalletContractStatus.Reviewing))
                {
                    throw new ArgumentValidationException(nameof(request.Id), "یک الحاقیه با وضعیت در حال بررسی وجود دارد امکان ایجاد الحاقیه وجود ندارد.");
                }
                else if (contracts.Any(x => x.Status != Domain.Core.Enums.WalletContractStatus.Reviewing && x.Id > currentContract.Id))
                {
                    throw new ArgumentValidationException(nameof(request.Id), "ایجاد الحاقیه فقط بر روی آخرین نسخه تایید شده امکان پذیر است.");
                }
            }

            await _walletContractService.ValidateInputData(request.TenantId, currentContract.OrganizationId, currentContract.PlanIds, request.Guarantor, request.Financier, request.Facilitators, request.Customers, request.TenantIpgSettingId);

            var endorsement = new WalletContract(
               request.TenantId,
               request.TenantIpgSettingId,
               currentContract.OrganizationId,
               request.StartDate,
               request.EndDate
           );
            endorsement.SetParentId(currentContract.Id);
            endorsement.SetRootParentId(currentContract.RootParentId ?? currentContract.Id);
            endorsement.SetCreatorUserId(_currentUserService.UserId);
            endorsement.SetClientId(_currentUserService.ClientId);

            _walletContractService.SetWalletContractPlans(endorsement, currentContract.PlanIds);

            if (request.AssignWalletToOrganizationCustomers)
            {
                var customerOrganization = await _customerRepository.GetCustomersIdByOrganizationId(request.TenantId, currentContract.OrganizationId);
                _walletContractService.SetWalletContractCustomers(endorsement, customerOrganization);
            }
            _walletContractService.SetWalletContractGuarantor(endorsement, request.Guarantor);
            _walletContractService.SetWalletContractFinancier(endorsement, request.Financier);
            _walletContractService.SetWalletContractFacilitators(endorsement, request.Facilitators);

            await _walletContractRepository.AddAsync(endorsement);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return endorsement.Id;
        }
    }
}
