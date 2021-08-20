using LiveCharts.Defaults;
using System.Collections.ObjectModel;
using IntakeTrackerApp.Extensions;

namespace IntakeTrackerApp.Controls;

/// <summary>
/// Interaction logic for TestSummary.xaml
/// </summary>
public partial class TestSummary : UserControl
{


    public SeriesCollection ReferralCatagoriesCollection { get; set; }
    public SeriesCollection ReferralsOverTimeCollection { get; set; }
    public SeriesCollection WaitingTimeCollection { get; set; }
    public Dictionary<string, double> ReferralCatagories { get; set; }
    public string[] ReferralTypes { get; set; }


    public Data Context { get; set; } = Data.Singleton;
    public ObservableCollection<PatientReferral.ReferralEvent> AllAwaitedEvents { get; set; } = new();

    public static Func<double, string> DateOnlyFormatter { get; set; } = value => DateOnly.FromDayNumber((int)value).ToShortDateString();
    public static Func<double, string> DateTimeFormatter { get; set; } = value => new DateTime((long)value).ToShortDateString();
    public string GlobalOne
    {
        get => $"Current Referrals: {Context.ReferralSummaries.Count}";
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
    }
    public ObservablePoint[] GenerateBars(int waitingTimeWidth, Dictionary<int, double> waitingTimes)
    {
        int barCount = waitingTimes.Keys.DefaultIfEmpty().Max() + 1;

        ObservablePoint[] bars = new ObservablePoint[barCount];

        for (int i = 0; i < bars.Length; i++)
            bars[i] = new(i * waitingTimeWidth, 0);

        foreach (var kvp in waitingTimes)
        {
            bars[kvp.Key].Y = kvp.Value;
        }
        return bars;
    }




    public TestSummary(TestType TypeFilter, bool IncludeNone)
    {
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

        Dictionary<int, double> waitingTimes = new();

        int waitingTimeWidth = 5;

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

                var w = Math.Max(0, x.WaitingTime / waitingTimeWidth);
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
            AddLine(GenerateBars(waitingTimeWidth, waitingTimes), 0.25);

        ReferralCatagoriesCollection = LiveChartsExtensions.GenerateSeries().
            AddColumns(ReferralCatagories.Values);




        ReferralTypes = ReferralCatagories.Keys.ToArray();

        InitializeComponent();
        //Apply changes to in application elements
        WaitingTimeAxisX.Sections = LiveChartsExtensions.GenerateSections().AddVerticalSeperator(AverageWaitTime, "Average Wait Time");


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

