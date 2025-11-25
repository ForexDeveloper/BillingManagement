using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.Shared.Exceptions;
using System;

namespace Domain.Core.Entities.TenantAggregate
{
    [Serializable]
    public class Tenant : BusinessIdentity
    {
        #region Property
        public string Title { get; private set; }
        public string? CreditProjectName { get; set; }
        public string? BrandName { get; set; }
        public string? InternalProjectManagerName { get; set; }
        public bool HasCoWallet { get; set; }
        public bool HasAnonymous { get; set; }
        #endregion #region Property

        private Tenant()
        {
        }

        public Tenant(int id, string title, string? creditProjectName, string? brandName, string internalProjectManagerName, bool hasCoWallet, bool hasAnonymous)
        {
                        Id = id;
            CreditProjectName = creditProjectName;
            BrandName = brandName;
            InternalProjectManagerName = internalProjectManagerName;
            HasCoWallet = hasCoWallet;
            HasAnonymous = hasAnonymous;

            SetTitle(title);
        }

        public void Update(string title, string? creditProjectName, string? brandName, string internalProjectManagerName, bool hasCoWallet, bool hasAnonymous)
        {
            CreditProjectName = creditProjectName;
            BrandName = brandName;
            InternalProjectManagerName = internalProjectManagerName;
            HasCoWallet = hasCoWallet;
            HasAnonymous = hasAnonymous;
            SetTitle(title);
        }

        public void SetTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentValidationException(nameof(Title), $"{nameof(title)} is required");

            Title = title;
        }

    }
}