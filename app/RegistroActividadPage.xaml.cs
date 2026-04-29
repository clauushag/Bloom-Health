using app.Data;
using app.Models;
using System.Globalization;

namespace app;

public partial class RegistroActividadPage : ContentPage
{
    private Frame _frameSeleccionadoAnteriormente;
    private SaludDatabase _database;
    private Usuario _usuarioActual;

    // Objetos que se rellenan con los datos del formulario
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

        // Obtenemos el usuario real de la BD — ya no hay ID hardcodeado
        _usuarioActual = await _database.ObtenerUsuarioAsync();
        if (_usuarioActual == null)
        {
            await Shell.Current.GoToAsync("//crearPerfil");
            return;
        }

        // Cargamos las estadísticas de hoy para mostrarlas al entrar
        await ActualizarResumenAsync();
    }

    // ── Selección de actividad ───────────────────────────────────────────────

    private void OnActividadTapped(object sender, TappedEventArgs e)
    {
        // Deseleccionamos la tarjeta anterior
        if (_frameSeleccionadoAnteriormente != null)
        {
            _frameSeleccionadoAnteriormente.BackgroundColor = Colors.White;
            _frameSeleccionadoAnteriormente.BorderColor = Colors.Transparent;
        }

        var frameActual = (Frame)sender;
        frameActual.BackgroundColor = Color.FromArgb("#F0F5F1");
        frameActual.BorderColor = Color.FromArgb("#8EB497");
        _frameSeleccionadoAnteriormente = frameActual;

        // Guardamos el tipo de actividad en el modelo
        if (e.Parameter != null)
        {
            fisico.Tipo_Actividad = e.Parameter.ToString();
            LabelActividadSeleccionada.Text = "Registrar " + fisico.Tipo_Actividad;
        }

        ContenedorFormulario.IsVisible = true;

        // Mostramos u ocultamos el campo distancia según la actividad
        // (Yoga y Gimnasio no necesitan distancia — viene de RequiereDistancia en Fisico.cs)
        ContenedorDistancia.IsVisible = fisico.RequiereDistancia;
    }

    // ── Guardar ──────────────────────────────────────────────────────────────

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        // Leemos los campos del formulario y los metemos en el modelo
        if (!LeerFormulario())
            return; // LeerFormulario ya muestra el alert de error si falla

        // Validación del modelo (EsValido() en Fisico.cs)
        if (!fisico.EsValido())
        {
            await DisplayAlert("Campos incompletos", "Rellena todos los campos obligatorios.", "OK");
            return;
        }

        try
        {
            // Calculamos el XP según la duración antes de guardar
            fisico.XP = fisico.CalcularXP();

            // Creamos el RegistroDiario con el usuario real
            registro.SetFecha(DateTime.Now);
            registro.ID_Usuario = _usuarioActual.ID_Usuario;

            // Insertamos primero el registro padre y luego el hijo Fisico
            int idRegistro = await _database.InsertarRegistroAsync(registro);
            fisico.ID_Registro = idRegistro;
            await _database.InsertarFisicoAsync(fisico);

            // Sumamos XP al avatar
            await _database.SumarXPAsync(_usuarioActual.ID_Usuario, fisico.XP);

            var retosCompletados = await _database.ComprobarRetosFisicoAsync(
    _usuarioActual.ID_Usuario, fisico);

            // Si hay retos completados los mostramos antes del resumen normal
            foreach (var reto in retosCompletados)
            {
                await DisplayAlert(
                    "🏆 ¡Reto completado!",
                    $"Has completado el reto '{reto.Nombre}'\n" +
                    $"🎉 +{reto.Puntos_Recompensa} XP de recompensa",
                    "¡Genial!");
            }

            // Actualizamos el resumen de hoy en pantalla
            await ActualizarResumenAsync();

            // Mostramos feedback al usuario con los datos de la sesión
            await MostrarResumenSesionAsync(fisico);

            // Reseteamos el formulario para permitir añadir otra actividad
            ResetearFormulario();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo guardar: {ex.Message}", "OK");
        }
    }

    // ── Métodos auxiliares ───────────────────────────────────────────────────

    /// <summary>
    /// Lee los Entry del formulario XAML y los mete en el objeto fisico.
    /// Devuelve false y muestra un alert si algún campo tiene formato incorrecto.
    /// </summary>
    private bool LeerFormulario()
    {
        // Tiempo — obligatorio siempre
        if (!int.TryParse(EntryTiempo.Text, out int tiempo) || tiempo <= 0)
        {
            DisplayAlert("Campo inválido", "Introduce una duración válida en minutos.", "OK");
            return false;
        }
        fisico.Tiempo_Ejercicio = tiempo;

        // Kcal — obligatorio siempre
        if (!double.TryParse(EntryKcal.Text, NumberStyles.Any,
                CultureInfo.InvariantCulture, out double kcal) || kcal <= 0)
        {
            DisplayAlert("Campo inválido", "Introduce unas calorías quemadas válidas.", "OK");
            return false;
        }
        fisico.Kcal_Quemadas = kcal;

        // Distancia — solo obligatorio si la actividad lo requiere
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

    /// <summary>
    /// Consulta la BD y actualiza las etiquetas de resumen de hoy:
    /// kcal quemadas, minutos totales, XP ganado y racha de días.
    /// </summary>
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

        // ← nuevo: cargamos los retos en progreso
        var retosEnProgreso = await _database.ObtenerRetosEnProgresoAsync(_usuarioActual.ID_Usuario);
        RetosEnProgresoCollectionView.ItemsSource = retosEnProgreso;
        ContenedorRetosEnProgreso.IsVisible = retosEnProgreso.Count > 0;
        LabelSinRetos.IsVisible = retosEnProgreso.Count == 0;
    }

    /// <summary>
    /// Muestra un alert con el resumen de la sesión recién guardada.
    /// </summary>
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

    /// <summary>
    /// Limpia el formulario y oculta el contenedor para poder registrar otra actividad.
    /// </summary>
    private void ResetearFormulario()
    {
        fisico = new Fisico();
        BindingContext = fisico;

        EntryTiempo.Text = "";
        EntryKcal.Text = "";
        EntryDistancia.Text = "";

        if (_frameSeleccionadoAnteriormente != null)
        {
            _frameSeleccionadoAnteriormente.BackgroundColor = Colors.White;
            _frameSeleccionadoAnteriormente.BorderColor = Colors.Transparent;
            _frameSeleccionadoAnteriormente = null;
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