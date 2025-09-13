using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.WalletContractAggregate;
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
        public List<WalletContract> WalletContracts { get; private set; }

        #endregion #region Property

        private Organization()
        {
        }

        public Organization(int id, string title, int tenantId, int? parentId = null)
        {
            Id = id;
            SetTitle(title);
            TenantId = tenantId;
            SetParentId(parentId);
        }

        public void SetTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentValidationException(nameof(title), $"{nameof(title)} is required");

            Title = title;
        }

        public void Update(int tenantId, string title, int? parentId = null)
        {
            TenantId = tenantId;
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