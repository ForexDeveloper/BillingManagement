using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Core.Entities.OrganizationAggregate
{
    [Serializable]
    public class Organization : BusinessIdentity
    {
        #region Property
        public int TenantId { get; private set; }
        public Tenant Tenant { get; private set; }
        public string Title { get; private set; }
        public int? ParentId { get; private set; }
        public Organization Parent { get; set; }
        public OrganizationTypeEnum OrganizationType { get; set; }
        public OrganizationIdentityTypeEnum OrganizationIdentityType { get; set; }
        public CoWalletNameEnum? CoWalletName { get; set; }

        //public List<WalletContract> WalletContracts { get; private set; }

        #endregion #region Property

        private Organization()
        {
        }

        public Organization(int id, string title, int tenantId, OrganizationTypeEnum organizationType, OrganizationIdentityTypeEnum organizationIdentityType, CoWalletNameEnum? coWalletName=null, int? parentId = null)
        {
            Id = id;
            SetTitle(title);
            TenantId = tenantId;
            CoWalletName = coWalletName;
            OrganizationIdentityType = organizationIdentityType;
            OrganizationType = organizationType;

            SetParentId(parentId);
        }

        public void SetTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentValidationException(nameof(title), $"{nameof(title)} is required");

            Title = title;
        }

        public void Update(int tenantId, string title, OrganizationTypeEnum organizationType, OrganizationIdentityTypeEnum organizationIdentityType, CoWalletNameEnum? coWalletName, int? parentId = null)
        {
            TenantId = tenantId;
            CoWalletName = coWalletName;
            OrganizationIdentityType = organizationIdentityType;
            OrganizationType = organizationType;

            SetTitle(title);
            SetParentId(parentId);
            SetEditDateTime(DateTime.Now);
        }
        public void SetParentId(int? parentId)
        {
            ParentId = parentId is 0 ? null : parentId;
        }
    }
}