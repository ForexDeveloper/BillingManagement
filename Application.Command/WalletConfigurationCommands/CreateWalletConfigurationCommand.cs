using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Entities.CurrencyTypeAggregate;
using Domain.Core.Entities.CurrencyTypeAggregate.Exceptions;
using Domain.Core.Entities.ProjectManegerAggregate;
using Domain.Core.Entities.ProjectManegerAggregate.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.Helper;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using Shared.IdentityServerProvider.Contracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Application.Command.WalletConfigurationCommands
{
    public class CreateWalletConfigurationCommand : IRequest<int>
    {
        #region Wallet
        public string Title { get; set; }
        public WalletType WalletTypeId { get; set; }
        public decimal MaxWallet { get; set; }
        public decimal? MaximumTotalCredit { get; set; }
        public int TenantId { get; set; }
        public int? ProjectManagerId { get; set; }
        public int? CurrencyTypeId { get; set; }

        #endregion #region Property

        public CreateInstallmentsDto Installments { get; set; }
        public CreateFinancialCommitmentDto Financial { get; set; }

        public CreateWalletConfigurationCommand(string title,
            WalletType walletTypeId,
            decimal maxWallet, decimal? maximumTotalCredit, int tenantId, int? projectManagerId,
            int? currencyTypeId, CreateInstallmentsDto installments,
            CreateFinancialCommitmentDto financial
            )
        {
            Title = title;
            WalletTypeId = walletTypeId;
            MaxWallet = maxWallet;
            TenantId = tenantId;
            ProjectManagerId = projectManagerId;
            CurrencyTypeId = currencyTypeId;
            Installments = installments;
            Financial = financial;
            MaximumTotalCredit = maximumTotalCredit;
        }
    }
    public class CreateInstallmentsDto
    {
        public List<int> MaxInstallments { get; set; }
        public decimal? PrepaymentMaxPercent { get; set; }
        public decimal? PrepaymentMinPercent { get; set; }
        public decimal? PrepaymentMaxAmount { get; set; }
        public decimal? PrepaymentMinAmount { get; set; }

    }
    public class CreateFinancialCommitmentDto
    {
        public decimal? InterestPeriodMaxPercent { get; set; }
        public decimal? InterestPeriodMinPercent { get; set; }
        public decimal? InterestPeriodMaxAmount { get; set; }
        public decimal? InterestPeriodMinAmount { get; set; }
        public decimal? PenaltyPeriodMaxPercent { get; set; }
        public decimal? PenaltyPeriodMinPercent { get; set; }
        public decimal? PenaltyPeriodMaxAmount { get; set; }
        public decimal? PenaltyPeriodMinAmount { get; set; }
        public decimal? WaiverPeriodMaxPercent { get; set; }
        public decimal? WaiverPeriodMinPercent { get; set; }
        public decimal? WaiverPeriodMaxAmount { get; set; }
        public decimal? WaiverPeriodMinAmount { get; set; }
    }

    public class CreateWalletConfigurationCommandHandler : IRequestHandler<CreateWalletConfigurationCommand, int>
    {

        private readonly ITenantRepository _tenantRepository;
        private readonly IProjectManagerRepository _projectManagerRepository;
        private readonly IWalletConfigurationRepository _walletConfigurationRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICurrencyRepository _currencyRepository;

        public CreateWalletConfigurationCommandHandler(ICurrentUserService currentUserService, ITenantRepository tenantRepository,
          IProjectManagerRepository projectManagerRepository, IWalletConfigurationRepository walletConfigurationRepository, IApplicationDbContextUnitOfWork unitOfWork, ICurrencyRepository currencyRepository)
        {
            _tenantRepository = tenantRepository;
            _walletConfigurationRepository = walletConfigurationRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _projectManagerRepository = projectManagerRepository;
            _currencyRepository = currencyRepository;
        }

        public async Task<int> Handle(CreateWalletConfigurationCommand request, CancellationToken cancellationToken)
        {
            if (request.ProjectManagerId != null)
            {
                var hasProjectMg = await _projectManagerRepository.CheckProjectManagers(request.ProjectManagerId.Value);
                if (!hasProjectMg)
                    throw new ProjectManagerNotFoundException("مدیر زیرساخت یافت نشد.");

            }
            var hasTenant = await _tenantRepository.CheckTenant(request.TenantId);
            if (!hasTenant)
            {
                throw new TenantNotFoundException("مالک زیر ساخت یافت نشد.");
            }

            if (request.CurrencyTypeId != null)
            {
                var hasCurrency = await _currencyRepository.CheckCurrency(request.CurrencyTypeId.Value);
                if (!hasCurrency)
                    throw new CurrencyNotFoundException("واحد ارز یافت نشد.");

            }

            var wealletConfiguration = new WalletConfiguration(request.Title, request.TenantId,
                request.WalletTypeId, request.ProjectManagerId, request.CurrencyTypeId,
                request.MaxWallet, request.MaximumTotalCredit);

            wealletConfiguration.SetInstallmentsInfo(request.Installments. MaxInstallments,
            request.Installments.PrepaymentMinPercent,
           request.Installments.PrepaymentMaxPercent,
            request.Installments.PrepaymentMaxAmount,
            request.Installments.PrepaymentMinAmount);

            if (request.Financial is not null)
            {
                wealletConfiguration.SetFinancialInfo(request.Financial.InterestPeriodMaxPercent,
           request.Financial.InterestPeriodMinPercent, request.Financial.InterestPeriodMaxAmount,
            request.Financial.InterestPeriodMinAmount,
            request.Financial.PenaltyPeriodMaxPercent,
            request.Financial.PenaltyPeriodMinPercent,
            request.Financial.PenaltyPeriodMaxAmount,
            request.Financial.PenaltyPeriodMinAmount,
            request.Financial.WaiverPeriodMaxPercent,
            request.Financial.WaiverPeriodMinPercent,
            request.Financial.WaiverPeriodMaxAmount,
            request.Financial.WaiverPeriodMinAmount);
            }
            wealletConfiguration.SetCreatorUserId(_currentUserService.UserId);
            wealletConfiguration.SetClientId(_currentUserService.ClientId);
            await _walletConfigurationRepository.AddAsync(wealletConfiguration);
            await _unitOfWork.SaveChangesAsync();
            return wealletConfiguration.Id;
        }

    }
}