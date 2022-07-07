
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

namespace IntakeTrackerApp.Handlers;

internal class SpreadsheetHandler : IDisposable
{
	private readonly Stream stream;
	private readonly SpreadsheetDocument document;
	public SpreadsheetHandler(string fileName, bool forReading)
	{
		if (!forReading)
		{
			stream = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
			document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
		}
		else
		{
			stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			document = SpreadsheetDocument.Open(stream, false);
		}
	}

	public string GetString(Cell c, SharedStringTable? sst)
	{
		if ((c.DataType != null) && (c.DataType == CellValues.SharedString))
		{
			int ssid = int.Parse(c.CellValue?.Text ?? throw new("No Cell Contents"));
			string str = sst?.ChildElements[ssid].InnerText ?? throw new("No String table");
			return str;
		}
		else if (c.CellValue != null)
		{
			return c.CellValue.Text;
		}
		return "";
	}

	public T[] LoadData<T>() where T : new()
	{
		DataTable spreadsheetTable = new();


		if (document.GetPartsOfType<WorkbookPart>().FirstOrDefault() is WorkbookPart workbookPart)
		{
			SharedStringTablePart? sstpart = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
			SharedStringTable? sst = sstpart?.SharedStringTable;

			if (workbookPart.GetPartsOfType<WorksheetPart>().FirstOrDefault() is WorksheetPart worksheetPart)
				if (worksheetPart.Worksheet.GetFirstChild<SheetData>() is SheetData data)
				{
					var rows = data.Elements<Row>().GetEnumerator();
					rows.MoveNext();
					foreach (var cell in rows.Current.Elements<Cell>())
					{
						Debug.WriteLine(cell.CellValue?.Text);
						spreadsheetTable.Columns.Add(new DataColumn(GetString(cell, sst)));
					}

					while (rows.MoveNext())
					{
						var n = spreadsheetTable.NewRow();
						n.ItemArray = rows.Current.Elements<Cell>().Select(x => GetString(x,sst) ).ToArray();

						spreadsheetTable.Rows.Add(n);
					}
				}
				else Debug.WriteLine($"No Sheet Data");
			else Debug.WriteLine($"No Worksheet Part");
		}
		else Debug.WriteLine($"No Workbook Part");

		T[] t = new T[spreadsheetTable.Rows.Count];
		var properties = typeof(PatientReferral).GetProperties().Where(x => x.CanWrite && x.CanRead).ToArray();

		for (int i = 0; i < t.Length; i++)
		{
			t[i] = new();
			foreach (var p in properties)
			{
				p.SetValue(t[i], LoadProperty(p.GetValue(t[i]), spreadsheetTable.Rows[i], p.PropertyType, p.Name));
			}
		}

		return t;
	}


	public object? LoadProperty(object? d, DataRow row, Type t, string name)
	{
		

		if (t == typeof(DateRecord))
		{
			return new DateRecord(
				(DateTime?)LoadProperty(null, row, typeof(DateTime), $"{name} Date"),
				(string)LoadProperty("", row, typeof(string), $"{name} Comment")!);
		}
		else if (t == typeof(Test))
		{
			Test test = (Test)d!;

			return new Test(test.Name)
			{
				ReportedDate = (DateRecord)LoadProperty(null, row, typeof(DateRecord), $"{name} Reported")!,
				TestDate = (DateRecord)LoadProperty(null, row, typeof(DateRecord), $"{name} Test")!,
				RequestedDate = (DateRecord)LoadProperty(null, row, typeof(DateRecord), $"{name} Requested")!,
			};
		}
		string s = (string)row[name];

		if (t == typeof(ulong))
			return ulong.Parse(s);
		else if (t == typeof(long))
			return long.Parse(s);
		else if (t == typeof(bool))
			return bool.Parse(s);
		else if (t == typeof(bool?))
			return s == ""? null : bool.Parse(s);

		else if (t == typeof(DateTime))
		{
			if (s == "") return null;
			else if(DateTime.TryParse(s, out var date))
				return date;
			else if (double.TryParse(s, out var oa)){
				return DateTime.FromOADate(oa);
			}else throw new Exception($"Cannot parse {s}");
		}
		else if (t == typeof(string))
			return s;

		throw new Exception($"No handler for {t}");
	}

	public void SaveData<T>(T[] table)
	{
		Columns columnsData = new();
		SheetData partSheetData = GenerateSheetdataForDetails(columnsData, table);
		//columnsData = GenerateColumnsForDetails(table);
		Debug.WriteLine(columnsData);
		WorkbookPart workbookPart = document.AddWorkbookPart();
		GenerateWorkbookPartContent(workbookPart);

		WorkbookStylesPart workbookStylesPart1 = workbookPart.AddNewPart<WorkbookStylesPart>("rId3");
		GenerateWorkbookStylesPartContent(workbookStylesPart1);

		WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>("rId1");
		GenerateWorksheetPartContent(worksheetPart, partSheetData, columnsData);
	}

	private void AddHeader(Columns columns, Row row, Type t, string name, ref uint index)
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
			index++;
			columns.Append(GenerateDateColumn(index));
			row.Append(CreateCell(name));
		}
	}

	private static Column GenerateDateColumn(uint index)
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

	private void AddObject(Row row, object? obj)
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

	private SheetData GenerateSheetdataForDetails<T>(Columns columns, T[] table)
	{
		SheetData sheetData = new();
		Row headerRow = new();
		uint i = 0;

		var properties = typeof(PatientReferral).GetProperties().Where(x => x.CanWrite && x.CanRead).ToArray();

		foreach (var p in properties)
		{
			AddHeader(columns, headerRow, p.PropertyType, p.Name, ref i);
		}
		sheetData.Append(headerRow);

		foreach (var r in table)
		{
			Row workRow = new();
			foreach (var p in properties)
			{
				AddObject(workRow, p.GetValue(r));
			}
			sheetData.Append(workRow);
		}
		return sheetData;
	}

	private Cell CreateCell(DateTime? t)
	{
		return new Cell()
		{
			DataType = new(CellValues.Date),
			CellValue = new(t?.ToString("s") ?? ""),
			StyleIndex = new(1u),
		};
	}

	private Cell CreateCell(string? t)
	{
		return new Cell()
		{
			DataType = new(CellValues.String),
			CellValue = new(t ?? ""),
		};
	}

	private void GenerateWorkbookPartContent(WorkbookPart workbookPart)
	{
		Workbook workbook = new();
		Sheets sheets = new();
		Sheet sheet = new() { Name = "Sheet1", SheetId = 1U, Id = "rId1" };
		sheets.Append(sheet);
		workbook.Append(sheets);
		workbookPart.Workbook = workbook;
	}

	private void GenerateWorksheetPartContent(WorksheetPart worksheetPart, SheetData sheetData, Columns columns)
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

		worksheetPart.Worksheet = worksheet;
	}

	private void GenerateWorkbookStylesPartContent(WorkbookStylesPart workbookStylesPart)
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


	public void Dispose()
	{
		document.Dispose();
		stream.Dispose();
	}
}

