using IntakeTrackerApp.DataManagement;

namespace IntakeTrackerApp.Controls;
/// <summary>
/// Allows objects to be used as tabs in the vault view
/// </summary>
public interface ITabable
{
	string Header { get; }
	object GenerateContent(Vault v, VaultViewControl control);
	void OnOpened();
	void Refresh();
}
/// <summary>
/// Tab object to reference a specific patient referral
/// </summary>
/// <param name="Referral"></param>
public record ReferralTab(PatientReferral Referral) : ITabable
{
	public string Header => Referral.Name;

	public object GenerateContent(Vault v, VaultViewControl control)
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

public record SummaryTab(TestType TestName) : ITabable
{
	public string Header => $"{TestName} Summary";

	private TestSummary? s;
	public object GenerateContent(Vault v, VaultViewControl control)
	{
		s = new TestSummary(v, TestName, control);
		return s;
	}
	public void OnOpened()
	{
		s?.FilteredReferrals.Refresh();
	}
	public void Refresh()
	{
		s?.FilteredReferrals.Refresh();
	}
}