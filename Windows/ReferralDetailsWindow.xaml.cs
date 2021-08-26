

using System.Text.RegularExpressions;

namespace IntakeTrackerApp;


/// <summary>
/// Interaction logic for NewPatientReferralWindow.xaml
/// </summary>
public partial class ReferralDetailsWindow : Window
{
	public ReferralDetailsViewModel viewModel { get; set; } = new();
	public ReferralDetailsWindow()
	{
		InitializeComponent();
		Windows.WindowUtility.HideMinimizeAndMaximizeButtons(this);
	}

	public ReferralDetailsWindow(
		PatientReferral referral)
	{
		viewModel.HospitalNumber = referral.LocalHospitalNumber;
		viewModel.ReferralType = referral.ReferralType;
		viewModel.FirstName = referral.FirstName;
		viewModel.LastName = referral.LastName;
		viewModel.DateOfBirth = referral.DateOfBirth;
		viewModel.DateOnReferral = referral.DateOnReferral;
		viewModel.DateReferralRecieved = referral.DateReferralReceived;
		viewModel.NHSNumber = referral.NHSNumberKey.ToString("000 000 0000");
		viewModel.TransferRegion = referral.TransferRegion;

		InitializeComponent();
		Windows.WindowUtility.HideMinimizeAndMaximizeButtons(this);

		viewModel.ValidateAll();
	}
	/// <summary>
	/// Applies every value EXECPT hospital number which is a key so must be done separately
	/// </summary>
	/// <param name="referral"></param>
	public void ApplyDateToReferral(PatientReferral referral)
	{
		//  referral.HospitalNumber = ulong.Parse(HospitalNumber);

		referral.FirstName = viewModel.FirstName;
		referral.LastName = viewModel.LastName;
		referral.DateOfBirth = viewModel.DateOfBirth!.Value;
		referral.DateOnReferral = viewModel.DateOnReferral!.Value;
		referral.DateReferralReceived = viewModel.DateReferralRecieved!.Value;
		referral.ReferralType = viewModel.ReferralType;
		referral.LocalHospitalNumber = viewModel.HospitalNumber;

		referral.TransferRegion = viewModel.ShowTransferRegion ? viewModel.TransferRegion : null;
	}
	private void okButton_Click(object sender, RoutedEventArgs e)
	{
		viewModel.ValidateAll();

		if (!viewModel.HasErrors)
		{
			DialogResult = true;
		}

	}

	private void cancelButton_Click(object sender, RoutedEventArgs e) => DialogResult = false;





}
public class ReferralDetailsViewModel : ErrorTrackerViewModelBase
{
	string hospitalNumber = "";
	//Every field needs a containing property with get and set for WPF to bind to it
	//Set command notifies other controls of the change and validates every variable
	//That is related to this control
	public string HospitalNumber
	{
		get => hospitalNumber; set
		{
			hospitalNumber = value;
			ValidateAll();
			NotifyPropertyChanged();
		}
	}

	private string referralType = "";
	public string ReferralType
	{
		get => referralType; set
		{
			referralType = value;
			ValidateAll();
			NotifyPropertyChanged();
		}
	}

	private string nhsNumber = "";
	public string NHSNumber
	{
		get => nhsNumber; set
		{
			nhsNumber = value;
			ValidateAll();
			NotifyPropertyChanged();
		}
	}
	public ulong FormatNHSNumber()
	{
		return ulong.Parse(ReplaceWhitespace(NHSNumber));
	}


	string firstName = "";
	public string FirstName
	{
		get => firstName; set
		{
			firstName = value;
			ValidateAll();
			NotifyPropertyChanged();
		}
	}

	string lastName = "";
	public string LastName
	{
		get => lastName; set
		{
			lastName = value;
			ValidateAll();
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
			ValidateAll();
			NotifyPropertyChanged();
		}
	}
	DateTime? dateOnReferral;

	public DateTime? DateOnReferral
	{
		get => dateOnReferral; set
		{
			dateOnReferral = value;
			ValidateAll();
			NotifyPropertyChanged();
		}
	}
	DateTime? dateReferralRecieved;

	public DateTime? DateReferralRecieved
	{
		get => dateReferralRecieved; set
		{
			dateReferralRecieved = value;
			ValidateAll();
			NotifyPropertyChanged();
		}
	}


	public bool ShowTransferRegion { get; set; } = false;
	///<summary>
	///Validate all variables related to the variables changed
	/// </summary>
	public override void ValidateAll()
	{
		Debug.WriteLine("Validating...");

		ValidateString(HospitalNumber, nameof(HospitalNumber));


		ValidateString(FirstName, nameof(FirstName));

		ValidateString(LastName, nameof(LastName));

		ValidateString(ReferralType, nameof(ReferralType));


		ValidateNotNull(DateOfBirth, nameof(DateOfBirth));

		ValidateNotNull(DateOnReferral, nameof(DateOnReferral));

		ValidateNotNull(DateReferralRecieved, nameof(DateReferralRecieved));


		ValidateDateOrder(DateOfBirth, nameof(DateOfBirth), DateOnReferral, nameof(DateOnReferral));

		ValidateDateOrder(DateOnReferral, nameof(DateOnReferral), DateReferralRecieved, nameof(DateReferralRecieved));

		const int nhsModulo = 11;
		const int nhsLength = 10;

		string num = ReplaceWhitespace(NHSNumber);
		bool valid = num.Length == nhsLength;
		//if it is not valid it cannot be numeric
		ulong numericNHSNum = 0;
		bool isNumeric = !valid || ulong.TryParse(num, out numericNHSNum);

		bool checksum = true; //dont bother with showing this error if not valid
		if (valid)
		{
			//Add spaces for formatting NHS number as XXX-XXX-XXXX

			if (isNumeric)
			{
				//apply the very simple modulo11 checksum
				static int GetDigit(ulong num, int n) => (int)(num / (ulong)Math.Pow(10, n) % 10);

				//loop through every value except check (index 9)

				//Get digit from numeric number - will be in range  0-9

				//Sum them to later find checkup

				int sum = Enumerable.Range(0, nhsLength).Select(i => GetDigit(numericNHSNum, i) * (10 - i)).Sum();

				//If check is 10 number is always invalid, but value 11 should be 0
				int check = (nhsModulo - sum % nhsModulo) % nhsModulo;

				checksum = GetDigit(numericNHSNum, nhsModulo - 1) == check;
			}

			nhsNumber = num.Insert(3, " ").Insert(7, " ");
			NotifyPropertyChanged(nameof(NHSNumber));
		}

		AddError(nameof(NHSNumber), "Must contain 10 digits", !valid);
		AddError(nameof(NHSNumber), "Must be numeric", !isNumeric);
		AddError(nameof(NHSNumber), "Invalid NHS Number", !checksum);

		ShowTransferRegion = ReferralType.ToLower().Contains("relocation");

		NotifyPropertyChanged(nameof(ShowTransferRegion));
	}


	private static readonly Regex sWhitespace = new (@"\s+|-");
	public static string ReplaceWhitespace(string input)
	{
		return sWhitespace.Replace(input, "");
	}

}

