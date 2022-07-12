using IntakeTrackerApp.DataManagement;
using IntakeTrackerApp.Extensions;
using IntakeTrackerApp.Windows;
using LiveCharts.Configurations;

namespace IntakeTrackerApp.Controls;


public class Event
{
	public string Name { get; set; }
	public DateRecord Date { get; set; }
	public int Catagory { get; set; }
	public double Progress { get; set; }
	public string CatagoryName => EventCatagoryFormatter(Catagory);

	public static string EventCatagoryFormatter(double value)
	{
		int rounded = (int)value;
		if (rounded == value && rounded < EventCatagories.Count)
			return EventCatagories![rounded];
		return "";
	}

	public static List<string> EventCatagories { get; set; } = new();

	public Event(string name, DateRecord date, int catagory, double progress = 0)
	{
		Name = name;
		Date = date;
		Catagory = catagory;
		Progress = progress;
	}

}

public enum EventCatagory
{
	MRI,
	EP,
	Bloods,
	LP,
	Contact,
	Correspondence,
	Referral
}

public class OrientationConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		if ((int)value < (int)parameter)
			return "Horizontal";
		return "Vertical";
	}
	[DoesNotReturn]
	public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
	{
		throw new("Error");
	}
}

/// <summary>
/// Interaction logic for PatientView.xaml
/// </summary>
public partial class PatientView : UserControl, INotifyPropertyChanged, ITrackable
{
	public PatientReferral Referral
	{
		get => (PatientReferral)GetValue(ReferralProperty);
		set => SetValue(ReferralProperty, value);
	}

	public static readonly DependencyProperty ReferralProperty
		= DependencyProperty.Register("Referral", typeof(PatientReferral), typeof(PatientView),
			new FrameworkPropertyMetadata(new PatientReferral())
			{
				AffectsRender = true,
			});

	// public static readonly RoutedUICommand ArchiveCommand = new("Archive Command", "ArchiveCommand", typeof(PatientView));
	public ChartValues<Event> BloodsValues { get; set; } = new();
	public ChartValues<Event> MRIValues { get; set; } = new();
	public ChartValues<Event> EPValues { get; set; } = new();
	public ChartValues<Event> LPValues { get; set; } = new();
	public ChartValues<Event> ContactValues { get; set; } = new();
	public ChartValues<Event> CorrispondanceValues { get; set; } = new();
	public ChartValues<Event> ReferralValues { get; set; } = new();
	public ChartValues<Event> AppointmentValues { get; set; } = new();


	public SeriesCollection ReferralsEventsCollection { get; set; }


	public static Func<double, string> DateTimeFormatter { get; set; } = value => new DateTime((long)value).ToShortDateString();
	public Func<double, string> EventCatagoryFormatter { get; set; } = Event.EventCatagoryFormatter;

	public Vault MainVault { get; set; }

	private class EventSeriesGroup
	{
		public long Min;
		public long Max;


		public EventGroup AddEventGroup(EventCatagory name, ChartValues<Event> Values, bool clear = true)
		{
			return new EventGroup(this, name, Values, clear);
		}

		public void AddTest(ChartValues<Event> Values, DateRecord begining, Test test, EventCatagory catagory)
		{
			if (test.Needed == true)
				AddEventGroup(catagory, Values).
					AddEvent("Needed", begining, 0).
					AddEvent("Requested", test.RequestedDate, 1.0 / 3.0).
					AddEvent("Test", test.TestDate, 2.0 / 3.0).
					AddEvent("Reported", test.ReportedDate, 1);
			else
				Values.Clear();
		}

	}

	private static EventSeriesGroup GenerateEventSeriesGroup()
	{
		EventSeriesGroup s = new();
		//Update min and max ranges of graph
		//X axis cannot be left to auto update as empty collections cause centering at 0,
		//resulting in dates starting from the year 0
		//Any dates outside the default range will extend it
		s.Min = DateTime.Today.AddDays(-1).Ticks;
		s.Max = DateTime.Today.AddDays(1).Ticks;

		return s;
	}

	private ref struct EventGroup
	{
		public readonly ChartValues<Event> Values;
		public readonly int Catagory;
		public EventSeriesGroup SeriesGroup;

		public EventGroup AddEvent(string name, DateRecord date, double progress = 0, bool enabled = true)
		{
			if (enabled && date.Date is DateTime d && d.Year > 1000)
			{
				Values.Add(new Event(name, date, Catagory, progress * 0.5));
				SeriesGroup.Min = d.Ticks > SeriesGroup.Min ? SeriesGroup.Min : d.Ticks;
				SeriesGroup.Max = d.Ticks < SeriesGroup.Max ? SeriesGroup.Max : d.Ticks;
			}
			return this;
		}
		public EventGroup(EventSeriesGroup seriesGroup, EventCatagory name, ChartValues<Event> values, bool clear = true)
		{
			if (clear)
			{
				values.Clear();
				Catagory = Event.EventCatagories.Count;
				Event.EventCatagories.Add(name.ToString());
			}
			else
			{
				Catagory = Event.EventCatagories.IndexOf(name.ToString());
			}


			Values = values;
			SeriesGroup = seriesGroup;
		}
	}



	public void RegenerateValues()
	{
		Event.EventCatagories.Clear();


		//Fill events

		var g = GenerateEventSeriesGroup();

		g.AddEventGroup(EventCatagory.Referral, ReferralValues).
			 AddEvent("Date On Referral", new(Referral.DateOnReferral), 0).
			 AddEvent("Date Referral Received", new(Referral.DateReferralReceived), .5).
			 AddEvent("Active Management", Referral.DateOfActiveManagement, 1);


		g.AddEventGroup(EventCatagory.Contact, ContactValues).
			AddEvent("Needed", Referral.DateOfActiveManagement, 0).
			AddEvent("Contact Attempted", Referral.ContactAttempted, .5).
			AddEvent("Contact Made", Referral.DateContactMade, 1);

		if (Referral.PreviousCorrespondenceNeeded == true)
			g.AddEventGroup(EventCatagory.Correspondence, CorrispondanceValues).
				AddEvent("Needed", Referral.DateOfActiveManagement, 0).
				AddEvent("Previous Correspondence Requested",
					Referral.PreviousCorrespondenceRequested, .5).
				AddEvent("Previous Correspondence Received",
					Referral.PreviousCorrespondenceReceived, 1);
		else CorrispondanceValues.Clear();



		g.AddTest(MRIValues, Referral.DateOfActiveManagement, Referral.MRI, EventCatagory.MRI);
		g.AddTest(EPValues, Referral.DateOfActiveManagement, Referral.EP, EventCatagory.EP);
		g.AddTest(LPValues, Referral.DateOfActiveManagement, Referral.LP, EventCatagory.LP);
		g.AddTest(BloodsValues, Referral.DateOfActiveManagement, Referral.Bloods, EventCatagory.Bloods);


		g.AddEventGroup(EventCatagory.Referral, AppointmentValues, false).
			AddEvent("Medical Appointment", Referral.MedicalAppointment, enabled: Referral.MedicalAppointmentNeeded == true).
			AddEvent("Nursing Appointment", Referral.NursingAppointment, enabled: Referral.NursingAppointmentNeeded == true);

		XAxis.MinValue = g.Min;
		XAxis.MaxValue = g.Max;
		YAxis.MaxValue = Math.Max(1, Event.EventCatagories.Count);
	}
	public PatientView(PatientReferral referral, Vault v) : this(v)
	{
		Referral = referral;
	}
	public PatientView(Vault v)
	{
		MainVault = v;
		//This determines the order of the keys
		ReferralsEventsCollection = LiveChartsExtensions.GenerateSeries().
			AddStepLine(ReferralValues, "Referral").
			AddStepLine(ContactValues, "Contact").
			AddStepLine(CorrispondanceValues, "Correspondence").
			AddStepLine(MRIValues, "MRI").
			AddStepLine(EPValues, "EP").
			AddStepLine(LPValues, "LP").
			AddStepLine(BloodsValues, "Bloods").
			AddSeries<ScatterSeries, Event>(AppointmentValues, "Appointments");



		//Map event codes to levels on the graph, with progress scores
		var eventMapper = Mappers.Xy<Event>()
			.X(value => value.Date.Date!.Value.Ticks)
			.Y(value => value.Catagory + value.Progress);
		//lets save the mapper globally
		Charting.For<Event>(eventMapper);
		Loaded += new RoutedEventHandler(PatientView_Loaded);



		InitializeComponent();


	}

	private void PatientView_Loaded(object sender, RoutedEventArgs e)
	{

		//Check DataContext Property here - Value is not null

		XAxis.Sections = LiveChartsExtensions.GenerateSections().AddTodayLine();

		RegenerateValues();


		//Create the graph on changes made
		MRI.PropertyChanged += Changed;
		LP.PropertyChanged += Changed;
		EP.PropertyChanged += Changed;

		Referral.PropertyChanged += (e, p) =>
		{
			var n = p.PropertyName?.ToLower();
			if (n == "archived")
			{
				NotifyPropertyChanged("Archived");
			}

			//Always regenerate graph when date changed
			if (n?.Contains("needed") ?? false)
			{
				RegenerateValues();
			}
		};

		PropertyChangedEventHandler r = new((object? e, PropertyChangedEventArgs p) => RegenerateValues());

		//Update graph on changes made
		Referral.PreviousCorrespondenceRequested.PropertyChanged += r;
		Referral.PreviousCorrespondenceReceived.PropertyChanged += r;
		Referral.Bloods.TestDate.PropertyChanged += r;
		Referral.Bloods.ReportedDate.PropertyChanged += r;
		Referral.Bloods.RequestedDate.PropertyChanged += r;

		Referral.MRI.PropertyChanged += r;

		Referral.MRI.TestDate.PropertyChanged += r;
		Referral.MRI.RequestedDate.PropertyChanged += r;
		Referral.MRI.ReportedDate.PropertyChanged += r;

		Referral.EP.PropertyChanged += r;

		Referral.EP.TestDate.PropertyChanged += r;
		Referral.EP.RequestedDate.PropertyChanged += r;
		Referral.EP.ReportedDate.PropertyChanged += r;

		Referral.LP.PropertyChanged += r;

		Referral.LP.TestDate.PropertyChanged += r;
		Referral.LP.RequestedDate.PropertyChanged += r;
		Referral.LP.ReportedDate.PropertyChanged += r;

		Referral.ContactAttempted.PropertyChanged += r;
		Referral.DateContactMade.PropertyChanged += r;

		Referral.MedicalAppointment.PropertyChanged += r;
		Referral.NursingAppointment.PropertyChanged += r;



	}

	public void Changed(object? o, PropertyChangedEventArgs args) => RegenerateValues();



	public event PropertyChangedEventHandler? PropertyChanged;
	// This method is called by the Set accessors of each property.  
	// The CallerMemberName attribute that is applied to the optional propertyName  
	// parameter causes the property name of the caller to be substituted as an argument.  
	public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	private void CreateMailFromTemplateButton_Click(object sender, RoutedEventArgs e)
	{
		Debug.WriteLine("creating mail template");
		LetterTemplateWindow t = new(this);
		t.ShowDialog();
	}

	private void EditPatientDetailsButton_Click(object sender, RoutedEventArgs e)
	{
		var referral = new ReferralDetailsWindow(Referral, MainVault);

		var result = referral.ShowDialog();
		if (result == true)
		{
			referral.PopulateReferral(Referral);

			if (Referral.NHSNumberKey != referral.viewModel.FormatNHSNumber())
			{
				if (MessageBox.Show("Change NHS Number? Save must take place", "Change NHS Number?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
				{
					Data.Context.Remove(Referral);
					VaultViewControl.CloseReferral(Referral);
					Data.Context.SaveChanges();
					Referral.NHSNumberKey = referral.viewModel.FormatNHSNumber();
					Data.Context.Add(Referral);
					VaultViewControl.OpenReferral(Referral);
				}
			}

		}


	}

	public void OnEventClicked(object sender, ChartPoint p)
	{

		//  FocusManager.SetFocusedElement(ReferralStack, CorrespondenceRequested.CommentControl);



		// Debug.WriteLine(((Event)p.Instance).Name);
	}
	public bool Archived
	{
		get => Referral.Archived;
		set
		{
			if (!value || value && MessageBox.Show("Archive Patient", $"Are you sure you want to archive {Referral.Name}", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
			{
				Referral.Archived = value;

				VaultViewControl.Singleton?.view.Refresh();
			}

			NotifyPropertyChanged();
		}
	}




}
