using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.FluentValidationRules
{
    /// <summary>
    /// Provides a way for an object (pertaining to a property on a specific class) to be validated.
    /// </summary>
    /// <typeparam name="T">Type of the data to be validated</typeparam>
    public class Validatable<T> : ExtendedPropertyChanged, IValidity, IDisposable
    {
        /// <summary>
        /// The name of the property being validated, which must be part of a class and match exactly with the name used there.
        /// </summary>
        public string ClassPropertyName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Validatable{T}"/> class.
        /// </summary>
        public Validatable(string propertyName)
        {
            ClassPropertyName = propertyName;
            _isValid = true;
            _errors = new List<string>();
            _firstError = string.Empty;
        }

        #region Properties

        private List<string> _errors;
        /// <summary>
        /// List of errors across for the <see cref="Validatable{T}"/> object.
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
        /// First or Default error of the main error list. 
        /// </summary>
        public string FirstError
        {
            get => _firstError;
            set => SetProperty(ref _firstError, value);
        }

        private T _value;
        /// <summary>
        /// The current value being assigned to the property.
        /// </summary>
        public T Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private bool _isValid;
        /// <summary>
        /// The value indicating whether the validation succeeded.
        /// </summary>
        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clear the validation Errors and reset IsValid status to true.
        /// </summary>
        /// <param name="onlyValidation">Set to true to clear only the validation result; false to also reset the Value to its Type's default.</param>
        public void Clear(bool onlyValidation = true)
        {
            IsValid = true;
            Errors.Clear();
            FirstError = string.Empty;

            if (!onlyValidation)
                Value = typeof(T) == typeof(string) ? (T)(object)string.Empty : default;
        }

        private void ReleaseManagedResources()
        {
            // Release resources
            ClassPropertyName = null;
            _errors = null;
            _firstError = null;
            _value = default;
        }
        #endregion

        #region IDispoble members

        // Track whether Dispose has been called. 
        private bool disposed = false;
        
        /// <summary>
        /// Clean up.
        /// </summary>
        public void Dispose()
        {
            // If this function is being called the user wants to release the
            // resources. lets call the Dispose which will do this for us.
            Dispose(true);

            // Now since we have done the cleanup already there is nothing left
            // for the Finalizer to do. So lets tell the GC not to call it later.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up.
        /// </summary>
        /// <param name="disposing">True if disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposed)
            {
                if (disposing)
                {
                    // someone want the deterministic release of all resources
                    //Let us release all the managed resources
                    ReleaseManagedResources();
                }
                else
                {
                    // Do nothing, no one asked a dispose, the object went out of
                    // scope and finalized is called so lets next round of GC 
                    // release these resources
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

        #endregion

        /// <summary>
        /// Destructor.
        /// </summary>
        ~Validatable()
        {
            // The object went out of scope and finalized is called
            // Lets call dispose in to release unmanaged resources 
            // the managed resources will anyways be released when GC 
            // runs the next time.
            Dispose(false);
        }
    }
}
