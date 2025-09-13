using Domain.Core.Enums;
using System;

namespace Application.Query.QueryModels;

public class CutomerWalletDetailsQueryModel
{
    public WalletType Type { get; set; }
    public WalletStatus Status { get; set; }
    public DateTime CreateDateTime { get; set; }
    public decimal? InitialAmount { get; set; }
    public decimal? Balance { get; set; }
    public int? InstallmentsCount { get; set; }
    public string TermsAndConditions { get; set; }
    public string OrganizationName { get; set; }

}