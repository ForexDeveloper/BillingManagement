using System;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Domain.Core.Entities.MerchantInstallmentAggregate.ValueObjects;

public sealed record GroupIdentityInstallments
{
    public int FromBusinessIdentityId { get; set; }

    public int ToBusinessIdentityId { get; set; }

    public int BillingPeriod { get; set; }

    public TimeInterval BillingPeriodType { get; set; }

    public DateTime? DailyBillingOriginDate { get; set; }

    public IEnumerable<MerchantInstallment> Installments { get; set; }
}