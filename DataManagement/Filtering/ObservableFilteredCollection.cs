

using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;

namespace IntakeTrackerApp.DataManagement.Filtering;

public class ObservableFilteredCollection<T>
{
	public ObservableCollection<IFilter<T>> Filters { get; init; }
	public ObservableCollection<T> Col { get; init; }

	public ListCollectionView View { get; init; }
	private readonly ObservableCollection<T> FilteredIn;

	private readonly List<T> FilteredOut;

	readonly object _inLock = new();

	CancellationTokenSource? stopFilter;

	public ObservableFilteredCollection(ObservableCollection<T> col, params IFilter<T>[] filters)
	{

		Filters = new(filters);
		Col = col;
		FilteredIn = new(col);
		BindingOperations.EnableCollectionSynchronization(FilteredIn, _inLock);

		View = new(FilteredIn);
		FilteredOut = new();



		Filters.CollectionChanged += FiltersChanged;
		Col.CollectionChanged += OnCollectionChanged;


		foreach (var filter in filters)
		{
			filter.AddChangedListener(RefreshAsync);
		}
		RefreshAsync();
	}


	void FiltersChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.NewItems != null)
			foreach (IFilter<T> item in e.NewItems)
			{
				item.AddChangedListener(RefreshAsync);
			}

		if (e.OldItems != null)
			foreach (IFilter<T> item in e.OldItems)
			{
				item.RemoveChangedListener(RefreshAsync);
			}

		RefreshAsync();
	}

	void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		// Do this synchronously because its not possible to add more then one at once, so refilting everything every time
		// would be very slow
		switch (e.Action)
		{
			case NotifyCollectionChangedAction.Add:
				foreach (T item in e.NewItems)
				{
					// Filter incoming items
					if (Filter(item))

						FilteredIn.Add(item);
					else
						FilteredOut.Add(item);
				}
				break;
			case NotifyCollectionChangedAction.Remove:
				foreach (T item in e.NewItems)
				{
					FilteredIn.Remove(item);

					FilteredOut.Remove(item);
				}
				break;

		}
	}


	public async void RefreshAsync()
	{
		if (stopFilter != null)
		{
			Debug.Print("Stopping existing filter");
			stopFilter.Cancel();
			stopFilter.Dispose();
		}

		//var p = new Progress<float>();
		//p.ProgressChanged += (s, x) => Debug.Print($"Progress at {x}");
		//IProgress<float> progress = p;

		stopFilter = new CancellationTokenSource();
		var ct = stopFilter.Token;




		try
		{
			await Task.Run(() =>
			{

				ConcurrentBag<T> toRemove = new();

				Parallel.ForEach(FilteredIn, r =>
				{
					if (r != null && !Filter(r))
					{
						toRemove.Add(r);
					}

					ct.ThrowIfCancellationRequested();
				});

				//progress.Report(0.25f);
				while (!toRemove.IsEmpty)
				{
					if (toRemove.TryTake(out var r))
					{
						// These should be done as one atomic unit to stop cancellation
						FilteredOut.Add(r);
						Debug.Assert(FilteredIn.Remove(r));
					}
					ct.ThrowIfCancellationRequested();
				}


				Debug.Assert(toRemove.IsEmpty);

				Parallel.ForEach(FilteredOut, r =>
				{
					if (r != null && Filter(r))
					{
						toRemove.Add(r);
					}
					ct.ThrowIfCancellationRequested();
				});




				while (!toRemove.IsEmpty)
				{
					if (toRemove.TryTake(out var r))
					{
						// These should be done as one atomic unit to stop cancellation
						FilteredIn.Add(r);
						Debug.Assert(FilteredOut.Remove(r));
					}
					ct.ThrowIfCancellationRequested();
				}

			}, ct);


			Debug.Print($"--- Filter completed {FilteredIn.Count} {FilteredOut.Count}");
		}
		catch (OperationCanceledException e)
		{
			Debug.Print("Filter cancelled");

			// Canceled
		}
		finally
		{
			Debug.Print($"--- Filter over {FilteredIn.Count} {FilteredOut.Count}");
			stopFilter = null;
		}

	}

	public bool Filter(T val)
	{

		for (int i = 0; i < Filters.Count; i++)
			if (Filters[i].Enabled && !Filters[i].Filter(val))
				return false;
		return true;

	}
}
