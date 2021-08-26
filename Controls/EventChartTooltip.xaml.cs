

namespace IntakeTrackerApp.Controls
{
	/// <summary>
	/// Interaction logic for EventChartLegend.xaml
	/// </summary>
	public partial class EventChartTooltip : UserControl, IChartTooltip
	{
		private TooltipData? _data;

		public EventChartTooltip()
		{
			InitializeComponent();

			//LiveCharts will inject the tooltip data in the Data property
			//your job is only to display this data as required

			DataContext = this;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public TooltipData Data
		{
			get => _data!;
			set
			{
				_data = value;
				OnPropertyChanged("Data");
			}
		}

		public TooltipSelectionMode? SelectionMode { get; set; } = TooltipSelectionMode.OnlySender;

		protected virtual void OnPropertyChanged(string? propertyName = null)
		{
			if (PropertyChanged != null)
				PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
