using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Attachments;
using Application.Query.ViewModels.Plans;
using Domain.Core.Entities.PlanAggregate.Exceptions;
using Domain.Core.Enums;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class GetPlanByIdQuery : IRequest<PlanViewModel>
{
    public int Id { get; }

    public int? TenantId { get; }

    public GetPlanByIdQuery(int id, int? tenantId = null)
    {
        Id = id;
        TenantId = tenantId;
    }
}

public class PlanGetByIdQueryHandler : BaseQueryHandler, IRequestHandler<GetPlanByIdQuery, PlanViewModel>
{
    private readonly IPlanReadOnlyRepository _planReadOnlyRepository;
    private readonly IAttachmentReadOnlyRepository _attachmentRepositoryReadOnlyRepository;

    public PlanGetByIdQueryHandler(IPlanReadOnlyRepository planReadOnlyRepository, IAttachmentReadOnlyRepository attachmentRepositoryReadOnlyRepository)
    {
        _planReadOnlyRepository = planReadOnlyRepository;
        _attachmentRepositoryReadOnlyRepository = attachmentRepositoryReadOnlyRepository;
    }

    public async Task<PlanViewModel> Handle(GetPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _planReadOnlyRepository.GetByIdAsync(request.Id);

        if (result == null)
            throw new PlanNotFoundException("شناسه بدرستی ارسال نشده.");

        var attachment = await _attachmentRepositoryReadOnlyRepository.GetAsync(EntityType.Plan, request.Id);

        var hasWalletContractPlan = await _planReadOnlyRepository.HasWalletContractPlanByPlanId(request.Id);
        return new PlanViewModel
        {
            PlanLogo = attachment is null ? null : new GetAttachmentVm
            {
                AttachmentCategory = (AttachmentCategory)attachment.AttachmentCategory,
                ContentType = attachment.ContentType,
                FileExtension = attachment.FileExtension,
                FileReference = attachment.FileReference,
                FileSize = attachment.FileSize,
            },
            BillingPeriod = result.BillingPeriod,
            InstallmentBreak = result.InstallmentBreak,
            Editable = !hasWalletContractPlan,
            Id = result.Id,
            MaxDailyDeposit = result.MaxDailyDeposit,
            MaxDailyTransactionCount = result.MaxDailyTransactionCount,
            MaxDailyWithdrawal = result.MaxDailyWithdrawal,
            MaxTotalCredit = result.MaxTotalCredit,
            MaxWallet = result.MaxWallet,
            TenantId = result.TenantId,
            TenantTitle = result.TenantTitle,
            Title = result.Title,
            WalletConfigurationId = result.WalletConfigurationId,
            WalletConfigurationTitle = result.WalletConfigurationTitle,
            GracePeriod = result.GracePeriod,
            InstallmentBreakType = result.InstallmentBreakType,
            Description = result.Description,
            TextColor = result.TextColor,
            PaymentType = result.PaymentType,
            BackgroundColor1 = result.BackgroundColor1,
            BackgroundColor2 = result.BackgroundColor2,
            BillingPeriodStartDate = result.BillingPeriodStartDate,
            BillingPeriodType = result.BillingPeriodType,
            InstallmentPaymentMethod = result.InstallmentPaymentMethod,
            Link = result.Link,
            PlanClosedloops = result.PlanClosedloops.Select(c => new PlanClosedloopViewModel
            {
                Id = c.Id,
                ClosedloopId = c.ClosedloopId,
                Title = c.Title,
            }).ToList(),
            PlanDetails = result.PlanDetails.Select(c => new PlanDetailViewModel
            {
                Id = c.Id,
                PlanId = c.PlanId,
                NumberOfInstallments = c.NumberOfInstallments.ToList(),
                OperationFee = c.OperationFee,
                OperationalFeeType = c.OperationalFeeType,
                PenaltyPercent = c.PenaltyPercent,
                PenaltyMaxAmount = c.PenaltyMaxAmount,
                PenaltyMinAmount = c.PenaltyMinAmount,
                InterestPercent = c.InterestPercent,
                InterestMaxAmount = c.InterestMaxAmount,
                InterestMinAmount = c.InterestMinAmount,
                WaiverPercent = c.WaiverPercent,
                WaiverMaxAmount = c.WaiverMaxAmount,
                WaiverMinAmount = c.WaiverMinAmount,
                PrepaymentPercent = c.PrepaymentPercent,
                PrepaymentMaxAmount = c.PrepaymentMaxAmount,
                PrepaymentMinAmount = c.PrepaymentMinAmount,
            }).ToList(),
            TermsAndConditions = result.TermsAndConditions,
            ReservedCredit = result.ReservedCredit,
            AssignedCredit = result.AssignedCredit
        };
    }
}