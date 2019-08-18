using Prism.Commands;
using Prism.Navigation;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using XamFluentValidationExample.Models;
using Plugin.FluentValidationRules;
using XamFluentValidationExample.Validators;

namespace XamFluentValidationExample.ViewModels
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class MainPageViewModel : ViewModelBase, IValidate<Beer>, IValidate<Email>
    {
        private Validatables _beerValidatables;
        private Validatables _emailValidatables;
        private AbstractValidator<Beer> _beerValidator;
        private AbstractValidator<Email> _emailValidator;

        private DelegateCommand _validateBeerCommand;
        private DelegateCommand _validateEmailCommand;
        private DelegateCommand<string> _clearValidationCommand;
        private DelegateCommand<string> _clearValidationForGroupCommand;
        private DelegateCommand _clearEverythingCommand;

        public MainPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = "Seeking Validation";

            SetupForValidation();
        }

        public Validatable<string> BeerName { get; set; } = new Validatable<string>(nameof(Beer.Name));
        public Validatable<int?> BeerRating { get; set; } = new Validatable<int?>(nameof(Beer.Rating));
        public string BadBeer1 { get; set; } = "Keystone Light"; // regular prop, no validation
        public string BadBeer2 { get; set; } = "Natural Ice"; // regular prop, no validation
        public int BeerFails { get; set; } = 0;
        public int BeerWins { get; set; } = 0;

        public Validatable<string> RecipientName { get; set; } = new Validatable<string>(nameof(Email.RecipientName));
        public Validatable<string> RecipientEmailAddress { get; set; } = new Validatable<string>(nameof(Email.RecipientEmailAddress));
        public int EmailFails { get; set; } = 0;
        public int EmailWins { get; set; } = 0;

        public DelegateCommand ValidateBeerCommand =>
            _validateBeerCommand ?? (_validateBeerCommand = new DelegateCommand(ExecuteValidateBeerCommand));

        public DelegateCommand ValidateEmailCommand =>
            _validateEmailCommand ?? (_validateEmailCommand = new DelegateCommand(ExecuteValidateEmailCommand));

        public DelegateCommand<string> ClearValidationCommand =>
            _clearValidationCommand ?? (_clearValidationCommand = new DelegateCommand<string>(ClearValidation));

        public DelegateCommand<string> ClearValidationForGroupCommand =>
            _clearValidationForGroupCommand ?? (_clearValidationForGroupCommand = new DelegateCommand<string>(ClearValidationForGroup));
        public DelegateCommand ClearEverythingCommand =>
            _clearEverythingCommand ?? (_clearEverythingCommand = new DelegateCommand(ClearEverything));

        public void SetupForValidation()
        {
            // set validators and prop groups
            _emailValidator = new EmailValidator();
            _emailValidatables = new Validatables(RecipientName, RecipientEmailAddress);

            _beerValidator = new BeerValidator();
            _beerValidatables = new Validatables(BeerName, BeerRating);

            // set some defaults
            BeerName.Value = "Guinness";
            BeerRating.Value = 8;
        }

        public void ExecuteValidateBeerCommand()
        {
            // create a class using your validatables' values the old fashioned way... or use the extension like below in ExecuteValidateEmailCommand
            var beer = new Beer
            {
                Name = BeerName.Value,
                Rating = BeerRating.Value,
            };

            if (Validate(beer).IsValidOverall)
            {
                BeerWins++;
            }
            else
            {
                BeerFails++;
            }
        }

        public void ExecuteValidateEmailCommand()
        {
            var email = _emailValidatables.Populate<Email>();
            // if updating an existing instance instead of creating a new one, just pass it in, e.g. _emailValidatables.Populate(email);
            // in either case, only the matching properties in the class will be updated

            if (Validate(email).IsValidOverall)
            {
                EmailWins++;
            }
            else
            {
                EmailFails++;
            }
        }

        public OverallValidationResult Validate(Email email)
        {
            return _emailValidator.Validate(email).ApplyResultsTo(_emailValidatables);
        }

        public OverallValidationResult Validate(Beer beer)
        {
            var context = new ValidationContext<Beer>(beer);
            context.RootContextData["BadBeers"] = new List<string> {BadBeer1, BadBeer2};

            // totally trivial example of cutting out the rule for "Manufacturer" on the Beer class, since we aren't capturing it here.
            // note if we didn't cut this out, then that rule would fail for our class instance and would be captured in the OverallValidationResult's NonSplitErrors list.
            var scopedRules = _beerValidator.GetRulesFor(_beerValidatables);

            var fluentValidationResult = new ValidationResult(scopedRules.SelectMany(x => x.Validate(context)).ToList());

            return fluentValidationResult.ApplyResultsTo(_beerValidatables);
        }

        public void ClearValidation(string clearOptions = "")
        {
            // this may get you into trouble if you have exact same property name on your different classes - so should probably do a check up-front in reality or have separate methods (like the one below)
            // or better yet, limit your viewmodels to one class to validate...
            _beerValidatables.Clear(clearOptions);

            // could pass whatever you want as string and parse yourself..
            // or could use the default already available, or call main Clear method directly... go wild
            var (clearOnlyValidation, classPropertyNames) = clearOptions.ParseClearOptions();
            _emailValidatables.Clear(clearOnlyValidation, classPropertyNames);
        }

        public void ClearValidationForGroup(string className)
        {
            if (className == "BeerGroup")
            {
                _beerValidatables.Clear();
            }
            else if (className == "EmailGroup")
            {
                _emailValidatables.Clear();
            }
        }

        public void ClearEverything()
        {
            _beerValidatables.Clear(onlyValidation: false);
            _emailValidatables.Clear(false);

            BadBeer1 = string.Empty;
            BadBeer2 = string.Empty;
            BeerWins = 0;
            BeerFails = 0;

            EmailWins = 0;
            EmailFails = 0;
        }
    }
}
