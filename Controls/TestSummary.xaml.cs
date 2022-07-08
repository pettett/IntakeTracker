using LiveCharts.Defaults;
using System.Collections.ObjectModel;
using IntakeTrackerApp.Extensions;
using System.Globalization;
using IntakeTrackerApp.DataManagement;

namespace IntakeTrackerApp.Controls;
public class CutoffConverter : UserControl, IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return ((uint)value) > Cutoff ;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}

	public uint Cutoff
	{
		get { return (uint)(GetValue(CutoffProperty) ?? 0); }
		set { SetValue(CutoffProperty, value); }
	}

	public static readonly DependencyProperty CutoffProperty =
	DependencyProperty.Register(
		nameof(Cutoff), typeof(uint), typeof(CutoffConverter)
		 );
}
/// <summary>
/// Interaction logic for TestSummary.xaml
/// </summary>
public partial class TestSummary : UserControl, INotifyPropertyChanged
{


	public SeriesCollection ReferralCatagoriesCollection { get; set; }
	public SeriesCollection ReferralsOverTimeCollection { get; set; }
	public SeriesCollection WaitingTimeCollection { get; set; }
	public Dictionary<string, double> ReferralCatagories { get; set; }
	public string[] ReferralTypes { get; set; }

	public Vault MainVault { get; init; }
	public Data Context { get; set; } = Data.Singleton;
	public ObservableCollection<PatientReferral.ReferralEvent> AllAwaitedEvents { get; set; } = new();

	public static Func<double, string> DateOnlyFormatter { get; set; } = value => DateOnly.FromDayNumber((int)value).ToShortDateString();
	public static Func<double, string> DateTimeFormatter { get; set; } = value => new DateTime((long)value).ToShortDateString();
	public string GlobalOne
	{
		get => $"Current Referrals: {Context.ReferralSummaries.Count}";
		set { }
	}
	public string MDTNewLastWeek
	{
		get => $"Recent Referrals (last week): {Context.ReferralSummaries.Where(r => r.DateReferralReceived.DaysSince() < 7).Count()}";
		set { }
	}
	public string MDTClinicLastWeek
	{
		get => $"Recent Clinic (last week): {Context.ReferralSummaries.Where(r => r.MedicalAppointmentNeeded == true && r.MedicalAppointment.DaysSince() < 7 ).Count()}";
		set { }
	}

	public double AverageWaitTime { get; set; }
	public string WaitingTimeInfo
	{
		get => $"Average Wait (days): {AverageWaitTime:0.00}";
		set { }
	}

	public TestType TypeFilter { get; set; }
	public bool IncludeNone { get; set; }

	public event PropertyChangedEventHandler? PropertyChanged;
	private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	/// <summary>
	/// Remove tests if type is not flagged, or if test is none
	/// </summary>
	/// <param name="x">Test</param>
	/// <returns></returns>
	public bool IsTrackedTest(PatientReferral.ReferralEvent x) => TypeFilter.HasFlag(x.Test) && !(x.Test == TestType.None && !IncludeNone);
	public void Refresh()
	{
		AllAwaitedEvents.Clear();
		for (int i = 0; i < Context.ReferralSummaries.Count; i++)
		{
			foreach (var x in Context.ReferralSummaries[i].GetAwaitedEvents.Where(IsTrackedTest))
			{
				AllAwaitedEvents.Add(x);
			}
		}
		NotifyPropertyChanged(nameof(GlobalOne));
		NotifyPropertyChanged(nameof(MDTNewLastWeek));
		NotifyPropertyChanged(nameof(MDTClinicLastWeek));
	
		
	}
	public double[] GenerateBars(uint waitingTimeWidth, Dictionary<uint, double> waitingTimes)
	{
		uint barCount = waitingTimes.Keys.DefaultIfEmpty().Max() + 1;

		double[] bars = new double[barCount];

		foreach (var kvp in waitingTimes)
		{
			bars[kvp.Key] = kvp.Value;
		}
		return bars;
	}


	const uint waitingTimeWidth = 5;
	public Func<double, string> Formatter { get; set; } = x => $"{waitingTimeWidth * x} - {waitingTimeWidth * (x + 1)-1} Days";

	public TestSummary(Vault v, TestType TypeFilter, bool IncludeNone)
	{
		MainVault = v;
		ReferralPoint.Init(); //Initialize referralpoints for mapping

		this.TypeFilter = TypeFilter;
		this.IncludeNone = IncludeNone;
		//Get data for charts
		ReferralCatagories = new()
		{
			{ "GP", .0 },
			{ "Neurology – outpatients", .0 },
			{ "Neurology-attending", .0 },
			{ "REI", .0 },
			{ "ENT", .0 },
			{ "Other Consultant", .0 },
			{ "Relocation", .0 },
			{ "Other", .0 },
		};

		Dictionary<uint, double> waitingTimes = new();


		int totalEvents = 0;
		double totalWait = 0;


		CollectionView allAwaitedEventsView = (CollectionView)CollectionViewSource.GetDefaultView(AllAwaitedEvents);
		PropertyGroupDescription groupDescription = new PropertyGroupDescription("r");
		allAwaitedEventsView.GroupDescriptions.Add(groupDescription);

		for (int i = 0; i < Context.ReferralSummaries.Count; i++)
		{
			var s = Context.ReferralSummaries[i];

			ReferralCatagories[ReferralCatagories.ContainsKey(s.ReferralType) ? s.ReferralType : "Other"]++;


			//Go Through all awaited events, measure waiting time and filter to made group for display
			List<PatientReferral.ReferralEvent> AwaitedEvents = new();

			//Remove tests if type is not flagged, or if test is none
			foreach (var x in s.GetAwaitedEvents.Where(IsTrackedTest))
			{


				AwaitedEvents.Add(x);
				AllAwaitedEvents.Add(x);

				totalEvents++;
				totalWait += x.WaitingTime;

				var w =  Math.Max(0, x.WaitingTime / waitingTimeWidth);
				if (waitingTimes.ContainsKey(w))
					waitingTimes[w]++;
				else
					waitingTimes[w] = 1;
			}
		}
		//Set out graph for when referrals happened
		//Time going back
		const int backDays = 50;
		var regionEnd = DateTime.Today;
		var regionStart = regionEnd.AddDays(-backDays);


		//Keep track of number of referrals at this point
		//For when referrals will also have an end date
		int current = 0;

		ReferralsOverTimeCollection = LiveChartsExtensions.GenerateSeries().AddStepLine(
			Context.ReferralSummaries.OrderBy(r => r.DateReferralReceived). //Order by date of referral
			Select<PatientReferral, ReferralPoint>((r, i) => //Create ticks
			{
				current++;
				return new(r.DateReferralReceived.Ticks, current, r);
			}
			));


		//Set out bar charts for different referral types
		AverageWaitTime = totalWait / totalEvents;


		//Create all the livecharts objects from gathered data


		WaitingTimeCollection = LiveChartsExtensions.GenerateSeries().
			AddHistogram(GenerateBars(waitingTimeWidth, waitingTimes));
	


		ReferralCatagoriesCollection = LiveChartsExtensions.GenerateSeries().
			AddColumns(ReferralCatagories.Values);



		ReferralTypes = ReferralCatagories.Keys.ToArray();

		InitializeComponent();
		//Apply changes to in application elements
		WaitingTimeAxisX.Sections = LiveChartsExtensions.GenerateSections().
			AddVerticalSeperator(AverageWaitTime/waitingTimeWidth, "Average Wait Time");


		ReferralsOverTimeAxisX.Sections = LiveChartsExtensions.GenerateSections().AddTodayLine();
		ReferralsOverTimeAxisX.MinValue = regionStart.Ticks;
		ReferralsOverTimeAxisX.MaxValue = regionEnd.Ticks;

		//Set a max value of one above greatest or 7 to stop decimal places appearing
		ReferralCatagoriesAxisY.MaxValue = Math.Max(7, ReferralCatagories.Values.Max() + 1);


	}


	public void OnReferralPointClicked(object sender, ChartPoint p)
	{
		((ReferralPoint)p.Instance).OnClicked();
	}
}

