


using IntakeTrackerApp.DataManagement;

namespace IntakeTrackerApp.Controls;

/// <summary>
/// Interaction logic for DateRecordControl.xaml
/// </summary>
public partial class DateRecordControl : UserControl, INotifyPropertyChanged
{



	public static readonly DependencyProperty DateRecordProperty = DependencyProperty.Register(
		"DateRecord", typeof(DateRecord), typeof(DateRecordControl), new FrameworkPropertyMetadata
		{
			BindsTwoWayByDefault = false,
			DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,

		});


	public DateRecord DateRecord
	{
		get => (DateRecord)GetValue(DateRecordProperty);
		set => SetProperty(DateRecordProperty, value);
	}


	public void SetProperty(DependencyProperty prop, object? value)
	{
		SetValue(prop, value);
		NotifyPropertyChanged();
	}


	public DateRecordControl()
	{
		InitializeComponent();
		ControlGrid.DataContext = this;

	}
	public event PropertyChangedEventHandler? PropertyChanged;
	public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

}

