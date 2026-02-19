using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.ObjectModel;
using Windows.System;

namespace Session_1_Dennis_Hilfinger;

public partial class AdminPage : ContentPage, IQueryAttributable
{
    private int UserId = 0;
    private ObservableCollection<UserDTO> Users = new ObservableCollection<UserDTO>();
    public AdminPage()
	{
		InitializeComponent();
        BindingContext = this;
    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        UserId = int.Parse((string)query["UserId"]);
        FillPicker();
        LoadUsers(null, null);
    }

    private async void Exit(object sender, EventArgs e)
    {
        Application.Current.Quit();
    }

    private async void FillPicker()
    {
        using(var db = new AirlineContext())
        {
            var offices = await db.Offices.ToListAsync();
            OfficePicker.Items.Clear();
            OfficePicker.Items.Add("");
            foreach(var off in offices)
            {
                OfficePicker.Items.Add(off.Title.ToString());
            }
        }
    }

    private async void LoadUsers(object sender, EventArgs e)
    {
        using(var db = new AirlineContext())
        {
            Users.Clear();
            IEnumerable<User> userList;
            if (OfficePicker.SelectedItem != null)
            {
                if (OfficePicker.SelectedItem.ToString().Trim() == string.Empty)
                {
                    userList = await db.Users
                    .Include(u => u.Role)
                    .Include(u => u.Office)
                    .ToListAsync();
                } else
                {
                    var office = await db.Offices.FirstOrDefaultAsync(o => o.Title == OfficePicker.SelectedItem.ToString());
                    userList = await db.Users
                        .Where(u => u.OfficeId == office.Id)
                        .Include(u => u.Role)
                        .Include(u => u.Office)
                        .ToListAsync();
                }
            } else
            {
                userList = await db.Users
                    .Include(u => u.Role)
                    .Include(u => u.Office)
                    .ToListAsync();
            }
            
            foreach(var user in userList)
            {
                Users.Add(new UserDTO
                {
                    Name = user.FirstName,
                    Lastname = user.LastName,
                    Age = user.Birthdate.HasValue ? GetAge(user.Birthdate.Value) : 0,
                    Role = user.Role.Title,
                    Email = user.Email,
                    Office = user.Office != null ? user.Office.Title : "None",
                    Active = user.Active.HasValue ? user.Active.Value : false
                });
            }
            UserGrid.ItemsSource = Users;
            int i = 0;
        }
    }

    private int GetAge(DateOnly birthdate) {
        int age = DateTime.Now.Year - birthdate.Year;
        if (DateTime.Now.DayOfYear < birthdate.DayOfYear)
            age--;
        return age;
    }

    private async void AddUser(object sender, EventArgs e)
    {
        ShellNavigationQueryParameters parameters = new ShellNavigationQueryParameters()
        {
            { "UserToEdit", null }
        };
        await Shell.Current.GoToAsync("AddEditUserPage", parameters);
    }   

    private async void UserSelected(object sender, EventArgs e)
    {
        if (UserGrid.SelectedItem != null)
        {
            RoleButton.IsEnabled = true;
            SuspensionButton.IsEnabled = true;
            UserDTO user = UserGrid.SelectedItem as UserDTO;
            SuspensionButton.Text = user.Active ? "Suspend account" : "Unsuspend account";
        } else
        {
            SuspensionButton.Text = "Suspend/Unsuspend account";
            RoleButton.IsEnabled = false;
            SuspensionButton.IsEnabled = false;
        }
    }
    private async void ChangeRole(object sender, EventArgs e)
    {
        if (UserGrid.SelectedItem != null)
        {
            UserDTO user = UserGrid.SelectedItem as UserDTO;
            ShellNavigationQueryParameters parameters = new ShellNavigationQueryParameters()
            {
                { "UserToEdit", user.Email }
            };
            await Shell.Current.GoToAsync("AddEditUserPage", parameters);
        }
    }
    private async void ChangeActive(object sender, EventArgs e)
    {
        if (UserGrid.SelectedItem != null)
        {
            UserDTO selectedUser = UserGrid.SelectedItem as UserDTO;
            using(var db = new AirlineContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Email == selectedUser.Email);
                if (user.Id == UserId)
                {
                    await DisplayAlert("Info", "You can't enable/disable your own account!", "Ok");
                    return;
                }
                user.Active = !user.Active;
                db.Update(user);
                db.SaveChanges();
                SuspensionButton.Text = bool.Parse(user.Active.ToString()) ? "Suspend account" : "Unsuspend account";
                LoadUsers(null, null);
            }

        }
    }
    class UserDTO
    {
        public string Name { get; set; }
        public string Lastname { get; set; }
        public int Age { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Office { get; set; }
        public bool Active { get; set; }
    }
}