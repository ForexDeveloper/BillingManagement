using Domain.Base;
using Domain.Core.AggregateRoots.CategoryAggregate;
using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using System;

namespace Domain.Core.Entities.ClosedloopAggregate
{
    [Serializable]
    public class ClosedloopCategory : BaseEntity<int>
    {
        public int ClosedloopId { get; private set; }
        public int CategoryId { get; private set; }
        public Closedloop Closedloop { get; private set; }
        public Category Category { get; private set; }
        private ClosedloopCategory()
        {
        }

        public ClosedloopCategory(int closedloopId, int categoryId)
        {
            ClosedloopId = closedloopId;
            CategoryId = categoryId;
        }
        public void SetWalletConfigurationAndCategory(int closedloopId, int categoryId)
        {
            ClosedloopId = closedloopId;
            CategoryId = categoryId;
        }


    }
}