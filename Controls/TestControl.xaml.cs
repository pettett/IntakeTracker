
using System.Runtime.CompilerServices;

namespace IntakeTrackerApp.Controls;


/// <summary>
/// Interaction logic for TestControl.xaml
/// </summary>
public partial class TestControl : UserControl, INotifyPropertyChanged, INotifyDataErrorInfo, ITrackable
{
    public static readonly DependencyProperty TestProperty = DependencyProperty.Register(
        "Test", typeof(Test), typeof(TestControl), new FrameworkPropertyMetadata
        {
            DefaultValue = new Test("ERROR", TestType.None),
            BindsTwoWayByDefault = true,
            DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        });


    public Test Test
    {
        get => (Test)GetValue(TestProperty);
        set
        {
            SetValue(TestProperty, value);
            NotifyPropertyChanged();
            Validate();
        }
    }
    public ErrorTracker errorTracker;

    public static DateTime? ToDateTime(DateOnly? date) => date is DateOnly d ? new DateTime(d.Year, d.Month, d.Day) : null;

    //This is the view-model, that responds to changes in the view by altering the model

    //Any changes to the dates requires:
    //Changing field value
    //Alerting UI to property change
    //Validation of data inputted

    public TestControl()
    {
        errorTracker = new(OnErrorsChanged);



        InitializeComponent();

    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Validate()
    {
       // errorTracker.ValidateDateOrder(RequestedDate, nameof(RequestedDate), TestDate, nameof(TestDate));
      //  errorTracker.ValidateDateOrder(TestDate, nameof(TestDate), ReportedDate, nameof(ReportedDate));

       // errorTracker.LogErrors();
    }





    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    public void OnErrorsChanged(object? obj, DataErrorsChangedEventArgs args) => ErrorsChanged?.Invoke(obj, args);
    public IEnumerable GetErrors(string? propertyName) => errorTracker.GetErrors(propertyName);
    public bool HasErrors => errorTracker.HasErrors;

}




