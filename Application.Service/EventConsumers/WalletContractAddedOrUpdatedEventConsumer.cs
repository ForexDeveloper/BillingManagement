using Application.Service.Contracts;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.EventBus.Events;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Service.EventConsumers;

public class WalletContractAddedOrUpdatedEventConsumer : IConsumer<FcmWalletContractAddedOrUpdatedEvent>
{
    private readonly IWalletContractRepository _walletContractRepository;
    private readonly IWalletContractService _walletContractService;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;
    public readonly ILogger<FcmWalletContractAddedOrUpdatedEvent> _logger;

    public WalletContractAddedOrUpdatedEventConsumer(ILogger<FcmWalletContractAddedOrUpdatedEvent> logger,
        IApplicationDbContextUnitOfWork unitOfWork,
        IWalletContractRepository walletContractRepository,
        IWalletContractService walletContractService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _walletContractRepository = walletContractRepository;
        _walletContractService = walletContractService;
    }

    public async Task Consume(ConsumeContext<FcmWalletContractAddedOrUpdatedEvent> context)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        bool succeed = true;
        try
        {
            var walletContract = await _walletContractRepository.GetAsync(context.Message.Id);
            if (walletContract == null)
            {
                await CreateWalletContract(context);
                return;
            }

            await UpdateWalletContract(context, walletContract);
        }
        catch (Exception ex)
        {
            succeed = false;
            _logger.LogCritical(new LogStruct
            {
                Message = ex.Message,
                ServiceName = "WalletContractAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = "",
                Exception = ex,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus
            });
            throw;
        }
        finally
        {
            _logger.LogTrace(new LogStruct
            {
                Message = "",
                ServiceName = "WalletContractAddedOrUpdatedEventConsumer_Consume",
                InputParams = context.Message,
                Results = succeed,
                ResponseTimeStopWatcher = stopWatch,
                Tags = LogMessageTag.EventBus,
            });
        }
    }

    private async Task CreateWalletContract(ConsumeContext<FcmWalletContractAddedOrUpdatedEvent> context)
    {
        var contract = new WalletContract(
               context.Message.Id,
               context.Message.TenantId,
               context.Message.StartDate,
               context.Message.EndDate,
               (WalletContractStatus)context.Message.Status,
               context.Message.ParentId,
               context.Message.RootParentId,
               context.Message.TenantIpgSettingId
           );

        _walletContractService.SetWalletContractGuarantor(contract, context.Message.WalletContractGuarantors);
        _walletContractService.SetWalletContractFinancier(contract, context.Message.WalletContractFinanciers);
        _walletContractService.SetWalletContractFacilitators(contract, context.Message.WalletContractFacilitators);
        await _walletContractRepository.AddAsync(contract);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UpdateWalletContract(ConsumeContext<FcmWalletContractAddedOrUpdatedEvent> context, WalletContract contract)
    {
        contract.UpdateWalletContract(context.Message.StartDate, context.Message.EndDate, context.Message.TenantIpgSettingId, context.Message.Status);

        UpdateWalletContractGuarantor(contract, context.Message.WalletContractGuarantors);
        UpdateWalletContractFinancier(contract, context.Message.WalletContractFinanciers);
        UpdateWalletContractFacilitators(contract, context.Message.WalletContractFacilitators);

        _walletContractRepository.Update(contract);
        await _unitOfWork.SaveChangesAsync();
    }

    private void UpdateWalletContractGuarantor(WalletContract contract, List<FcmWalletContractGuarantor> guarantors)
    {
        List<WalletContractGuarantor> inputGuarantorList = [];
        List<WalletContractGuarantor> newGuarantorList = [];
        List<WalletContractGuarantor> oldGuarantorList = contract.WalletContractGuarantors;
        List<WalletContractGuarantor> updatedGuarantorList = [];
        var guarantor = guarantors.First();

        var inputGuarantor = new WalletContractGuarantor(guarantor.Id, contract.Id, guarantor.GuarantorId, guarantor.PortionTypes, guarantor.CommissionCalculationType,
            guarantor.FixedAmountCommission, guarantor.FixedPercentageCommission, guarantor.TransactionMinCommissionAmount,
            guarantor.TransactionMaxCommissionAmount, guarantor.PeriodMinCommissionAmount, guarantor.PeriodMaxCommissionAmount, guarantor.PaymentMethodType);

        if (guarantor.TieredCommissions != null && guarantor.TieredCommissions.Any())
        {
            inputGuarantor.SetTieredCommissions(guarantor.TieredCommissions.Select(x => new Domain.Core.Entities.Shared.TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
        }

        inputGuarantorList.Add(inputGuarantor);

        var existingGuarantorIds = oldGuarantorList.Select(f => (f.GuarantorId)).ToHashSet();

        foreach (var input in inputGuarantorList)
        {
            if (existingGuarantorIds.Contains(input.GuarantorId))
            {
                var guarantorToUpdate = oldGuarantorList.First(f => f.GuarantorId == input.GuarantorId);

                guarantorToUpdate.Update(input.PortionTypes, input.CommissionCalculationType, input.FixedAmountCommission, input.FixedPercentageCommission, input.TransactionMinCommissionAmount,
                    input.TransactionMaxCommissionAmount, input.PeriodMinCommissionAmount, input.PeriodMaxCommissionAmount, input.PaymentMethodType);
                guarantorToUpdate.ClearTieredCommissions();

                if (input.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                    input.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                {
                    guarantorToUpdate.SetTieredCommissions(input.TieredCommissions);
                }
                guarantorToUpdate.SetEditDateTime(DateTime.Now);
            }
            else
            {
                var newGuarantor = new WalletContractGuarantor(guarantor.Id, contract.Id, input.GuarantorId,
                    input.PortionTypes.Select(b => (byte)b).ToList(),
                    (byte)input.CommissionCalculationType,
                    input.FixedAmountCommission, input.FixedPercentageCommission, input.TransactionMinCommissionAmount, input.TransactionMaxCommissionAmount, input.PeriodMinCommissionAmount, input.PeriodMaxCommissionAmount, (byte)input.PaymentMethodType);

                if (input.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                    input.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                {
                    newGuarantor.SetTieredCommissions(input.TieredCommissions);
                }

                newGuarantorList.Add(newGuarantor);
            }
        }

        contract.SetWalletContractGuarantors(newGuarantorList);

        foreach (var oldGuarantor in oldGuarantorList)
        {
            if (!inputGuarantorList.Any(i => i.GuarantorId == oldGuarantor.GuarantorId))
            {
                oldGuarantor.SetDeleted();
                oldGuarantor.SetEditDateTime(DateTime.Now);
            }
        }
    }

    private void UpdateWalletContractFinancier(WalletContract contract, List<FcmWalletContractFinancier> financiers)
    {
        if (financiers == null)
        {
            foreach (var walletContractFinancier in contract.WalletContractFinanciers)
            {
                walletContractFinancier.SetDeleted();
                walletContractFinancier.SetEditDateTime(DateTime.Now);
            }
            return;
        }

        var financier = financiers.First();

        if (contract.WalletContractFinanciers == null)
        {

            List<WalletContractFinancier> financierList = [];

            var newFinancier = new WalletContractFinancier(financier.Id, contract.Id, financier.FinancierId, financier.PortionTypes, financier.CommissionCalculationType, financier.FixedAmountCommission, financier.FixedPercentageCommission,
                financier.TransactionMinCommissionAmount, financier.TransactionMaxCommissionAmount, financier.PeriodMinCommissionAmount, financier.PeriodMaxCommissionAmount, financier.PaymentMethodType);

            if (financier.TieredCommissions != null && financier.TieredCommissions.Any())
            {
                newFinancier.SetTieredCommissions(financier.TieredCommissions
                    .Select(x => new Domain.Core.Entities.Shared.TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
            }

            financierList.Add(newFinancier);
            contract.SetWalletContractFinanciers(financierList);

            return;
        }

        List<WalletContractFinancier> inputFinancierList = [];
        List<WalletContractFinancier> newFinancierList = [];
        List<WalletContractFinancier> oldFinancierList = contract.WalletContractFinanciers;

        var inputFinancier = new WalletContractFinancier(financier.Id, contract.Id, financier.FinancierId, financier.PortionTypes, financier.CommissionCalculationType,
           financier.FixedAmountCommission, financier.FixedPercentageCommission, financier.TransactionMinCommissionAmount,
           financier.TransactionMaxCommissionAmount, financier.PeriodMinCommissionAmount, financier.PeriodMaxCommissionAmount, financier.PaymentMethodType);

        if (financier.TieredCommissions != null && financier.TieredCommissions.Any())
        {
            inputFinancier.SetTieredCommissions(financier.TieredCommissions.Select(x => new Domain.Core.Entities.Shared.TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
        }

        inputFinancierList.Add(inputFinancier);

        var existingFinancierIds = oldFinancierList.Select(f => f.FinancierId).ToHashSet();

        foreach (var input in inputFinancierList)
        {
            if (existingFinancierIds.Contains(input.FinancierId))
            {
                var financierToUpdate = oldFinancierList.First(f => f.FinancierId == input.FinancierId);

                financierToUpdate.Update(input.PortionTypes, input.CommissionCalculationType, input.FixedAmountCommission, input.FixedPercentageCommission,
                    input.TransactionMinCommissionAmount, input.TransactionMaxCommissionAmount, input.PeriodMinCommissionAmount, input.PeriodMaxCommissionAmount, input.PaymentMethodType);

                financierToUpdate.ClearTieredCommissions();

                if (input.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                    input.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                {
                    financierToUpdate.SetTieredCommissions(input.TieredCommissions);
                }
                financierToUpdate.SetEditDateTime(DateTime.Now);
            }
            else
            {
                var newFinancier = new WalletContractFinancier(financier.Id, contract.Id, input.FinancierId,
                    input.PortionTypes.Select(b => (byte)b).ToList(),
                    (byte)input.CommissionCalculationType,
                    input.FixedAmountCommission, input.FixedPercentageCommission, input.TransactionMinCommissionAmount, input.TransactionMaxCommissionAmount,
                    input.PeriodMinCommissionAmount, input.PeriodMaxCommissionAmount, (byte)input.PaymentMethodType);

                if (input.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                   input.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                {
                    newFinancier.SetTieredCommissions(input.TieredCommissions);
                }
                newFinancierList.Add(newFinancier);
            }
        }

        if (newFinancierList.Any())
        {
            contract.SetWalletContractFinanciers(newFinancierList);
        }

        foreach (var oldFinancier in oldFinancierList)
        {
            if (!inputFinancierList.Any(i => i.FinancierId == oldFinancier.FinancierId))
            {
                oldFinancier.SetDeleted();
                oldFinancier.SetEditDateTime(DateTime.Now);
            }
        }
    }

    private void UpdateWalletContractFacilitators(WalletContract contract, List<FcmWalletContractFacilitator> facilitators)
    {
        if (contract == null) throw new ArgumentNullException(nameof(contract));

        if (facilitators == null || !facilitators.Any())
        {
            foreach (var walletContractFacilitator in contract.WalletContractFacilitators)
            {
                walletContractFacilitator.SetDeleted();
                walletContractFacilitator.SetEditDateTime(DateTime.Now);
            }

            return;
        }

        if (contract.WalletContractFacilitators == null)
        {
            List<WalletContractFacilitator> facilitatorList = [];
            foreach (var facilitator in facilitators)
            {
                var newFacilitator = new WalletContractFacilitator(facilitator.Id, contract.Id, facilitator.FacilitatorId, facilitator.PortionTypes,
                    facilitator.CommissionCalculationType, facilitator.FixedAmountCommission, facilitator.FixedPercentageCommission,
                    facilitator.TransactionMinCommissionAmount, facilitator.TransactionMaxCommissionAmount, facilitator.PeriodMinCommissionAmount, facilitator.PeriodMaxCommissionAmount, facilitator.PaymentMethodType);

                if ((CommissionCalculationType)facilitator.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                    (CommissionCalculationType)facilitator.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                {
                    newFacilitator.SetTieredCommissions(facilitator.TieredCommissions.Select(x => new Domain.Core.Entities.Shared.TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
                }

                facilitatorList.Add(newFacilitator);
            }

            contract.SetWalletContractFacilitators(facilitatorList);

            return;
        }

        List<WalletContractFacilitator> inputFacilitatorList = [];
        List<WalletContractFacilitator> newFacilitatorList = [];
        List<WalletContractFacilitator> oldFacilitatorList = contract.WalletContractFacilitators;

        foreach (var facilitator in facilitators)
        {
            var inputFacilitator = new WalletContractFacilitator(facilitator.Id, contract.Id, facilitator.FacilitatorId, facilitator.PortionTypes, facilitator.CommissionCalculationType, facilitator.FixedAmountCommission,
                facilitator.FixedPercentageCommission, facilitator.TransactionMinCommissionAmount, facilitator.TransactionMaxCommissionAmount, facilitator.PeriodMinCommissionAmount, facilitator.PeriodMaxCommissionAmount, facilitator.PaymentMethodType);

            if (facilitator.TieredCommissions != null && facilitator.TieredCommissions.Any())
            {
                inputFacilitator.SetTieredCommissions(facilitator.TieredCommissions.Select(x => new Domain.Core.Entities.Shared.TieredCommission(x.FromAmount, x.ToAmount, x.Percentage, x.MinAmount, x.MaxAmount)).ToList());
            }

            inputFacilitatorList.Add(inputFacilitator);
        }

        var existingFacilitatorIds = oldFacilitatorList.Select(f => f.FacilitatorId).ToHashSet();

        foreach (var input in inputFacilitatorList)
        {
            if (existingFacilitatorIds.Contains(input.FacilitatorId))
            {
                var facilitatorToUpdate = oldFacilitatorList.First(f => f.FacilitatorId == input.FacilitatorId);
                facilitatorToUpdate.Update(input.PortionTypes, input.CommissionCalculationType, input.FixedAmountCommission, input.FixedPercentageCommission,
                    input.TransactionMinCommissionAmount, input.TransactionMaxCommissionAmount, input.PeriodMinCommissionAmount, input.PeriodMaxCommissionAmount, input.PaymentMethodType);

                facilitatorToUpdate.ClearTieredCommissions();

                if (input.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                    input.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                {
                    facilitatorToUpdate.SetTieredCommissions(input.TieredCommissions);
                }
                facilitatorToUpdate.SetEditDateTime(DateTime.Now);
            }
            else
            {
                var newFacilitator = new WalletContractFacilitator(input.Id, contract.Id, input.FacilitatorId,
                    input.PortionTypes.Select(b => (byte)b).ToList(),
                    (byte)input.CommissionCalculationType,
                    input.FixedAmountCommission, input.FixedPercentageCommission, input.TransactionMinCommissionAmount, input.TransactionMaxCommissionAmount,
                    input.PeriodMinCommissionAmount, input.PeriodMaxCommissionAmount, (byte)input.PaymentMethodType);

                if (input.CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                    input.CommissionCalculationType == CommissionCalculationType.CumulativeTiered)
                {
                    newFacilitator.SetTieredCommissions(input.TieredCommissions);
                }

                newFacilitatorList.Add(newFacilitator);
            }
        }

        if (newFacilitatorList.Any())
        {
            contract.SetWalletContractFacilitators(newFacilitatorList);
        }

        foreach (var oldFacilitator in oldFacilitatorList)
        {
            if (!inputFacilitatorList.Any(i => i.FacilitatorId == oldFacilitator.FacilitatorId))
            {
                oldFacilitator.SetDeleted();
                oldFacilitator.SetEditDateTime(DateTime.Now);
            }
        }
    }
}
