namespace app; //Caso de Uso de Carlota
using app.Data;
using app.Models;
using System.Globalization;

public partial class RegistroActividadPage : ContentPage
{
    private Frame  _cardSeleccionada;
    private SaludDatabase _database;
    private Usuario _usuarioActual;

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

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _database.InicializarAsync();

        _usuarioActual = await _database.ObtenerUsuarioAsync();
        if (_usuarioActual == null)
        {
            await Shell.Current.GoToAsync("//crearPerfil");
            return;
        }

        await ActualizarResumenAsync();
    }

    // ── Selección de actividad ───────────────────────────────────────────────

    private void OnActividadTapped(object sender, EventArgs e)
    {
        bool esDark = Application.Current.RequestedTheme == AppTheme.Dark
            || Application.Current.UserAppTheme == AppTheme.Dark;

        if (_cardSeleccionada != null)
        {
            _cardSeleccionada.BackgroundColor = esDark
                ? Color.FromArgb("#1E1E1E")
                : Colors.White;
            _cardSeleccionada.BorderColor = esDark
                ? Color.FromArgb("#3C3C3C")
                : Colors.Transparent;
        }

        var boton = (Button)sender;
        var grid = (Grid)boton.Parent;
        _cardSeleccionada = (Frame)grid.Children[0];

        _cardSeleccionada.BackgroundColor = esDark
            ? Color.FromArgb("#2A3A2A")
            : Color.FromArgb("#F0F5F1");
        _cardSeleccionada.BorderColor = Color.FromArgb("#8EB497");

        var actividad = boton.CommandParameter?.ToString();
        if (!string.IsNullOrEmpty(actividad))
        {
            fisico.Tipo_Actividad = actividad;
            LabelActividadSeleccionada.Text = "Registrar " + actividad;
        }

        ContenedorFormulario.IsVisible = true;
        ContenedorDistancia.IsVisible = fisico.RequiereDistancia;
    }
    // ── Guardar ──────────────────────────────────────────────────────────────

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        if (!LeerFormulario())
            return;

        if (!fisico.EsValido())
        {
            await DisplayAlert("Campos incompletos", "Rellena todos los campos obligatorios.", "OK");
            return;
        }

        try
        {
            fisico.XP = fisico.CalcularXP();

            registro.SetFecha(DateTime.Now);
            registro.ID_Usuario = _usuarioActual.ID_Usuario;

            int idRegistro = await _database.InsertarRegistroAsync(registro);
            fisico.ID_Registro = idRegistro;
            await _database.InsertarFisicoAsync(fisico);

            await _database.SumarXPAsync(_usuarioActual.ID_Usuario, fisico.XP);

            var retosCompletados = await _database.ComprobarRetosFisicoAsync(
                _usuarioActual.ID_Usuario, fisico);

            foreach (var reto in retosCompletados)
            {
                await DisplayAlert(
                    "🏆¡Reto completado!",
                    $"Has completado el reto '{reto.Nombre}'\n" +
                    $"🎉 +{reto.Puntos_Recompensa} XP de recompensa",
                    "¡Genial!");
            }

            await ActualizarResumenAsync();
            await MostrarResumenSesionAsync(fisico);
            ResetearFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo guardar: {ex.Message}", "OK");
        }
    }

    // ── Métodos auxiliares ───────────────────────────────────────────────────

    private bool LeerFormulario()
    {
        if (!int.TryParse(EntryTiempo.Text, out int tiempo) || tiempo <= 0)
        {
            DisplayAlert("Campo inválido", "Introduce una duración válida en minutos.", "OK");
            return false;
        }
        fisico.Tiempo_Ejercicio = tiempo;

        if (!double.TryParse(EntryKcal.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out double kcal) || kcal <= 0)
        {
            DisplayAlert("Campo inválido", "Introduce unas calorías quemadas válidas.", "OK");
            return false;
        }
        fisico.Kcal_Quemadas = kcal;

        if (fisico.RequiereDistancia)
        {
            if (!double.TryParse(EntryDistancia.Text, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out double distancia) || distancia <= 0)
            {
                DisplayAlert("Campo inválido", "Introduce una distancia válida en km.", "OK");
                return false;
            }
            fisico.Distancia = distancia;
        }

        return true;
    }

    private async Task ActualizarResumenAsync()
    {
        var resumenTask = _database.ObtenerResumenActividadHoyAsync(_usuarioActual.ID_Usuario);
        var rachaTask = _database.ObtenerRachaAsync(_usuarioActual.ID_Usuario);
        await Task.WhenAll(resumenTask, rachaTask);

        var resumen = resumenTask.Result;
        int racha = rachaTask.Result;

        LabelKcalHoy.Text = $"{resumen.KcalQuemadas:F0} kcal quemadas hoy";
        LabelMinutosHoy.Text = $"{resumen.MinutosTotales} min de actividad hoy";
        LabelXpHoy.Text = $"+{resumen.XpGanado} XP ganados hoy";
        LabelRacha.Text = racha == 0
            ? "Sin racha aún — ¡empieza hoy!"
            : $"🔥 {racha} día{(racha > 1 ? "s" : "")} de racha";

        var historial = await _database.ObtenerHistorialFisicoAsync(_usuarioActual.ID_Usuario);
        HistorialCollectionView.ItemsSource = historial;

        var retosEnProgreso = await _database.ObtenerRetosEnProgresoAsync(_usuarioActual.ID_Usuario);
        RetosEnProgresoCollectionView.ItemsSource = retosEnProgreso;
        ContenedorRetosEnProgreso.IsVisible = retosEnProgreso.Count > 0;
        LabelSinRetos.IsVisible = retosEnProgreso.Count == 0;
    }

    private async Task MostrarResumenSesionAsync(Fisico f)
    {
        string distanciaLinea = f.RequiereDistancia
            ? $"\n📍 Distancia: {f.Distancia:F1} km"
            : "";

        await DisplayAlert(
            "✅ ¡Actividad registrada!",
            $"🏃 {f.Tipo_Actividad}" +
            $"\n⏱ Duración: {f.Tiempo_Ejercicio} min" +
            $"\n🔥 Kcal quemadas: {f.Kcal_Quemadas:F0}" +
            distanciaLinea +
            $"\n⭐ XP ganado: +{f.XP}",
            "¡Genial!");
    }

    private void ResetearFormulario()
    {
        fisico = new Fisico();
        BindingContext = fisico;

        EntryTiempo.Text = "";
        EntryKcal.Text = "";
        EntryDistancia.Text = "";

        if (_cardSeleccionada != null)
        {
            bool esDark = Application.Current.UserAppTheme == AppTheme.Dark;
            _cardSeleccionada.BackgroundColor = esDark
                ? Color.FromArgb("#1E1E1E")
                : Colors.White;
            _cardSeleccionada.BorderColor = esDark
                ? Color.FromArgb("#3C3C3C")
                : Colors.Transparent;
            _cardSeleccionada = null;
        }

        ContenedorFormulario.IsVisible = false;
    }

    // ── Navegación ───────────────────────────────────────────────────────────

    private async void OnVolverClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//MainPage");

    private async void OnInicioTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//MainPage");

    private async void OnRetosTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//RetosPage");

    private async void OnPerfilTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//PerfilPage");
}