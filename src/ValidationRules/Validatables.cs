using System;
using System.Collections.Generic;
using System.Linq;

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
        /// <param name="validatables">List of <see cref="Validatable{T}"/> objects to be validated</param>
        public Validatables(params IValidity[] validatables)
        {
            _objects = validatables;
            _errors = new List<string>();
            _firstError = string.Empty;
        }

        private IValidity[] _objects;
        /// <summary>
        /// The array of <see cref="Validatable{T}"/> objects that comprise the group.
        /// </summary>
        public IValidity[] Objects
        {
            get => _objects;
            set => SetProperty(ref _objects, value);
        }

        private bool _areValid;
        /// <summary>
        /// The value indicating whether the validation succeeded across all <see cref="Validatable{T}"/> objects in the group.
        /// </summary>
        public bool AreValid
        {
            get => _areValid;
            set => SetProperty(ref _areValid, value);
        }

        private List<string> _errors;
        /// <summary>
        /// List of errors across all <see cref="Validatable{T}"/> objects in the group.
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
        /// Clears all Errors across all <see cref="Validatable{T}"/> objects (by default), or for specific <see cref="Validatable{T}"/> objects (if matching property names are supplied), and resets IsValid to true. Also may be used to clear the Values themselves.
        /// </summary>
        /// <param name="onlyValidation">Set to true to clear only the validation results; false to also reset the <see cref="Validatable{T}"/> objects' Values to their Type defaults.</param>
        /// <param name="forClassPropertyNames">Leave blank to Clear for each object in the group, otherwise, specify the target ClassPropertyName(s) for the <see cref="Validatable{T}"/>object(s) to Clear.</param>
        public void Clear(bool onlyValidation = true, params string[] forClassPropertyNames)
        {
            var clearAll = forClassPropertyNames == null || !forClassPropertyNames.Any() || forClassPropertyNames.All(string.IsNullOrWhiteSpace);

            foreach (var obj in _objects)
            {
                if (clearAll || forClassPropertyNames.Contains(obj.ClassPropertyName))
                    obj.Clear(onlyValidation);
            }

            if (clearAll)
            {
                AreValid = true;
                Errors.Clear();
                FirstError = string.Empty;
            }
            else
            {
                Errors = _objects.SelectMany(p => p.Errors).ToList();
                AreValid = !Errors.Any();
            }
        }

        /// <summary>
        /// Takes input as a string of the form {true/false}|{comma separated classPropertyNames} to call the <see cref="Clear(bool, string[])"/> method.
        /// Both portions and the pipe are optional, but if both portions are supplied then the pipe separator is required.
        /// The first portion defaults to true if not present, which means only Validation results will be cleared (and not Values).
        /// The 2nd portion defaults to all properties if not present. Otherwise, can supply one, or many with the use of commas.
        /// So if no/empty string is passed, then only validation (not underlying Values) will be cleared for all properties.
        /// Useful for when passing this information from a View in XAML.
        /// </summary>
        /// <param name="clearOptions">The options string to parse to call the main <see cref="Clear(bool, string[])"/> method on the <see cref="Validatable{T}"/> object(s).</param>
        public void Clear(string clearOptions)
        {
            if (string.IsNullOrWhiteSpace(clearOptions))
            {
                Clear();
            }
            else
            {
                var (clearOnlyValidation, classPropertyNames) = clearOptions.ParseClearOptions();
                Clear(clearOnlyValidation, classPropertyNames);
            }
        }
    }
}