using Windows.Devices.AllJoyn;

namespace Session_1_Dennis_Hilfinger
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            //ImportData();
        }

        private async void Login(object sender, EventArgs e)
        {
            string email = UsernameInput.Text;
            string password = PasswordInput.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Error", "Please enter both username and password.", "OK");
                return;
            }

            using(var db = new AirlineContext())
            {
                try
                {
                    var user = db.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower() && u.Password == GetMd5Password(password));
                    if (user != null)
                    {
                        if (user.Active == false)
                        {
                            await DisplayAlert("Error", "Your account is inactive. Please contact support.", "OK");
                            return;
                        }
                        var login = new Login
                        {
                            UserId = user.Id,
                            LoginTime = DateTime.UtcNow
                        };
                        db.Logins.Add(login);
                        db.SaveChanges();
                        await DisplayAlert("Success", "Login successful!", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "Invalid username or password.", "OK");
                    }
                } catch
                {
                    await DisplayAlert("Error", "An error occurred during login. Please try again later", "OK");
                }
            }
        }

        private async void Exit(object sender, EventArgs e)
        {
            Application.Current.Quit();
        }

        private async void ImportData()
        {
            using (var db = new AirlineContext())
            {
                string[] lines = File.ReadAllLines("Data/UserData.csv");

                int userId = 0;
                if (db.Users.Count() != 0)
                {
                    userId = db.Users.Max(u => u.Id);
                }

                foreach(var line in lines)
                {
                    string[] data = line.Split(',');

                    string role = data[0];
                    string email = data[1];
                    string password = data[2];
                    string md5Password = GetMd5Password(password);

                    string firstname = data[3];
                    string lastname = data[4];
                    string office = data[5];

                    var birthDateParts = data[6].Split('/');
                    var year = int.Parse(birthDateParts[2]);
                    var month = int.Parse(birthDateParts[0]);
                    var day = int.Parse(birthDateParts[1]);
                    DateOnly birthdate = new DateOnly(year, month, day);

                    bool active = data[7] == "1" ? true : false;

                    var roleId = db.Roles.FirstOrDefault(r => r.Title.ToLower() == role.ToLower()).Id;
                    var officeId = db.Offices.FirstOrDefault(o => o.Title.ToLower() == office.ToLower()).Id;

                    if (db.Users.Any(u => u.Email.ToLower() == email.ToLower()))
                    {
                        continue;
                    };

                    User user = new User
                    {
                        Id = userId,
                        RoleId = roleId,
                        OfficeId = officeId,
                        Email = email,
                        Password = md5Password,
                        FirstName = firstname,
                        LastName = lastname,
                        Birthdate = birthdate,
                        Active = active
                    };
                    db.Users.Add(user);
                    userId++;
                }
                db.SaveChanges();
            }
        }

        private string GetMd5Password(string password)
        {
            return string.Join("", System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("x2")));
        }

    }
}
