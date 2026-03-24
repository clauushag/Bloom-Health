using app.Data;
using app.Models;

namespace app;

public partial class MainPage : ContentPage
{
    private SaludDatabase _database;

    // Modificamos el constructor para recibir la base de datos
    public MainPage(SaludDatabase database)
    {
        InitializeComponent();
        _database = database;
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
{
    // 1. Verificamos que los campos no estén vacíos
    if (string.IsNullOrWhiteSpace(NombreEntry.Text) || string.IsNullOrWhiteSpace(SintomasEditor.Text))
    {
        await DisplayAlertAsync("Atención", "Por favor, llena todos los campos.", "OK");
        return; // Detenemos la ejecución si falta información
    }

    // 2. Creamos el paciente con los datos que escribió el usuario
    var nuevoPaciente = new Paciente 
    { 
        Nombre = NombreEntry.Text, 
        Sintomas = SintomasEditor.Text 
    };

    // 3. Lo guardamos en SQLite
    await _database.GuardarPacienteAsync(nuevoPaciente);
    
    // 4. Mostramos el mensaje de éxito
    await DisplayAlertAsync("Éxito", $"El paciente {nuevoPaciente.Nombre} ha sido guardado.", "OK");

    // 5. Limpiamos los campos para poder ingresar otro paciente
    NombreEntry.Text = string.Empty;
    SintomasEditor.Text = string.Empty;
}
}