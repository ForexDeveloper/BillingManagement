using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Application.Query.ViewModels
{
    public class CurrencyQueryModel
    {
        public int Id { get; set; }
        public string Title { get; set; }

    }
}