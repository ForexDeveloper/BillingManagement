using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Entities;
using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Entities.PlanAggregate.Exceptions;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.PlanCommands
{
    public class UpdateCashWalletConfigurationPlanCommand : IRequest<int>
    {
        public UpdateCashWalletConfigurationPlanCommand(int id, decimal maxWallet, decimal maxDailyWithdrawal, decimal? maxDailyDeposit, decimal? maxDailyTransactionCount, string termsAndConditions, int? tenantId = null)
        {
            Id = id;
            MaxDailyWithdrawal = maxDailyWithdrawal;
            MaxDailyDeposit = maxDailyDeposit;
            MaxDailyTransactionCount = maxDailyTransactionCount;
            TermsAndConditions = termsAndConditions;
            TenantId = tenantId;
            MaxWallet = maxWallet;
        }

        public int Id { get; set; }
        public int? TenantId { get; set; }
        public decimal MaxWallet { get; set; }
        public decimal? MaxDailyWithdrawal { get; set; }
        public decimal? MaxDailyDeposit { get; set; }
        public decimal? MaxDailyTransactionCount { get; set; }
        public string TermsAndConditions { get; set; }
        public class UpdateCashWalletConfigurationPlanCommandHandler : IRequestHandler<UpdateCashWalletConfigurationPlanCommand, int>
        {
            private readonly IPlanRepository _planRepository;
            private readonly IApplicationDbContextUnitOfWork _unitOfWork;

            public UpdateCashWalletConfigurationPlanCommandHandler(IWalletConfigurationRepository walletConfigurationRepository,
               IPlanRepository planRepository, IApplicationDbContextUnitOfWork unitOfWork)
            {
                _planRepository = planRepository;
                _unitOfWork = unitOfWork;
            }

            public async Task<int> Handle(UpdateCashWalletConfigurationPlanCommand request, CancellationToken cancellationToken)
            {
                var plan = await _planRepository.GetByIdAsync(request.Id) ??
                    throw new PlanNotFoundException("شناسه بدرستی ارسال نشده است");

                if (request.TenantId.HasValue && request.TenantId != plan.WalletConfiguration.TenantId)
                    throw new TenantForbiddenException();

                if (plan.WalletConfiguration == null)
                    throw new ArgumentValidationException(nameof(plan.WalletConfiguration), $"شناسه تنظیمات کیف پول معتبر نیست");

                if (plan.WalletConfiguration.WalletTypeId != WalletType.Cash)
                    throw new ArgumentValidationException(nameof(plan.WalletConfiguration), "فقط طرح کیف پول نقدی امکان ویرایش دارد.");

                if (request.MaxWallet > plan.WalletConfiguration.MaxWallet)
                    throw new ArgumentValidationException(nameof(plan.WalletConfiguration), "سقف  کیف پول از سقف کیف پول پیکربندی بزرگتر است.");


                if (request.MaxDailyWithdrawal > plan.MaxWallet)
                    throw new ArgumentValidationException(nameof(request.MaxDailyWithdrawal), "سقف برداشت بیشتر از سقف کیف است.");

                if (request.MaxDailyDeposit > plan.MaxWallet)
                    throw new ArgumentValidationException(nameof(request.MaxDailyDeposit), "سقف واریز بیشتر از سقف کیف است.");


                plan.SetPlan(request.MaxWallet, request.MaxDailyWithdrawal, request.MaxDailyDeposit, request.MaxDailyTransactionCount,
                    request.TermsAndConditions);

                plan.SetEditDateTime(DateTime.Now);

                _planRepository.Update(plan);
                await _unitOfWork.SaveChangesAsync();
                return plan.Id;
            }
        }
    }
}