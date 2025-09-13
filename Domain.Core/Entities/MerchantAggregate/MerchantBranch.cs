using Domain.Core.Entities.BusinessEntity;
using System;

namespace Domain.Core.Entities.MerchantAggregate
{
    [Serializable]
    public class MerchantBranch : BusinessIdentity
    {
        public int MerchantId { get; private set; }
        public Merchant Merchant { get; private set; }
        public string Title { get; private set; }
        public long TerminalId { get; private set; }
        public bool IsMerchant { get; private set; }

        private MerchantBranch()
        {

        }

        public MerchantBranch(int id, int merchantId, string title, long terminalId, bool isMerchant = false)
        {
            Id = id;
            MerchantId = merchantId;
            SetTitle(title);
            TerminalId = terminalId;
            IsMerchant = isMerchant;
        }

        public void Update(string title)
        {
            SetTitle(title);
        }

        private void SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentNullException($"{nameof(title)} is required");

            Title = title;
        }

    }
}
