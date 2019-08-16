using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace Plugin.FluentValidationRules
{
    /// <summary>
    /// Extensions to help work with FluentValidation and <see cref="Validatable{T}"/> objects.
    /// </summary>
    public static class FluentValidationExtensions
    {
        /// <summary>
        /// Takes a validation result and splits it into individual results for each <see cref="Validatable{T}"/> object.
        /// </summary>
        /// <param name="valResult">The original Fluent <see cref="ValidationResult"/> from a class instance comprised of the values of the individual <see cref="Validatable{T}"/> object properties.</param>
        /// <param name="validatableGroup">The <see cref="Validatable{T}"/> object properties to apply results to.</param>
        /// <returns>An <see cref="OverallValidationResult"/> summarizing the original results (on the entire instance) as well as any rule results that were not captured by the <see cref="Validatable{T}"/> objects.</returns>
        public static OverallValidationResult ApplyResultsTo(this ValidationResult valResult, Validatables validatableGroup)
        {
            var overallResult = new OverallValidationResult
            {
                IsValidOverall = valResult.IsValid,
                AllErrors = valResult.Errors.Select(e => e.ErrorMessage).ToList()
            };

            validatableGroup.AreValid = true;

            foreach (var obj in validatableGroup.Objects)
            {
                obj.IsValid = true;

                var relevantFailures = valResult.Errors
                    .Where(e => e.PropertyName == obj.ClassPropertyName).ToList();

                if (!relevantFailures.Any())
                    continue;
                
                var errors = relevantFailures.Select(e => e.ErrorMessage).ToList();
                obj.Errors = errors;
                obj.IsValid = false;

                validatableGroup.Errors.AddRange(errors);

                relevantFailures.ForEach(failure => valResult.Errors.Remove(failure));
            }

            if (validatableGroup.Errors.Any())
                validatableGroup.AreValid = false;

            overallResult.NonSplitErrors = valResult.Errors.Select(e => e.ErrorMessage).ToList();
            overallResult.IsValidForNonSplitErrors = overallResult.NonSplitErrors.Count == 0;

            return overallResult;
        }

        /// <summary>
        /// Takes a validation result and applies the rule results relevant to the <see cref="Validatable{T}"/>object.
        /// </summary>
        /// <param name="valResult">The original Fluent <see cref="ValidationResult"/> from a class instance that contains the value of the individual <see cref="Validatable{T}"/>object property.</param>
        /// <param name="validatableObj">The <see cref="Validatable{T}"/>object property to apply results to.</param>
        /// <returns>An <see cref="OverallValidationResult"/> summarizing the original results (on the entire instance) as well as any rule results that were not captured by the <see cref="Validatable{T}"/>object.</returns>
        public static OverallValidationResult ApplyResultsTo(this ValidationResult valResult, IValidity validatableObj)
        {
            var overallResult = new OverallValidationResult
            {
                IsValidOverall = valResult.IsValid,
                AllErrors = valResult.Errors.Select(e => e.ErrorMessage).ToList()
            };
            overallResult.NonSplitErrors = overallResult.AllErrors;

            validatableObj.IsValid = true;

            var relevantFailures = valResult.Errors
                .Where(e => e.PropertyName == validatableObj.ClassPropertyName).ToList();

            if (!relevantFailures.Any())
                return overallResult;
            
            var errors = relevantFailures.Select(e => e.ErrorMessage).ToList();

            validatableObj.Errors = errors;
            validatableObj.IsValid = false;

            relevantFailures.ForEach(failure => valResult.Errors.Remove(failure));
            overallResult.NonSplitErrors = valResult.Errors.Select(e => e.ErrorMessage).ToList();
            overallResult.IsValidForNonSplitErrors = overallResult.NonSplitErrors.Count == 0;

            return overallResult;
        }

        /// <summary>
        /// Returns the ValidationRule objects from an <see cref="AbstractValidator{T}"/> that are relevant to the given <see cref="Validatable{T}"/>object.
        /// </summary>
        /// <typeparam name="T">The Type of the class that the <see cref="Validatable{T}"/>object property pertains to, and therefore is the target Type for the <see cref="AbstractValidator{T}"/>.</typeparam>
        /// <param name="validator">The Fluent <see cref="AbstractValidator{T}"/> instance.</param>
        /// <param name="validatableObj">The <see cref="Validatable{T}"/>object for which to retrieve ValidationRules for.</param>
        /// <returns>A List of IValidationRule relevant to the <see cref="Validatable{T}"/>object.</returns>
        public static List<IValidationRule> GetRulesFor<T>(this AbstractValidator<T> validator, IValidity validatableObj)
        {
            var descriptor = validator.CreateDescriptor();
            return descriptor.GetRulesForMember(validatableObj.ClassPropertyName).ToList();
        }

        /// <summary>
        /// Returns the ValidationRule objects from an <see cref="AbstractValidator{T}"/> that are relevant to the given <see cref="Validatable{T}"/> objects.
        /// </summary>
        /// <typeparam name="T">The Type of the class that the <see cref="Validatable{T}"/>object properties pertains to, and therefore is the target Type for the <see cref="AbstractValidator{T}"/>.</typeparam>
        /// <param name="validator">The Fluent <see cref="AbstractValidator{T}"/> instance.</param>
        /// <param name="validatableGroup">The <see cref="Validatable{T}"/> objects for which to retrieve ValidationRules for.</param>
        /// <returns>A List of IValidationRule relevant to the <see cref="Validatable{T}"/> objects.</returns>
        public static List<IValidationRule> GetRulesFor<T>(this AbstractValidator<T> validator, Validatables validatableGroup)
        {
            var descriptor = validator.CreateDescriptor();

            var rules = new List<IValidationRule>();
            
            foreach (var obj in validatableGroup.Objects)
            {
                rules.AddRange(descriptor.GetRulesForMember(obj.ClassPropertyName));
            }

            return rules;
        }

        /// <summary>
        /// Populates a new instance of the type T and populates its matching property values with each <see cref="Validatable{T}"/> object in the group of <see cref="Validatables"/>.
        /// </summary>
        /// <typeparam name="T">The type of the class to be instantiated and populated.</typeparam>
        /// <param name="validatableGroup">The group of <see cref="Validatable{T}"/> objects whose Values to use to populate the class instance.</param>
        /// <returns>A newly instantiated class of type T with the property values set based on the <see cref="Validatables"/>.</returns>
        public static T Populate<T>(this Validatables validatableGroup)
            where T : class, new()
        {
            var instance = new T();
            var instanceType = instance.GetType();
            var validatableType = typeof(Validatable<>);

            foreach (var obj in validatableGroup.Objects)
            {
                var prop = instanceType.GetProperty(obj.ClassPropertyName);
                if (prop == null)
                    continue;

                var validatable = validatableType.MakeGenericType(prop.PropertyType);
                var valProp = validatable.GetProperty("Value");

                if (valProp != null)
                    prop.SetValue(instance, valProp.GetValue(obj), null);
            }

            return instance;
        }

        /// <summary>
        /// Takes input as a string of the form {true/false}|{comma separated classPropertyNames} to call the <see cref="Validatables.Clear(bool, string[])"/> method.
        /// Both portions and the pipe are optional, but if both portions are supplied then the pipe separator is required.
        /// The first portion defaults to true if not present, which means only Validation results will be cleared (and not Values).
        /// The 2nd portion defaults to all properties if not present. Otherwise, can supply one, or many with the use of commas.
        /// So if no/empty string is passed, then only validation (not underlying Values) will be cleared for all properties.
        /// Useful for when passing this information from a View in XAML.
        /// </summary>
        /// <param name="clearOptions">The options string to parse.</param>
        /// <returns>A tuple with the bool value for clearOnlyValidation and string[] classPropertyNames (or null).</returns>
        public static (bool clearOnlyValidation, string[] classPropertyNames) ParseClearOptions(this string clearOptions)
        {
            if (string.IsNullOrWhiteSpace(clearOptions))
            {
                throw new ArgumentException("No options to parse out of a null/empty string!");
            }

            List<string> classPropertyNames;

            if (clearOptions.Contains("|"))
            {
                var options = clearOptions.Split('|');

                var clearOnlyValidations = true;
                if (bool.TryParse(options.First(), out bool clearOption))
                {
                    clearOnlyValidations = clearOption;
                }
                else
                {
                    throw new ArgumentException("If pipe separator '|' is present, then string to the left of it must parseable to bool.");
                }

                if (!string.IsNullOrWhiteSpace(options[1]))
                {
                    classPropertyNames = options[1].Split(',').Select(s => s.Trim()).ToList();
                }
                else
                {
                    throw new ArgumentException("If pipe separator '|' is present, then string to the right of it must not be empty.");
                }

                return (clearOnlyValidations, classPropertyNames.ToArray());
            }

            if (bool.TryParse(clearOptions, out bool clearOnlyValidation))
            {
                return (clearOnlyValidation, null);
            }

            classPropertyNames = clearOptions.Split(',').Select(s => s.Trim()).ToList();
            return (true, classPropertyNames.ToArray());
        }
    }
}
