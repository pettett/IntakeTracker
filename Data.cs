using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.Json;

namespace IntakeTrackerApp;
public static class Settings
{
	public struct SettingsStruct
	{
		public string[] ReferralManagers { get; set; }
		public string[] TransferRegions { get; set; }
	}


	public static ObservableCollection<string> ReferralManagers { get; set; } = new() { "JH", "JL" };
	public static ObservableCollection<string> TransferRegions { get; set; } = new() { "Cornwall", "Torbay", "Exeter" };

	const string settingsFile = "settings.json";

	public static async Task ApplyChanges()
	{
		SettingsStruct s = new();
		s.ReferralManagers = new string[ReferralManagers.Count];
		ReferralManagers.CopyTo(s.ReferralManagers, 0);

		s.TransferRegions = new string[TransferRegions.Count];
		TransferRegions.CopyTo(s.TransferRegions, 0);

		using (var file = File.Create(settingsFile))
		{
			await JsonSerializer.SerializeAsync(file, s);
		}
	}
	public static void RevertChanges()
	{
		if (File.Exists(settingsFile))
		{
			using FileStream file = File.OpenRead(settingsFile);
			Load(JsonSerializer.Deserialize<SettingsStruct>(file));
		}
	}


	public static async Task LoadAsync()
	{
		Debug.WriteLine("Loading settings");
		if (File.Exists(settingsFile))
		{
			using FileStream file = File.OpenRead(settingsFile);
			Load(await JsonSerializer.DeserializeAsync<SettingsStruct>(file));
		}
	}

	static void Load(in SettingsStruct settings)
	{
		ReferralManagers.Clear();
		foreach (var item in settings.ReferralManagers)
			ReferralManagers.Add(item);

		TransferRegions.Clear();
		foreach (var item in settings.TransferRegions)
			TransferRegions.Add(item);
	}

}

public class Data
{



	static Data? singleton = null;
	public static Data Singleton
	{
		get
		{
			if (singleton == null)
			{
				singleton = new();
			}
			return singleton;
		}
	}

	public ObservableCollection<PatientReferral> ReferralSummaries { get; set; }
	PatientsContext context;
	public static PatientsContext Context => Singleton.context;

	bool includeArchive = false;
	public bool IncludeArchive
	{
		get => includeArchive;
		set
		{
			if (value != includeArchive)
			{
				includeArchive = value;
				LoadData();
			}
		}
	}
	public async Task LoadData()
	{
		await context.patientReferrals.Where(p => p.Archived == includeArchive).LoadAsync();
	}
	public Data()
	{

		context = new PatientsContextFactory().CreateDbContext();
		//Query summaries from all patients
		ReferralSummaries = context.patientReferrals.Local.ToObservableCollection();


		LoadData();
		Settings.LoadAsync();

		Debug.WriteLine("Loaded data");

	}
	public void Add(PatientReferral referral)
	{
		ReferralSummaries.Add(referral);
	}

	public void Save()
	{
		var o = Mouse.OverrideCursor;

		Mouse.OverrideCursor = Cursors.Wait;
		bool saved = false;
		//Record changes to variables per referral to notify changes
		List<(PatientReferral r, string p)> changed = new();

		while (!saved)
		{
			try
			{
				context.SaveChanges();
				saved = true;
			}
			catch (DbUpdateConcurrencyException ex)
			{
				foreach (var entry in ex.Entries)
				{

					PropertyValues proposedValues = entry.CurrentValues; //changed values
					PropertyValues? databaseValues = entry.GetDatabaseValues();  //value stored in database at the moment

					foreach (IProperty? property in proposedValues.Properties)
					{
						object? proposedValue = proposedValues[property];
						object? databaseValue = databaseValues?[property];

						if (proposedValue != null && !proposedValue.Equals(databaseValue))
						{

							// TODO: decide which value should be written to database
							object? value;

							if (property.Name == "Version")
							{
								//use higher version - that stored in database
								value = databaseValue;
							}
							else
							{
								//This is not as good as it could be, but hopefully it will only appear very rarely :/
								MessageBoxResult selection = MessageBox.Show(
									@$"Please select value of data to save:
Keep Database Data [NO]: 
――――――――――――――――――
{databaseValue}
――――――――――――――――――
Overwrite with Proposed Data [YES]:
――――――――――――――――――
{proposedValue}
――――――――――――――――――",
									$"Concurrency Error with {property.Name} ({property.ClrType})",
									MessageBoxButton.YesNo,
									MessageBoxImage.Warning);

								value = selection == MessageBoxResult.Yes ? proposedValue : databaseValue;

								if (selection == MessageBoxResult.No && entry.Entity is PatientReferral r)
								{
									//This property has been changed from its value on the UI
									changed.Add((r, property.Name));
								}
							}



							Debug.WriteLine($"Resolved {property.Name}: P:{proposedValue} D:{databaseValue}, selected {value}");
							proposedValues[property] = value;
						}
					}

					// Refresh original values to bypass next concurrency check
					entry.OriginalValues.SetValues(databaseValues!);

				}
			}
		}

		foreach ((PatientReferral r, string p) in changed)
		{
			r.NotifyPropertyChanged(p);
		}

		//Delay for a bit to make sure the user knows that something actually happened
		Task.Delay(100).Wait();

		Mouse.OverrideCursor = o;
	}
	public void SaveAndQuit()
	{
		context.SaveChanges();
		Close();
	}
	public bool IsSaved { get; set; } = false;

	public void Close()
	{
		context.Dispose();
	}




}

