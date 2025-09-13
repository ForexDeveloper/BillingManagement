using Domain.Base;
using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using System;
using System.Collections.Generic;

namespace Domain.Core.Entities.ClosedloopAggregate
{
    [Serializable]
    public class Closedloop : BaseEntity<int>
    {
        public int WalletConfigurationId { get; private set; }
        public WalletConfiguration WalletConfiguration { get; private set; }
        public string Title { get; private set; }
        public List<ClosedloopCategory> ClosedloopCategories { get; private set; }
        public List<ClosedloopMerchant> ClosedloopMerchants { get; private set; }

        private Closedloop()
        {
        }

        public Closedloop(int walletConfigurationId, string title)
        {
            WalletConfigurationId = walletConfigurationId;
            SetTitle(title);
        }
        public void SetClosedloop(int walletConfigurationId, string title)
        {
            WalletConfigurationId = walletConfigurationId;
            SetTitle(title);
        }
        public void SetClosedloopCategory(List<ClosedloopCategory> closedloopCategories)
        {
            ClosedloopCategories = closedloopCategories;
        }
        public void SetClosedloopMerchant(List<ClosedloopMerchant> closedloopMerchants)
        {
            ClosedloopMerchants = closedloopMerchants;
        }
        public void SetTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentValidationException(nameof(title), $"{nameof(title)} is required");

            Title = title;
        }

    }
}