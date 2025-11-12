using Domain.Base;
using Domain.Core.Entities.OrganizationAggregate;
using System;

namespace Domain.Core.Entities.CustomerAggregate
{
    [Serializable]
    public class CustomerOrganization : BaseEntity<int>
    {
        #region Property
        public int OrganizationId { get; set; }
        public Organization Organization { get; private set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; private set; }

        public CustomerOrganization(int organizationId)
        {
            OrganizationId = organizationId;
        }

        public void Update(int organizationId)
        {
            OrganizationId = organizationId;
        }

        #endregion #region Property
    }
}