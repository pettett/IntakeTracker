
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Data;
using System.Linq;
using System.Globalization;

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
				ExportXML(GenerateTable());
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
	public const string
		NHSNumber = "NHS Number",
		HospitalNumber = "Hospital Number",
		FirstName = "First Name",
		LastName = "Last Name",
		DOB = "DOB",
		DateOnReferral = "Date On Referral",
		DateReferralReceived = "Date Referral Received",
		DateOfActiveReferral = "Date Of Active Referral",
		BloodTestNeeded = "Blood Test Needed",
		BloodFormsSent = "Blood Forms Sent",
		BloodFormsPlanned = "Blood Forms Planned",
		BloodTestReported = "Blood Test Reported",
		BloodTestResults = "Blood Test Results",
		MRI = "MRI",
		EP = "EP",
		LP = "LP",
		ContactAttempted = "Contact Attempted",
		DateContactMade = "Date Contact Made",
		ContactMethod = "Preferred Contact Method",
		PreviousCorrespondenceNeeded = "Previous Correspondence Needed",
		PreviousCorrespondenceRequested = "Previous Correspondence Requested",
		PreviousCorrespondenceReceived = "Previous Correspondence Received",
		MedicalAppointmentNeeded = "Medical Appointment Needed",
		MedicalAppointment = "Medical Appointment",
		NursingAppointmentNeeded = "Nursing Appointment Needed",
		NursingAppointment = "Nursing Appointment";


	public string GetNeeded(bool? needed) => needed switch
	{
		false => "Unneeded",
		true => "Needed",
		_ => "Unknown",
	};

	public DataTable GenerateTable()
	{
		DataTable t = new();

		foreach ((string n, Type type) in new[] {
			(NHSNumber, typeof(ulong)),
			(HospitalNumber, typeof(string)),
			(FirstName, typeof(string)),
			(LastName, typeof(string)),
			(DOB, typeof(DateTime)),
			(DateOnReferral, typeof(DateTime)),
			(DateReferralReceived, typeof(DateTime)),


			(DateOfActiveReferral, typeof(DateRecord)),

			(BloodTestNeeded, typeof(string)),
			(BloodFormsSent, typeof(DateRecord)),
			(BloodFormsPlanned, typeof(DateRecord)),
			(BloodTestReported, typeof(DateRecord)),
			(BloodTestResults, typeof(string)),

			(MRI, typeof(Test)),
			(EP, typeof(Test)),
			(LP, typeof(Test)),


			(ContactMethod, typeof(string)),
			(ContactAttempted, typeof(DateRecord)),
			(DateContactMade, typeof(DateRecord)),

			(PreviousCorrespondenceNeeded, typeof(string)),
			(PreviousCorrespondenceRequested, typeof(DateRecord)),
			(PreviousCorrespondenceReceived, typeof(DateRecord)),


			(MedicalAppointmentNeeded, typeof(string)),
			(MedicalAppointment, typeof(DateRecord)),


			(NursingAppointmentNeeded, typeof(string)),
			(NursingAppointment, typeof(DateRecord)),

		})
			t.Columns.Add(new DataColumn(n, type));



		foreach (PatientReferral p in Data.Context.patientReferrals)
		{
			var row = t.NewRow();
			//general
			row.SetField(NHSNumber, p.NHSNumberKey);
			row.SetField(HospitalNumber, p.LocalHospitalNumber);
			row.SetField(FirstName, p.FirstName);
			row.SetField(LastName, p.LastName);
			row.SetField(DOB, p.DateOfBirth);
			row.SetField(DateOnReferral, p.DateOnReferral);
			row.SetField(DateReferralReceived, p.DateReferralReceived);
			row.SetField(DateOfActiveReferral, p.DateOfActiveManagement);

			//appointments
			row.SetField(NursingAppointmentNeeded, GetNeeded(p.NursingAppointmentNeeded));
			row.SetField(NursingAppointment, p.NursingAppointment);

			row.SetField(MedicalAppointmentNeeded, GetNeeded(p.MedicalAppointmentNeeded));
			row.SetField(MedicalAppointment, p.MedicalAppointment);

			//Previous correspondence 
			row.SetField(PreviousCorrespondenceNeeded, GetNeeded(p.PreviousCorrespondenceNeeded));
			row.SetField(PreviousCorrespondenceReceived, p.PreviousCorrespondenceReceived);
			row.SetField(PreviousCorrespondenceRequested, p.PreviousCorrespondenceRequested);

			//Blood tests
			row.SetField(BloodTestNeeded, GetNeeded(p.BloodTestNeeded));
			row.SetField(BloodFormsPlanned, p.BloodTestPlanned);
			row.SetField(BloodFormsSent, p.BloodFormsSent);
			row.SetField(BloodTestReported, p.BloodTestReported);
			row.SetField(BloodTestResults, p.BloodTestResults);

			row.SetField(MRI, p.MRI);
			row.SetField(LP, p.LP);
			row.SetField(EP, p.EP);

			row.SetField(ContactMethod, p.PreferredContactMethod);
			row.SetField(ContactAttempted, p.ContactAttempted);
			row.SetField(DateContactMade, p.DateContactMade);


			t.Rows.Add(row);
		}

		return t;
	}


	public void ExportXML(DataTable table)
	{
		bool done = false;
		while (!done)
		{
			try
			{
				using Stream stream = File.Open(FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
				using SpreadsheetDocument package = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
				CreatePartsForExcel(package, table);
				done = true;
			}
			catch (IOException)
			{
				done = MessageBoxResult.Cancel == MessageBox.Show("File is open in another application. Try again?", "File Open", MessageBoxButton.OKCancel);
			}
		}
	}

	private static void CreatePartsForExcel(SpreadsheetDocument document, DataTable table)
	{
		Columns columnsData = new();
		SheetData partSheetData = GenerateSheetdataForDetails(columnsData, table);
		columnsData = GenerateColumnsForDetails(table);

		WorkbookPart workbookPart1 = document.AddWorkbookPart();
		GenerateWorkbookPartContent(workbookPart1);

		WorkbookStylesPart workbookStylesPart1 = workbookPart1.AddNewPart<WorkbookStylesPart>("rId3");
		GenerateWorkbookStylesPartContent(workbookStylesPart1);

		WorksheetPart worksheetPart1 = workbookPart1.AddNewPart<WorksheetPart>("rId1");
		GenerateWorksheetPartContent(worksheetPart1, partSheetData, columnsData);
	}


	public static Column GenerateDateColumn(uint index)
	{
		return new()
		{
			Width = 12.5D,
			Min = index,
			Max = index,
			BestFit = true,
			CustomWidth = true
		};
	}
	static void AddHeader(Columns columns, Row row, Type t, string name, ref uint index)
	{
		if (t == typeof(Test))
		{
			AddHeader(columns, row, typeof(DateRecord), $"{name} Requested", ref index);
			AddHeader(columns, row, typeof(DateRecord), $"{name} Test", ref index);
			AddHeader(columns, row, typeof(DateRecord), $"{name} Reported", ref index);
		}
		else if (t == typeof(DateRecord))
		{
			AddHeader(columns, row, typeof(DateTime), $"{name} Date", ref index);
			AddHeader(columns, row, typeof(string), $"{name} Comment", ref index);
		}
		else
		{
			columns.Append(GenerateDateColumn(index)); 
			row.Append(CreateCell(name));
			index++;
		}
	}

	private static Columns GenerateColumnsForDetails(DataTable table)
	{
		Columns columns = new();
		uint i = 0;
		foreach (DataColumn c in table.Columns)
		{
			i++;

			if (c.DataType == typeof(DateTime))
			{
				columns.Append(GenerateDateColumn(i));
			}
			else if (c.DataType == typeof(DateRecord))
			{
				columns.Append(GenerateDateColumn(i));
				i++; //Width of 2
				columns.Append(GenerateDateColumn(i));
			}

		}
		return columns;
	}


	static void AddObject(Row row, object? obj)
	{
		switch (obj)
		{
			case DateTime t:
				row.Append(CreateCell(t));
				return;
			case Test t:
				AddObject(row, t.RequestedDate);
				AddObject(row, t.TestDate);
				AddObject(row, t.ReportedDate);
				return;
			case DateRecord d:
				AddObject(row, d.Date);
				AddObject(row, d.Comment);
				return;
			case ulong:
			case int:
				row.Append(
					new Cell()
					{
						DataType = CellValues.Number,
						//cannot be null
						CellValue = new(obj?.ToString() ?? ""),
					});
				return;
			default:
				row.Append(CreateCell(obj?.ToString()));
				return;
		}
	}

	private static SheetData GenerateSheetdataForDetails(Columns columns, DataTable table)
	{
		SheetData sheetData = new();
		Row headerRow = new();
		foreach (DataColumn c in table.Columns)
		{
			uint i = 0;
			AddHeader(columns, headerRow, c.DataType, c.ColumnName, ref i);
		}
		sheetData.Append(headerRow);

		foreach (DataRow r in table.Rows)
		{
			Row workRow = new();
			foreach (DataColumn c in table.Columns)
			{
				AddObject(workRow, r[c]);
			}
			sheetData.Append(workRow);
		}
		return sheetData;
	}
	public static Cell CreateCell(DateTime? t)
	{
		return new Cell()
		{
			DataType = new(CellValues.Date),
			CellValue = new(t?.ToString("s") ?? ""),
			StyleIndex = new(1u),
		};
	}
	public static Cell CreateCell(string? t)
	{
		return new Cell()
		{
			DataType = new(CellValues.String),
			CellValue = new(t ?? ""),
		};
	}
	private static void GenerateWorkbookPartContent(WorkbookPart workbookPart1)
	{
		Workbook workbook1 = new();
		Sheets sheets1 = new();
		Sheet sheet1 = new() { Name = "Sheet1", SheetId = 1U, Id = "rId1" };
		sheets1.Append(sheet1);
		workbook1.Append(sheets1);
		workbookPart1.Workbook = workbook1;
	}

	private static void GenerateWorksheetPartContent(WorksheetPart worksheetPart1, SheetData sheetData, Columns columns)
	{
		Worksheet worksheet = new() { MCAttributes = new MarkupCompatibilityAttributes() { Ignorable = "x14ac" } };
		worksheet.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
		worksheet.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
		worksheet.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
		SheetDimension sheetDimension1 = new() { Reference = "A1" };

		SheetViews sheetViews = new();

		SheetView sheetView = new()
		{
			TabSelected = true,
			WorkbookViewId = 0U
		};
		Selection selection = new()
		{
			ActiveCell = "A1",
			SequenceOfReferences = new ListValue<StringValue>() { InnerText = "A1" }
		};

		sheetView.Append(selection);

		sheetViews.Append(sheetView);
		SheetFormatProperties sheetFormatProperties1 = new() { DefaultRowHeight = 15D, DyDescent = 0.25D, };

		worksheet.Append(sheetDimension1);
		worksheet.Append(sheetViews);
		worksheet.Append(sheetFormatProperties1);
		worksheet.Append(sheetData);


		worksheet.InsertBefore(columns, sheetData);

		worksheetPart1.Worksheet = worksheet;
	}

	private static void GenerateWorkbookStylesPartContent(WorkbookStylesPart workbookStylesPart)
	{
		workbookStylesPart.Stylesheet = new Stylesheet
		{
			Fonts = new DocumentFormat.OpenXml.Spreadsheet.Fonts(new Font()),
			Fills = new Fills(new Fill()),
			Borders = new Borders(new DocumentFormat.OpenXml.Spreadsheet.Border()),
			CellStyleFormats = new CellStyleFormats(new CellFormat()),
			CellFormats =
			new CellFormats(
				new CellFormat(),
				new CellFormat
				{
					NumberFormatId = 14,
					ApplyNumberFormat = true
				})
		};
	}
	public void CancelButton_Clicked(object sender, RoutedEventArgs e)
	{
		DialogResult = false;
	}
}

