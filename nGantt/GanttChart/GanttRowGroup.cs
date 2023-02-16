using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace nGantt.GanttChart;

public class GanttRowGroup
{
    public GanttRowGroup()
    {
        Rows = new();
    }
    public ObservableCollection<GanttRow> Rows { get; set; }
}
