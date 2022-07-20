using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntakeTrackerApp.DataManagement.Filtering;

/// <summary>
/// Decorator class for filter
/// </summary>
/// <typeparam name="P"></typeparam>
/// <typeparam name="Q"></typeparam>
/// <param name="MappedFilter"></param>
/// <param name="Map"></param>
public record MapFilter<P, Q>(IFilter<Q> MappedFilter, Func<P, Q> Map) : IFilter<P>
{
	public bool Filter(P val) => MappedFilter.Filter(Map(val));
	public void AddChangedListener(Action listener) => MappedFilter.AddChangedListener(listener);
	public void RemoveChangedListener(Action listener) => MappedFilter.RemoveChangedListener(listener);
}
