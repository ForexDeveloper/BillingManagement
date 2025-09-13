using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.WalletConfigurations;
using Domain.Core.Entities.WalletConfigurationAggregate.Exceptions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetWalletConfigurationByIdQuery : IRequest<WalletConfigurationViewModel>
    {
        public int Id { get; }
        public int? TenantId { get; }

        public GetWalletConfigurationByIdQuery(int id, int? tenantId = null)
        {
            Id = id;
            TenantId = tenantId;
        }
    }

    public class WalletConfigurationGetByIdQueryHandler : BaseQueryHandler, IRequestHandler<GetWalletConfigurationByIdQuery, WalletConfigurationViewModel>
    {
        private readonly IWalletConfigurationReadOnlyRepository _WalletConfigurationReadOnlyRepository;
        private readonly IPlanReadOnlyRepository _planReadOnlyRepository;
        public WalletConfigurationGetByIdQueryHandler(IWalletConfigurationReadOnlyRepository WalletConfigurationReadOnlyRepository, IPlanReadOnlyRepository planReadOnlyRepository)
        {
            _WalletConfigurationReadOnlyRepository = WalletConfigurationReadOnlyRepository;
            _planReadOnlyRepository = planReadOnlyRepository;
        }

        public async Task<WalletConfigurationViewModel> Handle(GetWalletConfigurationByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _WalletConfigurationReadOnlyRepository.GetAsync(request.Id);
            if (result == null)
                throw new WalletConfigurationNotFoundException("کانفیگ کیف پول پیدا نشد.");

          var hasPlan=  await _planReadOnlyRepository.HasPlanByWalletConfigurationIdAsync(request.Id);

            return new WalletConfigurationViewModel
            {
                Id = result.Id,
                Title = result.Title,
                WalletTypeId = result.WalletTypeId,
                MaxWallet = result.MaxWallet,
                MaximumTotalCredit = result.MaximumTotalCredit,
                TenantId = result.TenantId,
                ProjectManagerId = result.ProjectManagerId,
                CurrencyTypeId = result.CurrencyTypeId,
                CurrencyTypeTitle = result.CurrencyTypeTitle,
                TenantTitle = result.TenantTitle,
                ProjectManagerFullName = result.ProjectManagerFullName,
                Editable = !hasPlan,
                Financial = new FinancialCommitmentViewModel
                {
                    InterestPeriodMaxPercent = result.InterestPeriodMaxPercent,
                    InterestPeriodMinPercent = result.InterestPeriodMinPercent,
                    InterestPeriodMaxAmount = result.InterestPeriodMaxAmount,
                    InterestPeriodMinAmount = result.InterestPeriodMinAmount,
                    PenaltyPeriodMaxPercent = result.PenaltyPeriodMaxPercent,
                    PenaltyPeriodMinPercent = result.PenaltyPeriodMinPercent,
                    PenaltyPeriodMaxAmount = result.PenaltyPeriodMaxAmount,
                    PenaltyPeriodMinAmount = result.PenaltyPeriodMinAmount,
                    WaiverPeriodMaxAmount = result.WaiverPeriodMaxAmount,
                    WaiverPeriodMaxPercent = result.WaiverPeriodMaxPercent,
                    WaiverPeriodMinAmount = result.WaiverPeriodMinAmount,
                    WaiverPeriodMinPercent = result.WaiverPeriodMinPercent
                },
                Installments = new InstallmentsViewModel
                {
                    MaxInstallments = result.MaxInstallments,
                    PrepaymentMinAmount = result.PrepaymentMinAmount,
                    PrepaymentMaxPercent = result.PrepaymentMaxPercent,
                    PrepaymentMaxAmount = result.PrepaymentMaxAmount,
                    PrepaymentMinPercent = result.PrepaymentMinPercent,
                }
            };
        }
    }
}