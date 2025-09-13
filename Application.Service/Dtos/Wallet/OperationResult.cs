using System;

namespace Application.Service.Dtos.Wallet;
public class OperationResult
{
    public string ErrorMessage { get; set; }
    public bool IsSuccess { get; set; }

    public OperationResult(bool isSuccess, string errorMessage = null)
    {
        ErrorMessage = errorMessage;
        IsSuccess = isSuccess;
    }
}
