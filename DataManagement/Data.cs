using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.ObjectModel;

namespace IntakeTrackerApp.DataManagement;

public class ObservableItem<T> : INotifyPropertyChanged
{

	private T item;

	public ObservableItem(T item)
	{
		this.item = item;
	}

	public T Item
	{
		get => item;
		set
		{
			item = value;
			PropertyChanged?.Invoke(this, new("Item"));
		}
	}

	public static implicit operator T(ObservableItem<T> i) => i.Item;
	public static implicit operator ObservableItem<T>(T i) => new(i);

	public event PropertyChangedEventHandler? PropertyChanged;

}


public class Data
{
	private Vault v;
	private static Data? singleton = null;
	public static Data? NullableSingleton
	{
		get
		{
			return singleton;
		}
	}
	public static Data Singleton
	{
		get
		{
			return singleton ?? throw new Exception("No data singleton");
		}
	}

	public ObservableCollection<PatientReferral> ReferralSummaries { get; set; }
	private object summeriesLock = new();

	public PatientsContext Referrals { get; set; }
	public static PatientsContext Context => Singleton.Referrals;

	private bool includeArchive = false;

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
		// sqlite doesn't support async operations as it is embedded, run it on a thread instead
		await Task.Run(() =>
		{
			Debug.WriteLine("Loading database..");

			Referrals.patientReferrals.Where(p => p.Archived == includeArchive).ToList().ForEach(x => x.Init());

			Debug.WriteLine("Loaded database");
		});

	}
	public Data(Vault v)
	{
		singleton = this;
		this.v = v;
		Referrals = new PatientsContextFactory() { v = v }.CreateDbContext();
		//Query summaries from all patients
		ReferralSummaries = Referrals.patientReferrals.Local.ToObservableCollection();

		BindingOperations.EnableCollectionSynchronization(ReferralSummaries, summeriesLock);


		LoadDataAsync();
	}

	public async void LoadDataAsync()
	{
		await Task.WhenAll(LoadData(), v.LoadSettingsAsync());
	}

	public void Add(PatientReferral referral)
	{
		ReferralSummaries.Add(referral);
	}

	public async Task Save()
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
				await Referrals.SaveChangesAsync();
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
		await Task.Delay(100);

		Mouse.OverrideCursor = o;
	}
	public void SaveAndQuit()
	{
		Referrals.SaveChanges();
		Close();
	}
	public bool IsSaved { get; set; } = false;

	public void Close()
	{
		Referrals.Dispose();
	}




}

