using Domain.Core.Enums;

namespace Application.Query.Queries;

public class GetWalletQueryModel
{
    public int  Id { get; set; }
    public WalletType WalletTypeId { get; set; }

}