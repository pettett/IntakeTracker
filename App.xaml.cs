using System.Globalization;
using System.Threading;
using System.Windows.Markup;

namespace IntakeTrackerApp;


/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	public App()
	{
		Debug.WriteLine("Loading App");

		Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
		Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");
		FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
					XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

		AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(App_DispatcherUnhandledException);

	}

	private void App_DispatcherUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		// Process unhandled exception

		// Prevent default unhandled exception processing

		MessageBox.Show($"Unhandled Exception - please send screen shot to Max if this wasn't supposed to happen:\n {e.ExceptionObject}",
			"Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);
	}
}

