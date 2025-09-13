using Domain.Base;
using Domain.Core.Entities.Shared.Exceptions;
using System;

namespace Domain.Core.AggregateRoots.WalletConfigurationAggregate
{
    [Serializable]
    public class CurrencyType : BaseEntity<int>
    {
        #region Property
        public string Title { get; private set; }
        public int Code { get; set; }

        #endregion #region Property

        private CurrencyType()
        {
        }

        public CurrencyType(int id, string title)
        {
            Id = id;
            SetTitle(title);
        }

        public void SetTitle(string title)
        {
            if (string.IsNullOrEmpty(Title))
                throw new ArgumentValidationException(nameof(title), $"{nameof(title)} is required");

            Title = title;
        }

    }
}