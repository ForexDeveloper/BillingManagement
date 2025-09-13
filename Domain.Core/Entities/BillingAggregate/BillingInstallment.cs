using Domain.Base;
using Domain.Core.Entities.InstallmentAggregate;

namespace Domain.Core.Entities.BillingAggregate;

public class BillingInstallment : BaseEntity<long>
{
    #region Property
    public long BillingId { get; private set; }
    public Billing Billing { get; private set; }

    public long InstallmentId { get; private set; }
    public Installment Installment { get; private set; }
    public bool IsPrevious { get; private set; }
    #endregion

    private BillingInstallment()
    {

    }

    public BillingInstallment(long installmentId, bool isPrevious = false)
    {
        InstallmentId = installmentId;
        IsPrevious = isPrevious;
    }

    public BillingInstallment(Installment installment, bool isPrevious = false)
    {
        Installment = installment;
        IsPrevious = isPrevious;
    }

    public BillingInstallment(long billingId, long installmentId)
    {
        BillingId = billingId;
        InstallmentId = installmentId;
        IsPrevious = false;
    }
}
