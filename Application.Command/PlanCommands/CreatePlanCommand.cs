using Application.Service.Contracts;
using Application.Service.Dtos.Attachments;
using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Entities.ClosedloopAggregate;
using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using Shared.EventBus.Contracts;
using Shared.EventBus.Events;
using Shared.IdentityServerProvider.Contracts;
using Shared.MinIO.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Command.PlanCommands
{
    public class CreatePlanDetailDto
    {
        public List<int> NumberOfInstallments { get; set; }
        public decimal? OperationFee { get; set; }
        public OperationalFeeType? OperationalFeeType { get; set; }
        public decimal? PenaltyPercent { get; set; }
        public decimal? PenaltyMaxAmount { get; set; }
        public decimal? PenaltyMinAmount { get; set; }
        public decimal? InterestPercent { get; set; }
        public decimal? InterestMaxAmount { get; set; }
        public decimal? InterestMinAmount { get; set; }
        public decimal? WaiverPercent { get; set; }
        public decimal? WaiverMaxAmount { get; set; }
        public decimal? WaiverMinAmount { get; set; }
        public decimal? PrepaymentPercent { get; set; }
        public decimal? PrepaymentMinAmount { get; set; }
        public decimal? PrepaymentMaxAmount { get; set; }
        public string TermsAndConditions { get; set; }
    }
    public class CreatePlanCommand : IRequest<int>
    {
        public CreatePlanCommand(int walletConfigurationId, string walletLogo,
            string title, decimal? maxDailyWithdrawal, decimal maxWallet, decimal maxTotalCredit,
            TimeInterval billingPeriodType, int billingPeriod, DateTime? billingPeriodStartDate,
            int? gracePeriod, TimeInterval? paymentType, TimeInterval? installmentBreakType, int? installmentBreak,
            InstallmentPaymentMethodType? installmentPaymentMethod, decimal? maxDailyDeposit,
            decimal? maxDailyTransactionCount, string backgroundColor1, string backgroundColor2,
            string textColor, string description, string link, List<int> planClosedloops,
            List<CreatePlanDetailDto> planDetails, string termsAndConditions, int? tenantId = null)
        {
            TenantId = tenantId;
            WalletConfigurationId = walletConfigurationId;
            WalletLogo = walletLogo;
            Title = title;
            MaxDailyWithdrawal = maxDailyWithdrawal;
            MaxWallet = maxWallet;
            MaxTotalCredit = maxTotalCredit;
            BillingPeriodType = billingPeriodType;
            BillingPeriodStartDate = billingPeriodStartDate;
            GracePeriod = gracePeriod;
            PaymentType = paymentType;
            InstallmentBreakType = installmentBreakType;
            InstallmentPaymentMethod = installmentPaymentMethod;
            MaxDailyDeposit = maxDailyDeposit;
            MaxDailyTransactionCount = maxDailyTransactionCount;
            BackgroundColor1 = backgroundColor1;
            BackgroundColor2 = backgroundColor2;
            TextColor = textColor;
            Description = description;
            Link = link;
            PlanClosedloops = planClosedloops;
            PlanDetails = planDetails;
            BillingPeriod = billingPeriod;
            InstallmentBreak = installmentBreak;
            TermsAndConditions = termsAndConditions;
        }

        public int? TenantId { get; set; }
        public int WalletConfigurationId { get; set; }
        public string WalletLogo { get; set; }
        public string Title { get; private set; }
        public decimal? MaxDailyWithdrawal { get; private set; }
        public decimal MaxWallet { get; private set; }
        public decimal MaxTotalCredit { get; private set; }
        public TimeInterval BillingPeriodType { get; set; }
        public int BillingPeriod { get; set; }
        public DateTime? BillingPeriodStartDate { get; set; }
        public int? GracePeriod { get; set; }
        public TimeInterval? PaymentType { get; set; }
        public TimeInterval? InstallmentBreakType { get; set; }
        public int? InstallmentBreak { get; set; }
        public InstallmentPaymentMethodType? InstallmentPaymentMethod { get; set; }
        public decimal? MaxDailyDeposit { get; set; }
        public decimal? MaxDailyTransactionCount { get; set; }
        public string BackgroundColor1 { get; set; }
        public string BackgroundColor2 { get; set; }
        public string TextColor { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public List<int> PlanClosedloops { get; set; }
        public List<CreatePlanDetailDto> PlanDetails { get; set; }
        public string TermsAndConditions { get; set; }

        public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, int>
        {
            private readonly IWalletConfigurationRepository _walletConfigurationRepository;
            private readonly IClosedloopRepository _closedloopRepository;
            private readonly ITenantRepository _tenantRepository;
            private readonly IPlanRepository _planRepository;
            private readonly IApplicationDbContextUnitOfWork _unitOfWork;
            private readonly ICurrentUserService _currentUserService;
            private readonly IFileManagerService _fileManagerService;
            private readonly IAttachmentService _attachmentService;
            private readonly IOutboxService _outboxService;
            public CreatePlanCommandHandler(IClosedloopRepository closedloopRepository,
                ICurrentUserService currentUserService, ITenantRepository tenantRepository,
                IWalletConfigurationRepository walletConfigurationRepository,
               IPlanRepository planRepository, IApplicationDbContextUnitOfWork unitOfWork,
               IFileManagerService fileManagerService, IAttachmentService attachmentService,
               IOutboxService outboxService)
            {
                _tenantRepository = tenantRepository;
                _planRepository = planRepository;
                _unitOfWork = unitOfWork;
                _currentUserService = currentUserService;
                _closedloopRepository = closedloopRepository;
                _walletConfigurationRepository = walletConfigurationRepository;
                _fileManagerService = fileManagerService;
                _attachmentService = attachmentService;
                _outboxService = outboxService;
            }

            public async Task<int> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
            {
                var type = await CheckValidation(request);
                var plan = CreatePlan(request, type);
                using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        await _planRepository.AddAsync(plan);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        if (!string.IsNullOrEmpty(request.WalletLogo))
                        {
                            await _attachmentService.CreateAsync(new AttachmentDto
                            {
                                AttachmentCategory = AttachmentCategory.PlanLogo,
                                EntityId = plan.Id,
                                EntityType = EntityType.Plan,
                                FileReference = request.WalletLogo
                            });
                        }

                        PublishPlanAddedEvent(plan);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                    catch (Exception)
                    {
                        transaction.Dispose();
                        throw;
                    }

                    transaction.Complete();
                }
                return plan.Id;
            }

            private void PublishPlanAddedEvent(Plan plan)
            {
                _outboxService.AddNewEvent(new FcmPlanAddedOrUpdatedEvent()
                {
                    Id = plan.Id,
                    TenantId = plan.WalletConfiguration.TenantId,
                    Title = plan.Title,
                    IsActive = plan.IsActive,
                    MaxWallet = plan.MaxWallet,
                    PlanDetails = plan.PlanDetails.Select(x => new PlanDetailEvent
                    {
                        Id = x.Id,
                        NumberOfInstallments = x.PlanDetailInstallments.Select(c => new PlanNumberOfInstallmentEvent
                        {
                            NumberOfInstallment = c.NumberOfInstallment,
                            Id = c.Id
                        }).ToList(),

                        OperationFee = x.OperationFee,
                        InterestPercent = x.InterestPercent,
                    }).ToList()
                });
            }

            private Plan CreatePlan(CreatePlanCommand request, WalletType type)
            {
                var plan = new Plan(request.WalletConfigurationId,
                    request.Title, request.MaxDailyWithdrawal, request.MaxWallet, request.MaxTotalCredit,
                    request.BillingPeriodType, request.BillingPeriod, request.BillingPeriodStartDate,
                    request.GracePeriod, request.PaymentType, request.InstallmentBreakType, request.InstallmentBreak,
                    request.InstallmentPaymentMethod,
                    request.MaxDailyDeposit, request.MaxDailyTransactionCount,
                    request.BackgroundColor1, request.BackgroundColor2, request.TextColor,
                    request.Description, request.Link, request.TermsAndConditions);

                if (type is not WalletType.Cash)
                {
                    foreach (var item in request.PlanDetails)
                    {
                        var planDetail = new PlanDetail(item.OperationFee, item.OperationalFeeType,
                         item.PenaltyPercent, item.PenaltyMaxAmount, item.PenaltyMinAmount,
                         item.InterestPercent, item.InterestMaxAmount, item.InterestMinAmount,
                         item.WaiverPercent, item.WaiverMaxAmount, item.WaiverMinAmount,
                         item.PrepaymentPercent, item.PrepaymentMinAmount, item.PrepaymentMaxAmount);

                        planDetail.SetPlanDetailInstallment(item.NumberOfInstallments
                            .Select(c => new PlanDetailInstallment(c)).ToList());

                        plan.SetPlanDetails(planDetail);
                    }
                }
                foreach (var item in request.PlanClosedloops)
                {
                    plan.SetPlanClosedloops(new PlanClosedloop(item));

                }
                return plan;
            }

            private async Task<WalletType> CheckValidation(CreatePlanCommand request)
            {
                var walletConfiguration = await _walletConfigurationRepository.GetByTenantIdAsync(request.WalletConfigurationId, request.TenantId);
                if (walletConfiguration == null)
                    throw new ArgumentValidationException(nameof(request.WalletConfigurationId), "شناسه تنظیمات کیف پول معتبر نیست");

                if (walletConfiguration.WalletTypeId is not WalletType.Cash && (request.PlanDetails == null || !request.PlanDetails.Any()))
                    throw new ArgumentValidationException(nameof(request.PlanDetails), "اطلاعات اجباری اقساط را وارد کنید.");

                if (request.PlanClosedloops != null && request.PlanClosedloops.Any())
                {
                    var hasClosedloop = await _closedloopRepository.CheckClosedloop(request.WalletConfigurationId, request.PlanClosedloops);
                    if (!hasClosedloop)
                        throw new ArgumentValidationException(nameof(request.PlanClosedloops), "شناسه close loop معتبر نیست.");
                }
                if (walletConfiguration.WalletTypeId is not WalletType.Cash)
                {
                    var totalInstallments = request.PlanDetails.SelectMany(c => c.NumberOfInstallments);
                    if (totalInstallments.Count() != totalInstallments.Distinct().Count())
                    {
                        throw new ArgumentValidationException(nameof(totalInstallments), "تعداد اقساط در هر طرح تکتا میباشد و براساس پیکربندی وارد میشود.  .");
                    }
                }

                if (walletConfiguration.WalletTypeId is not WalletType.Cash && walletConfiguration.MaxTotalCredit > 0
                     && request.MaxTotalCredit > walletConfiguration.MaxTotalCredit)
                    throw new ArgumentValidationException(nameof(request.MaxTotalCredit), "سقف تجمیع اعتبار کاربران بیشتر از سقف مجاز است.");

                if (request.MaxWallet > walletConfiguration.MaxWallet)
                    throw new ArgumentValidationException(nameof(request.MaxWallet), "سقف کیف پول بیشتر از سقف مجاز است.");

                if (request.MaxDailyWithdrawal > walletConfiguration.MaxWallet)
                    throw new ArgumentValidationException(nameof(request.MaxDailyWithdrawal), "سقف برداشت بیشتر از سقف مجاز است.");

                if (walletConfiguration.WalletTypeId is WalletType.BonCard or WalletType.Cash &&
                   request.MaxDailyDeposit > walletConfiguration.MaxWallet)
                    throw new ArgumentValidationException(nameof(request.MaxDailyDeposit), "سقف واریز بیشتر از سقف مجاز است.");

                if (walletConfiguration.WalletTypeId is WalletType.BonCard or WalletType.Cash &&
                    request.MaxDailyTransactionCount > walletConfiguration.MaxWallet)
                    throw new ArgumentValidationException(nameof(MaxDailyTransactionCount), "سقف تراکنش روزانه بیشتر از سقف مجاز است.");

                if (walletConfiguration.WalletTypeId is not WalletType.Cash && request.PlanDetails != null && request.PlanDetails.Any())
                {
                    foreach (var item in request.PlanDetails)
                    {
                        if (walletConfiguration.MaxInstallments.Any())
                        {
                            var installmentCount = walletConfiguration.MaxInstallments.Intersect(item.NumberOfInstallments).Count();
                            if (installmentCount != item.NumberOfInstallments.Count)
                            {
                                throw new ArgumentValidationException(nameof(item.NumberOfInstallments), "تعداد اقساط صحیح نیست.");
                            }
                        }

                        if (walletConfiguration.PenaltyPeriodMaxPercent > 0 && walletConfiguration.PenaltyPeriodMinPercent >= 0)
                        {
                            if (item.PenaltyPercent > walletConfiguration.PenaltyPeriodMaxPercent && item.PenaltyPercent < walletConfiguration.PenaltyPeriodMinPercent)
                                throw new ArgumentValidationException(nameof(item.PenaltyPercent), "درصد جریمه مجاز نیست.");

                            if (walletConfiguration.PenaltyPeriodMaxAmount > 0 && item.PenaltyMaxAmount > walletConfiguration.PenaltyPeriodMaxAmount)
                                throw new ArgumentValidationException(nameof(item.PenaltyMaxAmount), "حداکثر مبلغ  جریمه  مجاز نیست.");

                            if (walletConfiguration.PenaltyPeriodMinAmount > 0 && item.PenaltyMinAmount < walletConfiguration.PenaltyPeriodMinAmount)
                                throw new ArgumentValidationException(nameof(item.PenaltyMinAmount), "حداقل مبلغ  جریمه  مجاز نیست.");
                        }

                        if (walletConfiguration.InterestPeriodMaxPercent > 0 && walletConfiguration.InterestPeriodMinPercent >= 0)
                        {
                            if (item.InterestPercent > walletConfiguration.InterestPeriodMaxPercent && item.InterestPercent < walletConfiguration.InterestPeriodMinPercent)
                                throw new ArgumentValidationException(nameof(item.InterestPercent), "درصد  بهره  مجاز نیست.");

                            if (walletConfiguration.InterestPeriodMaxAmount > 0 && item.InterestMaxAmount > walletConfiguration.InterestPeriodMaxAmount)
                                throw new ArgumentValidationException(nameof(item.InterestMaxAmount), "حداکثر مبلغ  بهره  مجاز نیست.");

                            if (walletConfiguration.InterestPeriodMinAmount > 0 && item.InterestMinAmount < walletConfiguration.InterestPeriodMinAmount)
                                throw new ArgumentValidationException(nameof(item.InterestMinAmount), "حداقل مبلغ  بهره  مجاز نیست.");
                        }
                        if (walletConfiguration.WaiverPeriodMaxPercent > 0 && walletConfiguration.WaiverPeriodMinPercent >= 0)
                        {
                            if (item.WaiverPercent > walletConfiguration.WaiverPeriodMaxPercent && item.WaiverPercent < walletConfiguration.WaiverPeriodMinPercent)
                                throw new ArgumentValidationException(nameof(item.WaiverPercent), "درصد  پاداش  مجاز نیست.");

                            if (walletConfiguration.WaiverPeriodMaxAmount > 0 && item.WaiverMaxAmount > walletConfiguration.WaiverPeriodMaxAmount)
                                throw new ArgumentValidationException(nameof(item.WaiverMaxAmount), "حداکثر مبلغ  پاداش  مجاز نیست.");

                            if (walletConfiguration.WaiverPeriodMinAmount > 0 && item.WaiverMinAmount < walletConfiguration.WaiverPeriodMinAmount)
                                throw new ArgumentValidationException(nameof(item.WaiverMinAmount), "حداقل مبلغ  پاداش  مجاز نیست.");
                        }
                    }
                }

                return walletConfiguration.WalletTypeId;
            }

        }
    }
}