

using System.Text.RegularExpressions;

namespace IntakeTrackerApp;

/// <summary>
/// Interaction logic for NewPatientReferralWindow.xaml
/// </summary>
public partial class ReferralDetailsWindow : Window, INotifyPropertyChanged, INotifyDataErrorInfo
{
    ErrorTracker errorTracker;
    public ReferralDetailsWindow()
    {
        errorTracker = new(OnErrorsChanged);
        InitializeComponent();
        Windows.WindowUtility.HideMinimizeAndMaximizeButtons(this);
    }

    public ReferralDetailsWindow(
        PatientReferral referral) : this()
    {
        HospitalNumber = referral.HospitalNumber.ToString();
        ReferralType = referral.ReferralType;
        FirstName = referral.FirstName;
        LastName = referral.LastName;
        DateOfBirth = referral.DateOfBirth;
        DateOnReferral = referral.DateOnReferral;
        DateReferralRecieved = referral.DateReferralReceived;
        NHSNumber = referral.NHSNumber;
        TransferRegion = referral.TransferRegion;

        ValidateAll((Variable)(-1));
    }
    /// <summary>
    /// Applies every value EXECPT hospital number which is a key so must be done separately
    /// </summary>
    /// <param name="referral"></param>
    public void ApplyDateToReferral(PatientReferral referral)
    {
        //  referral.HospitalNumber = ulong.Parse(HospitalNumber);

        referral.FirstName = FirstName;
        referral.LastName = LastName;
        referral.DateOfBirth = DateOfBirth!.Value;
        referral.DateOnReferral = DateOnReferral!.Value;
        referral.DateReferralReceived = DateReferralRecieved!.Value;
        referral.ReferralType = ReferralType;
        referral.NHSNumber = NHSNumber;

        referral.TransferRegion = ShowTransferRegion ? TransferRegion : null;
    }
    private void okButton_Click(object sender, RoutedEventArgs e)
    {
        ValidateAll((Variable)(-1));

        if (HasErrors)
        {
            errorTracker.LogErrors();
            return;
        }

        DialogResult = true;
    }

    private void cancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;


    void OnErrorsChanged(object? obj, DataErrorsChangedEventArgs args) => ErrorsChanged?.Invoke(obj, args);

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    public event PropertyChangedEventHandler? PropertyChanged;


    [Flags]
    public enum Variable
    {
        HopNum = 0b0000_0001,
        FirstName = 0b0000_0010,
        LastName = 0b0000_0100,
        DOB = 0b0000_1000,
        DOR = 0b0001_0000,
        DRR = 0b0010_0000,
        ReferralType = 0b0100_0000,
        NHSNumber = 0b1000_0000,

    }
    string hospitalNumber = "";
    //Every field needs a containing property with get and set for WPF to bind to it
    //Set command notifies other controls of the change and validates every variable
    //That is related to this control
    public string HospitalNumber
    {
        get => hospitalNumber; set
        {
            hospitalNumber = value;
            ValidateAll(Variable.HopNum);
            NotifyPropertyChanged();
        }
    }

    private string referralType = "";
    public string ReferralType
    {
        get => referralType; set
        {
            referralType = value;
            ValidateAll(Variable.ReferralType);
            NotifyPropertyChanged();
        }
    }

    private string nhsNumber = "";
    public string NHSNumber
    {
        get => nhsNumber; set
        {
            nhsNumber = value;
            ValidateAll(Variable.NHSNumber);
            NotifyPropertyChanged();
        }
    }


    string firstName = "";
    public string FirstName
    {
        get => firstName; set
        {
            firstName = value;
            ValidateAll(Variable.FirstName);
            NotifyPropertyChanged();
        }
    }

    string lastName = "";
    public string LastName
    {
        get => lastName; set
        {
            lastName = value;
            ValidateAll(Variable.LastName);
            NotifyPropertyChanged();
        }
    }

    string? transferRegion;
    public string? TransferRegion
    {
        get => transferRegion; set
        {
            transferRegion = value;
            NotifyPropertyChanged();
        }
    }



    DateTime? dateOfBirth;

    public DateTime? DateOfBirth
    {
        get => dateOfBirth; set
        {

            dateOfBirth = value;
            ValidateAll(Variable.DOB);
            NotifyPropertyChanged();
        }
    }
    DateTime? dateOnReferral;

    public DateTime? DateOnReferral
    {
        get => dateOnReferral; set
        {
            dateOnReferral = value;
            ValidateAll(Variable.DOR);
            NotifyPropertyChanged();
        }
    }
    DateTime? dateReferralRecieved;

    public DateTime? DateReferralRecieved
    {
        get => dateReferralRecieved; set
        {
            dateReferralRecieved = value;
            ValidateAll(Variable.DRR);
            NotifyPropertyChanged();
        }
    }
    ///<summary>
    ///Validate all variables related to the variables changed
    /// </summary>
    public void ValidateAll(Variable changed)
    {
        Debug.WriteLine("Validating");
        if (changed.HasFlag(Variable.HopNum))
        {
            errorTracker.ValidateString(HospitalNumber, nameof(HospitalNumber));
            errorTracker.ValidatelNumber(HospitalNumber, nameof(HospitalNumber));
        }

        if (changed.HasFlag(Variable.FirstName))
            errorTracker.ValidateString(FirstName, nameof(FirstName));

        if (changed.HasFlag(Variable.LastName))
            errorTracker.ValidateString(LastName, nameof(LastName));

        if (changed.HasFlag(Variable.ReferralType))
            errorTracker.ValidateString(ReferralType, nameof(ReferralType));


        if (changed.HasFlag(Variable.DOB))
            errorTracker.ValidateNotNull(DateOfBirth, nameof(DateOfBirth));

        if (changed.HasFlag(Variable.DOR))
            errorTracker.ValidateNotNull(DateOnReferral, nameof(DateOnReferral));

        if (changed.HasFlag(Variable.DRR))
            errorTracker.ValidateNotNull(DateReferralRecieved, nameof(DateReferralRecieved));


        if (changed.HasFlag(Variable.DOB) || changed.HasFlag(Variable.DOR))
            errorTracker.ValidateDateOrder(DateOfBirth, nameof(DateOfBirth), DateOnReferral, nameof(DateOnReferral));

        if (changed.HasFlag(Variable.DOR) || changed.HasFlag(Variable.DRR))
            errorTracker.ValidateDateOrder(DateOnReferral, nameof(DateOnReferral), DateReferralRecieved, nameof(DateReferralRecieved));


        if (changed.HasFlag(Variable.NHSNumber))
        {
            string num = ReplaceWhitespace(NHSNumber);
            bool valid = num.Length == 10;
            errorTracker.AddError(nameof(NHSNumber), "NHS number must contain 10 digits",!valid);
            if (valid)
            {
                //Add spaces for formatting NHS number as XXX-XXX-XXXX
                nhsNumber = num.Insert(3, " ").Insert(7, " ");
                NotifyPropertyChanged(nameof(NHSNumber));
            }
        }

        ShowTransferRegion = ReferralType.ToLower().Contains("relocation");

        NotifyPropertyChanged(nameof(ShowTransferRegion));
    }

    private static readonly Regex sWhitespace = new Regex(@"\s+");
    public static string ReplaceWhitespace(string input)
    {
        return sWhitespace.Replace(input, "");
    }

    public bool ShowTransferRegion { get; set; } = false;

    public IEnumerable GetErrors(string? propertyName) => errorTracker.GetErrors(propertyName);

    public bool HasErrors => errorTracker.HasErrors;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        Debug.WriteLine(propertyName);

        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
