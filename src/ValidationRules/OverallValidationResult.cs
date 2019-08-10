using System.Collections.Generic;
using System.Linq;
using Plugin.FluentValidationRules.Extensions;

namespace Plugin.FluentValidationRules
{
    /// <summary>
    /// Meant to be used with the ApplyResultsTo extension to store a summary of the overall results from a FluentValidation run as well as those that were not captured by the <see cref="Validatable{T}"/> objects that were used with the extension.
    /// </summary>
    public class OverallValidationResult : ExtendedPropertyChanged
    {
        private bool _isValidOverall;
        /// <summary>
        /// The value indicating whether the validation succeeded across ALL ValidationRules run by the AbstractValidator.
        /// </summary>
        public bool IsValidOverall
        {
            get => _isValidOverall;
            set => SetProperty(ref _isValidOverall, value);
        }

        private bool _isValidForNonSplitErrors;
        /// <summary>
        /// The value indicating whether the validation succeeded across ONLY the ValidationRules run by the AbstractValidator that were NOT captured by the Validatable objects passed in to the ApplyResultsTo extension.
        /// </summary>
        public bool IsValidForNonSplitErrors
        {
            get => _isValidForNonSplitErrors;
            set => SetProperty(ref _isValidForNonSplitErrors, value);
        }

        private List<string> _allErrors = new List<string>();
        /// <summary>
        /// List of errors across ALL ValidationRules run by the AbstractValidator.
        /// </summary>
        public List<string> AllErrors
        {
            get => _allErrors;
            set
            {
                FirstOfAllErrors = value?.Count > 0 ? value.FirstOrDefault() : string.Empty;
                SetProperty(ref _allErrors, value);
            }
        }

        private string _firstOfAllErrors;
        /// <summary>
        /// Only the First of errors across ALL ValidationRules run by the AbstractValidator.
        /// </summary>
        public string FirstOfAllErrors
        {
            get => _firstOfAllErrors;
            set => SetProperty(ref _firstOfAllErrors, value);
        }

        private List<string> _nonSplitErrors = new List<string>();
        /// <summary>
        /// List of errors across ONLY the ValidationRules run by the AbstractValidator that were NOT captured by the Validatable objects passed in to the ApplyResultsTo extension.
        /// </summary>
        public List<string> NonSplitErrors
        {
            get => _nonSplitErrors;
            set
            {
                FirstOfNonSplitErrors = value?.Count > 0 ? value.FirstOrDefault() : string.Empty;
                SetProperty(ref _nonSplitErrors, value);
            }
        }

        private string _firstOfNonSplitErrors;
        /// <summary>
        /// The First of errors across ONLY the ValidationRules run by the AbstractValidator that were NOT captured by the Validatable objects passed in to the ApplyResultsTo extension.
        /// </summary>
        public string FirstOfNonSplitErrors
        {
            get => _firstOfNonSplitErrors;
            set =>  SetProperty(ref _firstOfNonSplitErrors, value);
        }

        /// <summary>
        /// Clear all validation Errors and reset IsValid statuses to true.
        /// </summary>
        public void Clear()
        {
            IsValidOverall = true;
            IsValidForNonSplitErrors = true;
            AllErrors.Clear();
            NonSplitErrors.Clear();
            FirstOfAllErrors = string.Empty;
            FirstOfNonSplitErrors = string.Empty;
        }
    }
}
