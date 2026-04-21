namespace app;

public partial class NutricionPage : ContentPage
{
    public NutricionPage()
    {
        InitializeComponent();
    }
    private async void OnAbrirScannerTapped(object sender, TappedEventArgs e)
{
    // Pedimos permiso de cámara en tiempo real antes de abrir (súper importante en Android modernos)
    var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
    if (status != PermissionStatus.Granted)
    {
        status = await Permissions.RequestAsync<Permissions.Camera>();
    }

    if (status == PermissionStatus.Granted)
    {
        // Si nos da permiso, abrimos la página de la cámara
        await Navigation.PushModalAsync(new ScannerPage());
    }
    else
    {
        await DisplayAlert("Error", "Necesitamos acceso a la cámara para escanear", "OK");
    }
}

}