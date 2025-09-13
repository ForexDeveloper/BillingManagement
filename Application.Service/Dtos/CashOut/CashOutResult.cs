namespace Application.Service.Dtos.CashOut;
public class CashOutResult
{
    public long Id { get; set; }
    public long TransactionId { get; set; }

    public CashOutResult(long id, long transactionId)
    {
        Id = id;
        TransactionId = transactionId;
    }
}
