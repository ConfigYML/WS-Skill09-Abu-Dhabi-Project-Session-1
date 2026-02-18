using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Session_1_Dennis_Hilfinger;

public partial class UserPage : ContentPage, IQueryAttributable
{
	private int UserId = 0;

	public UserPage()
	{
		InitializeComponent();
		CheckLastLogin();
		LoadData();
	}

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        UserId = int.Parse((string)query["UserId"]);
    }

    private async void CheckLastLogin()
	{
		//NOTE for later: take a look at Preferences API in .NET

		using(var db = new AirlineContext())
		{
			var lastLogin = db.Logins.OrderByDescending(l => l.LoginTime).ElementAt(1);
			if (lastLogin.LogoutTime == null)
			{
				Window win = new Window();
				win.Page = new CrashReportWindow(lastLogin);
				win.MaximumHeight = 400;
				win.MaximumWidth = 800;
				win.MinimumHeight = 400;
				win.MinimumWidth = 800;
                Application.Current.OpenWindow(win);
			}
        }
    }

	private async void LoadData()
	{
		using(var db = new AirlineContext())
		{
			var user = db.Users.FirstOrDefault(u => u.Id == UserId);
			var fullname = $"{user.FirstName} {user.LastName}";
			var timeSpent = db.Logins.Where(l => l.UserId == UserId && l.LogoutTime != null).Sum(l => EF.Functions.DateDiffSecond(l.LoginTime, l.LogoutTime.Value));
            TimeSpan timeSpentSpan = TimeSpan.FromSeconds(timeSpent);
			var crashCount = db.Logins.Where(l => l.UserId == UserId && l.ErrorMessage != null).Count();

            WelcomeLabel.Text = String.Format(
				"Hi {0}, Welcome to AMONIC Airlines Automation System Time spent on system: {1} Number of crashes: {2}", 
				fullname,
                timeSpentSpan.ToString(),
				crashCount);

            var logins = await db.Logins.Where(l => l.UserId == UserId).OrderByDescending(l => l.LoginTime).ToListAsync();
			if (logins.Count > 0)
			{
                logins.RemoveAt(0);
            }

			List<LoginDTO> loginDTOs = new List<LoginDTO>();
			foreach(var login in logins)
			{
				loginDTOs.Add(new LoginDTO
				{
					Date = DateOnly.FromDateTime(login.LoginTime.ToLocalTime()),
					LoginTime = TimeOnly.FromDateTime(login.LoginTime.ToLocalTime()),
					LogoutTime = login.LogoutTime.HasValue ? TimeOnly.FromDateTime(login.LogoutTime.Value.ToLocalTime()) : null,
					ErrorMessage = login.ErrorMessage
				});
			}
			LoginGrid.ItemsSource = loginDTOs;
		}
    }
    private async void Exit(object sender, EventArgs e)
    {
        Application.Current.Quit();
    }

    class LoginDTO
	{
		public DateOnly Date { get; set; }
        public TimeOnly LoginTime { get; set; }
		public TimeOnly? LogoutTime { get; set; }
		public string ErrorMessage { get; set; }
    }
}