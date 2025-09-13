using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Base
{
    [Serializable]
    public abstract class BaseEntity<TKey>
    {
        public TKey Id { get; protected set; }

        public DateTime CreatedDateTime { get; protected set; } = DateTime.Now;
        public DateTime EditDateTime { get; protected set; } = DateTime.Now;

        
        [MaxLength(100)]
        [Column(TypeName = "VARCHAR")]
        public string? CreatorUserId { get; protected set; }
        [MaxLength(200)]
        [Column(TypeName = "VARCHAR")]
        public string? ClientId { get; protected set; }

        public bool IsDeleted { get; protected set; }

        public void SetCreatorUserId(string userId)
        {
            CreatorUserId = userId;
        }
        public void SetEditDateTime(DateTime editDateTime)
        {
            EditDateTime = editDateTime;
        }


        public void SetClientId(string clientId)
        {
            ClientId = clientId;
        }
        public void SetDeleted()
        {
            IsDeleted = true;
        }
    }
}