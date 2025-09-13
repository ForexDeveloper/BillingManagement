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

        #endregion #region Property

        private Tenant()
        {
        }

        public Tenant(int id, string title, string? creditProjectName, string? brandName, string internalProjectManagerName)
        {
            Id = id;
            CreditProjectName = creditProjectName;
            BrandName = brandName;
            InternalProjectManagerName = internalProjectManagerName;
            SetTitle(title);
        }

        public void Update(string title, string? creditProjectName, string? brandName, string internalProjectManagerName)
        {
            CreditProjectName = creditProjectName;
            BrandName = brandName;
            InternalProjectManagerName = internalProjectManagerName;
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