using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace IntakeTrackerApp.DataManagement;

public class AppSettings
{
	public const string AppSettingsFile = "AppSettings.json";

	public ObservableCollection<Vault> allRecentVaults;


	public string AppSettingsPath
	{
		get => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppSettingsFile);
	}

	public class AppSettingsData
	{
		/// <summary>
		/// The currently open vault that the program should open on startup if not null
		/// </summary>
		public string? CurrentOpenVault { get; set; } = null;
		public List<string> LastUsedVaults { get; set; } = new();
	}

	public AppSettingsData Settings { get; set; }

	public AppSettings()
	{
		if (File.Exists(AppSettingsPath))
		{
			using var file = File.OpenRead(AppSettingsPath);
			Settings = JsonSerializer.Deserialize<AppSettingsData>(file) ?? new();
			Debug.WriteLine("Loaded settings directly");
		}
		else
		{
			Debug.WriteLine("Could not load settings, creating new");
			{ using var file = File.Create(AppSettingsPath); }

			Settings = new();
			SaveSettingsAsync().Start();
		}

		Debug.WriteLine($"Settings located at {AppSettingsPath}");

		allRecentVaults = new();
		foreach (var vaultPath in Settings.LastUsedVaults)
		{
			var v = new Vault(vaultPath);

			if (v.CheckValid())
			{
				Debug.WriteLine($"Loaded recent vault {vaultPath}");
				allRecentVaults.Add(v);
			}
		}

		allRecentVaults.CollectionChanged += (_, _) =>
		{
			Settings.LastUsedVaults = allRecentVaults.Select(v => v.Dir).ToList();
			SaveSettingsAsync().Start();
		};

	}


	public async Task SaveSettingsAsync()
	{
		using var file = File.OpenWrite(AppSettingsPath);

		await JsonSerializer.SerializeAsync(file, Settings);
	}
}

