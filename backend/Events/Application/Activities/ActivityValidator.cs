using Domain;
using FluentValidation;

namespace Application.Activities
{
    public class ActivityValidator : AbstractValidator<Activity>
    {
        public static readonly string[] AllowedCategories =
            ["drinks", "culture", "film", "food", "music", "travel"];

        public ActivityValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
            RuleFor(x => x.Date)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(d => d.Year is >= 2000 and <= 2100).WithMessage("Date is out of the allowed range");
            RuleFor(x => x.Category)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(c => AllowedCategories.Contains(c)).WithMessage("Unknown category");
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Venue).NotEmpty().MaximumLength(200);
        }
    }
}