using System.Threading.Tasks;

namespace Session_1_Dennis_Hilfinger;

public partial class CrashReportWindow : ContentPage
{
	private Login LoginData;
	public CrashReportWindow(Login loginData)
	{
		InitializeComponent();
		InfoLabel.Text = $"Your last login was on {loginData.LoginTime.ToLocalTime()} and you didn't properly log out. Please state the reason for this failed logout below:";
		LoginData = loginData;
	}

	private async void Submit(object sender, EventArgs e)
	{
		string reason = InfoInput.Text;

		if (String.IsNullOrEmpty(reason.Trim()))
		{
			await DisplayAlert("Info", "Please enter a reason for the missing logout time!", "Ok");
			return;
		}

		using(var db = new AirlineContext())
		{
			var login = db.Logins.FirstOrDefault(l => l.Id == LoginData.Id);
			login.ErrorMessage = reason;
			db.Update(login);
			db.SaveChanges();
			
			Application.Current.CloseWindow(this.GetParentWindow());
		}
	}
}