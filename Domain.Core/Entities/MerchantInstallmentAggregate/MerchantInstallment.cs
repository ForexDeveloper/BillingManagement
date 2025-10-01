using System;
using Domain.Core.Enums;
using Domain.Core.Helper;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.InstallmentAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;

namespace Domain.Core.Entities.MerchantInstallmentAggregate;

public sealed class MerchantInstallment : Installment
{
    public int TenantMerchantContractId { get; private set; }

    public TenantMerchantContract TenantMerchantContract { get; private set; }

    private MerchantInstallment()
    {

    }

    public MerchantInstallment(FinancialDocument financialDocument, int tenantId, int fromBusinessIdentityId,
        int toBusinessIdentityId, int tenantMerchantContractId, decimal amount, decimal cashAmount,
        decimal creditAmount, decimal prepaymentAmount, int number, DateTime dueDate, InstallmentType type) : base(
        financialDocument, tenantId, fromBusinessIdentityId, toBusinessIdentityId, amount, cashAmount, creditAmount,
        prepaymentAmount, number, dueDate, type)
    {
        TenantMerchantContractId = tenantMerchantContractId;
        SetCheckSum();
    }

    public MerchantInstallment(int tenantId, long financialDocumentId, int fromBusinessIdentityId,
        int toBusinessIdentityId, int tenantMerchantContractId, decimal amount, decimal cashAmount,
        decimal creditAmount, decimal prepaymentAmount, int number, DateTime dueDate, InstallmentType type) : base(
        tenantId, financialDocumentId, fromBusinessIdentityId, toBusinessIdentityId, amount, cashAmount, creditAmount,
        prepaymentAmount, number, dueDate, type)
    {
        TenantMerchantContractId = tenantMerchantContractId;
        SetCheckSum();
    }

    protected override void SetCheckSum()
    {
        CheckSum = $"{FromBusinessIdentityId}{ToBusinessIdentityId}{Amount:F10}{Status}{DueDate:yyyy-MM-ddTHH:mm:ss}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}".Hash();
    }

    protected override void ValidateCheckSum()
    {
        var comperedTo = $"{FromBusinessIdentityId}{ToBusinessIdentityId}{Amount:F10}{Status}{DueDate:yyyy-MM-ddTHH:mm:ss}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}";

        if (!comperedTo.Validate(CheckSum))
        {
            throw new ArgumentValidationException("InconsistentData", "اطلاعات موجود در دیتابیس صحیح نمی باشد.");
        }
    }
}