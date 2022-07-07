

namespace IntakeTrackerApp.Controls;

internal class CollapsibleGroupControl : ContentControl
{
	public static KeyValuePair<bool?, string>[] IsNeededOptions { get; set; } =
{
			new KeyValuePair<bool?, string>(null, "Unknown"),
			new KeyValuePair<bool?, string>(false, "Unneeded"),
			new KeyValuePair<bool?, string>(true, "Needed"),
		};

	public static readonly DependencyProperty HeaderProperty =
DependencyProperty.Register("Header", typeof(string),
typeof(CollapsibleGroupControl), new FrameworkPropertyMetadata
{
	BindsTwoWayByDefault = true,
	DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
	PropertyChangedCallback = HeadingChanged,
});

	private static void HeadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((CollapsibleGroupControl)d).Header = (string)e.NewValue;
	}

	public string Header { get; set; } = "";


	public static readonly DependencyProperty EnabledProperty =
DependencyProperty.Register("Enabled", typeof(bool?),
typeof(CollapsibleGroupControl), new FrameworkPropertyMetadata
{
	BindsTwoWayByDefault = true,
	DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
	PropertyChangedCallback = EnabledChanged,
});

	private static void EnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((CollapsibleGroupControl)d).Enabled = (bool?)e.NewValue;
	}

	public bool? Enabled { get; set; } = null;
}
