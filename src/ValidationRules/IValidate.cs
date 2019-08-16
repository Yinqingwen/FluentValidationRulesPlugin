using FluentValidation;

namespace Plugin.FluentValidationRules
{
    /// <summary>
    /// Add this to your ViewModel that contains <see cref="Validatable{T}"/> objects that you must <see cref="SetupForValidation"/> and then run and get results via  <see cref="Validate"/>.
    /// </summary>
    public interface IValidate<T>
    {
        /// <summary>
        /// Provide some setup routine (to be called from ViewModel's constructor) for validation, e.g. set any <see cref="AbstractValidator{T}"/> that will be used,
        /// set initial Values for any <see cref="Validatable{T}"/>, and define any <see cref="Validatables"/> (i.e. groups of <see cref="Validatable{T}"/> objects).
        /// </summary>
        void SetupForValidation();

        /// <summary>
        /// Provide method to run FluentValidation using a <see cref="AbstractValidator{T}"/> and then recommend to <see cref="FluentValidationExtensions.ApplyResultsTo"/> the Validatable(s).
        /// </summary>
        /// <param name="model">The class instance containing the  <see cref="Validatable{T}"/> property Values to validate.</param>
        /// <returns></returns>
        OverallValidationResult Validate(T model);

        /// <summary>
        /// Provide some setup routine to Clear the validation Errors (and even the Values if desired) and reset IsValid status to true for either all your <see cref="Validatable{T}"/> objects or only specific one(s).
        /// </summary>
        void ClearValidation(string clearOptions = "");
    }
}
