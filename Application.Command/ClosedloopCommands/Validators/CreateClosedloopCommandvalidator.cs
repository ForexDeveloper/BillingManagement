using Application.Command.Base;
using System.Linq;
using FluentValidation;


namespace Application.Command.ClosedloopCommands.Validators
{
    public class CreateClosedloopCommandvalidator : BaseCommandValidator<CreateClosedloopCommand>
    {
        public CreateClosedloopCommandvalidator()
        {
            RuleFor(x => x.TenantId).GreaterThan(0).WithMessage("انتخاب نام مالک زیر ساخت اجباریست.");

            RuleFor(x => x.WalletConfigurationId).GreaterThan(0).WithMessage("انتخاب اطلاعات پایه کیف پول اجباریست.");

            RuleFor(x => x.Title).NotEmpty().NotNull().WithMessage("وارد نمودن عنوان اجباریست.")
                 .MaximumLength(500).WithMessage("تعداد کارکتر های عنوان بیش از حد مجاز هست.");

            RuleFor(x => x.Merchants)
               .Must(c => c != null && c.Count > 0)
               .WithMessage("انتخاب حداقل یک پذیرنده یا دسته‌بندی پذیرنده‌ها اجباریست.").When(x => x.Categories == null
           || x.Categories.Count == 0);

            RuleFor(x => x.Categories)
              .Must(c => c != null && c.Count > 0)
              .WithMessage("انتخاب  حداقل یک دسته‌بندی پذیرنده‌ها یا پذیرنده‌ها اجباریست.").When(x => x.Merchants == null
          || x.Merchants.Count == 0);

            RuleFor(x => x.Merchants)
              .Must(c => c.GroupBy(c => c).Count() == c.Count)
              .WithMessage("پذیرنده تکراریست.")
              .When(x => x.Merchants != null && x.Merchants.Count > 0);

            RuleFor(x => x.Categories)
              .Must(c => c.GroupBy(c => c).Count() == c.Count)
              .WithMessage("دسته‌بندی پذیرنده‌ها تکراریست.")
              .When(x => x.Categories != null && x.Categories.Count > 0);

            RuleForEach(x => x.Merchants)
             .Must(c => c>0)
             .WithMessage("مقدار صفر برای پذیرنده صحیح نیست.")
             .When(x => x.Merchants != null && x.Merchants.Count > 0);

            RuleForEach(x => x.Categories)
             .Must(c => c > 0)
             .WithMessage("مقدار صفر برای دسته‌بندی پذیرنده‌ها صحیح نیست.")
             .When(x => x.Categories != null && x.Categories.Count > 0);
        }
    }
}
