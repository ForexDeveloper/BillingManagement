using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;
using Domain.Core.Entities.MerchantBillingAggregate;

namespace Domain.Core.Entities.MerchantAggregate
{
    [Serializable]
    public class Merchant : BusinessIdentity
    {
        #region Property
        public int TenantId { get; private set; }
        public string Title { get; private set; }
        public IdentityTypeEnum Type { get; private set; }
        public SaleType SaleType { get; private set; }
        public MerchantStatus Status { get; private set; }
        public List<MerchantBranch> MerchantBranches { get; private set; }

        #endregion #region Property

        private Merchant()
        {
        }

        public void SetMerchantBranches(MerchantBranch merchantBranch)
        {
            if (merchantBranch is null)
                throw new ArgumentValidationException(nameof(MerchantBranch), "مشخصات پذیرنده اجباریست.");
            MerchantBranches ??= new List<MerchantBranch>();
            MerchantBranches.Add(merchantBranch);
        }
        public Merchant(int id, int tenantId, string title, byte type, byte saleType, byte status)
        {
            Id = id;
            SetTitle(title);
            TenantId = tenantId;
            Type = (IdentityTypeEnum)type;
            SaleType = (SaleType)saleType;
            Status = (MerchantStatus)status;
        }

        public void SetTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentValidationException(nameof(Title), $"{nameof(title)} is required");

            Title = title;
        }

        public void SetMerchant(string title, byte type, byte saleType, byte status)
        {
            SetTitle(title);
            Type = (IdentityTypeEnum)type;
            SaleType = (SaleType)saleType;
            Status = (MerchantStatus)status;
        }

        //public void SetCategories(MerchantCategory merchantCategory)
        //{
        //    if (merchantCategory is null)
        //        throw new ArgumentValidationException(nameof(MerchantCategories), "مشخصات دسته بندی محصولات اجباریست.");

        //    MerchantCategories ??= new List<MerchantCategory>();
        //    MerchantCategories.Add(merchantCategory);
        //}

    }
}