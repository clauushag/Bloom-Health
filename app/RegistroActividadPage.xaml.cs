using app.Data;
using app.Models;
namespace app;

public partial class RegistroActividadPage : ContentPage
{
    // Variable para recordar cuál fue la última tarjeta que tocamos
    private Frame _frameSeleccionadoAnteriormente;
    private SaludDatabase _database;
    public Fisico fisico { get; set; }
    public RegistroDiario registro { get; set; }
    public RegistroActividadPage(SaludDatabase database)
    {
        InitializeComponent();
        _database = database;
        fisico = new Fisico();
        registro = new RegistroDiario();
        BindingContext = fisico;
    }

    private void OnActividadTapped(object sender, TappedEventArgs e)
    {
        // 1. Si ya había una tarjeta seleccionada antes, la volvemos a poner blanca
        if (_frameSeleccionadoAnteriormente != null)
        {
            _frameSeleccionadoAnteriormente.BackgroundColor = Colors.White;
            _frameSeleccionadoAnteriormente.BorderColor = Colors.Transparent;
        }

        // 2. Obtenemos la tarjeta (Frame) exacta que acabas de tocar ahora mismo
        var frameActual = (Frame)sender;

        // 3. Pintamos la tarjeta tocada de verde
        frameActual.BackgroundColor = Color.FromArgb("#F0F5F1");
        frameActual.BorderColor = Color.FromArgb("#8EB497");

        // 4. Guardamos esta tarjeta en la memoria para "deseleccionarla" la próxima vez
        _frameSeleccionadoAnteriormente = frameActual;

        // 5. Mostramos el formulario de abajo
        ContenedorFormulario.IsVisible = true;

        // 6. Cambiamos el título del formulario
        if (e.Parameter != null)
        {
            LabelActividadSeleccionada.Text = "Registrar " + e.Parameter.ToString();
        }
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        registro.SetFecha(DateTime.Now);
        registro.ID_Usuario = 1; // Aquí deberías poner el ID del usuario actual, esto es solo un ejemplo


        fisico.Tipo_Actividad = fisico.TiposActividad[0];
        fisico.Distancia = 5;
        fisico.Kcal_Quemadas = 300;
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(fisico));
        if (fisico.EsValido())
        {
            int id = await _database.InsertarRegistroAsync(registro);
            fisico.ID_Registro = id;
            await DisplayAlert("Error", $"Registro guardado con éxito. ID: {id}", "OK");
            await DisplayAlert("Error", $"Fisico: {id}, Distancia: {fisico.Distancia}, Kcal: {fisico.Kcal_Quemadas}, Tipo: {fisico.Tipo_Actividad}", "OK");
            await _database.InsertarFisicoAsync(fisico);
            await Shell.Current.GoToAsync("//MainPage");
        }
        else
        {
            await DisplayAlert("Error", "Rellena todos los campos", "OK");
        }
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

}