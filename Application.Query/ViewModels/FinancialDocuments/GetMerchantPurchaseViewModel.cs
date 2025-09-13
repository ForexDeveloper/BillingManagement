using Application.Query.Base;
using System;

namespace Application.Query.ViewModels.FinancialDocuments
{
    public class GetMerchantPurchasesViewModel : BasePaginatedListQueryResult<GetMerchantPurchaseViewModel>
    {
       
    }
    public class GetMerchantPurchaseViewModel
    {
        public long Id { get; set; }
        public string BranchName { get; set; }
        public string Mobile { get; set; }
        public decimal TotalAmount { get; set; }
        public long ReferenceNumber { get; set; }
        public DateTime CreateDateTime { get; set; }
    }
}
