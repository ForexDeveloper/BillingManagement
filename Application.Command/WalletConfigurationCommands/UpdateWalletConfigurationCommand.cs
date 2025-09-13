using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Entities.CurrencyTypeAggregate.Exceptions;
using Domain.Core.Entities.CurrencyTypeAggregate;
using Domain.Core.Entities.ProjectManegerAggregate;
using Domain.Core.Entities.ProjectManegerAggregate.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.WalletConfigurationAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Core.Entities;
using Domain.Core.Entities.PlanAggregate;
using Minio.DataModel;
using Domain.Core.Entities.Shared.Exceptions;

namespace Application.Command.WalletConfigurationCommands
{
    public class UpdateWalletConfigurationCommand : IRequest<int>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public WalletType WalletTypeId { get; set; }
        public decimal MaxWallet { get; set; }
        public decimal? MaximumTotalCredit { get; set; }
        public int? TenantId { get; set; }
        public int? ProjectManagerId { get; set; }
        public int? CurrencyTypeId { get; set; }
        public UpdateInstallmentsDto Installments { get; set; }
        public UpdateFinancialCommitmentDto Financial { get; set; }

        public UpdateWalletConfigurationCommand(int id, string title,
               WalletType walletTypeId,
               decimal maxWallet, decimal? maximumTotalCredit, int? projectManagerId,
               int? currencyTypeId, UpdateInstallmentsDto installments,
               UpdateFinancialCommitmentDto financial, int? tenantId = null
            )
        {
            Id = id;
            Title = title;
            WalletTypeId = walletTypeId;
            MaxWallet = maxWallet;
            MaximumTotalCredit = maximumTotalCredit;
            TenantId = tenantId;
            ProjectManagerId = projectManagerId;
            CurrencyTypeId = currencyTypeId;
            Installments = installments;
            Financial = financial;
        }

        public class UpdateInstallmentsDto
        {
            public List<int> MaxInstallments { get; set; }
            public decimal? PrepaymentMaxPercent { get; set; }
            public decimal? PrepaymentMinPercent { get; set; }
            public decimal? PrepaymentMaxAmount { get; set; }
            public decimal? PrepaymentMinAmount { get; set; }
        }
        public class UpdateFinancialCommitmentDto
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
    }

    public class UpdateWalletConfigurationCommandHandler : IRequestHandler<UpdateWalletConfigurationCommand, int>
    {
        private readonly IWalletConfigurationRepository _walletConfigurationRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly ITenantRepository _tenantRepository;
        private readonly IProjectManagerRepository _projectManagerRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IPlanRepository _planRepository;

        public UpdateWalletConfigurationCommandHandler(IWalletConfigurationRepository walletConfigurationRepository, ITenantRepository tenantRepository,
            IApplicationDbContextUnitOfWork unitOfWork, IProjectManagerRepository projectManagerRepository, ICurrencyRepository currencyRepository, IPlanRepository planRepository)
        {
            _walletConfigurationRepository = walletConfigurationRepository;
            _unitOfWork = unitOfWork;
            _tenantRepository = tenantRepository;
            _projectManagerRepository = projectManagerRepository;
            _currencyRepository = currencyRepository;
            _planRepository = planRepository;
        }

        public async Task<int> Handle(UpdateWalletConfigurationCommand request, CancellationToken cancellationToken)
        {
            var wealletConfiguration = await _walletConfigurationRepository.GetByIdAsync(request.Id);
            if (wealletConfiguration is null)
                throw new WalletConfigurationNotFoundException("کانفیگ کیف پول پول نشد.");

            if (request.TenantId.HasValue && request.TenantId != wealletConfiguration.TenantId)
            {
                throw new TenantForbiddenException();
            }

            var hasPlan = await _walletConfigurationRepository.HasPlanAsync(request.Id);

            if (request.ProjectManagerId != null)
            {
                var hasProjectMg = await _projectManagerRepository.CheckProjectManagers(request.ProjectManagerId.Value);
                if (!hasProjectMg)
                    throw new ProjectManagerNotFoundException("مدیر زیرساخت پیدا نشد.");
            }

            if (hasPlan)
            {
                if (wealletConfiguration.MaxWallet > request.MaxWallet)
                {
                    var hasMaxWallet = await _planRepository.CheckPlanMaxWalletAsync(wealletConfiguration.Id, request.MaxWallet);
                    if (hasMaxWallet)
                        throw new ArgumentValidationException(nameof(request.MaxWallet), "سقف کیف پول استفاده شده در طرح بیشتر  هست.");
                }

                if (wealletConfiguration.MaxTotalCredit > request.MaximumTotalCredit)
                {
                    var hasMaxWallet = await _planRepository.CheckPlanMaxTotalCreditAsync(wealletConfiguration.Id, request.MaximumTotalCredit.GetValueOrDefault());
                    if (hasMaxWallet)
                        throw new ArgumentValidationException(nameof(request.MaximumTotalCredit), "سقف کیف پول استفاده شده در طرح بیشتر  هست.");
                }
                wealletConfiguration.SetWalletConfiguration(request.Title, wealletConfiguration.TenantId,
                wealletConfiguration.WalletTypeId, request.ProjectManagerId, wealletConfiguration.CurrencyTypeId,
                request.MaxWallet, request.MaximumTotalCredit);
            }
            else
            {

                if (request.CurrencyTypeId != null)
                {
                    var hasCurrency = await _currencyRepository.CheckCurrency(request.CurrencyTypeId.Value);
                    if (!hasCurrency)
                        throw new CurrencyNotFoundException("واحد ارز یافت نشد.");
                }
                wealletConfiguration.SetWalletConfiguration(request.Title, wealletConfiguration.TenantId,
               request.WalletTypeId, request.ProjectManagerId, request.CurrencyTypeId,
               request.MaxWallet, request.MaximumTotalCredit);

                wealletConfiguration.UpdateInstallmentsInfo(request.Installments.MaxInstallments,
                request.Installments.PrepaymentMinPercent,
               request.Installments.PrepaymentMaxPercent,
                request.Installments.PrepaymentMaxAmount,
                request.Installments.PrepaymentMinAmount);

                if (request.Financial is not null)
                {
                    wealletConfiguration.UpdateFinancialInfo(request.Financial.PenaltyPeriodMaxPercent,
                                   request.Financial.InterestPeriodMinPercent, request.Financial.InterestPeriodMaxAmount,
                                   request.Financial.InterestPeriodMinAmount, request.Financial.PenaltyPeriodMaxPercent,
                                   request.Financial.PenaltyPeriodMinPercent, request.Financial.PenaltyPeriodMaxAmount,
                                   request.Financial.PenaltyPeriodMinAmount, request.Financial.WaiverPeriodMaxPercent,
                                   request.Financial.WaiverPeriodMinPercent, request.Financial.WaiverPeriodMaxAmount,
                                   request.Financial.WaiverPeriodMinAmount);
                }
            }

            _walletConfigurationRepository.Update(wealletConfiguration);
            await _unitOfWork.SaveChangesAsync();
            return wealletConfiguration.Id;
        }

    }
}