using System.Collections.Generic;
using System.Linq;
using Plugin.FluentValidationRules.Extensions;
using Plugin.FluentValidationRules.Interfaces;

namespace Plugin.FluentValidationRules
{
    /// <summary>
    /// Provides a way for a group of <see cref="Validatable{T}"/> to have their valid status and error lists populated.
    /// </summary>
    public class Validatables : ExtendedPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Validatables"/> class.
        /// </summary>
        /// <param name="objects">List of <see cref="Validatable{T}"/> objects to be validated</param>
        public Validatables(params object[] objects)
        {
            _objects = objects;
            _errors = new List<string>();
            _firstError = string.Empty;
        }

        private object[] _objects;
        /// <summary>
        /// The array of Validatable objects that comprise the group.
        /// </summary>
        public object[] Objects
        {
            get => _objects;
            set => SetProperty(ref _objects, value);
        }

        private bool _areValid;
        /// <summary>
        /// The value indicating whether the validation succeeded across all Validatable objects in the group.
        /// </summary>
        public bool AreValid
        {
            get => _areValid;
            set => SetProperty(ref _areValid, value);
        }

        private List<string> _errors;
        /// <summary>
        /// List of errors across all Validatable objects in the group.
        /// </summary>
        public List<string> Errors
        {
            get => _errors;
            set
            {
                FirstError = value?.Count > 0 ? value.FirstOrDefault() : string.Empty;
                SetProperty(ref _errors, value);
            }
        }

        private string _firstError;
        /// <summary>
        /// First or Default error of the main error list for the group.
        /// </summary>
        public string FirstError
        {
            get => _firstError;
            set => SetProperty(ref _firstError, value);
        }

        /// <summary>
        /// Clears all Errors across all Validatable objects (by default), or for a specific Validatable object (if matching property name is supplied), and resets IsValid to true.
        /// </summary>
        /// <param name="classPropertyName">Leave blank to Clear all validation results for each object in the group, otherwise, specify the target ClassPropertyName for the Validatable object to Clear.</param>
        public void Clear(string classPropertyName = "")
        {
            var props = new List<IValidity>();

            foreach (var obj in _objects)
            {
                if (!(obj is IValidity validatableObj))
                    continue;

                props.Add(validatableObj);

                if (string.IsNullOrEmpty(classPropertyName) || validatableObj.ClassPropertyName == classPropertyName)
                    validatableObj.Clear();
            }

            if (string.IsNullOrEmpty(classPropertyName))
            {
                AreValid = true;
                Errors.Clear();
                FirstError = string.Empty;
            }
            else
            {
                Errors = props.SelectMany(p => p.Errors).ToList();
                AreValid = !Errors.Any();
            }
        }
    }
}