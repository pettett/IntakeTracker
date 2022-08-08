using nGantt.PeriodSplitter;
using System.Windows.Input;

namespace nGantt.GanttChart
{
    public delegate void SelectionContextMenuItemClick(Period selectedPeriod);

    public class SelectionContextMenuItem
    {
        public SelectionContextMenuItem(SelectionContextMenuItemClick contextMenuItemClick, string name)
        {
            //TODO: fix
            // ContextMenuItemClickCommand = new DelegateCommand<Period>(x => contextMenuItemClick(x));
            this.Name = name;
        }

        public string Name { get; set; }

        public ICommand ContextMenuItemClickCommand { get; private set; }
    }
}
