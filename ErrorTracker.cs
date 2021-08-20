using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntakeTrackerApp
{
    public class ErrorTracker
    {
        public EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public ErrorTracker(EventHandler<DataErrorsChangedEventArgs> OnErrorsChanged)
        {
            ErrorsChanged = OnErrorsChanged;
        }

        private readonly Dictionary<string, List<string>> propErrors = new();
        public void AddError(string propertyName, string error, bool enabled)
        {
            if (!propErrors.ContainsKey(propertyName))
                propErrors[propertyName] = new List<string>();

            bool contains = propErrors[propertyName].Contains(error);
            if (!contains && enabled)
            {
                propErrors[propertyName].Add(error);
                OnErrorsChanged(propertyName);
            }
            else if (contains && !enabled)
            {
                propErrors[propertyName].Remove(error);
                OnErrorsChanged(propertyName);
            }
        }
        public void OnErrorsChanged(string propertyName)
        {
  
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
 
        }
        public IEnumerable GetErrors(string? propertyName)
        {

            if (propertyName != null && propErrors.TryGetValue(propertyName, out var e))
            {

                return e;
            }

            return new List<string>();

        }
        public void LogErrors()
        {
            foreach (KeyValuePair<string, List<string>> error in propErrors)
            {
                if (error.Value.Count > 0)
                {
                    Debug.WriteLine($"{error.Key} : {string.Join(",", error.Value)}");
                }
            }
        }

        public void ValidatelNumber(string value, string name)
        {


            AddError(name, "Must be number", !(value.Length != 0 && ulong.TryParse(value, out _)));

        }
        public void ValidateDateOrder(DateTime? firstDate, string firstName, DateTime? secondDate, string secondName)
        {
            bool invalid = firstDate is not null && secondDate is not null && DateTime.Compare(firstDate.Value, secondDate.Value) > 0;


            AddError(firstName, $"Should be before {secondName}", invalid);
            AddError(secondName, $"Should be after {firstName}", invalid);
        }
        public void ValidateDateOrder(DateOnly? firstDate, string firstName, DateOnly? secondDate, string secondName)
        {
            bool invalid = firstDate is not null && secondDate is not null && firstDate.Value > secondDate.Value;


            AddError(firstName, $"Should be before {secondName}", invalid);
            AddError(secondName, $"Should be after {firstName}", invalid);
        }


        public void ValidateString(string? value, string name)
        {


            AddError(name, $"Cannot be empty", value is "" or null);

        }
        public void ValidateNotNull<T>(T? n, string name) where T : struct
        {

            AddError(name, $"Cannot be empty", n is null);


        }
        public bool HasErrors => propErrors.Values.FirstOrDefault(r => r.Count > 0) != null;
    }
}
