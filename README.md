
<img src="/img/icon.png" />

# Fluent Validation Object Plugin for MVVM Property Validation in Xamarin and Windows
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](pull/new/master) [![Open Source Love png1](https://badges.frapsoft.com/os/v1/open-source.png?v=103)](#contribution) [![licence](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](LICENSE)


This is a small .NET library that helps you use [FluentValidation](https://github.com/JeremySkinner/FluentValidation) in your ViewModels to validate the values of any properties you're binding to your View that you'll be using to create a class. Wanted it to be as lightweight as possible, and not inhibit what you can do with FluentValidation.

This work is based on the approach described here in [David Britch's article](https://devblogs.microsoft.com/xamarin/validation-xamarin-forms-enterprise-apps/) and then the library which Luis Matos packaged up [here](https://github.com/luismts/ValidationRulesPlugin).

Taking that approach but using FluentValidation with it saves a lot of time/code - for example:
- now all Rules are stored neatly in an AbstractValidator per class, and you don't have to write your own rules for very common validations (e-mails, credit card numbers, etc.)
- you can create rules that have a context (e.g. pass in a list of existing values and have a rule that does not allow your property to match any of those), see the [RootContextData documentation here](https://fluentvalidation.net/start#root-context-data)
- and of course the fluent syntax reads well and let's you easily chain rules :)

### Documentation, Working Sample Project, and NuGet Package Coming Soon!

#### Platform Support

|Platform|Version|
| ------------------- | :-----------: |
|.NET Standard|2.0+|

## License :page_with_curl:
The MIT License (MIT) see [License](LICENSE) file.
