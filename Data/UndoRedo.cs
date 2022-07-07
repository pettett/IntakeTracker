
using System.Reflection;

namespace IntakeTrackerApp.Data;

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
        propertyInfo.SetValue(changed, prevValue);
        changed.NotifyPropertyChanged(propertyInfo.Name);
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
public class RedoCommand : ICommand
{
    public event EventHandler? CanExecuteChanged;
    public RedoCommand()
    {
        UndoRedo.CanExecuteChanged += () =>
        {
            CanExecuteChanged?.Invoke(this, new());
        };
    }
    public bool CanExecute(object? parameter) => UndoRedo.CanRedo();

    public void Execute(object? parameter) => UndoRedo.RedoChange();

}

public static class UndoRedo
{
    /// <summary>
    /// Stop changes being recorded in the event undo command triggers logged change
    /// </summary>
    private static bool IsUndoing = false;
    private static bool IsRedoing = false;



    public static Stack<Change> changes { get; set; } = new();
    public static Stack<Change> undos { get; set; } = new();
    public static void LogChange(ITrackable changed, object? prevValue, [CallerMemberName] string propertyName = "")
    {
        Change c = new(changed, changed.GetType().GetProperty(propertyName)!, prevValue);
        if (IsUndoing) undos.Push(c);
        else
        {
            changes.Push(c);
            if (!IsRedoing) undos.Clear();
        }

    }

    public static void UndoChange()
    {
        var change = changes.Pop();

        //Mark changes to be stored in redo stack
        IsUndoing = true;

        change.Undo();


        IsUndoing = false;

        CanExecuteChanged?.Invoke();
    }
    public static void RedoChange()
    {
        var change = undos.Pop();

        //Change will be placed on undo stack, and other redos will not be cleared
        IsRedoing = true;
        change.Undo();
        IsRedoing = false;
        CanExecuteChanged?.Invoke();
    }

    public static event Action? CanExecuteChanged;

    public static bool CanUndo() => changes.Count != 0;
    public static bool CanRedo() => undos.Count != 0;
}