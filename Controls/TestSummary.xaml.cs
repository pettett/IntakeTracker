

using IntakeTrackerApp.DataManagement;
using IntakeTrackerApp.DataManagement.Filtering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.ObjectModel;
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

public class TestStageConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return ((TestGroup)value).Name switch
		{
			TestStage.Unknown => "Awaiting Tirage",
			TestStage.WaitingForReport => "Awaiting Results",
			TestStage.WaitingForTest => "Awaiting Test",
			TestStage.WaitingForRequest => "Awaiting Request",
			_ => ""
		};
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}

}
public class Checkable<T> : INotifyPropertyChanged where T : class
{
	public T Value { get; set; }

	public Checkable(T value)
	{
		Value = value;
		Checked.PropertyChanged += (_, e) => PropertyChanged?.Invoke(this, e);
	}

	public ObservableItem<bool> Checked { get; set; } = true;


	public static implicit operator T(Checkable<T> i) => i.Value;
	public static implicit operator Checkable<T>(T i) => new(i);

	public event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>
/// Interaction logic for TestSummary.xaml
/// </summary>
public partial class TestSummary : UserControl, INotifyPropertyChanged
{

	public TestType TestType { get; init; }

	//	public SeriesCollection ReferralCatagoriesCollection { get; set; }
	//public SeriesCollection ReferralsOverTimeCollection { get; set; }
	//public SeriesCollection WaitingTimeCollection { get; set; }
	//public Dictionary<string, double> ReferralCatagories { get; set; }
	//public string[] ReferralTypes { get; set; }

	public Vault MainVault { get; init; }
	public Data Context => MainVault.Context!;

	//need 4 copies of the collection to apply different filters to them 

	public ObservableFilteredCollection<PatientReferral> FilteredReferrals { get; init; }

	public HideStringsFilter ManagersFilter { get; set; }
	public HideStringsFilter RegionsFilter { get; set; }

	//public static Func<double, string> DateOnlyFormatter { get; set; } = value => DateOnly.FromDayNumber((int)value).ToShortDateString();
	//public static Func<double, string> DateTimeFormatter { get; set; } = value => new DateTime((long)value).ToShortDateString();



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

	readonly VaultViewControl control;

	public void ToggleCol(bool enabled, string colName)
	{
		if (Resources[colName] is GridViewColumn col)
		{
			ArgumentNullException.ThrowIfNull(col);

			if (enabled)
			{
				GridView.Columns.Add(col);
			}
			else
			{
				GridView.Columns.Remove(col);
			}
		}
	}
	HideStringsFilter CreateStringFilter(ObservableCollection<string> options, string name, string col)
	{
		var Filter = new HideStringsFilter(options, name);
		Filter.Enabled.PropertyChanged += (_, _) => ToggleCol(Filter.Enabled, col);
		return Filter;
	}
	public TestSummary(Vault v, TestType TypeFilter, VaultViewControl control)
	{
		ArgumentNullException.ThrowIfNull(v);
		ArgumentNullException.ThrowIfNull(control);
		ArgumentNullException.ThrowIfNull(v.Context);

		DataContext = this;
		this.control = control;
		this.TestType = TypeFilter;
		this.TypeFilter = TypeFilter;
		MainVault = v;

		ManagersFilter = CreateStringFilter(MainVault.ReferralManagers, "Referral Manager", "ResponsibleOfActiveManagementColumn");

		RegionsFilter = CreateStringFilter(MainVault.TransferRegions, "Transfer Regions", "TransferRegionColumn");

		FilteredReferrals = new(
			v.Context.ReferralSummaries,
			new MapFilter<PatientReferral, string?>(ManagersFilter, x => x.ResponsibleOfActiveManagement),
			new MapFilter<PatientReferral, string?>(RegionsFilter, x => x.TransferRegion)
			);

		FilteredReferrals.View.GroupDescriptions.Add(new PropertyGroupDescription("MRI.TestGroup"));

		FilteredReferrals.View.SortDescriptions.Add(new SortDescription("MRI.TestStage", ListSortDirection.Descending));
		FilteredReferrals.View.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));


		InitializeComponent();


		var testSummaryPath = TypeFilter switch
		{
			TestType.MRI => "BloodsSummaryColumn",
			TestType.LP => "LPSummaryColumn",
			TestType.EP => "EPSummaryColumn",
			TestType.Bloods => "BloodsSummaryColumn",
			_ => throw new InvalidEnumArgumentException("Invalid test type"),
		};

		GridView.Columns.Add((GridViewColumn)Resources[testSummaryPath]);

	}


	public event PropertyChangedEventHandler? PropertyChanged;
	private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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


}

