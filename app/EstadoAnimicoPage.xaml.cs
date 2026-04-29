namespace app; // Recuerda cambiar "app" si tu proyecto se llama diferente
using System.Globalization;
using app.Data;
using app.Models;

public partial class EstadoAnimicoPage : ContentPage
{
    private Usuario _usuarioActual = null!;
    private readonly SaludDatabase _database;
    private string _estadoSeleccionado = string.Empty;
    private Dictionary<string, Border> _botonesEstado = null!;
    public EstadoAnimicoPage(SaludDatabase database)
    {
        InitializeComponent();
        _database = database;
        _botonesEstado = new Dictionary<string, Border>
        {
            { "Muy mal", BtnMuyMal },
            { "Mal",     BtnMal    },
            { "Regular", BtnRegular},
            { "Bien",    BtnBien   },
            { "Genial",  BtnGenial }
        };
                // Valor inicial del slider
        ActualizarLabelHoras(SliderSueno.Value);
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _usuarioActual = await _database.ObtenerUsuarioAsync();
    }
    private async void OnGuardarTapped(object sender, TappedEventArgs e)
    {
        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(_estadoSeleccionado))
        {
            await DisplayAlert("Atención", "Por favor selecciona tu estado de ánimo.", "OK");
            return;
        }

        // 1. Crea y guarda el RegistroDiario
        RegistroDiario registroDiario = new RegistroDiario
        {
            Fecha = DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss"),
            ID_Usuario = _usuarioActual.ID_Usuario  // el que tengas en sesión
        };
        int idRegistro = await _database.InsertarRegistroAsync(registroDiario);
        // Construye el objeto del modelo
        Mental registro = new Mental
        {
            ID_Registro = idRegistro,
            Estado_Animo  = _estadoSeleccionado,
            Horas_Sueno   = SliderSueno.Value,
            Notas_diario  = string.IsNullOrWhiteSpace(EditorNotas.Text)
                            ? null
                            : EditorNotas.Text.Trim()
        };
        if (!registro.EstaCompleto())
        {
            await DisplayAlert("Debug",$"ID_Registro: {registro.ID_Registro}\nEstado_Animo: {registro.Estado_Animo}\nHoras_Sueno: {registro.Horas_Sueno}\nNotas_Diario: {registro.Notas_diario}","OK");
            return;
        }

        try
        {
             // Persiste en la base de datos
        await _database.GuardarRegistroAsync(registro);

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error al guardar", ex.Message, "OK");
            return;
        }       
        // Feedback visual
        LblConfirmacion.IsVisible = true;
        await Task.Delay(2500);
        LblConfirmacion.IsVisible = false;

        // Reinicia el formulario
        ResetearFormulario();
    }
    // ─── Selección de estado de ánimo ────────────────────────────────
    private void OnEstadoTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not string estado) return;

        _estadoSeleccionado = estado;

        foreach (var (key, btn) in _botonesEstado)
        {
            bool seleccionado = key == estado;
            btn.BackgroundColor = seleccionado ? Color.FromArgb("#D6EDDA") : Colors.White;
            btn.Stroke          = seleccionado ? Color.FromArgb("#4CAF72") : Color.FromArgb("#EEF2EE");
            btn.StrokeThickness = seleccionado ? 2.5 : 1.5;
        }
    }

    // ─── Slider horas de sueño ───────────────────────────────────────
    private void OnSliderSuenoChanged(object sender, ValueChangedEventArgs e)
    {
        double redondeado = Math.Round(e.NewValue * 2) / 2.0;
        SliderSueno.Value = redondeado;
        ActualizarLabelHoras(redondeado);
    }

    private void OnMenosHoras(object sender, TappedEventArgs e)
    {
        double nuevo = Math.Max(SliderSueno.Minimum, SliderSueno.Value - 0.5);
        SliderSueno.Value = nuevo;
        ActualizarLabelHoras(nuevo);
    }

    private void OnMasHoras(object sender, TappedEventArgs e)
{
    double nuevo = Math.Min(SliderSueno.Maximum, SliderSueno.Value + 0.5);
    SliderSueno.Value = nuevo;
    ActualizarLabelHoras(nuevo);
}

    private void ActualizarLabelHoras(double horas)
    {
        LblHoras.Text = horas % 1 == 0 ? $"{(int)horas} horas" : $"{horas:0.0} horas";
    }

    private void ResetearFormulario()
    {
        // Quita la selección de estado
        _estadoSeleccionado = string.Empty;
        foreach (var btn in _botonesEstado.Values)
        {
            btn.BackgroundColor = Colors.White;
            btn.Stroke          = Color.FromArgb("#EEF2EE");
            btn.StrokeThickness = 1.5;
        }

        // Resetea horas y notas
        SliderSueno.Value  = 7;
        EditorNotas.Text   = string.Empty;
    }


    // Lógica para el botón de la flecha de atrás
    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
 // ─── Navegación (barra inferior) ─────────────────────────────────
    private async void OnInicioTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//MainPage");

    private async void OnRetosTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//RetosPage");

    private async void OnPerfilTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//PerfilPage");

}