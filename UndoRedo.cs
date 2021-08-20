
using System.Reflection;

namespace IntakeTrackerApp;

public interface ITrackable
{
    void NotifyPropertyChanged([CallerMemberName] string propertyName = "");
}

public class Change
{
    private readonly ITrackable changed;
    private readonly PropertyInfo propertyInfo;
    private readonly object? prevValue;

    public Change(ITrackable changed, PropertyInfo changedProperty, object? prevValue)
    {
        this.changed = changed;
        propertyInfo = changedProperty;
        this.prevValue = prevValue;
    }
    public void Undo()
    {
        UndoRedo.LockUndo = true;
        propertyInfo.SetValue(changed, prevValue);
        changed.NotifyPropertyChanged(propertyInfo.Name);
        UndoRedo.LockUndo = false;
    }
}

public class UndoCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;
    public UndoCommand()
    {
        UndoRedo.CanExecuteChanged += () =>
        {
            CanExecuteChanged?.Invoke(this, new());
        };
    }
    public bool CanExecute(object? parameter) => UndoRedo.CanUndo();

    public void Execute(object? parameter) => UndoRedo.UndoChange();

}

public static class UndoRedo
{
    /// <summary>
    /// Stop changes being recorded in the event undo command triggers logged change
    /// </summary>
    public static bool LockUndo = false;


    public static Stack<Change> changes { get; set; } = new();
    public static void LogChange(ITrackable changed, object? prevValue, [CallerMemberName] string propertyName = "")
    {
        if (LockUndo) return;
        changes.Push(new Change(changed, changed.GetType().GetProperty(propertyName)!, prevValue));
    }

    public static void UndoChange()
    {

        var change = changes.Pop();
        change.Undo();

        CanExecuteChanged?.Invoke();
    }
    public static event Action? CanExecuteChanged;

    public static bool CanUndo() => changes.Count != 0;
}