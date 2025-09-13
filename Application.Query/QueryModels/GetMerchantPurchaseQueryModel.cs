using System;

namespace Application.Query.QueryModels
{
    public class GetMerchantPurchaseQueryModel
    {
        public long Id { get; set; }
        public string BranchName { get; set; }
        public string Mobile { get; set; }
        public decimal TotalAmount { get; set; }
        public long PaymentId { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}
