namespace app;
using System.Globalization;
using app.Data;
using app.Models;

public partial class EstadoAnimicoPage : ContentPage
{
    private Usuario _usuarioActual = null!;
    private readonly SaludDatabase _database;
    private string _estadoSeleccionado = string.Empty;
    private Dictionary<string, Border> _botonesEstado = null!;

    private static readonly Dictionary<string, string> _emojis = new()
    {
        { "Muy mal", "😞" },
        { "Mal",     "😕" },
        { "Regular", "😐" },
        { "Bien",    "🙂" },
        { "Genial",  "😄" }
    };

    private static readonly Dictionary<string, int> _valorEstado = new()
    {
        { "Muy mal", 0 },
        { "Mal",     1 },
        { "Regular", 2 },
        { "Bien",    3 },
        { "Genial",  4 }
    };

    // Color de fondo normal según el tema actual
    private Color ColorNormal => Application.Current.RequestedTheme == AppTheme.Dark
        ? Color.FromArgb("#1E1E1E")
        : Colors.White;

    // Borde normal según el tema actual
    private Color BordeNormal => Application.Current.RequestedTheme == AppTheme.Dark
        ? Color.FromArgb("#3C3C3C")
        : Color.FromArgb("#EEF2EE");

    public EstadoAnimicoPage(SaludDatabase database)
    {
        InitializeComponent();
        _database = database;
        _botonesEstado = new Dictionary<string, Border>
        {
            { Mental.EstadosAnimo[0], BtnMuyMal },
            { Mental.EstadosAnimo[1], BtnMal    },
            { Mental.EstadosAnimo[2], BtnRegular},
            { Mental.EstadosAnimo[3], BtnBien   },
            { Mental.EstadosAnimo[4], BtnGenial }
        };
        ActualizarLabelHoras(SliderSueno.Value);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _usuarioActual = await _database.ObtenerUsuarioAsync();

        Mental registroHoy = await _database.ObtenerMentalHoyAsync(_usuarioActual.ID_Usuario);

        if (registroHoy != null)
        {
            MostrarResumenHoy(registroHoy);
            var semana = await _database.ObtenerMentalSemanaAsync(_usuarioActual.ID_Usuario);
            MostrarGraficaSemana(semana);
        }
        else
        {
            MostrarFormulario();
        }
    }

    private void MostrarFormulario()
    {
        FormularioLayout.IsVisible = true;
        ResumenLayout.IsVisible    = false;
    }

    private void MostrarResumenHoy(Mental registro)
    {
        FormularioLayout.IsVisible = false;
        ResumenLayout.IsVisible    = true;

        LblEmojiHoy.Text   = _emojis.GetValueOrDefault(registro.Estado_Animo, "😐");
        LblEstadoHoy.Text  = $"Hoy te sientes: {registro.Estado_Animo}";
        LblSuenoHoy.Text   = registro.Horas_Sueno % 1 == 0
                             ? $"{(int)registro.Horas_Sueno} horas"
                             : $"{registro.Horas_Sueno:0.0} horas";

        bool hayNotas = !string.IsNullOrWhiteSpace(registro.Notas_diario);
        GridNotasHoy.IsVisible = hayNotas;
        LblNotasHoy.Text       = registro.Notas_diario ?? string.Empty;
    }

    private void MostrarGraficaSemana(List<SaludDatabase.MentalConFecha> semana)
    {
        var hoy     = DateTime.Today;
        var colores = new[] { "#FF6B6B", "#FFA94D", "#FFD43B", "#69DB7C", "#4CAF72" };
        var dias    = new[] { "L", "M", "X", "J", "V", "S", "D" };

        GraficaEstados.Children.Clear();
        GraficaDias.Children.Clear();
        GraficaSueno.Children.Clear();

        for (int col = 0; col < 7; col++)
        {
            var fecha    = hoy.AddDays(col - 6);
            var fechaStr = fecha.ToString("yyyy-MM-dd");
            var reg      = semana.FirstOrDefault(r => r.Fecha == fechaStr);

            int    nivel  = reg != null ? _valorEstado.GetValueOrDefault(reg.Estado_Animo, 0) : -1;
            double altura = nivel >= 0 ? (nivel + 1) * 18.0 : 4;
            string color  = nivel >= 0 ? colores[nivel] : "#EEF2EE";

            var barraEstado = new Border
            {
                BackgroundColor   = Color.FromArgb(color),
                StrokeShape       = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(6, 6, 0, 0) },
                Stroke            = Colors.Transparent,
                HeightRequest     = altura,
                VerticalOptions   = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.Fill
            };
            Grid.SetColumn(barraEstado, col);
            GraficaEstados.Children.Add(barraEstado);

            if (nivel >= 0)
            {
                var emoji = new Label
                {
                    Text              = _emojis.GetValueOrDefault(reg!.Estado_Animo, ""),
                    FontSize          = 11,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions   = LayoutOptions.End,
                    Margin            = new Thickness(0, 0, 0, altura + 2)
                };
                Grid.SetColumn(emoji, col);
                GraficaEstados.Children.Add(emoji);
            }

            bool esHoy  = fecha == hoy;
            var lblDia  = new Label
            {
                Text              = dias[(int)fecha.DayOfWeek == 0 ? 6 : (int)fecha.DayOfWeek - 1],
                FontSize          = 11,
                FontAttributes    = esHoy ? FontAttributes.Bold : FontAttributes.None,
                TextColor         = esHoy ? Color.FromArgb("#2D3A2F") : Color.FromArgb("#7A8B7C"),
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(lblDia, col);
            GraficaDias.Children.Add(lblDia);

            double horas       = reg?.Horas_Sueno ?? 0;
            double alturaSueno = horas > 0 ? Math.Clamp(horas / 12.0 * 70, 4, 70) : 4;
            string colorSueno  = horas >= 7 ? "#4CAF72" : horas >= 5 ? "#FFD43B" : "#FF6B6B";
            if (horas == 0) colorSueno = "#EEF2EE";

            var barraSueno = new Border
            {
                BackgroundColor   = Color.FromArgb(colorSueno),
                StrokeShape       = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(6, 6, 0, 0) },
                Stroke            = Colors.Transparent,
                HeightRequest     = alturaSueno,
                VerticalOptions   = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.Fill
            };
            Grid.SetColumn(barraSueno, col);
            GraficaSueno.Children.Add(barraSueno);
        }
    }

    private async void OnGuardarTapped(object sender, TappedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_estadoSeleccionado))
        {
            await DisplayAlert("Atención", "Por favor selecciona tu estado de ánimo.", "OK");
            return;
        }

        RegistroDiario registroDiario = new RegistroDiario();
        registroDiario.SetFecha(DateTime.Now);
        registroDiario.ID_Usuario = _usuarioActual.ID_Usuario;
        int idRegistro = await _database.InsertarRegistroAsync(registroDiario);

        Mental registro = new Mental
        {
            ID_Registro   = idRegistro,
            Estado_Animo  = _estadoSeleccionado,
            Horas_Sueno   = SliderSueno.Value,
            Notas_diario  = string.IsNullOrWhiteSpace(EditorNotas.Text)
                            ? null
                            : EditorNotas.Text.Trim()
        };

        if (!registro.EstaCompleto())
        {
            await DisplayAlert("Debug",
                $"ID_Registro: {registro.ID_Registro}\nEstado_Animo: {registro.Estado_Animo}\n" +
                $"Horas_Sueno: {registro.Horas_Sueno}\nNotas_Diario: {registro.Notas_diario}", "OK");
            return;
        }

        try
        {
            await _database.GuardarRegistroAsync(registro);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error al guardar", ex.Message, "OK");
            return;
        }

        LblConfirmacion.IsVisible = true;
        await Task.Delay(2500);
        LblConfirmacion.IsVisible = false;

        MostrarResumenHoy(registro);
        var semana = await _database.ObtenerMentalSemanaAsync(_usuarioActual.ID_Usuario);
        MostrarGraficaSemana(semana);
    }

    // ─── Selección de estado de ánimo ────────────────────────────────
    private void OnEstadoTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not string estado) return;

        _estadoSeleccionado = estado;

        foreach (var (key, btn) in _botonesEstado)
        {
            bool seleccionado = key == estado;
            btn.BackgroundColor = seleccionado
                ? Color.FromArgb("#2A3A2A")      // verde oscuro al seleccionar
                : ColorNormal;                    // blanco o #1E1E1E según tema
            btn.Stroke = seleccionado
                ? Color.FromArgb("#8EB497")       // borde verde al seleccionar
                : BordeNormal;                    // borde normal según tema
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
        _estadoSeleccionado = string.Empty;
        foreach (var btn in _botonesEstado.Values)
        {
            btn.BackgroundColor = ColorNormal;   // ← usa el tema actual
            btn.Stroke          = BordeNormal;   // ← usa el tema actual
            btn.StrokeThickness = 1.5;
        }

        SliderSueno.Value = 7;
        EditorNotas.Text  = string.Empty;
    }

    // ─── Navegación ──────────────────────────────────────────────────
    private async void OnVolverClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//MainPage");

    private async void OnInicioTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//MainPage");

    private async void OnRetosTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//RetosPage");

    private async void OnPerfilTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//PerfilPage");
}