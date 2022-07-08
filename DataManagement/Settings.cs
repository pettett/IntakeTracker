namespace IntakeTrackerApp.DataManagement;


public class Settings
{

	public string[] ReferralManagers { get; set; } = Array.Empty<string>();
	public string[] TransferRegions { get; set; } = Array.Empty<string>();

	public uint MRIReportWarningThreshold { get; set; } = 21u;
	public uint LPAppointmentWarningThreshold { get; set; } = 14u;
	public uint LPReportedWarningThreshold { get; set; } = 28u;
	public uint EPAppointmentWarningThreshold { get; set; } = 21u;
	public uint EPReportedWarningThreshold { get; set; } = 2u;
	public uint BloodsAppointmentWarningThreshold { get; set; } = 7u;
	public uint BloodsReportedWarningThreshold { get; set; } = 2u;

	public DateTime? LastBackup { get; set; } = null;



}
