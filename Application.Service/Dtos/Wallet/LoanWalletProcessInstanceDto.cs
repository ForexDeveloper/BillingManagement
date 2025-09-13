using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Enums;
using Shared.EventBus.Events;
using System;
using System.Collections.Generic;

namespace Application.Service.Dtos.Wallet;

public class LoanWalletProcessInstanceDto
{
    public int TenantId { get; set; }
    public int CustomerId { get; set; }
    public long UserCreditGrantingProcessId { get; set; }
    public int CreditGrantingProcessId { get; set; }
    public int PlanId { get; set; }
    public decimal InitialCreditAmount { get; set; }
    public decimal OperationalFeeAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public int NumberOfInstallment { get; set; }
    public decimal VerificationFeeAmount { get; set; }
    public Plan Plan { get; set; }
    public Account TenantLoanAccount { get; set; }
    public int WalletContractId { get; set; }
    public int CurrencyTypeId { get; set; }
    public OperationalFeeType OperationalFeeType { get; set; }
    public bool IsDefault { get; set; }
    public WalletSettlementType SettlementType { get; set; }
    public DateTime? SettlementChequeRegistrationDate { get; set; }
    public List<CgmChequeDetailsDto> ChequeDetails { get; set; } = [];
}
