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

    // EDIT: CargarHistorial y OnAppearing - paralelizar consultas BD y mover trabajo pesado fuera del hilo principal

    private async Task CargarHistorial()
    {
        Usuario usuario = await Task.Run(() => _database.ObtenerUsuarioAsync());
        if (usuario == null) return;

        // Las dos consultas son independientes entre sí → se lanzan en paralelo
        var (historial, kcalHoy) = await Task.Run(async () =>
        {
            Task<List<Nutricional>> historialTask = _database.ObtenerHistorialNutricionalAsync(usuario.ID_Usuario);
            Task<double> kcalTask = _database.ObtenerKcalHoyAsync(usuario.ID_Usuario);

            await Task.WhenAll(historialTask, kcalTask);

            return (historialTask.Result, kcalTask.Result);
        });

        // Solo tocamos la UI una vez, en el hilo principal
        HistorialCollectionView.ItemsSource = historial;
        KcalHoyLabel.Text = $"{kcalHoy:F0} kcal ingeridas hoy";
    }

    private async void OnVolverClicked(object sender, EventArgs e) =>
    await Shell.Current.GoToAsync("//MainPage");
    private async void OnAddFoodClicked(object sender, EventArgs e)
    {
        try
        {
            double.TryParse(Kcal, NumberStyles.Any, CultureInfo.InvariantCulture, out double kcal);
            double.TryParse(Proteinas, NumberStyles.Any, CultureInfo.InvariantCulture, out double proteinas);
            double.TryParse(Carbos, NumberStyles.Any, CultureInfo.InvariantCulture, out double carbos);
            double.TryParse(Grasas, NumberStyles.Any, CultureInfo.InvariantCulture, out double grasas);
            double.TryParse(Fibra, NumberStyles.Any, CultureInfo.InvariantCulture, out double fibra);

            Usuario usuario = await _database.ObtenerUsuarioAsync();

            // Creamos primero el RegistroDiario
            RegistroDiario registro = new RegistroDiario { ID_Usuario = usuario.ID_Usuario };
            registro.SetFecha(DateTime.Now);
            int idRegistro = await _database.InsertarRegistroAsync(registro);

            // Luego el Nutricional vinculado
            Nutricional nutricional = new Nutricional
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
            await _database.SumarXPAsync(usuario.ID_Usuario, 10); // +10 XP por registrar comida
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

    private async void OnGuardarManualClicked(object sender, EventArgs e)
    {
        try
        {
            string nombre = ManualNombreEntry.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(nombre))
            {
                await DisplayAlert("Campo requerido", "El nombre del alimento es obligatorio.", "OK");
                return;
            }

            double.TryParse(ManualKcalEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double kcal);
            double.TryParse(ManualProteinasEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double proteinas);
            double.TryParse(ManualCarbosEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double carbos);
            double.TryParse(ManualGrasasEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double grasas);
            double.TryParse(ManualFibraEntry.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double fibra);

            if (kcal <= 0)
            {
                await DisplayAlert("Campo requerido", "Introduce un valor válido para las calorías.", "OK");
                return;
            }

            Usuario usuario = await _database.ObtenerUsuarioAsync();

            RegistroDiario registro = new RegistroDiario { ID_Usuario = usuario.ID_Usuario };
            registro.SetFecha(DateTime.Now);
            int idRegistro = await _database.InsertarRegistroAsync(registro);

            Nutricional nutricional = new Nutricional
            {
                ID_Registro = idRegistro,
                Nombre = nombre,
                Marca = ManualMarcaEntry.Text?.Trim() ?? "",
                Kcal = kcal,
                Proteinas = proteinas,
                Carbos = carbos,
                Grasas = grasas,
                Fibra = fibra,
                Imagen = ""
            };
            await _database.InsertarNutricionalAsync(nutricional);
            await _database.SumarXPAsync(usuario.ID_Usuario, 10); // +10 XP por registrar comida
            // Limpiar formulario
            ManualNombreEntry.Text = "";
            ManualMarcaEntry.Text = "";
            ManualKcalEntry.Text = "";
            ManualProteinasEntry.Text = "";
            ManualCarbosEntry.Text = "";
            ManualGrasasEntry.Text = "";
            ManualFibraEntry.Text = "";

            await CargarHistorial();
            await DisplayAlert("✅ Guardado", $"'{nombre}' añadido a tu registro.", "OK");
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
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Camera>();
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