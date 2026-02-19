
using Microsoft.EntityFrameworkCore;

namespace Session_1_Dennis_Hilfinger;

public partial class AddEditUserPage : ContentPage, IQueryAttributable
{
    private bool IsEditMode = false;
    public AddEditUserPage()
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        using (var db = new AirlineContext())
        {
            IsEditMode = true;

            var offices = db.Offices.Select(o => o.Title).ToList();
            OfficePicker.ItemsSource = offices;

            var userToEdit = query["UserToEdit"];
            // Edit user
            if (userToEdit != null)
            {
                Title = "Edit User";
                BirthdateLayout.IsVisible = false;
                PasswordLayout.IsVisible = false;

                EmailEntry.IsEnabled = false;
                NameEntry.IsEnabled = false;
                LastNameEntry.IsEnabled = false;
                OfficePicker.IsEnabled = false;


                var user = db.Users
                    .Include(u => u.Role)
                    .Include(u => u.Office)
                    .FirstOrDefault(u => u.Email == userToEdit);
                if (user != null)
                {
                    EmailEntry.Text = user.Email;
                    NameEntry.Text = user.FirstName;
                    LastNameEntry.Text = user.LastName;
                    if (user.Office != null)
                    {
                        OfficePicker.SelectedItem = user.Office.Title;
                    }
                    if (user.Role.Id == 1)
                    {
                        AdminRadioButton.IsChecked = true;
                        UserRadioButton.IsChecked = false;
                    } else
                    {
                        AdminRadioButton.IsChecked = false;
                        UserRadioButton.IsChecked = true;
                    }
                }
            }
            else // Add user
            {
                IsEditMode = false;
                Title = "Add User";
                RoleLayout.IsVisible = false;

            }
        }

    }

    private async void SaveUser(object sender, EventArgs e)
    {
        using (var db = new AirlineContext())
        {
            if (IsEditMode)
            {
                var user = db.Users.FirstOrDefault(u => u.Email == EmailEntry.Text);
                if (user != null)
                {
                    Role role;
                    if (AdminRadioButton.IsChecked)
                    {
                        role = db.Roles.FirstOrDefault(r => r.Title.ToLower().Contains("admin"));
                    } else
                    {
                        role = db.Roles.FirstOrDefault(r => r.Title.ToLower().Contains("user"));
                    }
                    user.RoleId = role.Id;
                    db.Update(user);
                    await db.SaveChangesAsync();
                    await DisplayAlert("Success", "User updated successfully!", "OK");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(EmailEntry.Text) || string.IsNullOrEmpty(NameEntry.Text) || string.IsNullOrEmpty(LastNameEntry.Text) || string.IsNullOrEmpty(PasswordEntry.Text) || OfficePicker.SelectedItem == null)
                {
                    await DisplayAlert("Error", "Please fill in all fields.", "OK");
                    return;
                }

                if(BirthdatePicker.Date > DateTime.Now)
                {
                    await DisplayAlert("Error", "Birthdate cannot be in the future.", "OK");
                    return;
                }

                var office = db.Offices.FirstOrDefault(o => o.Title == OfficePicker.SelectedItem.ToString());
                var role = db.Roles.FirstOrDefault(r => r.Title.ToLower().Contains("user"));
                var highestUserId = db.Users.Max(u => (int?)u.Id) ?? 0;
                var newUser = new User
                {
                    Id = highestUserId + 1,
                    Email = EmailEntry.Text,
                    FirstName = NameEntry.Text,
                    LastName = LastNameEntry.Text,
                    Password = GetMd5Password(PasswordEntry.Text),
                    Birthdate = DateOnly.FromDateTime(BirthdatePicker.Date),
                    RoleId = role.Id,
                    OfficeId = office.Id,
                    Active = true
                };
                db.Users.Add(newUser);
                await db.SaveChangesAsync();
                await DisplayAlert("Success", "User added successfully!", "OK");
                
            }
            //Navigate back
        }
    }

    private async void Cancel(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private string GetMd5Password(string password)
    {
        return string.Join("", System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("x2")));
    }

}