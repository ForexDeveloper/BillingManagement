using System;
using Domain.Base;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System.Collections.Generic;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate;

namespace Domain.Core.Entities.FinancialDocumentAggregate;

public class FinancialDocument : BaseEntity<long>
{
    #region Property

    public int FromBusinessIdentityId { get; private set; }

    public BusinessIdentity FromBusinessIdentity { get; private set; }

    public int ToBusinessIdentityId { get; private set; }

    public BusinessIdentity ToBusinessIdentity { get; private set; }

    public int TenantId { get; private set; }

    public Tenant Tenant { get; private set; }

    public decimal Amount { get; private set; }

    public FinancialDocumentType Type { get; private set; }

    public FinancialDocumentState State { get; private set; }

    public PaymentGatewayType? PaymentGatewayType { get; private set; }

    public string Description { get; private set; }

    public int? MerchantBranchId { get; private set; }

    public MerchantBranch MerchantBranch { get; private set; }

    public int? TenantMerchantContractId { get; private set; }

    public TenantMerchantContract TenantMerchantContract { get; private set; }

    public int? TenantPlatformContractId { get; private set; }

    public TenantPlatformContract TenantPlatformContract { get; private set; }

    public string CheckSum { get; private set; }

    public RefundReason? RefundReason { get; private set; }

    public string RefundDescription { get; private set; }

    public FinancialDocumentRefundType? RefundType { get; private set; }

    public decimal CreditAmount { get; set; }

    public decimal CashAmount { get; set; }

    public decimal PrepaymentAmount { get; set; }

    public decimal? Commission { get; private set; }

    public long? ParentId { get; private set; }

    public FinancialDocument Parent { get; private set; }

    public ICollection<FinancialDocument> ChildFinancialDocuments { get; private set; }

    #endregion

    private FinancialDocument()
    {
        
    }

    public FinancialDocument(long id)
    {
        Id = id;
    }

    public FinancialDocument(long id, int fromBusinessIdentityId, int toBusinessIdentityId, int tenantId, decimal amount,
        decimal creditAmount, decimal cashAmount, decimal prepaymentAmount,
        FinancialDocumentType type,
        byte state, byte? paymentGatewayType = null,
        string description = null, int? merchantBranchId = null, int? tenantMerchantContractId = null, int? tenantPlatformContractId = null,
        byte? refundReason = null, string refundDescription = null, byte? refundType = null, long? parentId = null)
    {
        Id = id;
        FromBusinessIdentityId = fromBusinessIdentityId;
        ToBusinessIdentityId = toBusinessIdentityId;
        TenantId = tenantId;
        Amount = amount;
        CreditAmount = creditAmount;
        CashAmount = cashAmount;
        PrepaymentAmount = prepaymentAmount;
        Type = type;
        State = (FinancialDocumentState)state;
        PaymentGatewayType = (PaymentGatewayType)paymentGatewayType;
        Description = description;
        MerchantBranchId = merchantBranchId;
        TenantMerchantContractId = tenantMerchantContractId;
        TenantPlatformContractId = tenantPlatformContractId;
        RefundReason = (RefundReason)refundReason;
        RefundDescription = refundDescription;
        RefundType = (FinancialDocumentRefundType)refundType;
        ParentId = parentId;
        SetCheckSum();
    }

    public void Update(int fromBusinessIdentityId, int toBusinessIdentityId, decimal amount, byte type, byte state)
    {
        FromBusinessIdentityId = fromBusinessIdentityId;
        ToBusinessIdentityId = toBusinessIdentityId;
        Amount = amount;
        Type = (FinancialDocumentType)type;
        State = (FinancialDocumentState)state;
        SetCheckSum();
    }

    public void SetState(FinancialDocumentState financialDocumentState)
    {
        State = financialDocumentState;
        SetEditDateTime(DateTime.Now);
    }

    public void SetCheckSum()
    {
        CheckSum = HashHelper.Hash($"{FromBusinessIdentityId}{ToBusinessIdentityId}{Type}{Amount:F10}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}");
    }

    public void SetCommission(decimal? commission)
    {
        Commission = commission;
    }

    public void ValidateCheckSum()
    {
        string comperedTo = $"{FromBusinessIdentityId}{ToBusinessIdentityId}{Type}{Amount:F10}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}";

        if (!comperedTo.Validate(CheckSum))
            throw new ArgumentValidationException("InconsistentData", "اطلاعات موجود در دیتابیس صحیح نمی باشد.");
    }
}
