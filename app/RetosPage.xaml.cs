using app.Data;
using app.Models;
using System.Collections.ObjectModel;

namespace app;

public partial class RetosPage : ContentPage
{
    private SaludDatabase _database;
    private Usuario _usuarioActual;

    private int _offset = 0;
    private const int PageSize = 20;
    private bool _cargando = false;
    private bool _hayMas = true;

    // ObservableCollection notifica cambios item a item — no redibujar todo
    private ObservableCollection<SaludDatabase.RetoConProgreso> _retos = new();

    public RetosPage(SaludDatabase database)
    {
        InitializeComponent();
        _database = database;

        // Asignamos UNA sola vez en el constructor — nunca más tocamos ItemsSource
        RetosCollectionView.ItemsSource = _retos;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _usuarioActual = await _database.ObtenerUsuarioAsync();
        if (_usuarioActual == null) return;

        IniciarRecargaEnBackground();
    }

    private void IniciarRecargaEnBackground()
    {
        _offset = 0;
        _hayMas = true;

        // Limpiamos en el hilo de UI antes de lanzar las tareas
        MainThread.BeginInvokeOnMainThread(() => _retos.Clear());

        _ = EjecutarConManejoDeErrorAsync(CargarMasRetosAsync());
        _ = EjecutarConManejoDeErrorAsync(ActualizarResumenAsync());
    }

    private static async Task EjecutarConManejoDeErrorAsync(Task tarea)
    {
        try { await tarea; }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RetosPage] Error: {ex.Message}");
        }
    }

    private async Task CargarMasRetosAsync()
    {
        if (_cargando || !_hayMas||_usuarioActual == null) return;
        _cargando = true;

        var nuevos = await _database.ObtenerRetosPageAsync(
            _usuarioActual.ID_Usuario, _offset, PageSize);

        if (nuevos.Count < PageSize)
            _hayMas = false;

        _offset += nuevos.Count;

        // Añadimos item a item — ObservableCollection renderiza cada uno
        // de forma incremental sin invalidar los ya visibles
        MainThread.BeginInvokeOnMainThread(() =>
        {
            foreach (var reto in nuevos)
                _retos.Add(reto);
        });

        _cargando = false;
    }

    private async Task ActualizarResumenAsync()
    {
        var (completados, enProgreso, pendientes) =
            await _database.ObtenerResumenRetosAsync(_usuarioActual.ID_Usuario);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            LabelCompletados.Text = completados.ToString();
            LabelEnProgreso.Text  = enProgreso.ToString();
            LabelPendientes.Text  = pendientes.ToString();
        });
    }

    private async void OnRetosThresholdReached(object sender, EventArgs e)
    {
        await CargarMasRetosAsync();
    }

    private async void OnAceptarRetoClicked(object sender, EventArgs e)
    {
        var btn  = (Button)sender;
        var reto = (SaludDatabase.RetoConProgreso)btn.BindingContext;

        if (reto.EstaCompletado || reto.EstaEnProgreso) return;

        await _database.AceptarRetoAsync(_usuarioActual.ID_Usuario, reto.ID_Reto);
        IniciarRecargaEnBackground();

        await DisplayAlert("✅ Reto aceptado",
            $"Has aceptado el reto '{reto.Nombre}'.\n" +
            $"Complétalo registrando tus actividades.", "¡Vamos!");
    }

    private async void OnInicioTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//MainPage");

    private async void OnRetosTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//RetosPage");

    private async void OnPerfilTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//PerfilPage");
}