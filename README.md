
<img src="/img/icon.png" />

# Fluent Validation Object Plugin for MVVM Property Validation in Xamarin and Windows
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](pull/new/master)[![Open Source Love png1](https://badges.frapsoft.com/os/v1/open-source.png?v=103)](#contribution) [![licence](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](LICENSE)


This is a small .NET library that helps you use [FluentValidation](https://github.com/JeremySkinner/FluentValidation) in your ViewModels to validate the values of any properties you're binding to your View that you'll be using to create a class. Wanted it to be as lightweight as possible, and not inhibit what you can do with FluentValidation.

This work is based on the approach described here in [David Britch's article](https://devblogs.microsoft.com/xamarin/validation-xamarin-forms-enterprise-apps/) and then the library which Luis Matos packaged up [here](https://github.com/luismts/ValidationRulesPlugin).

Taking that approach but using FluentValidation with it saves a lot of time/code - for example:
- now all Rules are stored neatly in an AbstractValidator per class, and you don't have to write your own rules for very common validations (e-mails, credit card numbers, etc.)
- you can create rules that have a context (e.g. pass in a list of existing values and have a rule that does not allow your property to match any of those), see the [RootContextData documentation here](https://fluentvalidation.net/start#root-context-data)
- and of course the fluent syntax reads well and let's you easily chain rules :)

Basically, you are given a few objects and extensions to play with:
| Thing        | That      |
| -------------: |:-------------|
| **Validatable\<T\>**      | let's you create properties in your ViewModel to bind to that hold the **Value** of Type T from the View and the **IsValid** result and corresponding **Errors** from the Validation |
| **Validatables**     | let you group numerous Validatable object properties for operating on them all at once for convenience   |
| **ApplyResultsTo** | is an extension method on FluentValidation's ValidationResult class that takes in either a single Validatable object or a group of Validatables and then populates their individual results, i.e. their IsValid status and Errors if any, as well as returns a summarized **OverallValidationResult** that contains all errors, an overall validity flag, and any errors that were not captured by the Validatables passed in that were still reported by your Validator     |
| **GetRulesFor** | is an extension method on FluentValidation's AbstractValidator class that takes in either a single Validatable object or a group of Validatables and returns their corresponding **IValidationRules**    |
| **IValidate\<T\>** | is an interface you may like to throw onto your ViewModel to remind you to **SetupValidation()** and have a **Validate(T model)** method to actually do the damn thang on a class of type T  |

### Examples

```csharp
            FromEmail = new Validatable<string>(nameof(TestClass.FromEmail));
            ToName = new Validatable<string>(nameof(TestClass.ToName));
            Rating = new Validatable<int?>(nameof(TestClass.Rating));
            MessageHtml = new Validatable<string>(nameof(TestClass.MessageHtml));
```


### NuGet
* Available on NuGet: [Plugin.ValidationRules](https://www.nuget.org/packages/Plugin.ValidationRules/) [![NuGet](https://img.shields.io/nuget/v/Plugin.ValidationRules.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.ValidationRules/)
* Build status: ![Build status](https://img.shields.io/badge/build-succeded-brightgreen.svg)

#### Platform Support

|Platform|Version|
| ------------------- | :-----------: |
|.NET Standard|2.0+|

## License :page_with_curl:
The MIT License (MIT) see [License](LICENSE) file.
