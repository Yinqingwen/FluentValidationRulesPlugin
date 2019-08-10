namespace Plugin.FluentValidationRules.Interfaces
{
    /// <summary>
    /// Add this to your ViewModel that contains Validatable Objects that you must SetupForValidation and then run and get results via Validate.
    /// </summary>
    public interface IValidate<T>
    {
        /// <summary>
        /// Provide some setup routine (to be called from ViewModel's constructor) for validation, e.g. set the AbstractValidator(s) that will be used,
        /// set initial Values for any Validatable, and define Validatables (i.e. groups of Validatable objects).
        /// </summary>
        void SetupForValidation();

        /// <summary>
        /// Provide method to run FluentValidation using a Validator and then recommend to ApplyResultsTo Validatables.
        /// </summary>
        OverallValidationResult Validate(T model);

        /// <summary>
        /// Provide some setup routine to Clear the validation Errors and reset IsValid status to true for either all your Validatables or only a specific one that targets a specified classPropertyName
        /// </summary>
        void ClearValidation(string classPropertyName = "");
    }
}
