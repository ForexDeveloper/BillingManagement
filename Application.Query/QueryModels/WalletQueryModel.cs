using Domain.Core.Enums;

namespace Application.Query.QueryModels;

public class WalletQueryModel
{
    public int MechantId { get; set; }
    public SaleType SaleType { get; set; }

}
