
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Data;
using System.Linq;
using System.Globalization;
using IntakeTrackerApp.Data;

namespace IntakeTrackerApp.Windows;

/// <summary>
/// Interaction logic for ExportWindow.xaml
/// </summary>
public partial class ExportWindow : Window
{
	public string FileName { get; set; }

	// [JsonSerializable(typeof(PatientReferral[]))]
	//  internal partial class MyJsonContext : JsonSerializerContext { }

	public string FileType => System.IO.Path.GetExtension(FileName)!;

	public ExportWindow(string fileName)
	{
		FileName = fileName;
		InitializeComponent();
	}
	public void SubmitButton_Clicked(object sender, RoutedEventArgs e)
	{
		bool result = true;
		switch (FileType)
		{
			case ".json":
				ExportJson();
				break;
			case ".xlsx":
				ExportXML();
				break;
			default:
				result = false;
				break;
		}

		if (result)
			Debug.WriteLine($"Exported to {FileName}");
		else
			Debug.WriteLine($"Error Exporting to {FileName}");

		DialogResult = result;
	}
	public static JsonSerializerOptions jsonOptions = new()
	{
		IgnoreReadOnlyFields = true,
		IgnoreReadOnlyProperties = true,
	};
	public void ExportJson()
	{
		if (File.Exists(FileName))
		{
			File.Delete(FileName);
		}

		using Stream stream = File.Create(FileName);
		using Utf8JsonWriter writer = new(stream, new() { Indented = true });

		JsonSerializer.Serialize(
			writer,
			Data.Context.patientReferrals.ToArray(),
			jsonOptions);
	}

	public string GetNeeded(bool? needed)
	{
		return needed switch
		{
			false => "Unneeded",
			true => "Needed",
			_ => "Unknown",
		};
	}

	public DataTable GenerateTable()
	{
		
		DataTable t = new();

		var properties = typeof(PatientReferral).GetProperties().Where(x => x.CanWrite && x.CanRead).ToArray();

		foreach (var property in properties)
		{
			var type = property.PropertyType;
			//dont use nullables, use the type inside nullables
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				type = type.GenericTypeArguments[0];

			t.Columns.Add(new DataColumn(property.Name, type));
		}
		Debug.WriteLine("Finished setting up columns");

		foreach (PatientReferral p in Data.Context.patientReferrals)
		{
			var row = t.NewRow();
			for (int i = 0; i < properties.Length; i++)
			{
				row[properties[i].Name] = properties[i].GetValue(p) ?? DBNull.Value;
			}
			t.Rows.Add(row);
		}

		return t;
	}


	public void ExportXML()
	{
		bool done = false;
		while (!done)
		{
			try
			{
				using Handlers.SpreadsheetHandler spreadsheet = new(FileName, false);
				spreadsheet.SaveData(Data.Context.patientReferrals.ToArray());

				done = true;
			}
			catch (IOException)
			{
				done = MessageBoxResult.Cancel == MessageBox.Show("File is open in another application. Try again?", "File Open", MessageBoxButton.OKCancel);
			}
		}
	}

	public void CancelButton_Clicked(object sender, RoutedEventArgs e)
	{
		DialogResult = false;
	}
}

