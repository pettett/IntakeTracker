using System.Runtime.CompilerServices;

namespace IntakeTrackerApp.Controls;


/// <summary>
/// Interaction logic for TodayDateControl.xaml
/// </summary>
public partial class TodayDateControl : UserControl, INotifyPropertyChanged
{
	public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(
		"SelectedDate", typeof(DateTime?), typeof(TodayDateControl), new FrameworkPropertyMetadata
		{
			BindsTwoWayByDefault = true,
			DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
		});


	public DateTime? SelectedDate
	{
		get => GetValue(SelectedDateProperty) as DateTime?;
		set
		{
			SetValue(SelectedDateProperty, value);
			NotifyPropertyChanged();
		}
	}


	public TodayDateControl()
	{
		InitializeComponent();

	}
	public void Today(object sender, EventArgs e)
	{
		if (SelectedDate == null || MessageBox.Show("Doing so could loose data", "Replace date with today?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
		{
			SelectedDate = DateTime.Today;
		}

		DateControl.SelectedDate = SelectedDate;

	}

	public event PropertyChangedEventHandler? PropertyChanged;
	public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

