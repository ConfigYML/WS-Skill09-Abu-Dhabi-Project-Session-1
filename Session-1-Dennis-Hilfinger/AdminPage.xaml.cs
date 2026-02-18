namespace Session_1_Dennis_Hilfinger;

public partial class AdminPage : ContentPage
{
	public AdminPage()
	{
		InitializeComponent();
	}


    private async void Exit(object sender, EventArgs e)
    {
        Application.Current.Quit();
    }
}