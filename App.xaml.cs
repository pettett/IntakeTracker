using IntakeTrackerApp.DataManagement;
using System.Globalization;
using System.Threading;
using System.Windows.Markup;

namespace IntakeTrackerApp;


/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

	public AppSettings Settings { get; init; }

	public App()
	{
		Debug.WriteLine("Loading App");

		Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
		Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
		FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
					XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

		AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(App_DispatcherUnhandledException);

		// Load the global app settings

		Settings = new AppSettings();



	}

	public async void SetLastUsedVault(Vault v)
	{
		Debug.Print($"New open vault {v.Name}");
		Settings.Settings.CurrentOpenVault = v.Dir;
		await Settings.SaveSettingsAsync();
	}

	private void App_DispatcherUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		// Process unhandled exception

		// Prevent default unhandled exception processing

		MessageBox.Show($"Unhandled Exception\n {e.ExceptionObject}",
			"Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);
	}
}

