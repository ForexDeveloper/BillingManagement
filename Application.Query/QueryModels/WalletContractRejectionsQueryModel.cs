using System;

namespace Application.Query.QueryModels;

public class WalletContractRejectionQueryModel
{
    public int Id { get; set; }
    public string? Reason { get; set; }
    public DateTime CreDateTime { get; set; }

    public WalletContractRejectionQueryModel(int id, string? reason, DateTime creDateTime)
    {
        Id = id;
        Reason = reason;
        CreDateTime = creDateTime;
    }
}