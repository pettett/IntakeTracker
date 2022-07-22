using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntakeTrackerApp.DataManagement.Filtering;

public abstract class FilterBase<T> : IFilter<T>
{
	public ObservableItem<bool> Enabled { get; init; } = false;
	public string Title { get; set; }

	protected FilterBase(string title)
	{
		Enabled.PropertyChanged += OnPropertyChanged;
		Title = title;
	}

	private event Action? filterChanged;
	public void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		OnFilterChanged();
	}
	public void OnFilterChanged()
	{
		Debug.Print("Filter changed");
		filterChanged?.Invoke();
	}
	public void AddChangedListener(Action listener)
	{
		filterChanged += listener;
	}

	public void RemoveChangedListener(Action listener)
	{
		filterChanged -= listener;
	}

	public abstract bool Filter(T val);
}
