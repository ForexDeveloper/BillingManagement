using Domain.Base;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System;
using System.Collections.Generic;

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
    public FinancialDocumentState State { get; private set; }
    public FinancialDocumentType Type { get; private set; }
    public string Description { get; private set; }
    public long? PaymentId { get; private set; } //unique
    public string CheckSum { get; private set; }

    public RefundReason? RefundReason { get; private set; }
    public string? RefundDescription { get; private set; }
    public FinancialDocumentRefundType? RefundType { get; private set; }
    public long? ParentId { get; private set; }
    public int? MerchantBranchId { get; private set; }
    public MerchantBranch MerchantBranch { get; private set; }
    public int? TenantMerchantContractId { get; private set; }
    public TenantMerchantContract TenantMerchantContract { get; private set; }
    public int? TenantPlatformContractId { get; private set; }
    public TenantPlatformContract TenantPlatformContract { get; private set; }
    public PaymentGatewayType? PaymentGatewayType { get; private set; }
    public FinancialDocument Parent { get; private set; }
    public ICollection<FinancialDocument> ChildFinancialDocuments { get; private set; }

    public List<FinancialDocumentPayment> FinancialDocumentPayments { get; private set; } = [];

    #endregion

    private FinancialDocument()
    {
    }

    public FinancialDocument(int fromBusinessIdentityId, int toBusinessIdentityId, int tenantId, decimal amount, long? paymentId,
        FinancialDocumentType type, FinancialDocumentState state, PaymentGatewayType? paymentGatewayType = null,
        string description = null, int? merchantBranchId = null, int? tenantMerchantContractId = null, int? tenantPlatformContractId = null)
    {
        TenantId = tenantId;
        FromBusinessIdentityId = fromBusinessIdentityId;
        ToBusinessIdentityId = toBusinessIdentityId;
        PaymentId = paymentId;
        Type = type;
        State = state;
        Description = description;
        MerchantBranchId = merchantBranchId;
        TenantMerchantContractId = tenantMerchantContractId;
        TenantPlatformContractId = tenantPlatformContractId;
        PaymentGatewayType = paymentGatewayType;
        SetAmount(amount);
        SetCheckSum();
    }

    public void SetAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentValidationException(nameof(amount), "مبلغ سند مالی نمی تواند کوچک تر مساوی صفر باشد");

        Amount = amount;
    }

    public void SetCheckSum()
    {
        CheckSum = HashHelper.Hash($"{FromBusinessIdentityId}{ToBusinessIdentityId}{Type}{Amount:F10}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}");
    }

    public void SetFinancialDocumentPayments(List<FinancialDocumentPayment> financialDocumentPayments)
    {
        FinancialDocumentPayments.AddRange(financialDocumentPayments);
    }

    public void SetState(FinancialDocumentState financialDocumentState)
    {
        State = financialDocumentState;
        SetEditDateTime(DateTime.Now);
    }

    public void SetParentId(long? parentId)
    {
        ParentId = parentId;
    }

    public void Refund(RefundReason refundReason, string? refundDescription)
    {
        RefundReason = refundReason;
        RefundDescription = refundDescription;
    }

    public void SetRefundType(FinancialDocumentRefundType refundType)
    {
        RefundType = refundType;
        SetEditDateTime(DateTime.Now);
    }

    public void ValidateCheckSum()
    {
        string comperedTo = $"{FromBusinessIdentityId}{ToBusinessIdentityId}{Type}{Amount:F10}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}";

        if (!comperedTo.Validate(CheckSum))
            throw new ArgumentValidationException("InconsistentData", "اطلاعات موجود در دیتابیس صحیح نمی باشد.");
    }
}
