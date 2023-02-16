using nGantt;
using nGantt.GanttChart;
using nGantt.PeriodSplitter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IntakeTrackerApp.Windows;

/// <summary>
/// Interaction logic for TimelineTest.xaml
/// </summary>
public partial class TimelineTest : Window
{


	private int GanttLengh { get; set; }
	private ObservableCollection<ContextMenuItem> ganttTaskContextMenuItems = new();
	private ObservableCollection<SelectionContextMenuItem> selectionContextMenuItems = new();

	public TimelineTest()
	{
		InitializeComponent();

		GanttLengh = 365;
		dateTimePicker.SelectedDate = DateTime.Parse("2012-02-01");
		DateTime minDate = dateTimePicker.SelectedDate.Value;
		DateTime maxDate = minDate.AddDays(GanttLengh);

		// Set selection -mode
		ganttControl1.TaskSelectionMode = nGantt.GanttControl.SelectionMode.Single;
		// Enable GanttTasks to be selected
		ganttControl1.AllowUserSelection = true;

		// listen to the GanttRowAreaSelected event
		ganttControl1.GanttRowAreaSelected += ganttControl1_GanttRowAreaSelected;

		// define ganttTask context menu and action when each item is clicked
		ganttTaskContextMenuItems.Add(new ContextMenuItem(ViewClicked, "View..."));
		ganttTaskContextMenuItems.Add(new ContextMenuItem(EditClicked, "Edit..."));
		ganttTaskContextMenuItems.Add(new ContextMenuItem(DeleteClicked, "Delete..."));
		ganttControl1.GanttTaskContextMenuItems = ganttTaskContextMenuItems;

		// define selection context menu and action when each item is clicked
		selectionContextMenuItems.Add(new SelectionContextMenuItem(NewClicked, "New..."));
		ganttControl1.SelectionContextMenuItems = selectionContextMenuItems;


		CreateData(minDate, maxDate);
	}

	private void NewClicked(Period selectionPeriod)
	{
		MessageBox.Show("New clicked for task " + selectionPeriod.Start.ToString() + " -> " + selectionPeriod.End.ToString());
	}

	private void ViewClicked(GanttTask ganttTask)
	{
		MessageBox.Show("New clicked for task " + ganttTask.Name);
	}

	private void EditClicked(GanttTask ganttTask)
	{
		MessageBox.Show("Edit clicked for task " + ganttTask.Name);
	}

	private void DeleteClicked(GanttTask ganttTask)
	{
		MessageBox.Show("Delete clicked for task " + ganttTask.Name);
	}

	void ganttControl1_GanttRowAreaSelected(object? sender, PeriodEventArgs e)
	{
		MessageBox.Show(e.SelectionStart.ToString() + " -> " + e.SelectionEnd.ToString());
	}

	private System.Windows.Media.Brush DetermineBackground(TimeLineItem timeLineItem)
	{
		return new SolidColorBrush(System.Windows.Media.Colors.Transparent);
	}

	private void CreateData(DateTime minDate, DateTime maxDate)
	{
		//Set max and min dates
		ganttControl1.Initialize(minDate, maxDate);

		// Create timelines and define how they should be presented
		ganttControl1.CreateTimeLine(new PeriodYearSplitter(minDate, maxDate), FormatYear);
		var gridLineTimeLine = ganttControl1.CreateTimeLine(new PeriodMonthSplitter(minDate, maxDate), FormatMonth);


		// Set the timeline to atatch gridlines to
		ganttControl1.SetGridLinesTimeline(gridLineTimeLine, DetermineBackground);

		// Create and data

		var rowgroup2 = ganttControl1.CreateGanttRowGroup("ExpandableGanttRowGroup", true);
		var row2 = ganttControl1.CreateGanttRow(rowgroup2, "GanttRow 2");
		var row3 = ganttControl1.CreateGanttRow(rowgroup2, "GanttRow 3");
		ganttControl1.AddGanttTask(row2, new GanttTask() { Start = DateTime.Parse("2012-02-10"), End = DateTime.Parse("2012-03-10"), Name = "GanttRow 2:GanttTask 1" });
		ganttControl1.AddGanttTask(row2, new GanttTask() { Start = DateTime.Parse("2012-03-25"), End = DateTime.Parse("2012-05-10"), Name = "GanttRow 2:GanttTask 2" });
		ganttControl1.AddGanttTask(row2, new GanttTask() { Start = DateTime.Parse("2012-06-10"), End = DateTime.Parse("2012-09-15"), Name = "GanttRow 2:GanttTask 3" });
		ganttControl1.AddGanttTask(row3, new GanttTask() { Start = DateTime.Parse("2012-01-07"), End = DateTime.Parse("2012-09-15"), Name = "GanttRow 3:GanttTask 1" });

	}

	private string FormatYear(Period period)
	{
		return period.Start.Year.ToString();
	}

	private string FormatMonth(Period period)
	{
		return period.Start.Month.ToString();
	}

	private string FormatDay(Period period)
	{
		return period.Start.Day.ToString();
	}

	private string FormatDayName(Period period)
	{
		return period.Start.DayOfWeek.ToString();
	}

	private void buttonPrevious_Click(object sender, EventArgs e)
	{
		//dateTimePicker.SelectedDate = ganttControl1.GanttData.MinDate.AddDays(-GantLenght);
	}

	private void buttonNext_Click(object sender, EventArgs e)
	{
		//	dateTimePicker.SelectedDate = ganttControl1.GanttData.MaxDate;
	}

	private void dateTimePicker_ValueChanged(object sender, EventArgs e)
	{
		DateTime minDate = dateTimePicker.SelectedDate.Value;
		DateTime maxDate = minDate.AddDays(GanttLengh);
		//	ganttControl1.ClearGantt();
		CreateData(minDate, maxDate);
	}

}
