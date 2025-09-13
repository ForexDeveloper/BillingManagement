using Domain.Base;
using Domain.Core.Entities.MerchantAggregate;
using System;

namespace Domain.Core.Entities.ClosedloopAggregate
{
    [Serializable]
    public class ClosedloopMerchant : BaseEntity<int>
    {
        public int ClosedloopId { get; private set; }
        public int MerchantId { get; private set; }
        public Closedloop Closedloop { get; private set; }
        public Merchant Merchant { get; private set; }
        private ClosedloopMerchant()
        {
        }

        public ClosedloopMerchant(int closedloopId, int merchantId)
        {
            ClosedloopId = closedloopId;
            MerchantId = merchantId;
        }
        public void SetClosedloopMerchant(int closedloopId, int merchantId)
        {
            ClosedloopId = closedloopId;
            MerchantId = merchantId;
        }
    }

}