namespace IntakeTrackerApp;


/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        Debug.WriteLine("Loading App");

        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(App_DispatcherUnhandledException);

    }
    void App_DispatcherUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // Process unhandled exception

        // Prevent default unhandled exception processing

        MessageBox.Show($"Unhandled Exception - please send screen shot to Max if this wasn't supposed to happen:\n {e.ExceptionObject}",
            "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);
    }
}

