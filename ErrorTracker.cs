using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using IntakeTrackerApp.Data;

namespace IntakeTrackerApp
{

    public abstract class ErrorTrackerViewModelBase : ViewModelBase, INotifyDataErrorInfo
	{
		[JsonIgnore, NotMapped] private readonly Dictionary<string, List<string>> propErrors = new();
		protected void OnErrorsChanged(object? obj, DataErrorsChangedEventArgs args)
		{
			ErrorsChanged?.Invoke(obj, args);
		}

		public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;


		public abstract void ValidateAll();
		public void NotifyPropertyChanged([CallerMemberName] string? propertyName = null, bool validate = false)
		{
			if (validate)
				ValidateAll();
			base.NotifyPropertyChanged(propertyName);
		}
		public override void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			ValidateAll();
			base.NotifyPropertyChanged(propertyName);
		}

		public void AddError(string propertyName, string error, bool enabled)
		{
			if (!propErrors.ContainsKey(propertyName))
				propErrors[propertyName] = new List<string>();

			bool contains = propErrors[propertyName].Contains(error);
			if (!contains && enabled)
			{
				propErrors[propertyName].Add(error);
			}
			else if (contains && !enabled)
			{
				propErrors[propertyName].Remove(error);
			}

			OnErrorsChanged(propertyName);
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

		public bool ValidateNumber(string value, string name)
		{
			bool numeric = value.Length != 0 && ulong.TryParse(value, out _);
			AddError(name, "Must be numeric", !numeric);
			return numeric;

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
