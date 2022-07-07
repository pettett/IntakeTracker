
using System.Text.Json;
using System.IO;
using System.Data;
using IntakeTrackerApp.Data;

namespace IntakeTrackerApp.Windows;

/// <summary>
/// Interaction logic for ImportWindow.xaml
/// </summary>
public partial class ImportWindow : Window, INotifyPropertyChanged
{
	private bool overrideDuplicates;
	private bool importArchive;
	private bool unArchiveImported;

	public string FileName { get; set; }
	public string FileType => System.IO.Path.GetExtension(FileName)!;
	public PatientReferral[]? AllInFile { get; set; }

	public IEnumerable<PatientReferral> Allocation =>
		AllInFile!.Where(r => !r.Archived || r.Archived == ImportArchive);
	public IEnumerable<PatientReferral> Duplicates =>
		AllInFile!.Where(r => !r.Archived || r.Archived == ImportArchive).Intersect(Data.Context.patientReferrals!);
	public IEnumerable<PatientReferral> Uniques =>
		AllInFile!.Where(r => !r.Archived || r.Archived == ImportArchive).Except(Data.Context.patientReferrals!);

	public int ImportCount
	{
		get
		{
			NotifyPropertyChanged(nameof(DuplicateCount));
			return AllInFile == null ? 0 : Uniques.Count();
		}
		set { }
	}

	public int DuplicateCount
	{
		get => OverrideDuplicates ? 0 : AllInFile == null ? 0 : Duplicates.Count();
		set { }
	}


	public bool OverrideDuplicates
	{
		get => overrideDuplicates; 
		set
		{
			overrideDuplicates = value;

			NotifyPropertyChanged(nameof(ImportCount));
		}
	}
	public bool ImportArchive
	{
		get => importArchive;
		set
		{
			importArchive = value;
			NotifyPropertyChanged();
			NotifyPropertyChanged(nameof(UnArchiveImported));
			NotifyPropertyChanged(nameof(ImportCount));
		}
	}
	public bool UnArchiveImported { get => unArchiveImported && importArchive; set => unArchiveImported = value; }


	public ImportWindow(string fileName)
	{
		FileName = fileName;
		AllInFile = FileType switch
		{
			".json" => ImportJson(),
			".xlsx" => ImportSpreadsheet(),
			_ => null,
		};


		if (AllInFile != null)
			Debug.WriteLine($"Imported from {FileName}");
		else
		{
			Debug.WriteLine($"Error Importing from {FileName}");
		}


		InitializeComponent();
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected override void OnContentRendered(EventArgs e)
	{
		base.OnContentRendered(e);

		// Close the window if there was an error
		if (AllInFile == null)
		{
			MessageBox.Show("Error importing file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			DialogResult = false;
		}
	}



	public void SubmitButton_Clicked(object sender, RoutedEventArgs e)
	{

		Data.Context.patientReferrals!.AddRange(Uniques);


		DialogResult = true;
	}

	public PatientReferral[]? ImportJson()
	{
		using Stream stream = File.Open(FileName, FileMode.Open);

		return JsonSerializer.Deserialize<PatientReferral[]>(stream, ExportWindow.jsonOptions);
	}

	public PatientReferral[]? ImportSpreadsheet()
	{
		using Handlers.SpreadsheetHandler spreadsheet = new(FileName, true);
		PatientReferral[] result = spreadsheet.LoadData<PatientReferral>();

		//PatientReferral[] result = new PatientReferral[t.Rows.Count];
	//	int i = 0;

	//	var properties = typeof(PatientReferral).GetProperties().Where(x => x.CanWrite && x.CanRead).ToArray();

	//	foreach (DataRow r in t.Rows)
	//	{
			//set values for every field
		//	result[i] = new();
	//		foreach (var property in properties)
	//		{
	//			property.SetValue(result[i], r[property.Name]);
	//		}

	//		i++;
	//	}

		return result;
	}


	public void CancelButton_Clicked(object sender, RoutedEventArgs e)
	{
		DialogResult = false;
	}

}

