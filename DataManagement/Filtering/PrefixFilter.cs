using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntakeTrackerApp.DataManagement.Filtering;

public class PrefixFilter : FilterBase<string>
{
	public PrefixFilter(string title) : base(title)
	{
		Prefix.PropertyChanged += (_, _) => OnFilterChanged();
	}

	public ObservableItem<string> Prefix { get; set; } = "";
	public override bool Filter(string val)
	{
		return val.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);
	}
}
