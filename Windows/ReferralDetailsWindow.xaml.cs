

using IntakeTrackerApp.DataManagement;
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
		PatientReferral referral,
		Vault v)
	{
		viewModel.v = v;
		viewModel.HospitalNumber = referral.LocalHospitalNumber;
		viewModel.ReferralType = referral.ReferralType;
		viewModel.FirstName = referral.FirstName;
		viewModel.LastName = referral.LastName;
		viewModel.DateOfBirth = referral.DateOfBirth;
		viewModel.DateOnReferral = referral.DateOnReferral;
		viewModel.DateReferralRecieved = referral.DateReferralReceived;
		viewModel.NHSNumber = referral.NHSNumberKey.ToString("000 000 0000");
		viewModel.TransferRegion = referral.TransferRegion;
		viewModel.Weight = referral.Weight;
		viewModel.Height = referral.Height;

		Debug.WriteLine($"Updating core details: height:{viewModel.Height}, weight: {viewModel.Weight}");
		InitializeComponent();
		Windows.WindowUtility.HideMinimizeAndMaximizeButtons(this);

		viewModel.ValidateAll();
	}
	/// <summary>
	/// Applies every value EXECPT hospital number which is a key so must be done separately
	/// </summary>
	/// <param name="referral"></param>
	public void PopulateReferral(PatientReferral referral)
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
		referral.Weight = viewModel.Weight;
		referral.Height = viewModel.Height;

		Debug.WriteLine($"Updating core details: height:{referral.Height}, weight: {referral.Weight}");
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
	public Vault v { get; set; }

	private string hospitalNumber = "";
	//Every field needs a containing property with get and set for WPF to bind to it
	//Set command notifies other controls of the change and validates every variable
	//That is related to this control
	public string HospitalNumber
	{
		get => hospitalNumber;
		set => SetProperty(ref hospitalNumber, value);
	}

	private string referralType = "";
	public string ReferralType
	{
		get => referralType;
		set => SetProperty(ref referralType, value);
	}

	private string nhsNumber = "";
	public string NHSNumber
	{
		get => nhsNumber;
		set => SetProperty(ref nhsNumber, value);
	}


	private string firstName = "";
	public string FirstName
	{
		get => firstName;
		set => SetProperty(ref firstName, value);
	}

	private string lastName = "";
	public string LastName
	{
		get => lastName;
		set => SetProperty(ref lastName, value);
	}

	private string? transferRegion;
	public string? TransferRegion
	{
		get => transferRegion;
		set => SetProperty(ref transferRegion, value);
	}
	private double? weight;
	public double? Weight
	{
		get => weight;
		set => SetProperty(ref weight, value);
	}
	private double? height;
	public double? Height
	{
		get => height;
		set => SetProperty(ref height, value);
	}

	private DateTime? dateOfBirth;
	public DateTime? DateOfBirth
	{
		get => dateOfBirth;
		set => SetProperty(ref dateOfBirth, value);
	}

	private DateTime? dateOnReferral;
	public DateTime? DateOnReferral
	{
		get => dateOnReferral;
		set => SetProperty(ref dateOnReferral, value);
	}

	private DateTime? dateReferralRecieved;
	public DateTime? DateReferralRecieved
	{
		get => dateReferralRecieved;
		set => SetProperty(ref dateReferralRecieved, value);
	}


	public bool ShowTransferRegion { get; set; } = false;
	public ulong FormatNHSNumber()
	{
		return ulong.Parse(ReplaceWhitespace(NHSNumber));
	}
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
				checksum = NHSNum.IsValid(numericNHSNum);
			}

			nhsNumber = num.Insert(3, " ").Insert(7, " ");
			NotifyPropertyChanged(nameof(NHSNumber), false);
		}

		AddError(nameof(NHSNumber), "Must contain 10 digits", !valid);
		AddError(nameof(NHSNumber), "Must be numeric", !isNumeric);
		AddError(nameof(NHSNumber), "Invalid NHS Number", !checksum);

		ShowTransferRegion = ReferralType.ToLower().Contains("relocation");

		NotifyPropertyChanged(nameof(ShowTransferRegion), false);
	}


	private static readonly Regex sWhitespace = new(@"\s+|-");
	public static string ReplaceWhitespace(string input)
	{
		return sWhitespace.Replace(input, "");
	}

}

