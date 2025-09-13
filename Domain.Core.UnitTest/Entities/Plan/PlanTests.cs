using Domain.Core.Entities.PlanAggregate.Exceptions;
using System;
using Xunit;

namespace Domain.Core.UnitTest.Entities.Plan;

public class PlanTests
{
    [Theory]
    [InlineData(100_000, 0, 100_001, true)]
    [InlineData(100_000, 90_000, 10_001, true)]
    [InlineData(100_000, 0, 100_000, false)]
    [InlineData(100_000, 90_000, 10_000, false)]
    [InlineData(100_000, 0, 50_000, false)]
    [InlineData(100_000, 90_000, 5_000, false)]
    public void ExceedsMaxTotalCredit_ShouldReturnExpectedResult(decimal maxTotalCredit, decimal initialReservedCredit, decimal requestedCredit, bool expectedResult)

    {
        var plan = PlanFaker.CreatePlan(maxTotalCredit: maxTotalCredit);
        plan.SetPrivateProperty(nameof(plan.ReservedCredit), initialReservedCredit);

        Assert.Equal(expectedResult, plan.ExceedsMaxTotalCredit(requestedCredit));
    }

    [Theory]
    [InlineData(100_000, 0, 100_001)]
    [InlineData(100_000, 90_000, 10_001)]
    public void IncreaseReservedCreditAmount_WithInvalidCreditAmount_ThrowException(decimal maxTotalCredit, decimal initialReservedCredit, decimal requestedCredit)
    {
        var plan = PlanFaker.CreatePlan(maxTotalCredit: maxTotalCredit);
        plan.SetPrivateProperty(nameof(plan.ReservedCredit), initialReservedCredit);

        var ex = Assert.Throws<PlanMaxTotalCreditExceededException>(() => plan.IncreaseReservedCreditAmount(requestedCredit));
        Assert.Equal("مبلغ درخواستی بیش از سقف مبلغ تجمیعی طرح است.", ex.Message);
    }

    [Theory]
    [InlineData(100_000, 0, 100_000, 100_000)]
    [InlineData(100_000, 90_000, 10_000, 100_000)]
    [InlineData(100_000, 0, 50_000, 50_000)]
    [InlineData(100_000, 90_000, 5_000, 95_000)]
    public void IncreaseReservedCreditAmount_WithValidCreditAmount_Return(decimal maxTotalCredit, decimal initialReservedCredit, decimal requestedCredit, decimal expectedResult)
    {
        var plan = PlanFaker.CreatePlan(maxTotalCredit: maxTotalCredit);
        plan.SetPrivateProperty(nameof(plan.ReservedCredit), initialReservedCredit);

        plan.IncreaseReservedCreditAmount(requestedCredit);

        Assert.Equal(expectedResult, plan.ReservedCredit);
    }

    [Theory]
    [InlineData(100_000, 0, 1)]
    [InlineData(100_000, 90_000, 90_001)]
    [InlineData(100_000, 90_000, 100_000)]
    public void DecreaseReservedCreditAmount_WithInvalidCreditAmount_ThrowException(decimal maxTotalCredit, decimal initialReservedCredit, decimal requestedCredit)
    {
        var plan = PlanFaker.CreatePlan(maxTotalCredit: maxTotalCredit);
        plan.SetPrivateProperty(nameof(plan.ReservedCredit), initialReservedCredit);

        var ex = Assert.Throws<PlanReservedCreditNegativeException>(() => plan.DecreaseReservedCreditAmount(requestedCredit));
        Assert.Equal("امکان منفی شدن مبلغ اعتبار رزرو شده وجود ندارد.", ex.Message);
    }

    [Theory]
    [InlineData(100_000, 0, 0, 0)]
    [InlineData(100_000, 90_000, 90_000, 0)]
    [InlineData(100_000, 90_000, 50_000, 40_000)]
    public void DecreaseReservedCreditAmount_WithValidCreditAmount_Return(decimal maxTotalCredit, decimal initialReservedCredit, decimal requestedCredit, decimal expectedResult)
    {
        var plan = PlanFaker.CreatePlan(maxTotalCredit: maxTotalCredit);
        plan.SetPrivateProperty(nameof(plan.ReservedCredit), initialReservedCredit);

        plan.DecreaseReservedCreditAmount(requestedCredit);

        Assert.Equal(expectedResult, plan.ReservedCredit);
    }

    [Theory]
    [InlineData("title")]
    [InlineData("طرح جدید")]
    [InlineData("ب")]
    [InlineData("P")]
    public void SetTitle_WithValidValue_Return(string validTitle)
    {
        var plan = PlanFaker.CreatePlan();

        plan.SetTitle(validTitle);

        Assert.Equal(validTitle, plan.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("    ")]
    public void SetTitle_WithInvalidValue_ThrowException(string invalidTitle)
    {
        var plan = PlanFaker.CreatePlan();

        var ex = Assert.Throws<ArgumentException>(() => plan.SetTitle(invalidTitle));
        Assert.Equal("نام طرح وارد نشده است.", ex.Message);
    }
}


#region Faker

public static class PlanFaker
{
    public static Core.Entities.PlanAggregate.Plan CreatePlan(string title = null, decimal? maxTotalCredit = null)
    {
        var plan = new Core.Entities.PlanAggregate.Plan(1, title ?? "plan title", 0, 0, maxTotalCredit ?? 0, Enums.TimeInterval.Day
          , 1, DateTime.Now, 1, Enums.TimeInterval.Day, Enums.TimeInterval.Month, 0, Enums.InstallmentPaymentMethodType.Customer, 0, 0, "", "", "", "", "", "");

        return plan;
    }
}


#endregion

public static class ReflectionExtensions
{

    public static void SetPrivateProperty<T>(this T obj, string propertyName, object value)
    {
        // Get the private property using reflection
        var propertyInfo = typeof(T).GetProperty(propertyName);

        if (propertyInfo == null)
        {
            throw new InvalidOperationException($"Private property '{propertyName}' not found in type '{typeof(T).Name}'.");
        }

        // Set the value of the private property
        propertyInfo.SetValue(obj, value);
    }
}