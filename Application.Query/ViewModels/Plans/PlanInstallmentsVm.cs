using System;

namespace Application.Query.ViewModels.Plans;

public class PlanInstallmentsVm
{
    public int Number { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
}

