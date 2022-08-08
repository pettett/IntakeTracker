using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace IntakeTrackerApp.DataManagement;
/// <summary>
/// Represents a save file for the program, containing values for all settings along with the database context
/// </summary>
public class Vault
{
	/// <summary>
	/// Get the name of the folder this vault exists in. <br/>
	/// E.G. the vault C:/users/me/documents/vault1/ will return vault1
	/// </summary>
	public string Name => Path.GetFileName(vaultPath) ?? "error";
	/// <summary>
	/// Directory of this vaults root folder
	/// </summary>
	public string Dir => vaultPath;


	private const string referralsFile = "referrals.db";
	private const string settingsFile = "settings.json";
	/// <summary>
	/// Path to referrals.db within this vault
	/// </summary>
	public string DatabasePath => Path.Join(vaultPath, referralsFile);
	/// <summary>
	/// Path to settings.json within this vault
	/// </summary>
	public string SettingsPath => Path.Join(vaultPath, settingsFile);
	/// <summary>
	/// Directory for local backups
	/// </summary>
	public string LocalBackupDirectory => Path.Join(vaultPath, "backup");
	public string RoamingBackupDirectory => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Referral_Tracker", Name, "backup");

	private readonly string vaultPath;


	public ObservableCollection<string> ReferralManagers { get; set; } = new() { "JH", "JL" };
	public ObservableCollection<string> TransferRegions { get; set; } = new() { "Cornwall", "Torbay", "Exeter" };
	public ObservableItem<uint> MRIReportWarningThreshold { get; set; } = new(21u);
	public ObservableItem<uint> LPAppointmentWarningThreshold { get; set; } = new(14u);
	public ObservableItem<uint> LPReportedWarningThreshold { get; set; } = new(28u);
	public ObservableItem<uint> EPAppointmentWarningThreshold { get; set; } = new(21u);
	public ObservableItem<uint> EPReportedWarningThreshold { get; set; } = new(2u);
	public ObservableItem<uint> BloodsAppointmentWarningThreshold { get; set; } = new(7u);
	public ObservableItem<uint> BloodsReportedWarningThreshold { get; set; } = new(2u);
	/// <summary>
	/// Time of last backup
	/// </summary>
	private DateTime? lastBackup = null;
	/// <summary>
	/// referral database
	/// </summary>
	public Data? Context { get; set; }
	/// <summary>
	/// Check the vault's folder exists and contains referrals.db and settings.json
	/// </summary>
	/// <returns></returns>
	public bool CheckValid()
	{
		if (!Directory.Exists(vaultPath))
			return false;


		if (!File.Exists(SettingsPath))
			return false;


		if (!File.Exists(DatabasePath))
			return false;


		return true;
	}
	/// <summary>
	/// Save the database of this vault  <br/>
	/// Also copy to backups if required
	/// </summary>
	public async void SaveData()
	{
		if (Context != null)
		{
			await Context.Save();
			string backupFile = $"referrals_{DateTime.Now:dd_MM_yyyy}.db";
			string localBackupFile = Path.Join(LocalBackupDirectory, backupFile);
			string roamingBackupFile = Path.Join(RoamingBackupDirectory, backupFile);
			if (!File.Exists(localBackupFile))
			{
				if (lastBackup == null || (DateTime.Now - lastBackup.Value).Hours > 24)
				{

					lastBackup = DateTime.Now;
					Directory.CreateDirectory(LocalBackupDirectory);
					Directory.CreateDirectory(RoamingBackupDirectory);
					File.Copy(DatabasePath, localBackupFile);
					File.Copy(DatabasePath, roamingBackupFile);
					await SaveSettingsChangesAsync();
				}
				else
				{
					Debug.Print("Not long enough since last backup for another one");
				}

			}
			else
			{
				Debug.Print("Backup from today already exists");
			}

		}

	}

	public static async Task CopyFileAsync(string sourcePath, string destinationPath, bool overwrite)
	{
		if (!overwrite && File.Exists(destinationPath)) return;

		using Stream source = File.Open(sourcePath, FileMode.Open);
		using Stream destination = File.Create(destinationPath);
		await source.CopyToAsync(destination);
	}

	/// <summary>
	/// Load the data within this vault, also setting it as open
	/// </summary>
	/// <returns>A reference to the referral database</returns>
	public Data LoadContext()
	{
		((App)Application.Current).SetLastUsedVault(this);

		Context = new Data(this);
		return Context;
	}

	public Vault(string vaultPath)
	{
		this.vaultPath = vaultPath;
	}
	/// <summary>
	/// Create a vault in a folder from patientReferrals.db stored next to the EXE as with older versions
	/// of this system
	/// </summary>
	/// <param name="vaultPath">Path of the new vault</param>
	/// <param name="legacyFolder">Path of the legacy area, the folder containing the exe</param>
	/// <returns></returns>
	public static async Task<Vault> CreateFromLegacy(string vaultPath, string legacyFolder)
	{
		Vault v = new(vaultPath);
		Debug.Print("Copying legacy database");
		File.Copy(Path.Join(legacyFolder, "patientReferrals.db"), v.DatabasePath, false);

		var settings = v.SettingsPath;

		if (File.Exists(settings))
		{
			Debug.Print("Copying legacy settings");
			await CopyFileAsync(settings, v.SettingsPath, false);
			await v.LoadSettingsAsync();
		}
		else
		{
			//save a default set of settings
			await v.SaveSettingsChangesAsync();
		}

		return v;
	}


	public async Task SaveSettingsChangesAsync()
	{
		Settings s = new()
		{
			ReferralManagers = new string[ReferralManagers.Count],

			TransferRegions = new string[TransferRegions.Count],

			MRIReportWarningThreshold = MRIReportWarningThreshold.Item,
			LPAppointmentWarningThreshold = LPAppointmentWarningThreshold.Item,
			LPReportedWarningThreshold = LPReportedWarningThreshold.Item,
			EPAppointmentWarningThreshold = EPAppointmentWarningThreshold.Item,
			EPReportedWarningThreshold = EPReportedWarningThreshold.Item,
			BloodsAppointmentWarningThreshold = BloodsAppointmentWarningThreshold.Item,
			BloodsReportedWarningThreshold = BloodsReportedWarningThreshold.Item,
			LastBackup = lastBackup
		};


		ReferralManagers.CopyTo(s.ReferralManagers, 0);
		TransferRegions.CopyTo(s.TransferRegions, 0);

		Debug.Print($"Saving settings to {SettingsPath}");

		using var file = File.Create(SettingsPath);

		await JsonSerializer.SerializeAsync(file, s);
	}

	/// <summary>
	/// Load settings.json
	/// </summary>
	public void LoadSettings()
	{
		if (File.Exists(SettingsPath))
		{
			using FileStream file = File.OpenRead(SettingsPath);

			Debug.Print($"Loading settings from {file.Name}");

			ApplySettings(JsonSerializer.Deserialize<Settings>(file));
		}
	}
	/// <summary>
	/// Load settings.json async
	/// </summary>
	public async Task LoadSettingsAsync()
	{
		if (File.Exists(SettingsPath))
		{
			using FileStream file = File.OpenRead(SettingsPath);

			Debug.Print($"Loading settings from {file.Name}");

			ApplySettings(await JsonSerializer.DeserializeAsync<Settings>(file));
		}

	}
	/// <summary>
	/// Apply a Settings object to the vault's parameters, allowing for the change of settings to work with observable objects
	/// </summary>
	/// <param name="settings"></param>
	private void ApplySettings(Settings? settings)
	{
		if (settings == null) return;

		ReferralManagers.Clear();
		foreach (var item in settings.ReferralManagers)
			ReferralManagers.Add(item);

		TransferRegions.Clear();
		foreach (var item in settings.TransferRegions)
			TransferRegions.Add(item);

		MRIReportWarningThreshold.Item = settings.MRIReportWarningThreshold;
		LPAppointmentWarningThreshold.Item = settings.LPAppointmentWarningThreshold;
		LPReportedWarningThreshold.Item = settings.LPReportedWarningThreshold;
		EPAppointmentWarningThreshold.Item = settings.EPAppointmentWarningThreshold;
		EPReportedWarningThreshold.Item = settings.EPReportedWarningThreshold;
		BloodsAppointmentWarningThreshold.Item = settings.BloodsAppointmentWarningThreshold;
		BloodsReportedWarningThreshold.Item = settings.BloodsReportedWarningThreshold;

	}

}

