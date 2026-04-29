namespace app;

public partial class MenstrualPage : ContentPage
{
    public MenstrualPage()
    {
        InitializeComponent();
    }

    private async void OnInicioTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }

    private async void OnRetosTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//RetosPage");
    }

    private async void OnPerfilTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PerfilPage");
    }

    private async void OnVolverClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//MainPage");

}