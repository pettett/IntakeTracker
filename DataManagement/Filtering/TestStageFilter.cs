using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntakeTrackerApp.DataManagement.Filtering;

public class TestStageFilter : FilterBase<PatientReferral>
{
	private readonly TestType type;
	private readonly TestStage validStages;

	public TestStageFilter(TestType type, TestStage validStages) : base("Test Stage")
	{
		this.type = type;
		this.validStages = validStages;
		Enabled.Item = true;
	}

	public override bool Filter(PatientReferral val)
	{
		return validStages.HasFlag(val.Test(type)?.TestStage ?? 0);
	}


}
