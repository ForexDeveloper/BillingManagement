using Application.Service.Contracts;
using Application.Service.Dtos.Attachments;
using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Entities;
using Domain.Core.Entities.ClosedloopAggregate;
using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Entities.PlanAggregate.Exceptions;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MassTransit.Util;
using MediatR;
using Shared.EventBus.Contracts;
using Shared.EventBus.Events;
using Shared.IdentityServerProvider.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Command.PlanCommands
{

    public class UpdatePlanDetailDto
    {
        public int Id { get; set; }
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
    public class UpdatePlanCommand : IRequest<int>
    {
        public UpdatePlanCommand(int id, int walletConfigurationId, string walletLogo,
            string title, decimal? maxDailyWithdrawal, decimal maxWallet, decimal maxTotalCredit,
            TimeInterval billingPeriodType, int billingPeriod, DateTime? billingPeriodStartDate,
            int? gracePeriod, TimeInterval? paymentType, TimeInterval? installmentBreakType, int? installmentBreak,
            InstallmentPaymentMethodType? installmentPaymentMethod, decimal? maxDailyDeposit,
            decimal? maxDailyTransactionCount, string backgroundColor1, string backgroundColor2,
            string textColor, string description, string link, List<int> planClosedloops,
            List<UpdatePlanDetailDto> planDetails, string termsAndConditions, int? tenantId = null)
        {
            Id = id;
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
        public int Id { get; set; }
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
        public List<UpdatePlanDetailDto> PlanDetails { get; set; }
        public string TermsAndConditions { get; set; }
        public class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand, int>
        {
            private readonly IWalletConfigurationRepository _walletConfigurationRepository;
            private readonly IClosedloopRepository _closedloopRepository;
            private readonly ITenantRepository _tenantRepository;
            private readonly IPlanRepository _planRepository;
            private readonly IApplicationDbContextUnitOfWork _unitOfWork;
            private readonly ICurrentUserService _currentUserService;
            private readonly IOutboxService _outboxService;
            private readonly IAttachmentService _attachmentService;
            private readonly IWalletRepository _walletRepository;

            public UpdatePlanCommandHandler(IClosedloopRepository closedloopRepository,
                ICurrentUserService currentUserService, ITenantRepository tenantRepository,
                IWalletConfigurationRepository walletConfigurationRepository,
               IPlanRepository planRepository, IApplicationDbContextUnitOfWork unitOfWork,
               IOutboxService outboxService, IAttachmentService attachmentService, IWalletRepository walletRepository)
            {
                _tenantRepository = tenantRepository;
                _planRepository = planRepository;
                _unitOfWork = unitOfWork;
                _currentUserService = currentUserService;
                _closedloopRepository = closedloopRepository;
                _walletConfigurationRepository = walletConfigurationRepository;
                _outboxService = outboxService;
                _attachmentService = attachmentService;
                _walletRepository = walletRepository;
            }

            public async Task<int> Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
            {
                var plan = await _planRepository.GetByIdAsync(request.Id) ??
                    throw new PlanNotFoundException("شناسه بدرستی ارسال نشده است");

                if (request.TenantId.HasValue && request.TenantId != plan.WalletConfiguration.TenantId)
                {
                    throw new TenantForbiddenException();
                }
                if (plan.WalletConfiguration.Id != request.WalletConfigurationId)
                    throw new ArgumentValidationException(nameof(request.WalletConfigurationId), $"شناسه تنظیمات کیف پول معتبر نیست");

                var hasWalletContractPlan = await _planRepository.HasWalletContractPlanByPlanId(request.Id);

                if (hasWalletContractPlan)
                {
                    await CheckValidation(request, plan);

                    plan.SetPlan(request.Title, request.MaxDailyWithdrawal,
                        request.MaxWallet, request.MaxTotalCredit, request.TermsAndConditions);

                    SetPlanClosedloops(request, plan);
                }
                else
                {
                    await CheckValidation(request, plan.WalletConfiguration);

                    plan.SetPlan(request.Title, request.MaxDailyWithdrawal,
                         request.MaxWallet, request.MaxTotalCredit, request.BillingPeriodType, request.BillingPeriod,
                         request.BillingPeriodStartDate, request.GracePeriod, request.PaymentType, request.InstallmentBreakType, request.InstallmentBreak,
                         request.InstallmentPaymentMethod, request.MaxDailyDeposit, request.MaxDailyTransactionCount,
                         request.BackgroundColor1, request.BackgroundColor2, request.TextColor, request.Description, request.Link, request.TermsAndConditions);

                    SetPlanDetail(request, plan);
                    SetPlanClosedloops(request, plan);

                    if (!string.IsNullOrEmpty(request.WalletLogo))
                    {
                        await _attachmentService.UpdateAsync(new AttachmentDto
                        {
                            AttachmentCategory = AttachmentCategory.PlanLogo,
                            EntityId = plan.Id,
                            EntityType = EntityType.Plan,
                            FileReference = request.WalletLogo
                        });
                    }
                }

                using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {
                        _planRepository.Update(plan);
                        await _unitOfWork.SaveChangesAsync();
                        PublishPlanUpdatedEvent(plan);
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
            private void PublishPlanUpdatedEvent(Plan plan)
            {
                _outboxService.AddNewEvent(new FcmPlanAddedOrUpdatedEvent()
                {
                    Id = plan.Id,
                    TenantId = plan.WalletConfiguration.TenantId,
                    Title = plan.Title,
                    IsActive = plan.IsActive,
                    MaxWallet = plan.MaxWallet,
                    PlanDetails = plan.PlanDetails.Where(c => !c.IsDeleted).Select(x => new PlanDetailEvent
                    {
                        Id = x.Id,
                        NumberOfInstallments = x.PlanDetailInstallments.Where(c => !c.IsDeleted).Select(c => new PlanNumberOfInstallmentEvent
                        {
                            NumberOfInstallment = c.NumberOfInstallment,
                            Id = c.Id
                        }).ToList(),
                        OperationFee = x.OperationFee,
                        InterestPercent = x.InterestPercent,
                    }).ToList()
                });
            }
            private void SetPlanDetail(UpdatePlanCommand request, Plan plan)
            {
                if (plan.WalletConfiguration.WalletTypeId is not WalletType.Cash)
                {
                    var keys = request.PlanDetails.Where(c => c.Id > 0).Select(c => c.Id);
                    var planDetailsDelete = plan.PlanDetails.Where(c => !keys.Contains(c.Id)).ToList();

                    foreach (var item in planDetailsDelete)
                    {
                        item.SetDeleted();
                    }
                    foreach (var item in request.PlanDetails.Where(c => c.Id > 0))
                    {
                        var planDetail = plan.PlanDetails.FirstOrDefault(c => c.Id == item.Id);
                        if (planDetail != null)
                        {
                            planDetail.SetPlanDetail(item.OperationFee, item.OperationalFeeType,
                            item.PenaltyPercent, item.PenaltyMaxAmount, item.PenaltyMinAmount,
                            item.InterestPercent, item.InterestMaxAmount, item.InterestMinAmount,
                            item.WaiverPercent, item.WaiverMaxAmount, item.WaiverMinAmount,
                            item.PrepaymentPercent, item.PrepaymentMinAmount, item.PrepaymentMaxAmount);

                            List<int> InstallmentDeleted = planDetail.PlanDetailInstallments
                                .Select(c => c.NumberOfInstallment).Except(item.NumberOfInstallments).ToList();

                            List<int> InstallmentAdded = item.NumberOfInstallments
                                .Except(planDetail.PlanDetailInstallments.Select(c => c.NumberOfInstallment)).ToList();

                            foreach (var installment in planDetail.PlanDetailInstallments.Where(c => InstallmentDeleted.Contains(c.NumberOfInstallment)))
                            {
                                installment.SetDeleted();
                            }

                            foreach (var Installment in InstallmentAdded)
                            {
                                planDetail.SetPlanDetailInstallment(new PlanDetailInstallment(Installment));
                            }
                        }
                    }
                    foreach (var item in request.PlanDetails.Where(c => c.Id == 0))
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
            }
            private void SetPlanClosedloops(UpdatePlanCommand request, Plan plan)
            {
                var planClosedloops = plan.PlanClosedloops.Where(c => !request.PlanClosedloops.Contains(c.ClosedLoopId)).ToList();
                foreach (var item in planClosedloops)
                {
                    item.SetDeleted();
                }
                foreach (var planClosedloop in request.PlanClosedloops)
                {
                    if (!plan.PlanClosedloops.Any(c => c.ClosedLoopId == planClosedloop))
                    {
                        plan.SetPlanClosedloops(new PlanClosedloop(planClosedloop));
                    }
                }
            }
            private async Task CheckValidation(UpdatePlanCommand request, WalletConfiguration walletConfiguration)
            {

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
                        throw new ArgumentValidationException(nameof(totalInstallments), "تعداد اقساط در هر طرح تکتا میباشد و براساس پیکربندی وارد میشود.");
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
                    throw new ArgumentValidationException(nameof(request.MaxDailyTransactionCount), "سقف تراکنش روزانه بیشتر از سقف مجاز است.");

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
                                throw new ArgumentValidationException(nameof(item.PenaltyPercent), "درصد  جریمه  مجاز نیست.");

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
            }

            private async Task CheckValidation(UpdatePlanCommand request, Plan plan)
            {
                if (request.MaxWallet > plan.MaxWallet && request.MaxWallet > plan.WalletConfiguration.MaxWallet)
                {
                    throw new ArgumentValidationException(nameof(request.MaxWallet), "سقف کیف پول بیشتر از سقف مجاز است.");
                }
                else if (request.MaxWallet < plan.MaxWallet)
                {
                    var hasWallet = await _walletRepository.CheckWalletInitialAmountAsync(plan.Id, request.MaxWallet);
                    if (hasWallet)
                        throw new ArgumentValidationException(nameof(request.MaxWallet), "سقف کیف پول کمتر از کیف پول های صادر شده است.");
                }

                if (request.MaxTotalCredit > plan.MaxTotalCredit && request.MaxTotalCredit > plan.WalletConfiguration.MaxTotalCredit)
                {
                    throw new ArgumentValidationException(nameof(request.MaxTotalCredit), "سقف تجمیع اعتبار کاربران بیشتر از سقف مجاز است.");
                }
                else if (request.MaxTotalCredit < plan.MaxTotalCredit)
                {
                    var hasMaxTotalCredit = await _walletRepository.CheckWalletMaxTotalCreditAsync(plan.Id, request.MaxTotalCredit);
                    if (hasMaxTotalCredit)
                        throw new ArgumentValidationException(nameof(request.MaxTotalCredit), "سقف تجمیع اعتبار کاربران کمتر از مجموع کیف پول های صادر شده است.");
                }

                if (request.MaxDailyWithdrawal > request.MaxWallet)
                    throw new ArgumentValidationException(nameof(request.MaxDailyWithdrawal), "سقف برداشت بیشتر از سقف مجاز است.");

                if (request.PlanClosedloops != null && request.PlanClosedloops.Any())
                {
                    var hasClosedloop = await _closedloopRepository.CheckClosedloop(request.WalletConfigurationId, request.PlanClosedloops);
                    if (!hasClosedloop)
                        throw new ArgumentValidationException(nameof(request.PlanClosedloops), "شناسه close loop معتبر نیست.");
                }

            }
        }
    }
}