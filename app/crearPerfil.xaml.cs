using app.Data;
using app.Models;

namespace app;

public partial class crearPerfil : ContentPage
{
    private SaludDatabase _database;
    public Usuario UsuarioActual { get; set; }

    // Modificamos el constructor para recibir la base de datos
    public crearPerfil(SaludDatabase database)
    {
        InitializeComponent();
        _database = database;
        UsuarioActual = new Usuario();
        BindingContext = UsuarioActual;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _database.InicializarAsync(); // Aseguramos que la base de datos esté inicializada antes de usarla


    }
    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(UsuarioActual));
        Console.WriteLine($"Fecha de Nacimiento: {UsuarioActual.GetFechaNacimiento()}");
        if (UsuarioActual.EsValido())
        {
            await _database.InsertarUsuarioAsync(UsuarioActual);
            await Shell.Current.GoToAsync("//MainPage");
        }
        else
        {
            await DisplayAlert("Error", "Rellena todos los campos", "OK");
        }
    }
}