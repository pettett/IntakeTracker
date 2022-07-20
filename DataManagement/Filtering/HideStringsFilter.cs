

using IntakeTrackerApp.Controls;
using System.Collections.ObjectModel;

namespace IntakeTrackerApp.DataManagement.Filtering;



public class HideStringsFilter : IFilter<string?>
{

	public ObservableCollection<string> Options { get; set; }
	public ObservableCollection<Checkable<string>> OptionsChecks { get; set; } = new();
	public ObservableItem<bool> Enabled { get; set; } = false;
	public HashSet<string> Hidden { get; set; } = new();
	public string Title { get; set; }

	public event Action? OnFilterChanged;

	private void UpdateChecks()
	{
		OptionsChecks.Clear();
		foreach (var s in Options)
		{
			OptionsChecks.Add(s);
		}

		foreach (var s in OptionsChecks)
		{
			s.PropertyChanged += Option_Checked;
		}
	}

	public HideStringsFilter(ObservableCollection<string> options, string title)
	{
		Options = options;

		// keeping a reference to the original columns allows us to add and remove them,
		// and because it can only be used in one place at a time, will stop duplcate columns
		Title = title;
		Enabled.PropertyChanged += OnEnabled;
		options.CollectionChanged += (_, _) => UpdateChecks();
		UpdateChecks();
	}

	public void OnEnabled(object? sender, PropertyChangedEventArgs args)
	{
		Debug.Print($"Changed {Title}");
	}

	public void Option_Checked(object? sender, PropertyChangedEventArgs e)
	{
		Debug.Print($"Option Checked {sender}");

		if (sender is Checkable<string> s)
		{

			bool changed = s.Checked.Item switch
			{
				true => Hidden.Remove(s.Value),
				false => Hidden.Add(s.Value)
			};


			Debug.Assert(s.Checked.Item == !Hidden.Contains(s.Value), $"{s.Value} is inconsistent");

			if (Enabled && changed)
				OnFilterChanged?.Invoke();
		}
	}
	public bool Filter(string? val)
	{
		Debug.Print(val);
		//True if passes, false if fails
		return Enabled && (val == null || !Hidden.Contains(val)) || !Enabled;
	}

	public void AddChangedListener(Action listener)
	{
		OnFilterChanged += listener;
	}

	public void RemoveChangedListener(Action listener)
	{
		OnFilterChanged += listener;
	}
}
