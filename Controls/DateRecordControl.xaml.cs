


namespace IntakeTrackerApp.Controls;

/// <summary>
/// Interaction logic for DateRecordControl.xaml
/// </summary>
public partial class DateRecordControl : UserControl, INotifyPropertyChanged
{

    public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(
        "SelectedDate", typeof(DateTime?), typeof(DateRecordControl), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true,
            DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            
        });

    public static readonly DependencyProperty CommentProperty = DependencyProperty.Register(
        "Comment", typeof(string), typeof(DateRecordControl), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true,
            DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });


    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set
        {
            SetValue(SelectedDateProperty, value);
            NotifyPropertyChanged();
        }
    }

    public string Comment
    {
        get => (string)GetValue(CommentProperty);
        set
        {
            SetValue(CommentProperty, value);
            NotifyPropertyChanged();
        }
    }


    public DateRecordControl()
    {
        InitializeComponent();

    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}

