using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IntakeTrackerApp;


public enum TestStage
{
    Unneeded,
    Unknown,
    WaitingForRequest,
    WaitingForTest,
    WaitingForReport,
    Complete,
}


public abstract class ViewModelBase : INotifyPropertyChanged, ITrackable
{
    public event PropertyChangedEventHandler? PropertyChanged;
    // This method is called by the Set accessors of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.
    public void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    protected void SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, newValue) && propertyName != null)
        {
            UndoRedo.LogChange(this, field, propertyName);

            field = newValue;
            NotifyPropertyChanged(propertyName);
        }

    }
}




[Owned]
public class DateRecord : ViewModelBase
{
    private DateTime? _date;
    //Any changes to the dates requires:
    //Adding change to undo
    //Changing field value
    //Alerting UI to property change
    //Validation of data inputted

    [Column(TypeName = "Date")]
    public DateTime? Date { get => _date; set => SetProperty(ref _date, value); }

    private string _Comment;
    public string Comment { get => _Comment; set => SetProperty(ref _Comment, value); }

    //Not null and in the past
    [JsonIgnore] public bool HasOccurred => _date != null && _date.Value.CompareTo(DateTime.Now) < 0;
    [JsonIgnore] public bool Booked => _date != null;

    public DateRecord(DateTime? Date, string Comment = "")
    {
        _date = Date;
        _Comment = Comment;
    }
    public DateRecord()
    {
        _date = null;
        _Comment = "";
    }
    public int DaysSince() => _date!.Value.DaysSince();

}

/// <summary>
/// View Model for patient referrals
/// </summary>

public sealed class PatientReferral : ViewModelBase, IEquatable<PatientReferral?>, ICommand, INotifyPropertyChanged
{


    [Key] public ulong HospitalNumber { get; init; }
    /// <summary>
    /// Backing fields start with _
    /// </summary>
    [Timestamp] private long _Version;
    [Timestamp] public long Version { get => Version; set => SetProperty(ref _Version, value); }
    private string _NHSNumber = "";
    public string NHSNumber { get => _NHSNumber; set => SetProperty(ref _NHSNumber, value); }
    /// <summary>
    /// Previous hospital patient was registered under (if any)
    /// </summary>
    private string? _TransferRegion;
    public string? TransferRegion { get => _TransferRegion; set => SetProperty(ref _TransferRegion, value); }
    public bool IsTransfer => TransferRegion != null;
    public bool IsNotTransfer => !IsTransfer;
    //Summary information
    public string _FirstName  = "";
    public string FirstName { get => _FirstName; set => SetProperty(ref _FirstName, value); }
    public string _LastName = "";
    public string LastName { get => _LastName; set => SetProperty(ref _LastName, value); }
    public DateTime _DateOfBirth;
    public DateTime DateOfBirth { get => _DateOfBirth; set => SetProperty(ref _DateOfBirth, value); }

    public string Name => $"{_FirstName} {_LastName}";

    public int Age => DateTime.Compare(DateOfBirth, DateTime.Today) >= 0 ? 0 : DateOnly.FromDateTime(DateTime.Today).
        AddYears(-DateOfBirth.Year).
        AddMonths(-DateOfBirth.Month).
        AddDays(-DateOfBirth.Day).Year;


    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter)
    {
        return true;
    }
    public void Execute(object? parameter)
    {
        MainWindow.Singleton?.AddTab(new ReferralTab(this));
    }
    //referral
    public DateTime _DateOnReferral;
    public DateTime DateOnReferral { get => _DateOnReferral; set => SetProperty(ref _DateOnReferral, value); }
    public DateTime _DateReferralReceived;
    public DateTime DateReferralReceived { get => _DateReferralReceived; set => SetProperty(ref _DateReferralReceived, value); }


    public string _ReferralType = "";
    public string ReferralType { get => _ReferralType; set => SetProperty(ref _ReferralType, value); }
    public string _BriefDetails = "";
    public string BriefDetails { get => _BriefDetails; set => SetProperty(ref _BriefDetails, value); }
    public DateRecord DateOfActiveManagement { get; set; } = new();
    //Active management
    public string _ResponsibleOfActiveManagement  = "";
    public string ResponsibleOfActiveManagement { get => _ResponsibleOfActiveManagement; set => SetProperty(ref _ResponsibleOfActiveManagement, value); }
    public string _ActiveReferralActions = "";
    public string ActiveReferralActions { get => _ActiveReferralActions; set => SetProperty(ref _ActiveReferralActions, value); }

    //Contact
    public string _PreferredContactMethod = "";
    public string PreferredContactMethod { get => _PreferredContactMethod; set => SetProperty(ref _PreferredContactMethod, value); }

    public DateRecord ContactAttempted { get; set; } = new();
    public DateRecord DateContactMade { get; set; } = new();

    public Test MRI { get; set; } = new("MRI", TestType.MRI);
    public Test LP { get; set; } = new("LP", TestType.LP);
    public Test EP { get; set; } = new("EP", TestType.EP);

    // blood test
    public bool? _BloodTestNeeded;
    public bool? BloodTestNeeded { get => _BloodTestNeeded; set => SetProperty(ref _BloodTestNeeded, value); }
    public DateRecord BloodFormsSent { get; set; } = new();
    public DateRecord BloodTestPlanned { get; set; } = new();
    public DateRecord BloodTestReported { get; set; } = new();

    public string _BloodTestResults  = "";
    public string BloodTestResults { get => _BloodTestResults; set => SetProperty(ref _BloodTestResults, value); }
    //Previous correspondence
    public bool? _PreviousCorrespondenceNeeded  = null;
    public bool? PreviousCorrespondenceNeeded { get => _PreviousCorrespondenceNeeded; set => SetProperty(ref _PreviousCorrespondenceNeeded, value); }

    public DateRecord PreviousCorrespondenceRequested { get; set; } = new();
    public DateRecord PreviousCorrespondenceReceived { get; set; } = new();

    public string _PreviousCorrespondenceSummary  = "";
    public string PreviousCorrespondenceSummary { get => _PreviousCorrespondenceSummary; set => SetProperty(ref _PreviousCorrespondenceSummary, value); }

    //nursing appointment
    public bool? _NursingAppointmentNeeded = null;
    public bool? NursingAppointmentNeeded { get => _NursingAppointmentNeeded; set => SetProperty(ref _NursingAppointmentNeeded, value); }
    public DateRecord NursingAppointment { get; set; } = new();
    //Medical appointment
    public bool? _MedicalAppointmentNeeded  = null;
    public bool? MedicalAppointmentNeeded { get => _MedicalAppointmentNeeded; set => SetProperty(ref _MedicalAppointmentNeeded, value); }
    public DateRecord MedicalAppointment { get; set; } = new();


    public bool _Archived  = false;
    public bool Archived { get => _Archived; set => SetProperty(ref _Archived, value); }

    public bool IsAwaited(Test test, TestType t, out ReferralEvent e)
    {
        var stage = test.GetTestStage();
        e = new();
        e.r = this;
        e.Test = t;
        e.WaitingTime = test.GetDaysSinceLastEvent(DateReferralReceived);

        e.Display = stage switch
        {
            TestStage.WaitingForReport => $"{test.Name} Report",
            TestStage.WaitingForTest => test.TestDate.Booked ?
                $"{ test.Name} Test on {test.TestDate.Date!.Value.ToShortDateString()}" :
                $"{test.Name} Test (No Date)",
            TestStage.WaitingForRequest => $"{test.Name} to be requested",
            TestStage.Unknown => $"{test.Name} evaluation",
            _ => string.Empty,
        };

        e.Comment = stage switch
        {
            TestStage.WaitingForReport => test.ReportedDate.Comment,
            TestStage.WaitingForTest => test.TestDate.Comment,
            TestStage.WaitingForRequest => test.RequestedDate.Comment,
            _ => string.Empty,
        };


        if (e.WaitingTime < 0)
        {
            //Test has a date but not occurred yet

            //This things precursor is scheduled but has not occurred, so we are waiting for neither
            return false;
        }

        return stage != TestStage.Complete && stage != TestStage.Unneeded;
    }

    public struct ReferralEvent
    {
        public ReferralEvent(PatientReferral r, string display, int waiting, string comment = "", TestType Test = TestType.None)
        {
            Display = display;
            WaitingTime = waiting;
            this.Test = Test;
            this.r = r;
            Comment = comment;
        }
        public PatientReferral r { get; set; }
        public string Group { get => r.Name; }
        public string Display { get; set; }
        public int WaitingTime { get; set; }
        public string Comment { get; set; }
        public TestType Test { get; set; }
        public string DaysLabel
        {
            get => WaitingTime == 1 ? "Day" : "Days";
            set { }
        }
    }

    [JsonIgnore, NotMapped]
    public string QuickStatus
    {
        get
        {
            bool allTestsOccured = MRI.TestDate.HasOccurred && EP.TestDate.HasOccurred && LP.TestDate.HasOccurred;
            bool allTestsReported = MRI.ReportedDate.HasOccurred && EP.ReportedDate.HasOccurred && LP.ReportedDate.HasOccurred;

            if (!allTestsOccured)
            {
                return "Awaiting investigations";

            }
            else if (allTestsOccured && !allTestsReported)
            {
                //All tests completed, not all results received
                return "Investigations complete; awaiting results";
            }
            else if (MedicalAppointmentNeeded == true && !MedicalAppointment.HasOccurred ||
               NursingAppointmentNeeded == true && !NursingAppointment.HasOccurred)
            {
                return "Awaiting appointment";
            }
            else
            {
                return "Waiting to start treatment";
            }
        }
    }

    public IEnumerable<ReferralEvent> GetAwaitedEvents
    {
        get
        {
            if (IsAwaited(MRI, TestType.MRI, out var w))
                yield return w;
            if (IsAwaited(LP, TestType.LP, out w))
                yield return w;
            if (IsAwaited(EP, TestType.EP, out w))
                yield return w;

            if (BloodTestNeeded is null)
            {
                yield return new(this, "Blood Test Evaluation", DateReferralReceived.DaysSince(), Test: TestType.Bloods);
            }
            else if (BloodTestNeeded is true)
            {
                if (!BloodFormsSent.HasOccurred)
                {

                    yield return new(this, "For Blood Test Forms Sending", DateReferralReceived.DaysSince(), Test: TestType.Bloods);
                }
                else if (!BloodTestPlanned.HasOccurred)
                {

                    yield return new(this, "Blood Test planning", BloodFormsSent.DaysSince(), Test: TestType.Bloods);
                }
                else if (!BloodTestReported.HasOccurred)
                {
                    yield return new(this, "Blood Test Results", BloodTestPlanned.DaysSince(), Test: TestType.Bloods);
                }
            }

            //Show messages for Previous Correspondence
            if (PreviousCorrespondenceNeeded is null)
            {
                yield return new(this, "Previous Correspondence Evaluation", DateReferralReceived.DaysSince());
            }
            else if (PreviousCorrespondenceNeeded is true)
            {
                if (!PreviousCorrespondenceRequested.HasOccurred)
                {
                    yield return new(this,
                        "Previous Correspondence Request",
                        DateReferralReceived.DaysSince()
                        );
                }
                else if (!PreviousCorrespondenceReceived.HasOccurred)
                {
                    yield return new(this,
                        "Previous Correspondence to arrive",
                        PreviousCorrespondenceRequested.DaysSince()
                        );
                }
            }


            if (ReadyForDiagnostics())
            {
                yield return new(this, "Diagnostic", 0);
            }
        }
    }


    public bool ReadyForDiagnostics()
    {
        var mri = MRI.GetTestStage();
        return mri == TestStage.Complete || mri == TestStage.Unneeded;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as PatientReferral);
    }

    public bool Equals(PatientReferral? other)
    {
        return other != null &&
               HospitalNumber == other.HospitalNumber &&
               NHSNumber == other.NHSNumber;
    }

    public static bool operator ==(PatientReferral? left, PatientReferral? right)
    {
        return EqualityComparer<PatientReferral>.Default.Equals(left, right);
    }

    public static bool operator !=(PatientReferral? left, PatientReferral? right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return (int)HospitalNumber;
    }
}


