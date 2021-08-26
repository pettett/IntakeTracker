using Microsoft.EntityFrameworkCore;

namespace IntakeTrackerApp
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

	[Owned]
	public class Test : ViewModelBase
	{


		private readonly string name = "";
		private readonly TestType test = TestType.None;

		private bool? _needed;
		public DateRecord RequestedDate { get; init; } = new();
		public DateRecord TestDate { get; init; } = new();
		public DateRecord ReportedDate { get; init; } = new();


		public string Name
		{
			get => name;
			init => name = value;
		}

		public Test(string name, TestType type)
		{
			this.name = name;
			test = type;
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
			if (Needed == null)
			{
				return TestStage.Unknown;
			}
			else if (Needed == false)
			{

				return TestStage.Unneeded;

			}
			else if (RequestedDate.Date is null)
			{
				return TestStage.WaitingForRequest;
			}
			else if (TestDate.Date is null || !TestDate.HasOccurred)
			{
				return TestStage.WaitingForTest;
			}
			else if (ReportedDate.Date is null)
			{
				return TestStage.WaitingForReport;
			}
			else
			{
				return TestStage.Complete;
			}

		}

		public int GetDaysSinceLastEvent(DateTime referralReceivedDate) => GetTestStage() switch
		{
			TestStage.Unknown => referralReceivedDate.DaysSince(),
			TestStage.WaitingForRequest => referralReceivedDate.DaysSince(),
			TestStage.WaitingForTest => RequestedDate.Date!.Value.DaysSince(),
			TestStage.WaitingForReport => TestDate.Date!.Value.DaysSince(),
			_ => -1
		};



	}
	public static class DateTimeUtility
	{
		public static int DaysSince(this DateTime e)
		{
			return DateOnly.FromDateTime(DateTime.Today).DayNumber - DateOnly.FromDateTime(e).DayNumber;
		}
	}
}

