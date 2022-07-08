using IntakeTrackerApp.DataManagement;
using System.Net.Http;
using System.Net.Http.Headers;

namespace IntakeTrackerApp.Windows;

/// <summary>
/// Interaction logic for AddRandomPatients.xaml
/// </summary>
public partial class AddRandomPatients : Window
{
	HttpClient client = new();
	readonly Vault v;
	public AddRandomPatients(Vault v)
	{
		this.v = v;
		client.DefaultRequestHeaders.Accept.Add(
		new MediaTypeWithQualityHeaderValue("application/json"));
		client.BaseAddress = new Uri("https://randomuser.me/api/");
		InitializeComponent();
	}
	Results? d;
	private record Results(List<Person> results);

	private void ConfirmButton_Click(object sender, RoutedEventArgs e)
	{
		Random rand = new();
		foreach (var p in d.results)
		{
			PatientReferral r = new()
			{
				NHSNumberKey = NHSNum.GenRandom(rand),
				FirstName = p.name.first,
				LastName = p.name.last,
			};
			v.Context.Add(r);
		}
		Close();
	}

	private new record Name(string title, string first, string last);
	private record Person(Name name, string gender);

	private async void GenerateButton_Click(object sender, RoutedEventArgs e)
	{
		HttpResponseMessage response = await client.GetAsync($"?results={(uint)AmountSlider.Value}");
		response.EnsureSuccessStatusCode();

		// Parse the response body.
		d = await System.Text.Json.JsonSerializer.DeserializeAsync<Results>(await response.Content.ReadAsStreamAsync());


		PersonList.ItemsSource = d.results;


	}

}

