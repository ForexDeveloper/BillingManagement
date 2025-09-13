using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Application.Query.QueryModels;

public class GetCustomerWalletInstallmentsInfoQueryModel
{
    public DateTime FirstInstallmentDueDate { get; set; }
    public DateTime LastInstallmentDueDate { get; set; }

    public int PaiedInstallmentCount { get; set; }
    public decimal PaiedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public IEnumerable<CustomerWalletInstallment> CustomerWalletInstallments { get; set; }
}
public class CustomerWalletInstallment
{
    public DateTime DueDate { get; set; }
    public string InstallmentIdentity { get; set; }
    public InstallmentState InstallmentState { get; set; }
}

