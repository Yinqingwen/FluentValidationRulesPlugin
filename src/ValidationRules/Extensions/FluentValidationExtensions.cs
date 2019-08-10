using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Plugin.FluentValidationRules.Interfaces;

namespace Plugin.FluentValidationRules.Extensions
{
    public static class FluentValidationExtensions
    {
        /// <summary>
        /// Takes a validation result and splits it into individual results for each Validatable object
        /// </summary>
        /// <param name="valResult">The original Fluent ValidationResult from a class instance comprised of the values of the individual Validatable object properties.</param>
        /// <param name="validatableGroup">The Validatable object properties to apply results to.</param>
        /// <returns>An OverallValidationResult summarizing the original results (on the entire instance) as well as any rule results that were not captured by the Validatable objects.</returns>
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
                if (!(obj is IValidity validatableObj))
                    continue;

                validatableObj.IsValid = true;

                var relevantFailures = valResult.Errors
                    .Where(e => e.PropertyName == validatableObj.ClassPropertyName).ToList();

                if (!relevantFailures.Any())
                    continue;
                
                var errors = relevantFailures.Select(e => e.ErrorMessage).ToList();
                validatableObj.Errors = errors;
                validatableObj.IsValid = false;

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
        /// Takes a validation result and applies the rule results relevant to the Validatable object.
        /// </summary>
        /// <param name="valResult">The original Fluent ValidationResult from a class instance that contains the value of the individual Validatable object property.</param>
        /// <param name="validatableObj">The Validatable object property to apply results to.</param>
        /// <returns>An OverallValidationResult summarizing the original results (on the entire instance) as well as any rule results that were not captured by the Validatable object.</returns>
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
        /// Returns the ValidationRule objects from an AbstractValidator that are relevant to the given Validatable object.
        /// </summary>
        /// <typeparam name="T">The Type of the class that the Validatable object property pertains to, and therefore is the target Type for the AbstractValidator.</typeparam>
        /// <param name="validator">The Fluent AbstractValidator instance.</param>
        /// <param name="validatableObj">The Validatable object for which to retrieve ValidationRules for.</param>
        /// <returns>A List of IValidationRule relevant to the Validatable object.</returns>
        public static List<IValidationRule> GetRulesFor<T>(this AbstractValidator<T> validator, IValidity validatableObj)
        {
            var descriptor = validator.CreateDescriptor();
            return descriptor.GetRulesForMember(validatableObj.ClassPropertyName).ToList();
        }

        /// <summary>
        /// Returns the ValidationRule objects from an AbstractValidator that are relevant to the given Validatable objects.
        /// </summary>
        /// <typeparam name="T">The Type of the class that the Validatable object properties pertains to, and therefore is the target Type for the AbstractValidator.</typeparam>
        /// <param name="validator">The Fluent AbstractValidator instance.</param>
        /// <param name="validatableGroup">The Validatable objects for which to retrieve ValidationRules for.</param>
        /// <returns>A List of IValidationRule relevant to the Validatable objects.</returns>
        public static List<IValidationRule> GetRulesFor<T>(this AbstractValidator<T> validator, Validatables validatableGroup)
        {
            var descriptor = validator.CreateDescriptor();

            var rules = new List<IValidationRule>();
            
            foreach (var obj in validatableGroup.Objects)
            {
                if (!(obj is IValidity validatableObj))
                    continue;

                rules.AddRange(descriptor.GetRulesForMember(validatableObj.ClassPropertyName));
            }

            return rules;
        }
    }
}
