using IntakeTrackerApp.DataManagement;
using IntakeTrackerApp.Extensions;
using IntakeTrackerApp.Windows;
using nGantt;
using nGantt.GanttChart;
using nGantt.PeriodSplitter;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Controls;

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
	public PatientReferral Referral { get; set; }

	// public static readonly RoutedUICommand ArchiveCommand = new("Archive Command", "ArchiveCommand", typeof(PatientView));


	public ObservableCollection<string> Notifications { get; set; } = new();


	public static Func<double, string> DateTimeFormatter { get; set; } = value => new DateTime((long)value).ToShortDateString();
	public Func<double, string> EventCatagoryFormatter { get; set; } = Event.EventCatagoryFormatter;

	public Vault MainVault { get; set; }









	public void RegenerateValues()
	{
		Event.EventCatagories.Clear();


		//Fill events



		//XAxis.MinValue = g.Min;
		//XAxis.MaxValue = g.Max;
		//YAxis.MaxValue = Math.Max(1, Event.EventCatagories.Count);
	}
	public PatientView(PatientReferral referral, Vault v)
	{
		Referral = referral;

		MainVault = v;
		//This determines the order of the keys


		Loaded += PatientView_Loaded;
		void TestChanged(object? sender, PropertyChangedEventArgs? e)
		{
			void AddSummary(string? summary) { if (summary != null) Notifications.Add(summary); }
			Notifications.Clear();
			AddSummary(Referral.MRISummary);
			AddSummary(Referral.EPSummary);
			AddSummary(Referral.LPSummary);
			AddSummary(Referral.BloodsSummary);

			if (sender != null)
			{
				ClearGantt();

				CreateData();
			}
		}

		Referral.MRI.AddListener(TestChanged);
		Referral.EP.AddListener(TestChanged);
		Referral.LP.AddListener(TestChanged);
		Referral.Bloods.AddListener(TestChanged);

		TestChanged(null, null);

		Debug.WriteLine(Referral.Name);
		Debug.WriteLine(Notifications);

		DataContext = this;


		InitializeComponent();



		CreateData();
	}
	private void ClearGantt()
	{
		ganttControl.ClearGantt();
	}

	private void CreateData()
	{
		DateTime mostMinDate = DateTime.Today.AddYears(-100);


		DateTime minDate = DateTime.Compare(Referral.DateReferralReceived, mostMinDate) > 0 ? Referral.DateReferralReceived : mostMinDate;
		DateTime maxDate = DateTime.Today;

		Debug.WriteLine(minDate.ToShortDateString(), maxDate.ToShortDateString());

		//Set max and min dates
		ganttControl.Initialize(minDate, maxDate);

		// Create timelines and define how they should be presented
		ganttControl.CreateTimeLine(new PeriodYearSplitter(minDate, maxDate), FormatYear);
		var gridLineTimeLine = ganttControl.CreateTimeLine(new PeriodMonthSplitter(minDate, maxDate), FormatMonth);



		// Set the timeline to atatch gridlines to
		//ganttControl.SetGridLinesTimeline(gridLineTimeLine, DetermineBackground);

		// Create and data

		var rowgroup = ganttControl.CreateGanttRowGroup("Tests");



		AddTestToGantt(rowgroup, Referral.MRI);
		AddTestToGantt(rowgroup, Referral.LP);
		AddTestToGantt(rowgroup, Referral.EP);
		AddTestToGantt(rowgroup, Referral.Bloods);


	}



	void AddTestToGantt(GanttRowGroup rowgroup, Test test)
	{
		if (test.Needed is true)
		{
			var testRow = ganttControl.CreateGanttRow(rowgroup, test.Name);
			if (test.RequestedDate.Date is DateTime req)

				ganttControl.AddGanttTask(testRow, new GanttTask()
				{
					Start = req,
					End = test.TestDate.Date ?? DateTime.Today,
					Name = $"Awaiting {test.Name} Test"
				});

			if (test.TestDate.Date is DateTime testDate)

				ganttControl.AddGanttTask(testRow, new GanttTask()
				{
					Start = testDate,
					End = test.ReportedDate.Date ?? DateTime.Today,
					Name = $"Awaiting {test.Name} Results"
				});
		}
	}

	private string FormatYear(Period period)
	{
		return period.Start.Year.ToString();
	}

	private string FormatMonth(Period period)
	{
		return period.Start.Month.ToString();
	}

	private void PatientView_Loaded(object sender, RoutedEventArgs e)
	{

		//Check DataContext Property here - Value is not null

		//	XAxis.Sections = LiveChartsExtensions.GenerateSections().AddTodayLine();

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

	public bool Archived
	{
		get => Referral.Archived;
		set
		{
			if (!value || value && MessageBox.Show("Archive Patient", $"Are you sure you want to archive {Referral.Name}", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
			{
				Referral.Archived = value;

				VaultViewControl.Singleton?.FilteredReferrals.View.Refresh();
			}

			NotifyPropertyChanged();
		}
	}




}
