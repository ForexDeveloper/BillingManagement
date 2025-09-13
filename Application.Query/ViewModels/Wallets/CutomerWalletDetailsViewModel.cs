using Domain.Core.Enums;
using Domain.Core.Helper;
using System;

namespace Application.Query.ViewModels.Wallets
{
    public class CutomerWalletDetailsViewModel
    {
        public WalletType Type { get; set; }
        public WalletStatus Status { get; set; }
        public string TypeTitle => Type.GetEnumDescription();
        public string StatusTitle => Status.GetEnumDescription();
        public DateTime CreateDateTime { get; set; }
        public decimal? InitialAmount { get; set; }
        public decimal? Balance { get; set; }
        public int? InstallmentsCount { get; set; }
        public string TermsAndConditions { get; set; }
        public string OrganizationName { get; set; }
    }
}
