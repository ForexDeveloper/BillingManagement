using Domain.Base;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using System;

namespace Domain.Core.Entities.ProjectManegerAggregate
{
    [Serializable]
    public class ProjectManager : BaseEntity<int>
    {
        #region Property
        public bool IsActive { get;private set; }
        public string UserId { get; private set; }
        public string FullName { get; private set; }
        public int TenantId { get; private set; }
        public Tenant Tenant { get; private set; }

        #endregion #region Property

        private ProjectManager()
        {
        }
      
        public ProjectManager(int tenantId, string userId, string fullName )
        {
            TenantId = tenantId;
            UserId = userId;
            SetFullName(fullName);
        }

        public void SetFullName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                throw new ArgumentValidationException(nameof(fullName), $"{nameof(fullName)} is required");

            FullName = fullName;
        }
        public void Activate()
        {
            IsActive = true;
        }
        public void Deactivate()
        {
            IsActive = false;
        }
    }
}