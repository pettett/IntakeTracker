using IntakeTrackerApp.DataManagement;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;
using System.IO;

namespace IntakeTrackerApp.Controls
{
	/// <summary>
	/// Interaction logic for StartupControl.xaml
	/// </summary>
	public partial class StartupControl : UserControl
	{
		string ObsoleteDatabaseName => Path.Join(AppDomain.CurrentDomain.BaseDirectory, "patientReferrals.db");
		public bool CanMigrateData => File.Exists(ObsoleteDatabaseName);
		public Visibility MigrationVisibility => CanMigrateData switch { true => Visibility.Visible, false => Visibility.Collapsed };
		public ObservableCollection<Vault> RecentVaults { get => ((App)Application.Current).Settings.allRecentVaults; }

		public StartupControl()
		{
			DataContext = this;
			InitializeComponent();
		}


		private void Create_Click(object sender, RoutedEventArgs e)
		{

		}

		private void Open_Click(object sender, RoutedEventArgs e)
		{

		}

		private async void Migrate_Click(object sender, RoutedEventArgs e)
		{
			CommonOpenFileDialog dialog = new();
			dialog.InitialDirectory = "C:\\Users";
			dialog.IsFolderPicker = true;
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
				if (Directory.GetFiles(dialog.FileName).Length > 0 || Directory.GetDirectories(dialog.FileName).Length > 0)
					MessageBox.Show("Folder must be empty", "Invalid Folder", MessageBoxButton.OK, MessageBoxImage.Error);
				else
				{
					Vault v = new(dialog.FileName);

					if (MessageBox.Show(v.Dir, $"Migrate to: {v.Name}", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
					{

						v = await Vault.CreateFromLegacy(dialog.FileName, AppDomain.CurrentDomain.BaseDirectory);


						((App)Application.Current).Settings.allRecentVaults.Add(v);


					}
				}
			}
		}


		private void RecentVaultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var v = RecentVaults[RecentVaultsList.SelectedIndex];

			Debug.Print($"Clicked {RecentVaultsList.SelectedIndex}, {v.Name}");


			Application.Current.MainWindow.Content = new VaultViewControl(v);




		}

	}
}
