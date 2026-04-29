using app.Data;
using app.Models;

namespace app;

public partial class RetosPage : ContentPage
{
    private SaludDatabase _database;
    private Usuario _usuarioActual;

    // Paginación
    private int _offset = 0;
    private const int PageSize = 20;
    private bool _cargando = false;
    private bool _hayMas = true;

    private List<SaludDatabase.RetoConProgreso> _retos = new();

    public RetosPage(SaludDatabase database)
    {
        InitializeComponent();
        _database = database;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _usuarioActual = await _database.ObtenerUsuarioAsync();
        if (_usuarioActual == null) return;

        // Recargamos desde 0 cada vez que entramos
        // (por si se completó un reto desde RegistroActividadPage)
        await RecargarAsync();
    }

    private async Task RecargarAsync()
    {
        _offset = 0;
        _hayMas = true;
        _retos.Clear();

        await CargarMasRetosAsync();
        await ActualizarResumenAsync();
    }

    /// <summary>
    /// Carga el siguiente bloque de 20 retos y los añade a la lista.
    /// El CollectionView con RemainingItemsThreshold llama a esto automáticamente
    /// cuando el usuario llega al final.
    /// </summary>
    private async Task CargarMasRetosAsync()
    {
        if (_cargando || !_hayMas) return;
        _cargando = true;

        var nuevos = await _database.ObtenerRetosPageAsync(
            _usuarioActual.ID_Usuario, _offset, PageSize);

        if (nuevos.Count < PageSize)
            _hayMas = false; // ya no hay más páginas

        _retos.AddRange(nuevos);
        _offset += nuevos.Count;

        // Asignamos una nueva lista para forzar el refresco del CollectionView
        RetosCollectionView.ItemsSource = null;
        RetosCollectionView.ItemsSource = _retos;

        _cargando = false;
    }

    private async Task ActualizarResumenAsync()
    {
        var (completados, enProgreso, pendientes) =
            await _database.ObtenerResumenRetosAsync(_usuarioActual.ID_Usuario);

        LabelCompletados.Text = completados.ToString();
        LabelEnProgreso.Text  = enProgreso.ToString();
        LabelPendientes.Text  = pendientes.ToString();
    }

    // Se llama automáticamente cuando el usuario llega al final del scroll
    private async void OnRetosThresholdReached(object sender, EventArgs e)
    {
        await CargarMasRetosAsync();
    }

    // El usuario pulsa "Aceptar reto"
    private async void OnAceptarRetoClicked(object sender, EventArgs e)
    {
        var btn  = (Button)sender;
        var reto = (SaludDatabase.RetoConProgreso)btn.BindingContext;

        if (reto.EstaCompletado || reto.EstaEnProgreso) return;

        await _database.AceptarRetoAsync(_usuarioActual.ID_Usuario, reto.ID_Reto);
        await RecargarAsync();

        await DisplayAlert("✅ Reto aceptado",
            $"Has aceptado el reto '{reto.Nombre}'.\n" +
            $"Complétalo registrando tus actividades.", "¡Vamos!");
    }

    // ── Navegación ───────────────────────────────────────────────────────────
    private async void OnInicioTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//MainPage");

    private async void OnRetosTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//RetosPage");

    private async void OnPerfilTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//PerfilPage");
}