using Domain.Core.Enums;
using Domain.Core.Helper;
using FluentValidation;

namespace Application.Command.FileManagerCommands.Validators;

public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    public UploadDocumentCommandValidator()
    {
        RuleFor(c => c.AttachmentCategory).
            IsInEnum().WithMessage("دسته بندی فایل {AttachmentCategory} صحیح نیست.");

        RuleFor(c => c.File)
            .NotNull().WithMessage("انتخاب فایل اجباری است.");

        RuleFor(c => c.File.FileName)
           .Must(BaseValidationHelpers.IsImageFile)
           .WithMessage("فایل انتخابی معتبر نیست.")
           .When(c => c.File != null && c.AttachmentCategory is AttachmentCategory.PlanLogo);
    }
}
