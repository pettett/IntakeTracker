
namespace IntakeTrackerApp.Extensions;

public static class ListboxExtensions
{
	public static DependencyProperty IgnoreScrollProperty = DependencyProperty.RegisterAttached("IgnoreScroll", typeof(bool), typeof(ListboxExtensions), new UIPropertyMetadata(false, IgnoreScrollChanged));

	public static bool GetIgnoreScroll(DependencyObject dependencyObject)
	{
		return (bool)dependencyObject.GetValue(IgnoreScrollProperty);
	}

	public static void SetIgnoreScroll(DependencyObject dependencyObject, bool value)
	{
		dependencyObject.SetValue(IgnoreScrollProperty, value);
	}

	private static void IgnoreScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var newValue = (bool)e.NewValue;
		var oldValue = (bool)e.OldValue;

		var frameworkElement = d as FrameworkElement;
		if (frameworkElement == null) return;

		if (!newValue || oldValue || frameworkElement.IsFocused) return;

		var lb = frameworkElement as ListBox;
		if (lb == null) return;

		lb.PreviewMouseWheel += LbOnPreviewMouseWheel;
	}

	private static void LbOnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
	{
		if (!(sender is ListBox) || e.Handled) return;

		e.Handled = true;
		var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
		{
			RoutedEvent = UIElement.MouseWheelEvent,
			Source = sender
		};

		var parent = ((Control)sender).Parent as UIElement;
		if (parent != null) parent.RaiseEvent(eventArg);
	}
}


public static class LiveChartsExtensions
{
	public static SeriesCollection GenerateSeries() => new SeriesCollection();
	public static SectionsCollection GenerateSections() => new SectionsCollection();


	public static SeriesCollection AddColumns<T>(this SeriesCollection s, IEnumerable<T> collection)
	{
		s.AddSeries<ColumnSeries, T>(new ChartValues<T>(collection));
		return s;
	}
	public static SeriesCollection AddLine<T>(this SeriesCollection s, IEnumerable<T> collection, double smoothness = 1)
	{
		s.AddSeries<LineSeries, T>(new ChartValues<T>(collection), out var series);
		series.LineSmoothness = smoothness;

		return s;
	}
	public static SeriesCollection AddStepLine<T>(this SeriesCollection s, IEnumerable<T> collection, string title = "") =>
		AddStepLine(s, new ChartValues<T>(collection), title);
	public static SeriesCollection AddStepLine<T>(this SeriesCollection s, ChartValues<T> collection, string title = "")
	{
		return s.AddSeries<StepLineSeries, T>(collection, title);
	}

	public static SeriesCollection AddSeries<TSeries, TData>(this SeriesCollection s, ChartValues<TData> collection, string title = "")
		where TSeries : Series, new() => AddSeries<TSeries, TData>(s, collection, out _, title);


	public static SeriesCollection AddSeries<TSeries, TData>(this SeriesCollection s, ChartValues<TData> collection, out TSeries series, string title = "")
	where TSeries : Series, new()
	{
		series = new TSeries
		{
			Values = collection,
			Title = title
		};
		s.Add(series);
		return s;
	}


	public static SectionsCollection AddVerticalSeperator(this SectionsCollection s, double value, string name, Brush? stroke = null)
	{

		s.Add(new AxisSection
		{
			Value = value,
			ToolTip = name,
			SectionWidth = 0,
			Stroke = stroke ?? Brushes.Red,
			StrokeThickness = 1,
			StrokeDashArray = new DoubleCollection(new[] { 4d })
		});

		return s;
	}
	public static SectionsCollection AddTodayLine(this SectionsCollection s) => s.AddVerticalSeperator(DateTime.Today.Ticks, "Today");
}