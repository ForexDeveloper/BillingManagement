using Application.Query.Base;

namespace Application.Query.QueryModels;

public class WalletContractCustomersQueryModel : BasePaginatedListQueryResult<WalletContractCustomerQueryModel>
{
}

public class WalletContractCustomerQueryModel
{
    public int Id { get; set; }
    public string FullName { get; set; }
}