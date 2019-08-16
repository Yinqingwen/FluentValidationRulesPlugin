
# <img src="/img/icon.png" width="42"/> FluentValidation Plugin for Property Validation in Xamarin and Windows MVVM Apps

[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](pull/new/master) [![Open Source Love png1](https://badges.frapsoft.com/os/v1/open-source.png?v=103)](#contribution) [![licence](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](LICENSE)


This is a small .NET library that helps you use [FluentValidation](https://github.com/JeremySkinner/FluentValidation) in your ViewModels to validate the values of any properties you're binding to your View that you'll be using to populate a class instance. Wanted it to be as lightweight as possible, and not inhibit what you can do with FluentValidation. Had to fight the urge to include custom controls to help with displaying validation results, so you can do that in whatever way you please!

This work is based on the approach described here in [David Britch's article](https://devblogs.microsoft.com/xamarin/validation-xamarin-forms-enterprise-apps/) and then the library that Luis Matos packaged up [here](https://github.com/luismts/ValidationRulesPlugin) which this is a fork of.

Taking that approach but using FluentValidation with it saves a lot of time/code - for example:
- now all Rules are stored neatly in an AbstractValidator per class, and you don't have to write your own rules for very common validations (e-mails, credit card numbers, etc.)
- you can create rules that have a context (e.g. pass in a list of existing values and have a rule that does not allow your property to match any of those), see the [RootContextData documentation here](https://fluentvalidation.net/start#root-context-data)
- and of course the fluent syntax reads well and let's you easily chain rules :)

Basically, you are given a few objects and extensions to play with:
| Thing        | That      |
| -------------: | :------------- |
| **Validatable\<T\>**      | let's you create properties in your ViewModel to bind to that hold the **Value** of Type T from the View and the **IsValid** result and corresponding **Errors** from the Validation. Also let's you **Clear()** either just the validation results or both those and the Values themselves. |
| **Validatables**     | let you group numerous Validatable object properties for operating on them all at once for convenience, including an extension **Populate\<T\>** to instantiate and populate a class of type T with the Validatabless Values for each property. |
| **ApplyResultsTo** | is an extension method on FluentValidation's ValidationResult class that takes in either a single Validatable object or a group of Validatables and then populates their individual results, i.e. their IsValid status and Errors if any, as well as returns a summarized **OverallValidationResult** that contains all errors, an overall validity flag, and any errors that were not captured by the Validatables passed in that were still reported by your Validator.     |
| **GetRulesFor** | is an extension method on FluentValidation's AbstractValidator class that takes in either a single Validatable object or a group of Validatables and returns their corresponding **IValidationRules**.    |
| **IValidate\<T\>** | is an interface you may like to throw onto your ViewModel to remind you to **SetupForValidation()** and have a **Validate(T model)** method to actually do the damn thang on a class of type T, as well as **ClearValidation()** results or the underlying values too. |

## Usage

Instead of having regular properties to bind to, you now use **Validatable\<T\>** objects in your ViewModel and you bind to their Value property in the view. You can also bind to the FirstError property on a Validatable object in the View to show an error label for example after validation has been triggered, which you do as you normally would via FluentValidation but then use the extension ApplyResultsTo on the ValidationResult and pass in your Validatables to have that result correctly split among your Validatable properties.

Probably the easiest way to get a feel for what this library is trying to help with is just to look at some of the snippets below as well as the working [sample project](https://github.com/mzhukovs/FluentValidationRulesPlugin/tree/master/example/XamFluentValidationExample) (which uses Xamarin Forms) in the example folder.

<p align="center">
<img src="/img/screenshot.png" />
</p>


#### 1) Add the NuGet to your project
* [Plugin.FluentValidationRules](https://www.nuget.org/packages/Plugin.ValidationRules/) [![NuGet](https://img.shields.io/nuget/v/Plugin.FluentValidationRules.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.FluentValidationRules/)
* Build status: ![Build status](https://img.shields.io/badge/build-succeded-brightgreen.svg)

##### Platform Support

|Platform|Version|
| ------------------- | :-----------: |
|.NET Standard|2.0+|

#### 2) Add the Validatable properties to your ViewModel, these are properties that you'll want input captured for from a user and mapped into a new instance of a class.

Let's assume we have a simple class, Email:
```csharp
    public class Email
    {
        public string RecipientEmailAddress { get; set; }
        public string RecipientName { get; set; }
    }
```

Let's create Validatable objects in our ViewModel that will map to the Email class properties:

Recommend using [PropertyChanged.Fody](https://github.com/Fody/PropertyChanged):
```csharp
    public Validatable<string> RecipientName { get; set; } = new Validatable<string>(nameof(Email.RecipientName));
    public Validatable<string> EmailAddress { get; set; } = new Validatable<string>(nameof(Email.RecipientEmailAddress));
```
:bulb: Thanks to nameof(), it is clear to which class and property the Validatables are intended to map to, and they need not have the same name.

Otherwise:
```csharp
    private Validatable<string> _recipientName;
    private Validatable<string> _emailAddress;

    public YourViewModelConstructor()
    {
        _recipientName = new Validatable<string>(nameof(Email.RecipientName));
        _emailAddress = new Validatable<string>(nameof(Email.RecipientEmailAddress));
    }

    public Validatable<string> RecipientName
    {
        get => _recipientName;
        set => SetProperty(ref _recipientName, value);
    }

    public Validatable<string> EmailAddress
    {
        get => _emailAddress;
        set => SetProperty(ref _emailAddress, value);
    }
```

#### 3) Create a Validator for the Email class as you normally would with FluentValidation

```csharp
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
```

#### 4) Add the IValidate<Email> interface to your ViewModel class (it is telling you what it validates, how neat is that!?), and implement its members

First, the setup method, which you should call from your ViewModel's constructor:
```csharp
    public void SetupForValidation()
    {
        // set validators and prop groups
        _emailValidator = new EmailValidator();
        _emailValidatables = new Validatables(RecipientName, EmailAddress);

        // mayebe even set some defaults
        RecipientName.Value = "Fred Fredovich";
    }
```
:bulb: The Validatables class helps us group our Validatable objects so it's easy to operate on all of them at the same time

Next, the method that will actually do the validation, given an instance of the class:
```csharp
    public OverallValidationResult Validate(Email email)
    {
        return _emailValidator.Validate(email).ApplyResultsTo(_emailValidatables);
    }
```
:bulb: The ApplyResultsTo extension will populate the IsValid and Errors list of each Validatable, as well as return an OverallValidationResult (which you may also bind to in your view if you wish) that summarizes results from the validation as a whole plus any that may not have been captured by the properties you mapped.
:bulb: Here, you may use the GetRulesFor extension method on your AbstractValidator passing in your Validatables to get the rules that only pertain to them, and then execute just those. This is not necessary in this example, but may be in a more complex situation.
:bulb: You may also of course use all that the FluentValidation library has to offer, e.g. setting [RootContextData](https://fluentvalidation.net/start#root-context-data) for use with custom rules that you may have defined on your AbstractValidator.

Finally, you must provide a method that clear's the validation results (and if desirable, the current value stored):
```csharp
    public void ClearValidation(string clearOptions = "")
    {
        _emailValidatables.Clear(clearOptions);
    }
```
:bulb: The options string takes the form **{true/false}|{comma separated classPropertyNames}** to call the Clear(bool, string[]) method, which you may prefer to call directly instead. Otherwise, this is  useful for when passing this information from a View in XAML as a single command parameter.
* Both portions and the pipe are optional, but if both portions are supplied then the pipe separator is required.
* The 1st portion defaults to true if not present, which means only Validation results will be cleared (and not Values). Spaces and capitalization don't matter.
* The 2nd portion defaults to all properties if not present. Otherwise, can supply one, or many with the use of commas. Spaces don't matter. Capitalization must match exactly with the property name on the target class.
* So if no/empty string is passed, then only validation (not underlying Values) will be cleared for all properties.
* For example, these are all valid: "", " False ",  " TRUE |PropName1, PropName2 ,PropName3 ", "PropName1", " ProperName1,PropName2"

#### 5) Add some Commands that your View will use to check/clear the Validation

Note, this example is using the [Prism Library's](https://github.com/PrismLibrary/Prism) DelegateCommand, which you obviously don't have to use:
```csharp
    private DelegateCommand<string> _clearValidationCommand;
    private DelegateCommand _validateEmailCommand;

    public DelegateCommand<string> ClearValidationCommand =>
        _clearValidationCommand ?? (_clearValidationCommand = new DelegateCommand<string>(ClearValidation)); // already defined above in step 4 as part of the interface requirements

    public DelegateCommand ValidateEmailCommand =>
        _validateEmailCommand ?? (_validateEmailCommand = new DelegateCommand(ExecuteValidateEmailCommand));

    public void ExecuteValidateEmailCommand()
    {
        var email = _emailValidatables.Populate<Email>();
        var overallValidationResult = Validate(email); // remember, this will also populate each individual Validatable's IsValid status and Errors list.

        if (overallValidationResult.IsValidOverall)
        {
            // do something with the validated email instance
        }
        else
        {
            // do something else
        }

        if (overallValidationResult.NonSplitErrors.Any())
        {
            // do something with errors that don't pertain to any of our Validatables (which is not possible in our little example here)
        }
    }
```
:bulb: The Populate\<T\> extension will create a new instance of type T and populate its properties with the Values of the Validatable objects in our Validatables group. If you are working with an existing instance, then just set the corresponding properties manually to each Validatable object's Value property.

#### 6) Update your View

This example uses Xamarin Forms XAML, and shows how we would bind to our EmailAddress Validatable in this ongoing example:
```csharp
    <Entry
        Placeholder="Email"
        Text="{Binding EmailAddress.Value}">
        <Entry.Behaviors>
            <!-- Note this behavior is included in the Prism Library -->
            <behaviors:EventToCommandBehavior
                Command="{Binding ClearValidationCommand}"
                CommandParameter="RecipientEmailAddress"
                EventName="Focused" />
        </Entry.Behaviors>
    </Entry>
    <Label
        Style="{StaticResource ErrorLabelStyle}"
        Text="{Binding EmailAddress.FirstError}" />

    <Button
        Command="{Binding ValidateEmailCommand}"
        Text="Validate" />
```
:bulb: This is probably the most common use case - we have
1. an entry to take in our uer's input (only showing 1 instead of both for each property for brevity)
2. a button that will perform the validation
3. a label showing the first of potential many errors under the entry, or none of course if validation succeeded
4. a behavior for clearing the validation error label once the user activates the entry again (presumably to fix the error)

But you could also a button to clear all the validation at once, or even along with the actual values (clear the whole form), etc.

#### And that's it! For a more complete example, refer to the [sample project](https://github.com/mzhukovs/FluentValidationRulesPlugin/tree/master/example/XamFluentValidationExample) in the example folder in this repo. Thank you and hope this is helpful!

## License :page_with_curl:
The MIT License (MIT) see [License](LICENSE) file.
