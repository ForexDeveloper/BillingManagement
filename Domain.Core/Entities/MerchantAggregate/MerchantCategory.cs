using Domain.Base;
using Domain.Core.AggregateRoots.CategoryAggregate;
using Domain.Core.Entities.MerchantAggregate;
using System;

namespace Domain.Core.Entities.Merchants
{
    [Serializable]
    public class MerchantCategory : BaseEntity<int>
    {
        public int MerchantId { get; private set; }
        public int CategoryId { get; private set; }
        public Merchant Merchant { get; private set; }
        public Category Category { get; private set; }
        private MerchantCategory()
        {
        }
        public MerchantCategory( int categoryId)
        {
            CategoryId = categoryId;    
        }
    }
}
