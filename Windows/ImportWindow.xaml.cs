
using System.Text.Json;
using System.IO;
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

    public IEnumerable<PatientReferral> Allocation
    {
        get => AllInFile!.Where(r => !r.Archived || r.Archived == ImportArchive);
    }
    public IEnumerable<PatientReferral> Duplicates
    {
        get => AllInFile!.Where(r => !r.Archived || r.Archived == ImportArchive).Intersect(Data.Context.patientReferrals!);
    }
    public IEnumerable<PatientReferral> Uniques
    {
        get => AllInFile!.Where(r => !r.Archived || r.Archived == ImportArchive).Except(Data.Context.patientReferrals!);
    }
    public int ImportCount
    {
        get
        {
            NotifyPropertyChanged(nameof(DuplicateCount));
            return Uniques.Count();
        }
        set { }
    }

    public int DuplicateCount
    {
        get
        {
            if (OverrideDuplicates)
                return 0;
            else

                return Duplicates.Count();
        }
        set { }
    }


    public bool OverrideDuplicates
    {
        get => overrideDuplicates; set
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
            _ => null,
        };


        if (AllInFile != null)
            Debug.WriteLine($"Imported from {FileName}");
        else
        {
            Debug.WriteLine($"Error Imported from {FileName}");
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

        // Your code here.
        if (AllInFile == null)
            DialogResult = false;
    }



    public void SubmitButton_Clicked(object sender, RoutedEventArgs e)
    {

        Data.Context.patientReferrals!.AddRange(Uniques);
        

        DialogResult = true;
    }

    public PatientReferral[]? ImportJson()
    {
        using Stream stream = File.Open(FileName, FileMode.Open);

        return JsonSerializer.Deserialize<PatientReferral[]>(stream,ExportWindow.jsonOptions);
    }

    public void CancelButton_Clicked(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

}

