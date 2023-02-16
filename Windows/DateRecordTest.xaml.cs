

using IntakeTrackerApp.DataManagement;

namespace IntakeTrackerApp.Windows;

/// <summary>
/// Interaction logic for DateRecordTest.xaml
/// </summary>
public partial class DateRecordTest : Window
{
	public DateRecord Record { get; set; } = new();

	public DateRecordTest()
	{
		Record.AddListener((s, e) => Debug.WriteLine(e));

		DataContext = this;
		InitializeComponent();
	}
}
