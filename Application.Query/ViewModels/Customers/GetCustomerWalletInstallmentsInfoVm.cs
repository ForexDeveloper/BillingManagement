using Domain.Core.Enums;
using Domain.Core.Helper;
using System;
using System.Collections.Generic;

namespace Application.Query.ViewModels.Customers
{
    public class GetCustomerWalletInstallmentsInfoVm
    {
        public DateTime FirstInstallmentDueDate { get; set; }
        public DateTime LastInstallmentDueDate { get; set; }

        public int PaidInstallmentsCount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public List<CustomerWalletInstallmentsVm> CustomerWalletInstallments { get; set; }
    }


    public class CustomerWalletInstallmentsVm
    {
        public DateTime DueDate { get; set; }
        public string InstallmentIdentity { get; set; }
        public InstallmentState InstallmentState { get; set; }
        public string InstallmentStateTitle => InstallmentState.GetEnumDescription();
    }
}
