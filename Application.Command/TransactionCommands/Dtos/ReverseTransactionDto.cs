namespace Application.Command.TransactionCommands.Dtos;

public class ReverseTransactionDto
{
    public ReverseTransactionDto(long paymentId, bool isReversed)
    {
        PaymentId = paymentId;
        IsReversed = isReversed;
    }

    public long PaymentId { get; set; }
    public bool IsReversed { get; set; }
}
