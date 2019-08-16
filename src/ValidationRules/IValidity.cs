using System.Collections.Generic;

namespace Plugin.FluentValidationRules
{
    /// <summary>
    /// Interface for <see cref="Validatable{T}"/> objects.
    /// </summary>
    public interface IValidity
    {
        /// <summary>
        /// The name of the property on the actual class of type T that the <see cref="Validatable{T}"/> object represents.
        /// </summary>
        string ClassPropertyName { get; set; }

        /// <summary>
        /// True if validation passed, or has not yet been run, or has been reset.
        /// </summary>
        bool IsValid { get; set; }

        /// <summary>
        /// List of any validation errors as a result from a validation run.
        /// </summary>
        List<string> Errors { get; set; }

        /// <summary>
        /// Method to clear validation results (remove <see cref="Errors"/> and set <see cref="IsValid"/> to true), and optionally reset the underlying Value itself too.
        /// </summary>
        /// <param name="onlyValidation">True (default) to reset only the validation results. False to also reset the Value.</param>
        void Clear(bool onlyValidation = true);
    }
}
