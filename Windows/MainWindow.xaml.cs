namespace IntakeTrackerApp.Windows;

using IntakeTrackerApp.Controls;
using IntakeTrackerApp.DataManagement;



/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	public MainWindow()
	{

		InitializeComponent();

		// Open the last vault if exists, else open launcher
		var a = (App)Application.Current;

		var lv = a.Settings.Settings.CurrentOpenVault;

		Debug.WriteLine($"Last used vault {lv}");

		Vault? v = null;

		if (lv != null)
		{
			v = new Vault(lv);

			if (!v.CheckValid())
			{
				v = null;
			}
		}


		if (v != null)
		{
			Content = new VaultViewControl(v);
		}
		else
		{
			Content = new StartupControl();
		}
	}


	protected override void OnClosing(CancelEventArgs e)
	{
		if (Data.Singleton == null) return;

		// clean up database connections
		if (!Data.Singleton.IsSaved)
		{
			var choice = MessageBox.Show("Save data?", "Quitting", MessageBoxButton.YesNoCancel);
			if (choice == MessageBoxResult.Cancel)
			{
				e.Cancel = true;
				return;
			}
			else if (choice == MessageBoxResult.Yes)
			{
				Data.Singleton.SaveAndQuit();
				base.OnClosing(e);
				return;
			}
		}

		Data.Singleton.Close();


		base.OnClosing(e);
	}
}



