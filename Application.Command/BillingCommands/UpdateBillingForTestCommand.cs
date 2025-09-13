using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.InstallmentAggregate;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.BillingCommands;

public class UpdateBillingForTestCommand : IRequest
{
    public int WalletId { get; set; }
    public TimeInterval TimeInterval { get; set; }
    public int Time { get; set; }

    public UpdateBillingForTestCommand(int walletId, TimeInterval timeInterval, int time)
    {
        WalletId = walletId;
        TimeInterval = timeInterval;
        Time = time;
    }

    public class UpdateBillingForTestCommandHandler : IRequestHandler<UpdateBillingForTestCommand>
    {
        private readonly IBillingRepository _billingRepository;
        private readonly IInstallmentRepository _installmentRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;

        private UpdateBillingForTestCommand _command;

        public UpdateBillingForTestCommandHandler(
            IWalletRepository walletRepository,
            IApplicationDbContextUnitOfWork unitOfWork,
            IBillingRepository billingRepository, IInstallmentRepository installmentRepository)
        {
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
            _billingRepository = billingRepository;
            _installmentRepository = installmentRepository;
        }

        public async Task Handle(UpdateBillingForTestCommand command, CancellationToken cancellationToken)
        {
            _command = command;

            var wallet = await _walletRepository.GetByIdAsync(_command.WalletId);

            var billings = await _billingRepository.GetBillingsByAccountIdAsync(wallet.AccountId);
            var installments = await _installmentRepository.GetInstallmentsByAccountId(wallet.AccountId);
            PersianCalendar pc = new();

            foreach (var bill in billings)
            {
                DateTime startDate;
                DateTime endDate;

                if (_command.TimeInterval == TimeInterval.Month)
                {
                    startDate = AddPersianMonths(bill.StartDate, _command.Time, pc);
                    endDate = AddPersianMonths(bill.EndDate, _command.Time, pc);
                }
                else if (_command.TimeInterval == TimeInterval.Week)
                {
                    startDate = bill.StartDate.AddDays(_command.Time * 7);
                    endDate = bill.EndDate.AddDays(_command.Time * 7);
                }
                else
                {
                    startDate = bill.StartDate.AddDays(_command.Time);
                    endDate = bill.EndDate.AddDays(_command.Time);
                }

                bill.UpdateDate(startDate, endDate);
            }

            foreach (var installment in installments)
            {
                DateTime startDate;
                DateTime endDate;

                if (_command.TimeInterval == TimeInterval.Month)
                {
                    startDate = AddPersianMonths(installment.StartDate, _command.Time, pc);
                    endDate = AddPersianMonths(installment.DueDate, _command.Time, pc);

                    if (installment.LastPenaltyCalculationDate != null)
                        installment.SetLastPenaltyCalculationDate(
                            AddPersianMonths(installment.LastPenaltyCalculationDate.Value, _command.Time, pc));
                }
                else if (_command.TimeInterval == TimeInterval.Week)
                {
                    startDate = installment.StartDate.AddDays(_command.Time * 7);
                    endDate = installment.DueDate.AddDays(_command.Time * 7);

                    if (installment.LastPenaltyCalculationDate != null)
                        installment.SetLastPenaltyCalculationDate(
                            installment.LastPenaltyCalculationDate.Value.AddDays(_command.Time * 7));
                }
                else
                {
                    startDate = installment.StartDate.AddDays(_command.Time);
                    endDate = installment.DueDate.AddDays(_command.Time);

                    if (installment.LastPenaltyCalculationDate != null)
                        installment.SetLastPenaltyCalculationDate(
                            installment.LastPenaltyCalculationDate.Value.AddDays(_command.Time));
                }


                installment.UpdateDate(startDate, endDate);
            }

            _billingRepository.UpdateRange(billings);
            _installmentRepository.UpdateRange(installments);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public static DateTime AddPersianMonths(DateTime baseDate, int months, PersianCalendar pc)
        {
            int persianYear = pc.GetYear(baseDate);
            int persianMonth = pc.GetMonth(baseDate);
            int persianDay = pc.GetDayOfMonth(baseDate);

            persianMonth += months;
            while (persianMonth > 12)
            {
                persianMonth -= 12;
                persianYear++;
            }
            while (persianMonth < 1)
            {
                persianMonth += 12;
                persianYear--;
            }

            int daysInTargetMonth = pc.GetDaysInMonth(persianYear, persianMonth);
            if (persianDay > daysInTargetMonth)
                persianDay = daysInTargetMonth;

            return pc.ToDateTime(persianYear, persianMonth, persianDay,
                baseDate.Hour, baseDate.Minute, baseDate.Second, baseDate.Millisecond);
        }
    }
}