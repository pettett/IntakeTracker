using IntakeTrackerApp.DataManagement;

namespace IntakeTrackerApp.Controls;
/// <summary>
/// Allows objects to be used as tabs in the vault view
/// </summary>
public interface ITabable
{
	bool CanClose { get; }
	string Header { get; }
	object GenerateContent(Vault v);
	void OnOpened();
	void Refresh();
}
/// <summary>
/// Tab object to reference a specific patient referral
/// </summary>
/// <param name="Referral"></param>
public record ReferralTab(PatientReferral Referral) : ITabable
{
	public bool CanClose => true;
	public string Header => Referral.Name;

	public object GenerateContent(Vault v)
	{
		return new PatientView(Referral, v);
	}
	public void OnOpened()
	{

	}
	public void Refresh()
	{

	}
}

public record SummaryTab(TestType TestName, bool IncludeNone) : ITabable
{
	public bool CanClose => !IncludeNone;
	public string Header => $"{TestName} Summary";

	private TestSummary? s;
	public object GenerateContent(Vault v)
	{
		s = new TestSummary(v, TestName, IncludeNone);
		return s;
	}
	public void OnOpened()
	{
		s?.Refresh();
	}
	public void Refresh()
	{
		s?.Refresh();
	}
}