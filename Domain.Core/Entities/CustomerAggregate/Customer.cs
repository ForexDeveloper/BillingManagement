using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System;
using System.Collections.Generic;

namespace Domain.Core.Entities.CustomerAggregate
{
    [Serializable]
    public class Customer : BusinessIdentity
    {
        #region Property
        public int TenantId { get; private set; }
        public Tenant Tenant { get; private set; }
        public string? FullName { get; private set; }
        public string? NationalId { get; private set; }
        public string? Mobile { get; private set; }
        public bool IsShahkarConfirmed { get; private set; }
        public string UniqueIdentifier { get; private set; }
        public IdentityTypeEnum CustomerType { get; private set; }
        public List<CustomerOrganization> CustomerOrganizations { get; private set; } = [];

        #endregion #region Property

        private Customer(string mobile, string nationalId)
        {
            Mobile = mobile;
            NationalId = nationalId;
        }

        public Customer(int id, string? fullName, string? mobile, string? nationalId, int tenantId,
            bool isShahkarConfirmed, string uniqueIdentifier, IdentityTypeEnum customerType)
        {
            Id = id;
            SetFullName(fullName);
            TenantId = tenantId;
            SetMobile(mobile);
            NationalId = nationalId;
            IsShahkarConfirmed = isShahkarConfirmed;
            UniqueIdentifier = uniqueIdentifier;
            CustomerType = customerType;
        }

        public void SetFullName(string title)
        {
            FullName = title;
        }

        public void SetMobile(string mobile)
        {
            if (!string.IsNullOrEmpty(mobile) && !BaseValidationHelpers.IsValidMobileNumber(mobile))
                throw new ArgumentValidationException(nameof(Mobile), "شماره موبایل معتبر نمی باشد.");

            Mobile = mobile;
        }

        public void Update(string? fullName, string? mobile, string? nationalId,
            bool isShahkarConfirmed, string uniqueIdentifier, IdentityTypeEnum customerType)
        {
            SetFullName(fullName);
            SetMobile(mobile);
            SetEditDateTime(DateTime.Now);
            IsShahkarConfirmed = isShahkarConfirmed;
            UniqueIdentifier = uniqueIdentifier;
            CustomerType = customerType;
        }

        public void SetCustomerOrganizations(int organizationId)
        {
            CustomerOrganizations.Add(new CustomerOrganization(organizationId));
        }

    }
}