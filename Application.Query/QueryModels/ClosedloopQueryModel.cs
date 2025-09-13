using System.Collections.Generic;

namespace Application.Query.QueryModels
{
    public class ClosedloopQueryModel
    {
        public int TenantId { get; set; }
        public string TenantTitle { get; set; }
        public int Id { get; set; }
        public int WalletConfigurationId { get; set; }
        public string WalletConfigurationTitle { get; set; }
        public string Title { get; set; }
        public IEnumerable<ClosedloopCategoryQueryModel> Categories { get; set; }
        public IEnumerable<ClosedloopMerchantQueryModel> Merchants { get; set; }
    }
    public class ClosedloopCategoryQueryModel
    {
        public int Id { get; set; }
        public string CategorIdTitle { get; set; }
        public int CategorId { get; set; }
    }
    public class ClosedloopMerchantQueryModel
    {
        public int Id { get; set; }
        public string MerchantTitle { get; set; }
        public int MerchantId { get; set; }
    }

}
