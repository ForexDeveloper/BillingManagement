using Domain.Core.Enums;
using System.Collections.Generic;

namespace Application.Query.QueryModels
{
    public class GetWalletByMerchantIdQueryModel
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public WalletType WalletType { get; set; }
        public WalletStatus WalletStatus { get; set; }
        public bool IsDefault { get; set; }
        public string Title { get; set; }
        public decimal Balance { get; set; }
        public int NumberOfInstallment { get; set; }
        public IEnumerable<GetPlanDetailQueryModel> PlanDetails { get; set; }
    }
    public class GetPlanDetailQueryModel
    {
        public decimal? PrepaymentPercent { get; set; }
        public decimal? PrepaymentMinAmount { get; set; }
        public decimal? PrepaymentMaxAmount { get; set; }
        public IEnumerable<int> Installments { get; set; }
    }
}
