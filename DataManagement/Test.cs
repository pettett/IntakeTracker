using Microsoft.EntityFrameworkCore;

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
    /// <summary>
    /// Database object for a 3 stage test
    /// </summary>
    [Owned]
    public class Test : ViewModelBase
    {
        private readonly string name = "";

        private bool? _needed;
        public DateRecord RequestedDate { get; init; } = new();
        public DateRecord TestDate { get; init; } = new();
        public DateRecord ReportedDate { get; init; } = new();


        public string Name
        {
            get => name;
            init => name = value;
        }

        public Test(string name)
        {
            this.name = name;
        }

        public Test()
        {
            name = Name;
        }

        public bool? Needed
        {
            get => _needed;
            set => SetProperty(ref _needed, value);
        }

        public TestStage GetTestStage()
        {
            return this switch
            {
                { Needed: null } => TestStage.Unknown,
                { Needed: false } => TestStage.Unneeded,
                { RequestedDate.Date: null, TestDate.Date: null, ReportedDate.Date: null } => TestStage.WaitingForRequest,
                { TestDate.Date: null } or { TestDate.HasOccurred: false } => TestStage.WaitingForTest,
                { ReportedDate.Date: null } => TestStage.WaitingForReport,
                _ => TestStage.Complete,
            };
        }

        public uint GetDaysSinceLastEvent(DateTime referralReceivedDate)
        {
            return GetTestStage() switch
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
        public static uint DaysSince(this DateTime e)
        {
            return (uint)(DateOnly.FromDateTime(DateTime.Today).DayNumber - DateOnly.FromDateTime(e).DayNumber);
        }
    }
}

