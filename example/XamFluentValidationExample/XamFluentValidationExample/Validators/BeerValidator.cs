using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FluentValidation;
using XamFluentValidationExample.Models;

namespace XamFluentValidationExample.Validators
{
    public class BeerValidator : AbstractValidator<Beer>
    {
        public BeerValidator()
        {
            RuleFor(b => b.Name)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .Custom((b, context) =>
                {
                    if (!context.ParentContext.RootContextData.ContainsKey("BadBeers"))
                    {
                        context.AddFailure("You need to supply some bad beers for me to check against.");
                    }
                    else
                    {
                        var badBeers = (context.ParentContext.RootContextData["BadBeers"] as List<string>).Select(badBeer => badBeer.ToLower()).ToList();

                        if (badBeers.Contains(b.ToLower()))
                        {
                            context.AddFailure($"{b} is a bad beer! No dice pal.");
                        }
                    }
                });

            RuleFor(b => b.Manufacturer)
                .NotEmpty();

            RuleFor(b => b.Rating)
                .InclusiveBetween(1, 10).When(b => b != null);
        }
    }
}
