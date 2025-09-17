using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Application.Service.Helper;
using Domain.Core.Enums;
using Infrastructure.Data.Repository.EfCore;
using Domain.Core.Entities.MerchantInstallmentAggregate;

namespace Service.Rest.V1.Controllers;

[ApiVersion("1.0")]
[Route("api/merchantBilling")]
[ApiController]
public class AAAMerchantBillingController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    private readonly IMerchantInstallmentRepository _merchantInstallmentRepository;

    public AAAMerchantBillingController(ApplicationDbContext dbContext, IMerchantInstallmentRepository merchantInstallmentRepository)
    {
        _dbContext = dbContext;
        _merchantInstallmentRepository = merchantInstallmentRepository;
    }

    [HttpPost]
    public async Task<ActionResult> SetMerchantInstallments(long tenantMerchantContractId)
    {
        try
        {

            //var contract = await _dbContext.TenantMerchantContracts.FirstOrDefaultAsync(p => p.Id == tenantMerchantContractId);

            //var period = contract.BillingPeriod;

            //var periodType = contract.BillingPeriodType;

            await _merchantInstallmentRepository.Calculate(CancellationToken.None);

            const int amount = 100000;

            const int part = 7;

            var t = amount / part;

            var t1 = amount % part;

            var depositDate = DateTime.Now;

            var ttt = DateTime.Today;

            var installments = new List<MerchantInstallment>();

            var installmentDates = DateHelper.CalculateMerchantInstallments(depositDate, 24,
                TimeInterval.Day, 29, 4, TimeInterval.Month);

            var installmentDates2 = DateHelper.CalculateInstallments(depositDate, 4, 24, TimeInterval.Day, 29);

            var t2 = JsonSerializer.Serialize(installmentDates);

            var t3 = JsonSerializer.Serialize(installmentDates2);

            foreach (var installmentDate in installmentDates)
            {
                var installment = new MerchantInstallment(tenantId: 2, financialDocumentId: 1,
                    fromBusinessIdentityId: 2, toBusinessIdentityId: 3,
                    tenantMerchantContractId: (int)tenantMerchantContractId, amount: 31, number: 2, dueDate: installmentDate,
                    type: B2bInstallmentType.Installment);

                installments.Add(installment);
            }

            await Task.CompletedTask;

            return Ok(installmentDates);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    [HttpPost("billing")]
    public async Task<IActionResult> SetMerchantBilling()
    {
        await Task.CompletedTask;

        return Ok();
    }
}