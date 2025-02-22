using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;


namespace UpdatePiWebConnection.Common
{
    public class ValidationViewModelBase : ViewModelBase, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errorsByPropertyName = new Dictionary<string, List<string>>();

        public bool HasErrors => _errorsByPropertyName.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (_errorsByPropertyName.ContainsKey(propertyName))
                return _errorsByPropertyName[propertyName];

            return Enumerable.Empty<string>();
        }

        public void OnErrorsChanged(string propertyName)
        {
            base.OnPropertyChanged(nameof(HasErrors));

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected void ValidateDataAnnotations(object instance, object value, [CallerMemberName] string propertyName = null)
        {
            var context = new ValidationContext(instance) { MemberName = propertyName };
            var results = new List<ValidationResult>();
            Validator.TryValidateProperty(value, context, results);
            if (results.Any())
            {
                foreach (var result in results)
                    AddError(result.ErrorMessage, propertyName);
            }
            else
            {
                ClearErrors(propertyName);
            }
        }

        protected void AddError(string errorMessage, [CallerMemberName] string propertyName = null)
        {
            if (!_errorsByPropertyName.ContainsKey(propertyName))
                _errorsByPropertyName[propertyName] = new List<string>();

            if (!_errorsByPropertyName[propertyName].Contains(errorMessage))
            {
                _errorsByPropertyName[propertyName].Add(errorMessage);
                OnErrorsChanged(propertyName);
            }
        }

        protected void ClearErrors([CallerMemberName] string propertyName = null)
        {
            if (_errorsByPropertyName.ContainsKey(propertyName))
            {
                _errorsByPropertyName.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }
    }
}
