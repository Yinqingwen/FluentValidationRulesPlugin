using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using XamFluentValidationExample.Models;

namespace XamFluentValidationExample.Validators
{
    public class EmailValidator : AbstractValidator<Email>
    {
        public EmailValidator()
        {
            RuleFor(e => e.RecipientEmailAddress)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .EmailAddress();

            RuleFor(e => e.RecipientName)
                .NotEmpty();

            When(e => e.RecipientName != null, () =>
            {
                RuleFor(e => e.RecipientName)
                    .MinimumLength(3).WithMessage("How you bout to enter a FULL 'name' with less than 3 chars!?")
                    .Must(name => name.Contains(" ")).WithMessage("Expecting at least first and last name separated by a space!");
            });
        }
    }
}
