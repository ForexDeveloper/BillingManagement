using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Billings;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.BillingAggregate.Exceptions;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Enums;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetCustomerBillInfoQuery : IRequest<GetBillingRemainAmount>
    {
        public long Id { get; set; }
        public int TenantId { get; set; }
        public int CustomerId { get; set; }

        public GetCustomerBillInfoQuery(long id, int tenantId, int customerId)
        {
            Id = id;
            TenantId = tenantId;
            CustomerId = customerId;
        }
    }

    public class GetCustomerBillInfoQueryHandler : IRequestHandler<GetCustomerBillInfoQuery, GetBillingRemainAmount>
    {
        private readonly IBillingReadOnlyRepository _billingReadOnlyRepository;
        private readonly IBillingRepository _billingRepository;

        public GetCustomerBillInfoQueryHandler(IBillingReadOnlyRepository billingReadOnlyRepository,
            IBillingRepository billingRepository)
        {
            _billingReadOnlyRepository = billingReadOnlyRepository;
            _billingRepository = billingRepository;
        }

        public async Task<GetBillingRemainAmount> Handle(GetCustomerBillInfoQuery request, CancellationToken cancellationToken)
        {
            var billing = await _billingReadOnlyRepository.GetCustomerIdAndBillAmountsByIdAsync(request.TenantId, request.Id) ?? throw new BillingNotFoundException("صورت حساب پیدا نشد.");

            if (billing.CustomerId != request.CustomerId)
            {
                throw new ArgumentValidationException("CustomerId", "صورت حساب مورد نظر به شما تعلق ندارد.");
            }

            if (billing.State == BillingState.CompletePaid)
            {
                throw new ArgumentValidationException("BillingId", "صورت حساب قبلا پرداخت شده است.");
            }

            var billingRemainAmount = billing.Amount
                                      + billing.PreviousDebitAmount
                                      + billing.PreviousPenaltyAmount
                                      - billing.PreviousCreditAmount
                                      - billing.BillingPayments.Sum(x => x.Amount);

            if (billingRemainAmount == 0)
                throw new ArgumentValidationException("BillingId", "صورت حساب قبلا پرداخت شده است.");

            //cutoff time 
            if (DateTime.Today == billing.StartDate.Date)
            {
                TimeSpan start = TimeSpan.Zero;// 00:00 AM
                TimeSpan end = TimeSpan.FromMinutes(15);// 00:15 AM
                TimeSpan timeOfDay = DateTime.Now.TimeOfDay;

                bool isBetween = timeOfDay >= start && timeOfDay <= end;
                if (isBetween)
                {
                    throw new ArgumentValidationException(nameof(request.Id), "امکان پرداخت صورت‌حساب به دلیل به روز رسانی سیستم وجود ندارد، در صورت امکان دقایقی دیگر تلاش نمایید.");
                }
            }

            var isExistPreviousNotCompletePaidBillingAsync = await _billingRepository.IsExistPreviousNotCompletePaidBillingAsync(billing.FromAccountId, request.Id);
            if (isExistPreviousNotCompletePaidBillingAsync)
            {
                throw new ArgumentValidationException(nameof(request.Id), "مشکلی رخ داده است لطفا دقایقی دیگر مجددا تلاش نمایید.");
            }

            if (billing.StartDate > DateTime.Today || DateTime.Today > billing.EndDate)
            {
                var isLastBilling = await _billingRepository.IsLastBilling(billing.FromAccountId, request.Id);
                if (!isLastBilling)
                    throw new ArgumentValidationException("BillingId", "صورت حساب قابل پرداخت نمی باشد.");
            }

            return new GetBillingRemainAmount
            {
                CustomerId = billing.CustomerId,
                RemainBillAmount = billingRemainAmount
            };
        }
    }
}