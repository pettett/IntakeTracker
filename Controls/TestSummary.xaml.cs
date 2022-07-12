using IntakeTrackerApp.DataManagement;
using System.Globalization;

namespace IntakeTrackerApp.Controls;
public class CutoffConverter : UserControl, IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return ((uint)value) > Cutoff;
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

	public string TestSummaryPath { get; init; }
	public TestType TestType { get; init; }

	//	public SeriesCollection ReferralCatagoriesCollection { get; set; }
	//public SeriesCollection ReferralsOverTimeCollection { get; set; }
	//public SeriesCollection WaitingTimeCollection { get; set; }
	//public Dictionary<string, double> ReferralCatagories { get; set; }
	//public string[] ReferralTypes { get; set; }

	public Vault MainVault { get; init; }
	public Data Context { get; set; } = Data.Singleton;
	//need 4 copies of the collection to apply different filters to them
	public ListCollectionView AwaitingTirage { get; set; }
	public ListCollectionView AwaitingRequest { get; set; }
	public ListCollectionView AwaitingTest { get; set; }
	public ListCollectionView AwaitingResults { get; set; }

	private Dictionary<string, bool> managerFilter = new();

	public static Func<double, string> DateOnlyFormatter { get; set; } = value => DateOnly.FromDayNumber((int)value).ToShortDateString();
	public static Func<double, string> DateTimeFormatter { get; set; } = value => new DateTime((long)value).ToShortDateString();

	public ObservableItem<bool> FilterMangers { get; set; } = new(false);


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
		get => $"Recent Clinic (last week): {Context.ReferralSummaries.Where(r => r.MedicalAppointmentNeeded == true && r.MedicalAppointment.DaysSince() < 7).Count()}";
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


	const uint waitingTimeWidth = 5;
	public Func<double, string> Formatter { get; set; } = x => $"{waitingTimeWidth * x} - {waitingTimeWidth * (x + 1) - 1} Days";
	VaultViewControl control;


	public TestSummary(Vault v, TestType TypeFilter, VaultViewControl control)
	{
		DataContext = this;
		this.control = control;
		this.TestType = TypeFilter;
		MainVault = v;

		switch (TypeFilter)
		{
			case TestType.MRI:
				TestSummaryPath = "MRI";
				break;

			case TestType.LP:
				TestSummaryPath = "LP";
				break;
			case TestType.EP:
				TestSummaryPath = "EP";
				break;
			case TestType.Bloods:
				TestSummaryPath = "Bloods";
				break;
			default:
				throw new InvalidEnumArgumentException("Invalid test type");
		}


		AwaitingTirage = new(v.Context.ReferralSummaries)
		{
			Filter = WaitingForTirageFilter
		};

		AwaitingRequest = new(v.Context.ReferralSummaries)
		{
			Filter = WaitingForRequestFilter
		};


		AwaitingTest = new(v.Context.ReferralSummaries)
		{
			Filter = WaitingForTestFilter
		};


		AwaitingResults = new(v.Context.ReferralSummaries)
		{
			Filter = WaitingForResultsFilter
		};


		//AwaitingTirage = new(Context.Referrals.patientReferrals.Where(
		//	p => !p.MRI.Needed.HasValue
		//).AsNoTracking().Select(p => new ReferralData(p, TypeFilter)));

		//AwaitingRequest = new(Context.Referrals.patientReferrals.Where(
		//	p => p.MRI.Needed.HasValue && p.MRI.Needed.Value &&
		//	p.MRI.RequestedDate.Date == null && p.MRI.TestDate.Date == null &&
		//	p.MRI.ReportedDate.Date == null
		//	).AsNoTracking()
		//	.Select(p => new ReferralData(p, TypeFilter))
		//);

		//AwaitingTest = new(

		//	Context.Referrals.patientReferrals.Where(
		//		p => p.MRI.Needed.HasValue && p.MRI.Needed.Value &&
		//		p.MRI.RequestedDate.Date != null && p.MRI.TestDate.Date == null
		//	).AsNoTracking().Select(p => new ReferralData(p, TypeFilter))
		//);

		//AwaitingResults = new(Context.Referrals.patientReferrals.Where(
		//	p => p.MRI.Needed.HasValue && p.MRI.Needed.Value && p.MRI.TestDate.Date != null && p.MRI.RequestedDate.Date != null && p.MRI.ReportedDate.Date == null
		//	).AsNoTracking().Select(p => new ReferralData(p, TypeFilter))
		//);

		Debug.Print(AwaitingResults.Count.ToString());

		ReferralPoint.Init(); //Initialize referralpoints for mapping

		this.TypeFilter = TypeFilter;
		//Get data for charts
		//ReferralCatagories = new()
		//{
		//	{ "GP", .0 },
		//	{ "Neurology – outpatients", .0 },
		//	{ "Neurology-attending", .0 },
		//	{ "REI", .0 },
		//	{ "ENT", .0 },
		//	{ "Other Consultant", .0 },
		//	{ "Relocation", .0 },
		//	{ "Other", .0 },
		//};

		//Dictionary<uint, double> waitingTimes = new();
		InitializeComponent();

		// keeping a reference to the original columns allows us to add and remove them,
		// and because it can only be used in one place at a time, will stop duplcate columns
		GridViewColumn managerRequestCol = (GridViewColumn)Resources["ResponsibleOfActiveManagementColumn"];
		GridViewColumn managerResultsCol = (GridViewColumn)Resources["ResponsibleOfActiveManagementColumn"];
		GridViewColumn managerTirageCol = (GridViewColumn)Resources["ResponsibleOfActiveManagementColumn"];
		GridViewColumn managerTestCol = (GridViewColumn)Resources["ResponsibleOfActiveManagementColumn"];

		FilterMangers.PropertyChanged += (_, _) =>
		{
			if (FilterMangers)
			{
				((GridView)AwaitingRequestList.View).Columns.Add(managerRequestCol);
				((GridView)AwaitingResultsList.View).Columns.Add(managerResultsCol);
				((GridView)AwaitingTirageList.View).Columns.Add(managerTirageCol);
				((GridView)AwaitingTestList.View).Columns.Add(managerTestCol);
			}
			else
			{
				((GridView)AwaitingRequestList.View).Columns.Remove(managerRequestCol);
				((GridView)AwaitingResultsList.View).Columns.Remove(managerResultsCol);
				((GridView)AwaitingTirageList.View).Columns.Remove(managerTirageCol);
				((GridView)AwaitingTestList.View).Columns.Remove(managerTestCol);
			}
			Refresh();
		};




		//PropertyGroupDescription groupDescription = new("r");
		//allAwaitedEventsView.GroupDescriptions.Add(groupDescription);
	}
	/// <summary>
	/// Test against "global filters", the filters shown to the right of the list
	/// </summary>
	/// <param name="p"></param>
	/// <returns>True if this referral passes</returns>
	private bool PassesGlobalFilter(PatientReferral p)
	{
		if (FilterMangers)
		{
			if (!managerFilter.GetValueOrDefault(p.ResponsibleOfActiveManagement, true))
			{
				return false;
			}
		}
		return true;
	}

	private bool WaitingForResultsFilter(object obj)
	{           //CONDITIONS: 
		if (obj is PatientReferral p)
		{
			if (!PassesGlobalFilter(p)) return false;

			var t = p.Test(TestType);
			return t.Needed.HasValue
				&& t.Needed.Value
				&& t.TestDate.Booked
				&& t.RequestedDate.Booked
				&& !t.ReportedDate.Booked;
		}
		return false;

	}

	private bool WaitingForTestFilter(object obj)
	{           //CONDITIONS:
				// test needed
				//  | requested but no date
				//  OR
				//  | test still in the future

		if (obj is PatientReferral p)
		{
			if (!PassesGlobalFilter(p)) return false;

			var t = p.Test(TestType)!;

			return t.Needed is true
			&& (
				t.RequestedDate.Booked && !p.MRI.TestDate.Booked
				|| t.TestDate.Booked && !t.TestDate.HasOccurred
			);
		}
		return false;
	}

	private bool WaitingForRequestFilter(object obj)
	{           //CONDITIONS: 

		if (obj is PatientReferral p)
		{
			if (!PassesGlobalFilter(p)) return false;

			var t = p.Test(TestType)!;

			return t.Needed.HasValue
		&& t.Needed.Value
		&& !t.RequestedDate.Booked
		&& !t.TestDate.Booked
		&& !t.ReportedDate.Booked;
		}
		return false;
	}

	private bool WaitingForTirageFilter(object obj)
	{           //CONDITIONS: 

		if (obj is PatientReferral p)
		{
			if (!PassesGlobalFilter(p)) return false;

			var t = p.Test(TestType)!;

			return !t.Needed.HasValue;
		}
		return false;

	}


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
	//public bool IsTrackedTest(PatientReferral.ReferralEvent x) => TypeFilter.HasFlag(x.Test) && !(x.Test == TestType.None && !IncludeNone);
	public void Refresh()
	{
		AwaitingTirage.Refresh();
		AwaitingRequest.Refresh();
		AwaitingTest.Refresh();
		AwaitingResults.Refresh();
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



	public void OnReferralPointClicked(object sender, ChartPoint p)
	{
		((ReferralPoint)p.Instance).OnClicked();
	}


	private void AwaitingResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		PatientReferral? item = (sender as ListView)?.SelectedItem as PatientReferral;
		if (item != null)
			control.AddTab(new ReferralTab(item));
	}



	private void ManagerCheckBox_Checked(object sender, RoutedEventArgs e)
	{
		CheckBox? s = sender as CheckBox;
		string? name = s?.Content as string;
		if (name != null)
		{
			var enabled = managerFilter.GetValueOrDefault(name, true);
			managerFilter[name] = !enabled;
			Debug.Print($"{name} is now enabled={!enabled}");
			if (FilterMangers)
				Refresh();
		}

	}


}

