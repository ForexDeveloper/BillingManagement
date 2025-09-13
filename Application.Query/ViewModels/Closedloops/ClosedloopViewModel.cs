using System.Collections.Generic;

namespace Application.Query.ViewModels.Closedloops
{
    public class ClosedloopViewModel
    {
        public int TenantId { get; set; }
        public string TenantTitle { get; set; }
        public int Id { get; set; }
        public int WalletConfigurationId { get; set; }
        public string WalletConfigurationTitle { get; set; }
        public string Title { get; set; }
        public List<ClosedloopCategoryViewModel> Categories { get; set; }
        public List<ClosedloopMerchantViewModel> Merchants { get; set; }
    }


    public class ClosedloopCategoryViewModel
    {
        public int Id { get; set; }
        public string CategorIdTitle { get; set; }
        public int CategorId { get; set; }
    }
    public class ClosedloopMerchantViewModel
    {
        public int Id { get; set; }
        public string MerchantTitle { get; set; }
        public int MerchantId { get; set; }
    }
}
