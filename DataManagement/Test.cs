using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IntakeTrackerApp.DataManagement
{
    [Flags]
    public enum TestType
    {
        None = 0,
        MRI = 1,
        EP = 2,
        LP = 4,
        Bloods = 8,
        All = -1,
    }

    public class TestGroup
    {
        public TestStage Name { get; set; }
        public bool Expanded { get; set; }
    }


    /// <summary>
    /// Database object for a 3 stage test
    /// </summary>
    [Owned]
    public class Test : ViewModelBase
    {
        public static Dictionary<TestType, Dictionary<TestStage, TestGroup>> TestGroups { get; set; } = new();

        private bool? _needed;
        public DateRecord RequestedDate { get; init; } = new();
        public DateRecord TestDate { get; init; } = new();
        public DateRecord ReportedDate { get; init; } = new();

        [NotMapped]
        public string Name => Type.ToString();

        [NotMapped]
        public TestType Type { get; set; }
        public Test(TestType type)
        {
            this.Type = type;
        }
        public Test()
        {
        }
        public override void AddListener(PropertyChangedEventHandler listener)
        {
            base.AddListener(listener);
            RequestedDate.AddListener(listener);
            TestDate.AddListener(listener);
            ReportedDate.AddListener(listener);
        }

        public bool? Needed
        {
            get => _needed;
            set => SetProperty(ref _needed, value);
        }
        /// <summary>
        /// Get the group this test belongs in - just the test stage along with a record of if the group has been expanded
        /// </summary>
        public TestGroup TestGroup
        {
            get
            {
                var s = TestStage;

                if (!TestGroups.ContainsKey(Type))
                {
                    // Im never going to write another type definition again at this rate lol
                    var g = TestGroups[Type] = new();
                    var group = new TestGroup()
                    {
                        Name = s,
                        Expanded = false
                    };
                    g[s] = group;
                    return group;
                }
                else
                {
                    var g = TestGroups[Type];
                    if (g.ContainsKey(s))
                        return g[s];
                    else
                    {
                        var group = new TestGroup()
                        {
                            Name = s,
                            Expanded = false
                        };
                        g[s] = group;
                        return group;
                    }
                }

            }
        }
        /// <summary>
        /// Get the current stage of the test
        /// </summary>
        public TestStage TestStage =>

             this switch
             {
                 { Needed: null } => TestStage.Unknown,
                 { Needed: false } => TestStage.Unneeded,
                 { Needed: true, RequestedDate.Booked: false, TestDate.Booked: false, ReportedDate.Booked: false } => TestStage.WaitingForRequest,
                 { Needed: true, RequestedDate.Booked: true, TestDate.Booked: false } //booked but not tested
                 or { Needed: true, TestDate.Booked: true, TestDate.HasOccurred: false } //has appointment for test, not always logged in requested date
                 => TestStage.WaitingForTest,
                 { Needed: true, ReportedDate.Booked: false } => TestStage.WaitingForReport,
                 _ => TestStage.Complete,
             };


        public int GetDaysSinceLastEvent(DateTime referralReceivedDate)
        {
            return TestStage switch
            {
                TestStage.Unknown => referralReceivedDate.DaysSince(),
                TestStage.WaitingForRequest => referralReceivedDate.DaysSince(),
                TestStage.WaitingForTest => RequestedDate.Date!.Value.DaysSince(),
                TestStage.WaitingForReport => TestDate.Date!.Value.DaysSince(),
                _ => 0
            };
        }


    }
    public static class DateTimeUtility
    {
        /// <summary>
        /// Number of days that have passed since this event happening
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static int DaysSince(this DateTime e)
        {
            return (DateOnly.FromDateTime(DateTime.Today).DayNumber - DateOnly.FromDateTime(e).DayNumber);
        }

        public static int DaysTo(this DateTime e)
        {
            return -e.DaysSince();
        }


        public static string DaysSinceLabel(this DateTime e) => e.DaysSince() switch
        {
            0 => "today",
            1 => "yesterday",
            int n => $"{n} days ago"
        };
        public static string DaysToLabel(this DateTime e) => e.DaysTo() switch
        {
            0 => "today",
            1 => "tomorrow",
            int n => $"in {n} days"
        };
    }
}

