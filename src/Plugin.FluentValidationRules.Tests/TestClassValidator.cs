using FluentValidation;

namespace Plugin.FluentValidationRules.Tests
{
    public class TestClassValidator : AbstractValidator<TestClass>
    {
        public TestClassValidator()
        {
            RuleFor(t => t.FromEmail)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .EmailAddress();

            RuleFor(t => t.ToName)
                .NotEmpty();

            When(t => t.ToName != null, () =>
            {
                RuleFor(t => t.ToName)
                    .MinimumLength(3).WithMessage("How you bout to enter a FULL 'name' with less than 3 chars!?")
                    .Must(name => name.Contains(" ")).WithMessage("Expecting at least first and last name separated by a space!");
            });

            RuleFor(t => t.MessageHtml)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .MinimumLength(3)
                .Must(cm => cm.Contains("DOCTYPE")).WithMessage("Must include DOCTYPE in HTML Message.");

            RuleFor(t => t.Rating)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .InclusiveBetween(1, 10);
        }
    }
}