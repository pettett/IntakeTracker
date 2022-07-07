
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using IntakeTrackerApp.Data;

namespace IntakeTrackerApp.Windows;


public static class WindowUtility
{
	private const int GWL_STYLE = -16, WS_MAXIMIZEBOX = 0x10000, WS_MINIMIZEBOX = 0x20000;

	[DllImport("user32.dll")]
	extern private static int GetWindowLong(IntPtr hwnd, int index);

	[DllImport("user32.dll")]
	extern private static int SetWindowLong(IntPtr hwnd, int index, int value);

	/// <summary>
	/// Hides the Minimize and Maximize buttons in a Window. Must be called in the constructor.
	/// </summary>
	/// <param name="window">The Window whose Minimize/Maximize buttons will be hidden.</param>
	public static void HideMinimizeAndMaximizeButtons(this Window window)
	{
		window.SourceInitialized += (s, e) =>
		{
			IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
			int currentStyle = GetWindowLong(hwnd, GWL_STYLE);

			SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
		};
	}

}
/// <summary>
/// Base class for setting tree rendering
/// </summary>
public abstract class Setting
{
	public Setting(string name)
	{
		Name = name;
	}

	public string Name { get; init; }
	public bool ExpandedByDefault { get; init; }
	public Setting[] Children { get; init; } = Array.Empty<Setting>();
	public virtual void OnClose() { }
}
/// <summary>
/// Setting that exists as tree root
/// </summary>
public class General : Setting
{
	public General() : base("General") { }


}

public class ListSetting : Setting, ICommand
{
	public class Holder<T>
	{
		public Holder(T data)
		{
			Data = data;
		}

		public T Data { get; set; }
	}

	public ObservableCollection<string> DataLink { get; init; }

	public ObservableCollection<Holder<string>> NameList { get; set; } = new();
	public string Singular { get; set; }
	public string ButtonName => $"Add {Singular}";
	public string LabelName => $"Edit {Name}";

	public ListSetting(ObservableCollection<string> dataLink, string plural, string singular) : base(plural)
	{
		DataLink = dataLink;
		//Load name list from settings
		foreach (var m in DataLink)
		{
			NameList.Add(new(m));
		}
		Singular = singular;

	}

	public event EventHandler? CanExecuteChanged;

	public void AddNewRow()
	{
		NameList.Add(new(""));
	}

	public override void OnClose()
	{
		//Turn apply name list in settings
		DataLink.Clear();

		foreach (var m in NameList)
		{
			DataLink.Add(m.Data);
		}
	}

	public bool CanExecute(object? parameter)
	{
		return true;
	}

	public void Execute(object? parameter)
	{
		AddNewRow();
	}
}
public class ThresholdSettings : Setting
{
	public ThresholdSettings() : base("Warning Thresholds")
	{
	}
	public ObservableItem<uint> MRIReportWarningThreshold
	{
		get => Settings.MRIReportWarningThreshold;
		set => Settings.MRIReportWarningThreshold = value;
	}
	public ObservableItem<uint> LPAppointmentWarningThreshold
	{
		get => Settings.LPAppointmentWarningThreshold;
		set => Settings.LPAppointmentWarningThreshold = value;
	}
	public ObservableItem<uint> LPReportedWarningThreshold
	{
		get => Settings.LPReportedWarningThreshold;
		set => Settings.LPReportedWarningThreshold = value;
	}
	public ObservableItem<uint> EPAppointmentWarningThreshold
	{
		get => Settings.EPAppointmentWarningThreshold;
		set => Settings.EPAppointmentWarningThreshold = value;
	}
	public ObservableItem<uint> EPReportedWarningThreshold
	{
		get => Settings.EPReportedWarningThreshold;
		set => Settings.EPReportedWarningThreshold = value;
	}
	public ObservableItem<uint> BloodsAppointmentWarningThreshold
	{
		get => Settings.BloodsAppointmentWarningThreshold;
		set => Settings.BloodsAppointmentWarningThreshold = value;
	}
	public ObservableItem<uint> BloodsReportedWarningThreshold
	{
		get => Settings.BloodsReportedWarningThreshold;
		set => Settings.BloodsReportedWarningThreshold = value;
	}
}


public record SettingHolder(Setting setting);
/// <summary>
/// Interaction logic for Settings.xaml
/// </summary>
public partial class SettingsWindow : Window
{
	private Setting root = new General()
	{
		Children = new Setting[]
		{
			new ListSetting(Settings.ReferralManagers,  "Referral Managers",    "Referral Manager"),
			new ListSetting(Settings.TransferRegions,   "Transfer Regions",     "Transfer Region"),
			new ThresholdSettings(),
		},
		ExpandedByDefault = true,
	};




	public SettingsWindow()
	{

		InitializeComponent();


		WindowUtility.HideMinimizeAndMaximizeButtons(this);

		//Create tree and select root
		SettingsTreeView.Items.Add(GenerateTree(root));

		SetSetting(new SettingHolder(root));


	}
	/// <summary>
	/// Recursively load the wpf tree from the settings tree
	/// </summary>
	/// <param name="setting"></param>
	/// <returns></returns>
	public TreeViewItem GenerateTree(Setting setting)
	{
		TreeViewItem item = new()
		{
			Header = new SettingHolder(setting),
			IsExpanded = setting.ExpandedByDefault,
		};
		foreach (var child in setting.Children)
			item.Items.Add(GenerateTree(child));

		return item;
	}

	private void CloseSetting()
	{

		(SettingContent.Content as Setting)?.OnClose();
	}

	private void SetSetting(SettingHolder holder)
	{
		CloseSetting();

		SettingBox.Header = holder;
		SettingContent.Content = holder.setting;
	}

	private bool choice;

	private void SettingsWindow_Closing(object sender, CancelEventArgs e)
	{
		CloseSetting();
		if (choice)
			_ = Settings.ApplyChanges();
		else
			Settings.RevertChanges();
	}


	private void SettingsView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		Debug.WriteLine($"Selected {e.NewValue }");

		CloseSetting();

		SetSetting((SettingHolder)((TreeViewItem)e.NewValue).Header);


	}

	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		choice = false;
		DialogResult = false;
	}

	private void OKButton_Click(object sender, RoutedEventArgs e)
	{
		choice = true;
		DialogResult = true;
	}
}

