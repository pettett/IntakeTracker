using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using IntakeTrackerApp.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;

using System.IO;
using System.Text.RegularExpressions;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;

namespace IntakeTrackerApp.Windows;

/// <summary>
/// Interaction logic for LetterTemplateWindow.xaml
/// </summary>
public partial class LetterTemplateWindow : Window, INotifyPropertyChanged
{
	public string? SaveLocation { get; set; } = null;
	public string PatientAddress { get; set; } = "";
	public string ReferringPhysician { get; set; } = "";
	public string ConsultantName { get; set; } = "";

	public readonly PatientView referral;

	public event PropertyChangedEventHandler? PropertyChanged;
	public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public LetterTemplateWindow(PatientView referral)
	{
		this.referral = referral;
		InitializeComponent();
	}

	private void CreateTemplateButton_Click(object sender, RoutedEventArgs e)
	{
		//TODO: Male this work
		//opemxml doesnt make this easy because runs can split lines or even words of text

		if (SaveLocation == null) return;
		string rootDir = AppDomain.CurrentDomain.BaseDirectory;
		Debug.WriteLine(rootDir);
		using (var resultDoc = WordprocessingDocument.Create(SaveLocation, WordprocessingDocumentType.Document))
		{

			using (var mainDoc = WordprocessingDocument.Open(
				@$"{rootDir}{System.IO.Path.DirectorySeparatorChar}New patient pathway clinic letter.docx",
				false))
			{


				// copy parts from source document to new document
				foreach (var part in mainDoc.Parts)
					resultDoc.AddPart(part.OpenXmlPart, part.RelationshipId);
				// perform replacements in resultDoc.MainDocumentPart
			}
			var r = referral.Referral;
			SearchAndReplaceInText.openxml_replace_text(resultDoc.MainDocumentPart.Document.Body,
				new (string, string)[] {
					("{hospital_number}", r.LocalHospitalNumber),
					("{nhs_number}",r.NHSNumberKey.ToString("000 000 0000")),
					("{patient_name}",r.Name),
					("{patient_dob}",r.DateOfBirth.ToShortDateString()),
					("{patient_address}", PatientAddress),
					("{referring_physician}", ReferringPhysician),
					("{consultant_name}", ConsultantName),
				});


		}


	}

	private void SelectSaveLocationButton_Click(object sender, RoutedEventArgs e)
	{
		var d = new SaveFileDialog();
		d.FileName = $"Patient Letter for {referral.Name} {DateTime.Now.Year}_{DateTime.Now.Month}_{DateTime.Now.Day}"; // Default file extension
		d.DefaultExt = ".docx"; // Default file extension
		d.Filter = "Word files|*.docx"; // Filter files by extension
		if (d.ShowDialog() == true && d.CheckPathExists)
		{
			SaveLocation = d.FileName;
			NotifyPropertyChanged(SaveLocation);
		}
	}
}
//https://gist.github.com/shimondoodkin/7471075
public static class SearchAndReplaceInText
{

	public static void AllIndexesOf(List<int> indexes, string? str, string? substr, bool ignoreCase = false) // modified of http://stackoverflow.com/a/14308894/466363
	{
		if (string.IsNullOrWhiteSpace(str) ||
			string.IsNullOrWhiteSpace(substr))
		{
			return;
		}


		int index = 0;

		while ((index = str.IndexOf(substr, index, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) != -1)
		{
			indexes.Add(index++);
		}

		return;
	}



	public static void openxml_replace_text(OpenXmlElement el, (string from, string to)[] replacements)//version 2
	{
		// this is a quite smart and simple algorithm by Shimon Doodkin
		// the idea is to concat all texts and search it as string.
		// then replace text by positions step by step
		List<int> foundat = new();
		List<int> foundatend = new();
		StringBuilder innertext = new();
		List<Text> tofixnewlines = new();
		List<Text> todeleteempty = new();
		List<string> tofixnewlines_str = new();
		foreach ((string from, string to) in replacements)
		{
			innertext.Clear();
			foreach (Text eltext in el.Descendants<Text>()) { innertext.Append(eltext.Text); } // maybe to add space if previous element had no space at the end and this element has no space at beggining or add new line...no... but this problem is only with tables..
			string innertextstr = innertext.ToString();

			foundat.Clear();
			foundatend.Clear();

			AllIndexesOf(foundat,innertextstr, from);

			for (int z = 0; z < foundat.Count; z++)
			{
				foundatend.Add(foundat[z] + from.Length - 1);
			}

			//if (foundat.Count != 0)
			//{
			//    Console.WriteLine("from:'" + from + "' between " + foundat[0] + " to " + foundatend[0]);
			//    for (int i = 0; i < innertextstr.Length; i++)
			//    {
			//        Console.WriteLine(" [" + i + "]: " + ((int)innertextstr[i]) + " '" + innertextstr[i] + "'");        
			//    }
			//}
			//Console.WriteLine(innertext.ToString().Contains(from) ? "contains" : "not found");
			 tofixnewlines.Clear();
			todeleteempty.Clear();
			tofixnewlines_str.Clear();

			int currenttext_from = 0, currenttext_to = -1;
			int innertextpos = 0;
			if (foundat.Count != 0)
			{
				foreach (Text eltext in el.Descendants<Text>())
				{

					currenttext_from = currenttext_to + 1;
					currenttext_to += eltext.Text.Length;
					//Console.WriteLine("currenttext_from: " + currenttext_from + " currenttext_to: " + currenttext_to);
					if (foundat.Count == 0) break;
					if (foundat.First() <= currenttext_from && currenttext_from <= foundatend.First() // the beggining of this block is inside a found
						 || foundat.First() <= currenttext_to && currenttext_to <= foundatend.First() // the end of this block is inside a found
						 || currenttext_to <= foundat.First() && foundatend.First() <= currenttext_to // found is inside block
						)
					{
						//Console.WriteLine("#"+eltext.OuterXml);
						StringBuilder newtext = new ();
						//is innertextpos in a match?
						innertextpos = currenttext_from;
						for (int curchar = 0; curchar < eltext.Text.Length; curchar++)
						{
							if (foundat.Count == 0) break;
							if (innertextpos == foundat.First())
							{
								newtext.Append(to);
							}
							else if (innertextpos >= foundat.First() && innertextpos <= foundatend.First())
							{
								int replacewithcharat = innertextpos - foundat.First();
								//newtext.Append(to[replacewithcharat]);
								if (innertextpos == foundatend.First())
								{
									//if (replacewithcharat < to.Length)
									//{
									//newtext.Append(to.Substring(replacewithcharat + 1));
									//}
									//append add rest;
									foundat.RemoveAt(0);
									foundatend.RemoveAt(0);
								}
							}
							else
								newtext.Append(eltext.Text[curchar]);
							innertextpos++;
						}
						string newtextstr = newtext.ToString();

						if (newtextstr.IndexOf('\n') == -1)
							eltext.Text = newtextstr;
						else
						{
							eltext.Text = "to be replaced";
							tofixnewlines.Add(eltext);
							tofixnewlines_str.Add(newtextstr);
						}

						if (newtextstr.Length == 0)
						{
							todeleteempty.Add(eltext);
						}

						/*
						 * example word document with a newline
	<w:body>
	  <w:p w:rsidR="00377636" w:rsidRDefault="00F653FC">
		 <w:pPr>
			<w:rPr>
			   <w:rtl />
			</w:rPr>
		 </w:pPr>
		 <w:r>
			<w:t>AAA</w:t>
		 </w:r>
		 <w:r>
			<w:rPr>
			   <w:rFonts w:hint="cs" />
			   <w:rtl />
			</w:rPr>
			<w:br />
		 </w:r>
		 <w:r>
			<w:t>BBB</w:t>
		 </w:r>
		 <w:bookmarkStart w:id="0" w:name="_GoBack" />
		 <w:bookmarkEnd w:id="0" />
	  </w:p>
	  <w:sectPr w:rsidR="00377636" w:rsidSect="002510AE">
		 <w:pgSz w:w="11906" w:h="16838" />
		 <w:pgMar w:top="1440" w:right="1800" w:bottom="1440" w:left="1800" w:header="708" w:footer="708" w:gutter="0" />
		 <w:cols w:space="708" />
		 <w:bidi />
		 <w:rtlGutter />
		 <w:docGrid w:linePitch="360" />
	  </w:sectPr>
	</w:body>
						 */
					}
					// else
					//     Console.WriteLine(eltext.OuterXml);
				}

				//fix newlines:
				for (int i = 0; i < tofixnewlines.Count; i++)
				{
					string[] lines = tofixnewlines_str[i].Replace("\r", "").Split('\n');
					Text last_el = tofixnewlines[i];
					OpenXmlElement newline_el;
					OpenXmlElement copy_el;
					last_el.Text = lines[0];
					Text next_el;
					for (int j = 1; j < lines.Length; j++)
					{
						//create nextline text
						copy_el = last_el.Parent.CloneNode(true);
						next_el = copy_el.Descendants<Text>().First();
						next_el.Text = lines[j];

						//create newline  //"<w:r><w:rPr><w:rFonts w:hint="cs" /><w:rtl /></w:rPr><w:br /></w:r>"
						newline_el = last_el.Parent.CloneNode(true);
						IEnumerable<OpenXmlElement> se = newline_el.ChildElements.Where(e => e.LocalName != "rPr");
						foreach (OpenXmlElement item in se) item.Remove();
						newline_el.AppendChild(new Break());//<w:br />


						last_el.Parent.InsertAfterSelf(copy_el);
						last_el.Parent.InsertAfterSelf(newline_el);//add a newline after the last_el.Parent(the add order is switched,i always add after the first element but in reverse order)

						last_el = next_el;

					}
				}

				for (int i = 0; i < todeleteempty.Count; i++)
				{
					Text eltext = todeleteempty[i];
					//if (eltext.Parent.ChildElements.Count <= 2 && newtextstr.Length == 0)// run.childern<=2 means Run countains the only w:rPr and w:t or just w:t
					//  {
					eltext.Parent.Remove();//remove empty run,not sure if this is good, i dont know mybe run could countain other elements besides text like images.
										   // }
				}
			}
		}
	}
}
