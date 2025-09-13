using System;

namespace Application.Query.ViewModels.TenantPlatfromContracts;

public class TenantPlatformContractInstallmentVm
{
    public DateTime DueDateTime { get; set; }
    public decimal Amount { get; set; }

    public TenantPlatformContractInstallmentVm(DateTime dueDateTime, decimal amount)
    {
        DueDateTime = dueDateTime;
        Amount = amount;
    }
}