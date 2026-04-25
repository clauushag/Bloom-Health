using System.Globalization;
using app.Data;
using app.Models;

namespace app;

[QueryProperty(nameof(Nombre), "nombre")]
[QueryProperty(nameof(Marca), "marca")]
[QueryProperty(nameof(Kcal), "kcal")]
[QueryProperty(nameof(Proteinas), "proteinas")]
[QueryProperty(nameof(Carbos), "carbos")]
[QueryProperty(nameof(Grasas), "grasas")]
[QueryProperty(nameof(Fibra), "fibra")]
[QueryProperty(nameof(Imagen), "imagen")]
public partial class NutricionPage : ContentPage
{
    private readonly SaludDatabase _database;

    public string Nombre { get; set; } = "";
    public string Marca { get; set; } = "";
    public string Kcal { get; set; } = "";
    public string Proteinas { get; set; } = "";
    public string Carbos { get; set; } = "";
    public string Grasas { get; set; } = "";
    public string Fibra { get; set; } = "";
    public string Imagen { get; set; } = "";

    public NutricionPage(SaludDatabase database)
    {
        InitializeComponent();
        _database = database;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Si venimos del scanner, mostramos la tarjeta
        if (!string.IsNullOrEmpty(Nombre))
            MostrarResultado();

        // Siempre recargamos el historial
        await CargarHistorial();
    }

    private void MostrarResultado()
    {
        double.TryParse(Kcal, NumberStyles.Any, CultureInfo.InvariantCulture, out double kcal);
        double.TryParse(Proteinas, NumberStyles.Any, CultureInfo.InvariantCulture, out double proteinas);
        double.TryParse(Carbos, NumberStyles.Any, CultureInfo.InvariantCulture, out double carbos);
        double.TryParse(Grasas, NumberStyles.Any, CultureInfo.InvariantCulture, out double grasas);
        double.TryParse(Fibra, NumberStyles.Any, CultureInfo.InvariantCulture, out double fibra);

        FoodNameLabel.Text = Nombre;
        FoodBrandLabel.Text = string.IsNullOrEmpty(Marca) ? "Sin marca" : Marca;
        KcalLabel.Text = $"{kcal:F0}";
        ProteinasLabel.Text = $"{proteinas:F1}g";
        CarbosLabel.Text = $"{carbos:F1}g";
        GrasasLabel.Text = $"{grasas:F1}g";
        FibraLabel.Text = $"{fibra:F1}g";
        FoodImage.Source = string.IsNullOrEmpty(Imagen) ? null : Imagen;
        ResultadoCard.IsVisible = true;
    }

    private async Task CargarHistorial()
    {
        var usuario = await _database.ObtenerUsuarioAsync();
        if (usuario == null) return;

        var historial = await _database.ObtenerHistorialNutricionalAsync(usuario.ID_Usuario);
        HistorialCollectionView.ItemsSource = historial;

        var kcalHoy = await _database.ObtenerKcalHoyAsync(usuario.ID_Usuario);
        KcalHoyLabel.Text = $"{kcalHoy:F0} kcal ingeridas hoy";
    }

    private async void OnAddFoodClicked(object sender, EventArgs e)
    {
        try
        {
            double.TryParse(Kcal, NumberStyles.Any, CultureInfo.InvariantCulture, out double kcal);
            double.TryParse(Proteinas, NumberStyles.Any, CultureInfo.InvariantCulture, out double proteinas);
            double.TryParse(Carbos, NumberStyles.Any, CultureInfo.InvariantCulture, out double carbos);
            double.TryParse(Grasas, NumberStyles.Any, CultureInfo.InvariantCulture, out double grasas);
            double.TryParse(Fibra, NumberStyles.Any, CultureInfo.InvariantCulture, out double fibra);

            var usuario = await _database.ObtenerUsuarioAsync();

            // Creamos primero el RegistroDiario
            var registro = new RegistroDiario { ID_Usuario = usuario.ID_Usuario };
            registro.SetFecha(DateTime.Now);
            int idRegistro = await _database.InsertarRegistroAsync(registro);

            // Luego el Nutricional vinculado
            var nutricional = new Nutricional
            {
                ID_Registro = idRegistro,
                Nombre = Nombre,
                Marca = Marca,
                Kcal = kcal,
                Proteinas = proteinas,
                Carbos = carbos,
                Grasas = grasas,
                Fibra = fibra,
                Imagen = Imagen
            };
            await _database.InsertarNutricionalAsync(nutricional);

            ResultadoCard.IsVisible = false;
            Nombre = "";

            await CargarHistorial();
            await DisplayAlert("✅ Guardado", $"'{nutricional.Nombre}' añadido a tu registro.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void OnCloseCardClicked(object sender, EventArgs e)
    {
        ResultadoCard.IsVisible = false;
        Nombre = "";
    }

    private async void OnAbrirScannerClicked(object sender, EventArgs e)
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.Camera>();

        if (status == PermissionStatus.Granted)
            await Shell.Current.GoToAsync("ScannerPage");
        else
            await DisplayAlert("Error", "Necesitamos acceso a la cámara para escanear", "OK");
    }

    private async void OnInicioTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//MainPage");

    private async void OnRetosTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//RetosPage");

    private async void OnPerfilTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//PerfilPage");
}