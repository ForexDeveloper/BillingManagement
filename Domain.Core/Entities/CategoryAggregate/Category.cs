using Domain.Base;
using Domain.Core.Entities.Merchants;
using Domain.Core.Entities.Shared.Exceptions;
using System;
using System.Collections.Generic;


namespace Domain.Core.AggregateRoots.CategoryAggregate
{
    [Serializable]
    public class Category : BaseEntity<int>
    {
        #region Property
        public int? ParentId { get; set; }
        public string Title { get; set; }
        public virtual Category Parent { get; private set; }
        public virtual ICollection<Category> Categories { get; private set; }
        public List<MerchantCategory> MerchantAndCategories { get; private set; }

        #endregion #region Property

        private Category()
        {
        }

        public Category(int id,string title, int? parentId)
        {
            Id = id;
            SetTitle(title);
            Title = title;
            ParentId = parentId;
        }

        public void SetTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
                throw new ArgumentValidationException(nameof(title), $"{nameof(title)} is required");

            Title = title;
        }


    }
}