using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;

namespace IntakeTrackerApp;

using Windows;
using Controls;

public class DateOnlyConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        Debug.WriteLine($"Using converter from {value}");

        if (value == null)
            return null;

        if (DateOnly.TryParse(value as string, out var r))
        {
            return r;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ((DateOnly)value).ToString();
    }
}

public interface ITabable
{
    bool CanClose { get; }
    string Header { get; }
    object GenerateContent();
    void OnOpened();
}

public record ReferralTab(PatientReferral Referral) : ITabable
{
    public bool CanClose => true;
    public string Header => Referral.Name;

    public object GenerateContent()
    {
        return new PatientView(Referral);
    }
    public void OnOpened()
    {

    }
}

public record SummaryTab(TestType TestName, bool IncludeNone) : ITabable
{
    public bool CanClose => !IncludeNone;
    public string Header => $"{TestName} Summary";
    TestSummary? s;
    public object GenerateContent()
    {
        s = new TestSummary(TestName, IncludeNone);
        return s;
    }
    public void OnOpened()
    {
        s?.Refresh();
    }
}

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    public class FilteredColumnHeading : DependencyObject
    {
        public static readonly DependencyProperty FilterProperty = DependencyProperty.Register(
            "Filter", typeof(string), typeof(FilteredColumnHeading), new FrameworkPropertyMetadata("")
            {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                PropertyChangedCallback = FilterChanged,

            });
        public static void FilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Debug.WriteLine($"Filtered, {((FilteredColumnHeading)d).Filter}");
            Singleton?.view.Refresh();
        }

        public string Heading { get; set; } = "";
        public string Filter
        {
            get => (string)GetValue(FilterProperty); set => SetValue(FilterProperty, value);
        }

        public bool FilterText(string text)
        {
            return Filter == "" || Filter != "" && text.ToLower().Contains(Filter.ToLower());
        }
    }
    public class CloseButton : Button
    {
        public CloseButton(ITabable tab, TabItem tabItem, MainWindow context)
        {
            Tab = tab;
            TabItem = tabItem;
            Context = context;
        }

        readonly ITabable Tab;
        readonly TabItem TabItem;
        readonly MainWindow Context;
        protected override void OnClick()
        {
            Context.CloseTab(Tab, TabItem);
            base.OnClick();
        }
    }
    public static readonly RoutedUICommand NewReferralCmd
= new("New Referral command", "NewReferral", typeof(MainWindow));

    public static readonly RoutedUICommand OpenMRICommand
        = new("Open MRI Command", "OpenMRICommand", typeof(MainWindow));

    public static readonly RoutedUICommand OpenEPCommand
        = new("Open EP Command", "OpenEPCommand", typeof(MainWindow));

    public static readonly RoutedUICommand OpenLPCommand
        = new("Open LP Command", "OpenLPCommand", typeof(MainWindow));

    public static readonly RoutedUICommand OpenBloodsCommand
         = new("Open Bloods Command", "OpenBloodsCommand", typeof(MainWindow));


    public static readonly RoutedUICommand ExportCommand
         = new("Export Command", "ExportCommand", typeof(MainWindow));
    public static readonly RoutedUICommand ImportCommand
         = new("Import Command", "ImportCommand", typeof(MainWindow));

    public static readonly RoutedUICommand SettingsCommand
         = new("Settings Command", "SettingsCommand", typeof(MainWindow));


    public UndoCommand UndoCommand { get; set; } = new();

    public double PopupHeight => Height;


    ListCollectionView view;
    public Data Context { get; set; } = Data.Singleton;

    List<ITabable> openTabs = new();
    public static MainWindow? Singleton { get; private set; }
    //Currently selected patient
    PatientReferral? selected;
    public PatientReferral? Selected
    {
        get => selected;
        set
        {
            selected = value;
            if (selected is not null)
            {
                OpenReferral(selected);
            }
        }
    }



    public FilteredColumnHeading FirstNameColumn { get; set; } = new() { Heading = "First Name" };
    public FilteredColumnHeading LastNameColumn { get; set; } = new() { Heading = "Last Name" };

    public bool IncludeArchived
    {
        get => Context.IncludeArchive;
        set
        {
            if (!value || value && MessageBox.Show("Include Archive", "Are you sure you want to include archived data?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                Context.IncludeArchive = value;

            }
        }
    }


    public MainWindow()
    {
        Singleton = this;
        //LoadAllPatients();
        Context = Data.Singleton;
        Debug.WriteLine("Loading Main Window...");
        InitializeComponent();
        Debug.WriteLine("Loaded Main Window...");

        TabScreen.SelectionChanged += TabScreen_SelectionChanged;

        view = (ListCollectionView)CollectionViewSource.GetDefaultView(PatientsList.ItemsSource);
        view.Filter = ReferralFilter;
        view.CustomSort = p;
        AddTab(new SummaryTab(TestType.All, true));
    }
    private bool ReferralFilter(object item)
    {
        if (item is PatientReferral r)
        {
            return FirstNameColumn.FilterText(r.FirstName) &&
                   LastNameColumn.FilterText(r.LastName);

        }
        else
        {
            return false;
        }
    }


    public void CloseTab(ITabable referral, TabItem tab)
    {
        if (openTabs.Contains(referral))
        {
            openTabs.Remove(referral);
            TabScreen.Items.Remove(tab);
        }
    }

    public void OpenReferral(PatientReferral referral)
    {

        AddTab(new ReferralTab(referral));
    }
    public void AddTab(ITabable tab)
    {

        //Tabs.Add(new ReferralTab(referral));
        //index = 0;
        //return;
        var i = openTabs.IndexOf(tab);
        if (i == -1)
        {
            Debug.WriteLine("Opening Tab");
            openTabs.Add(tab);


            var item = new TabItem();
            if (tab.CanClose)
            {
                StackPanel stack = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                };

                stack.Children.Add(new TextBlock() { Text = tab.Header });
                stack.Children.Add(new CloseButton(tab, item, this)
                {
                    Content = new Image()
                    {
                        Source = new BitmapImage(new Uri("/Images/Close_16x.png", UriKind.Relative)),
                        Width = 16,
                        Height = 16,
                    },
                    Width = 20,
                    Height = 20,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0, 0, 0, 0),
                    Margin = new Thickness(8, 0, 0, 0),
                });
                item.Header = stack;
            }
            else
            {
                item.Header = new TextBlock() { Text = tab.Header };
            }

            item.Content = tab.GenerateContent();

            TabScreen.Items.Add(item);

            //Immediately switch to this new tab
            TabScreen.SelectedIndex = TabScreen.Items.Count - 1;
        }
        else
        {

            //Switch to the existing tab
            TabScreen.SelectedIndex = i;
        }



    }



    private void PatientsList_MouseDown(object sender, MouseButtonEventArgs e)
    {
        HitTestResult r = VisualTreeHelper.HitTest(this, e.GetPosition(this));
        if (r.VisualHit.GetType() != typeof(ListBoxItem))
            PatientsList.UnselectAll();
    }
    public static ListSortDirection? NextDir(ListSortDirection? direction) => direction switch
    {
        ListSortDirection.Ascending => ListSortDirection.Descending,
        ListSortDirection.Descending => null,
        null => ListSortDirection.Descending,
        _ => throw new NotImplementedException(),
    };

    public class PatientComparer : IComparer
    {
        bool direction;
        string comparing = "";
        public string Comparing
        {
            get => comparing; set
            {
                if (comparing == value)
                {
                    direction = !direction;
                }
                comparing = value;
            }
        }
        public int Compare(object? x, object? y)
        {
            if (x is PatientReferral r1 && y is PatientReferral r2)
            {
                if (direction)//reverse
                    (r2, r1) = (r1, r2);


                return comparing switch
                {
                    "DateReferralReceived" => r1.DateReferralReceived.CompareTo(r2.DateReferralReceived),
                    "FirstName" => r1.FirstName.CompareTo(r2.FirstName),
                    "LastName" => r1.LastName.CompareTo(r2.LastName),
                    _ => 0,
                };
            }
            else
            {

                return 0;
            }

        }

    }
    PatientComparer p = new();
    public void ChangeSortingState(string columnName)
    {
        using (view.DeferRefresh())
        {
            p.Comparing = columnName;
        }
        view.Refresh();
    }

    public void ReferralRecievedColumnHeader_Click(object sender, RoutedEventArgs e) => ChangeSortingState("DateReferralReceived");
    public void FirstNameColumnHeader_Click(object sender, RoutedEventArgs e) => ChangeSortingState("FirstName");
    public void LastNameColumnHeader_Click(object sender, RoutedEventArgs e) => ChangeSortingState("LastName");





    void TabScreen_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.RemovedItems.Count > 0)
            openTabs[TabScreen.SelectedIndex].OnOpened();

    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // clean up database connections
        if (!Context.IsSaved)
        {
            var choice = MessageBox.Show("Save data?", "Quitting", MessageBoxButton.YesNoCancel);
            if (choice == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            else if (choice == MessageBoxResult.Yes)
            {
                Context.SaveAndQuit();
                base.OnClosing(e);
                return;
            }

        }

        Context.Close();


        base.OnClosing(e);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    // This method is called by the Set accessor of each property.  
    // The CallerMemberName attribute that is applied to the optional propertyName  
    // parameter causes the property name of the caller to be substituted as an argument.  
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    private void NewReferral_Executed(object sender, RoutedEventArgs e)
    {
        // FrameworkElement fe = sender as FrameworkElement;
        //if (canOpenPopop)
        //NewPatientPopup.IsOpen = true;

        var dialogue = new ReferralDetailsWindow();
        var result = dialogue.ShowDialog();
        if (result == true)
        {
            if (MessageBox.Show("Enter Patient", "Confirm Entry", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
            {
                return;
            }


            PatientReferral p = new()
            {
                HospitalNumber = ulong.Parse(dialogue.HospitalNumber)
            };

            dialogue.ApplyDateToReferral(p);

            Data.Singleton.Add(p);
            PatientsList.Items.Refresh();


        }
    }
    private void Save_Executed(object sender, RoutedEventArgs e)
    {
        Context.Save();
    }

    private void OpenMRI_Executed(object sender, RoutedEventArgs e) => OpenSummary_Executed(TestType.MRI);
    private void OpenMRI_CanExecute(object sender, CanExecuteRoutedEventArgs e) => OpenSummary_CanExecute(TestType.MRI, e);

    private void OpenLP_Executed(object sender, RoutedEventArgs e) => OpenSummary_Executed(TestType.LP);
    private void OpenLP_CanExecute(object sender, CanExecuteRoutedEventArgs e) => OpenSummary_CanExecute(TestType.LP, e);

    private void OpenEP_Executed(object sender, RoutedEventArgs e) => OpenSummary_Executed(TestType.EP);
    private void OpenEP_CanExecute(object sender, CanExecuteRoutedEventArgs e) => OpenSummary_CanExecute(TestType.EP, e);

    private void OpenBloods_Executed(object sender, RoutedEventArgs e) => OpenSummary_Executed(TestType.Bloods);
    private void OpenBloods_CanExecute(object sender, CanExecuteRoutedEventArgs e) => OpenSummary_CanExecute(TestType.Bloods, e);

    private void OpenSummary_Executed(TestType type) => AddTab(new SummaryTab(type, false));
    private void OpenSummary_CanExecute(TestType type, CanExecuteRoutedEventArgs e)
    {
        bool open = openTabs.Any(x => x is SummaryTab t && t.TestName == type);
        e.CanExecute = !open;
    }
    public void ConfigureDialog(FileDialog dialog)
    {
        dialog.FileName = $"Exported Data {DateTime.Now.Year}_{DateTime.Now.Month}_{DateTime.Now.Day}"; // Default file extension
        dialog.DefaultExt = ".json"; // Default file extension
        dialog.Filter = "JSON files|*.json"; // Filter files by extension
    }

    private void Import_Executed(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        ConfigureDialog(openFileDialog);
        if (openFileDialog.ShowDialog() == true)
        {
            ImportWindow importSettings = new(openFileDialog.FileName);
            importSettings.ShowDialog();
        }
    }
    private void Export_Executed(object sender, RoutedEventArgs e)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        ConfigureDialog(saveFileDialog);

        if (saveFileDialog.ShowDialog() == true)
        {
            Windows.ExportWindow exportSettings = new(saveFileDialog.FileName);
            exportSettings.ShowDialog();
        }
    }

    private void Crash_Click(object sender, RoutedEventArgs e)
    {
        throw new("Program has crashed. Oh no!");
    }
    private void Concurrency_Click(object sender, RoutedEventArgs e)
    {
        // Change the person's name in the database to simulate a concurrency conflict
        Data.Context.Database.ExecuteSqlRaw(
            "UPDATE patientReferrals SET BloodTestResults = 'Simulated Conflict Data' WHERE FirstName = 'test'");
    }
    private void OpenSettings_Executed(object sender, RoutedEventArgs e)
    {
        SettingsWindow s = new SettingsWindow();

        s.ShowDialog();
    }
}



