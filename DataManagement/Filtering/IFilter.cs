

namespace IntakeTrackerApp.DataManagement.Filtering;

public interface IFilter<T>
{
	public ObservableItem<bool> Enabled { get; }
	public bool Filter(T val);
	public void AddChangedListener(Action listener);
	public void RemoveChangedListener(Action listener);

}
