using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Customers.Wallets;
using Application.Query.ViewModels.Wallets;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.BillingAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.Helper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.ReadonlyRepositories;

public class BillingReadOnlyRepository : IBillingReadOnlyRepository
{
    private readonly ReadonlyApplicationDbContext _context;


    public BillingReadOnlyRepository(ReadonlyApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BillsQueryModel> GetCustomerBillsAsync(GetCustomerBillsQuery query, CancellationToken cancellationToken)
    {
        var billQuery = _context.Billings
            .Join(_context.Accounts,
                  bill => bill.FromAccountId,
                  acc => acc.Id,
                  (bill, acc) => new { bill, acc })
            .Join(_context.Wallets,
                  temp => temp.acc.Id,
                  wallet => wallet.AccountId,
                  (temp, wallet) => new { temp.bill, temp.acc, wallet })
            .Join(_context.Plans,
                  temp => temp.wallet.PlanId,
                  pln => pln.Id,
                  (temp, pln) => new { Bill = temp.bill, Account = temp.acc, Wallet = temp.wallet, Plan = pln })
            .Where(temp => temp.Account.BusinessIdentityId == query.CustomerId
            && (!query.TenantId.HasValue || temp.Bill.TenantId == query.TenantId)
                           && (!query.WalletId.HasValue || temp.Wallet.Id == query.WalletId));

        var (currentShamsiMonthGregorianStartDate, currentShamsiMonthGregorianEndDate) = GetFirstlastMiladiDatesOfCurrentShamsiMonth();

        var today = DateTime.Today.Date;

        switch (query.DateType)
        {
            default:
            case BillDateType.None:
                break;
            case BillDateType.CurrentMonth:

                billQuery = billQuery.Where(p => p.Bill.State != BillingState.Overdue && p.Bill.State != BillingState.CompletePaid &&
                   ((p.Bill.EndDate.Date >= currentShamsiMonthGregorianStartDate.Date
                    && p.Bill.EndDate.Date <= currentShamsiMonthGregorianEndDate.Date)
                   || (p.Bill.EndDate.Date.AddDays(p.Bill.GracePeriod) >= currentShamsiMonthGregorianStartDate.Date
                         && p.Bill.EndDate.Date.AddDays(p.Bill.GracePeriod) <= currentShamsiMonthGregorianEndDate.Date)))
                    ;
                break;

            case BillDateType.NextMonths:

                billQuery = billQuery.Where(p => p.Bill.State != BillingState.CompletePaid && p.Bill.EndDate.Date > currentShamsiMonthGregorianEndDate);
                break;

            case BillDateType.Overdued:

                billQuery = billQuery.Where(p => p.Bill.State == BillingState.CompletePaid || p.Bill.EndDate.AddDays(p.Bill.GracePeriod).Date < today);

                break;
        }

        var totalCounts = await billQuery.CountAsync(cancellationToken);

        var result = await billQuery
          .Select(p => new BillQueryModel
          {
              Id = p.Bill.Id,
              BillingPeriodType = p.Plan.BillingPeriodType.GetValueOrDefault(),
              WalletId = p.Wallet.Id,
              PayableAmount = p.Bill.Amount + p.Bill.PreviousDebitAmount + p.Bill.PreviousPenaltyAmount - p.Bill.PreviousCreditAmount - p.Bill.BillingPayments.Sum(p => p.Amount),
              DueDate = p.Bill.EndDate,
              BillingStatus = p.Bill.State,
              WalletName = p.Plan.Title,
              PlanId = p.Wallet.PlanId,
          })
          .OrderBy(x => x.DueDate)
          .Skip((query.PageIndex - 1) * query.PageSize)
          .Take(query.PageSize)
          .ToListAsync(cancellationToken);

        return new BillsQueryModel
        {
            Items = result,
            PageIndex = query.PageIndex,
            PageSize = query.PageSize,
            TotalCount = totalCounts
        };
    }

    public async Task<GetCustomerWalletViewModel> GetLastUnpaidBillOfCurrentMonth(int accountId, CancellationToken cancellationToken)
    {
        var (firstDayGregorian, lastDayGregorian) = GetFirstlastMiladiDatesOfCurrentShamsiMonth();

        var bill = await _context.Billings
                                .Join(_context.Accounts,
                                      bill => bill.FromAccountId,
                                      acc => acc.Id,
                                      (bill, acc) => new { bill, acc })
                                .Join(_context.Wallets,
                                      temp => temp.acc.Id,
                                      wallet => wallet.AccountId,
                                      (temp, wallet) => new { temp.bill, temp.acc, wallet })
                                .Join(_context.Plans,
                                      temp => temp.wallet.PlanId,
                                      pln => pln.Id,
                                      (temp, pln) => new { Bill = temp.bill, Account = temp.acc, Wallet = temp.wallet, Plan = pln })
                                .Where(p => p.Bill.FromAccountId == accountId &&
                                    (p.Bill.State == BillingState.Pending || p.Bill.State == BillingState.PartiallyPaid))
                                .Where(p =>
                                (p.Bill.EndDate.Date >= DateTime.Now && firstDayGregorian.Date <= p.Bill.EndDate.Date && p.Bill.EndDate.Date <= lastDayGregorian.Date)
                                ||
                                (p.Bill.EndDate.AddDays(p.Bill.GracePeriod).Date >= DateTime.Now.Date && firstDayGregorian.Date > p.Bill.EndDate.Date
                                    && firstDayGregorian.Date <= p.Bill.EndDate.AddDays(p.Plan.GracePeriod ?? 0).Date && p.Bill.EndDate.AddDays(p.Plan.GracePeriod ?? 0).Date <= lastDayGregorian.Date)
                                 ||
                                (p.Bill.StartDate.Date <= DateTime.Now && p.Bill.StartDate.Date >= firstDayGregorian.Date
                                    && p.Bill.StartDate.Date <= lastDayGregorian.Date)
                                    )
                                .OrderBy(p => p.Bill.EndDate)
                                .Select(p => p.Bill).FirstOrDefaultAsync(cancellationToken);

        decimal lastBillAmount = 0;

        if (bill is not null)
        {
            lastBillAmount = bill.Amount
           + bill.PreviousDebitAmount
           + bill.PreviousPenaltyAmount
           - bill.PreviousCreditAmount
           - bill.BillingPayments.Sum(x => x.Amount);
        }

        return new GetCustomerWalletViewModel()
        {
            BillStatus = bill?.State,
            LastBillId = bill?.Id,
            LastBillDate = bill?.EndDate,
            LastBillAmount = lastBillAmount
        };
    }
    public async Task<GetCustomerIdAndBillRemainAmountQueryModel> GetCustomerIdAndBillAmountsByIdAsync(int tenantId, long id)
    {
        return await _context.Billings
            .Include(b => b.FromAccount)
            .Include(b => b.BillingPayments)
            .Where(x => x.Id == id && x.TenantId == tenantId)
            .Select(x => new GetCustomerIdAndBillRemainAmountQueryModel
            {
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                FromAccountId = x.FromAccountId,
                CustomerId = x.FromAccount.BusinessIdentityId,
                Amount = x.Amount,
                BillingPayments = x.BillingPayments.ToList(),
                PreviousDebitAmount = x.PreviousDebitAmount,
                PreviousCreditAmount = x.PreviousCreditAmount,
                PreviousPenaltyAmount = x.PreviousPenaltyAmount,
                State = x.State
            })
            .FirstOrDefaultAsync();
    }

    public async Task<GetCustomerBillDetailsQueryModel> GetCustomerBillDetailsAsync(GetCustomerBillDetailsQuery query, CancellationToken cancellationToken)
    {
        var billDetail = await _context.Billings
            .Include(p => p.BillingPayments)
            .Join(_context.BillingInstallments,
            bill => bill.Id,
            bilIns => bilIns.BillingId,
            (bill, billinginstallment) => new
            {
                Bill = bill,
                BillingInstallment = billinginstallment
            }).Join(_context.Installments,
            BillingInstallment => BillingInstallment.BillingInstallment.InstallmentId,
            install => install.Id,
            (billIns, install) => new
            {
                billIns.BillingInstallment,
                billIns.Bill,
                Installment = install
            })
            .Join(_context.Accounts,
                  bill => bill.Bill.FromAccountId,
                  acc => acc.Id,
                  (bill, acc) => new { bill, acc })
            .Join(_context.LoanWallets,
                  temp => temp.acc.Id,
                  wallet => wallet.AccountId,
                  (temp, wallet) => new { temp.bill, temp.acc, wallet })
            .Join(_context.Plans,
                  temp => temp.wallet.PlanId,
                  pln => pln.Id,
                  (temp, pln) => new { temp.bill.Bill, temp.bill.Installment, temp.acc, temp.wallet, pln })
            .Where(p => p.Bill.Id == query.BillId && p.acc.BusinessIdentityId == query.CustomerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (billDetail == null)
            throw new BillingNotFoundException("جزییات صورت حساب پیدا نشد.");

        bool isPayable = false;

        if (billDetail.Bill.SettlementType == WalletSettlementType.Cash)
        {
            var previousBilling = await _context.Billings
            .Where(x => x.Id < query.BillId)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

            if (previousBilling != null)
            {
                var previousBillingPayable = IsPayable(previousBilling);

                if (!previousBillingPayable)
                {
                    isPayable = IsPayable(billDetail.Bill);
                }
            }
            else
            {
                isPayable = IsPayable(billDetail.Bill);
            }
        }

        return new GetCustomerBillDetailsQueryModel
        {
            Id = billDetail.Bill.Id,
            BillStatus = billDetail.Bill.State,
            PlanId = billDetail.pln.Id,
            WalletName = billDetail.pln.Title,
            InstallmentId = billDetail.Installment.Id,
            IsPayable = isPayable,
            PaymentDeadline = billDetail.Bill.EndDate,
            InstallmentAmount = billDetail.Bill.Amount,
            InstallmentsCount = billDetail.wallet.NumberOfInstallment,
            PaidAmount = billDetail.Bill.BillingPayments.Sum(p => p.Amount),
            PayableAmount = billDetail.Bill.Amount + billDetail.Bill.PreviousDebitAmount + billDetail.Bill.PreviousPenaltyAmount - billDetail.Bill.PreviousCreditAmount - billDetail.Bill.BillingPayments.Sum(p => p.Amount),
            PaidDetails = billDetail.Bill.BillingPayments.Select(p => new PaidDetails { Amount = p.Amount, Description = FinancialDocumentType.Billing.GetEnumDescription(), PaymentDateTime = p.PayDate, Id = p.Id }),
            TotalAmount = billDetail.Bill.Amount + billDetail.Bill.PreviousDebitAmount + billDetail.Bill.PreviousPenaltyAmount - billDetail.Bill.PreviousCreditAmount,
            OverDueAmount = billDetail.Bill.PreviousDebitAmount,
            PenaltyAmount = billDetail.Bill.PreviousPenaltyAmount,
            InstallmentNumber = billDetail.Installment.Number.GetValueOrDefault(),
            BillingPeriodType = billDetail.pln.BillingPeriodType,
            BillingPeriod = billDetail.pln.BillingPeriod,
            BillingPeriodText = billDetail.pln.BillingPeriodType != TimeInterval.Day ? billDetail.pln.BillingPeriodType.GetEnumDescription() : billDetail.pln.BillingPeriod + " روز یکبار",
        };

    }

    private bool IsPayable(Billing bill)
    {
        if (bill.State == BillingState.Overdue || bill.State == BillingState.CompletePaid)
        {
            return false;
        }

        var (currentShamsiMonthGeorgianStartDate, currentShamsiMonthGeorgianEndDate) = GetFirstlastMiladiDatesOfCurrentShamsiMonth();

        return
            (bill.StartDate.Date <= DateTime.Now && bill.StartDate.Date >= currentShamsiMonthGeorgianStartDate.Date && bill.StartDate.Date <= currentShamsiMonthGeorgianEndDate.Date)
            ||
            (bill.EndDate.Date >= DateTime.Now && bill.EndDate.Date >= currentShamsiMonthGeorgianStartDate.Date && bill.EndDate.Date <= currentShamsiMonthGeorgianEndDate.Date)
            ||
            (bill.EndDate.AddDays(bill.GracePeriod).Date >= DateTime.Now.Date && bill.EndDate.AddDays(bill.GracePeriod).Date >= currentShamsiMonthGeorgianStartDate.Date && bill.EndDate.AddDays(bill.GracePeriod).Date <= currentShamsiMonthGeorgianEndDate.Date);
    }

    public GetCustomerBillPaymentDetailQueryModel GetCustomerBillPaymentDetailAsync(int customerId, long billId,
        int? tenantId, int paymentId)
    {
        var random = new Random();

        var randomPaymentType = (FinancialDocumentType)random.Next(1, 4);

        var queryModel = new GetCustomerBillPaymentDetailQueryModel
        {
            TotalAmount = 1500.00m,
            PaymentDate = DateTime.UtcNow,
            PaymentType = randomPaymentType
        };

        switch (randomPaymentType)
        {
            case FinancialDocumentType.Purchase:
                queryModel.PurchaseDetail = new PurchaseDetail
                {
                    ReferenceNumber = "REF123456",
                    MerchantName = "Merchant A",
                    Branch = "Branch 1",
                    BranchCode = 1234,
                    CashAmount = 500.00m,
                    CashWithdrawFrom = "ATM",
                    CashTrackingCode = 1111,
                    CreditAmount = 1000.00m,
                    CreditWithdrawFrom = "Credit System",
                    CreditTrackingCode = 2222
                };
                break;

            case FinancialDocumentType.WalletCharge:
                queryModel.WalletChargeDetail = new WalletChargeDetail
                {
                    DepositTo = "Wallet 1",
                    TrackingCode = 3333
                };
                break;

            case FinancialDocumentType.Billing:
                queryModel.BillingDetail = new BillingDetail
                {
                    Wallet = "Wallet 2",
                    WithdrawFrom = "Bank Account 1",
                    TrackingCode = 4444,
                    BillingDueDate = DateTime.UtcNow.AddDays(30)
                };
                break;

            case FinancialDocumentType.OperationalFee:
                queryModel.OperationalFeeDetail = new OperationalFeeDetail
                {
                    Wallet = "Wallet 3",
                    WithdrawFrom = "Service Fee System",
                    TrackingCode = 5555
                };
                break;
        }

        return queryModel;
    }

    #region private methods
    private (DateTime firstDayGregorian, DateTime lastDayGregorian) GetFirstlastMiladiDatesOfCurrentShamsiMonth()
    {
        var now = DateTime.Now;
        var pc = new PersianCalendar();

        // Step 1: Get current Persian date parts
        var year = pc.GetYear(now);
        var month = pc.GetMonth(now);

        // Step 2: First day of Shamsi month
        var firstDayGregorian = pc.ToDateTime(year, month, 1, 0, 0, 0, 0);

        // Step 3: Last day of Shamsi month (days depend on month and leap year)
        var daysInMonth = pc.GetDaysInMonth(year, month);

        var lastDayGregorian = pc.ToDateTime(year, month, daysInMonth, 23, 59, 59, 999);
        return (firstDayGregorian, lastDayGregorian);
    }

    // private int GetDaysInPersianMonth(PersianCalendar pc, int year, int month)
    // {
    //     if (month <= 6) return 31;
    //     if (month <= 11) return 30;
    //     return pc.IsLeapYear(year) ? 30 : 29;
    // }


    #endregion
}