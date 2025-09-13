using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using System;
using System.Collections.Generic;

namespace Domain.Core.Entities.CustomerAggregate
{
    [Serializable]
    public class Customer : BusinessIdentity
    {
        #region Property
        public int TenantId { get; set; }
        public Tenant Tenant { get; private set; }
        public string FullName { get; private set; }
        public string NationalId { get; set; }
        public string Mobile { get; set; }
        public List<CustomerOrganization> CustomerOrganizations { get; private set; } = [];

        #endregion #region Property

        private Customer(string mobile, string nationalId)
        {
            Mobile = mobile;
            NationalId = nationalId;
        }

        public Customer(int id, string fullName, string mobile, string nationalId, int tenantId)
        {
            Id = id;
            SetFullName(fullName);
            TenantId = tenantId;
            SetMobile(mobile);
            NationalId = nationalId;
        }

        public void SetFullName(string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentValidationException(nameof(title), $"{nameof(title)} is required");

            FullName = title;
        }

        public void SetMobile(string mobile)
        {
            if (string.IsNullOrEmpty(mobile))
                throw new ArgumentValidationException(nameof(Mobile), $"{nameof(mobile)} is required");

            if (!BaseValidationHelpers.IsValidMobileNumber(mobile))
                throw new ArgumentValidationException(nameof(Mobile), "شماره موبایل معتبر نمی باشد.");

            Mobile = mobile;
        }

        public void Update(string fullName, string mobile)
        {
            SetFullName(fullName);
            SetMobile(mobile);
            SetEditDateTime(DateTime.Now);
        }

        public void SetCustomerOrganizations(int organizationId)
        {
            CustomerOrganizations.Add(new CustomerOrganization(organizationId));
        }

    }
}