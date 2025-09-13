using System;

namespace Application.Query.ViewModels.WalletContracts;

public class WalletContractRejectionVm
{
    public int Id { get; set; }
    public string Reason { get; set; }
    public DateTime CreateDateTime { get; set; }

    public WalletContractRejectionVm(int id, string reason, DateTime createDateTime)
    {
        Id = id;
        Reason = reason;
        CreateDateTime = createDateTime;
    }
}