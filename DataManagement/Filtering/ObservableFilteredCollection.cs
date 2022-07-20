

using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace IntakeTrackerApp.DataManagement.Filtering;

public record ObservableFilteredCollection<T>(ObservableCollection<IFilter<T>> Filters, ObservableCollection<T> Col, ListCollectionView View)
{


	public ObservableFilteredCollection(ObservableCollection<T> col, params IFilter<T>[] filters)
		: this(new(filters), col, new(col))
	{
		Filters.CollectionChanged += FiltersChanged;
		View.Filter = Filter;
	}
	void FiltersChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems != null)
			foreach (IFilter<T> item in e.NewItems)
			{
				item.AddChangedListener(Refresh);
			}

		if (e.OldItems != null)
			foreach (IFilter<T> item in e.OldItems)
			{
				item.RemoveChangedListener(Refresh);
			}

		Refresh();
	}

	public void Refresh()
	{
		View.Refresh();
	}

	public bool Filter(object? val)
	{
		if (val is T t)
			return Filters.All(f => f.Filter(t));
		else return false;
	}
}
