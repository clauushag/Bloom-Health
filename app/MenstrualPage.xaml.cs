using app.Data;
using app.Models;

namespace app;

public partial class MenstrualPage : ContentPage
{
    // ── DEPENDENCIAS ────────────────────────────────────────────────
    private SaludDatabase _database;
    private Usuario _usuarioActual;

    // ── ESTADO DE LA UI ─────────────────────────────────────────────
    private Frame _faseSeleccionada = null;
    private Frame _sentimientoSeleccionado = null;
    private HashSet<string> _sintomasSeleccionados = new();

    private string _faseSeleccionadaKey = "";        // "FaseMenstruacion", etc.
    private string _sentimientoSeleccionadoKey = ""; // "SentimientoFeliz", etc.

    // Fecha de inicio del ciclo actual: si la usuaria no la ha guardado nunca,
    // la cogemos de hoy la primera vez que selecciona "Menstruacion".
    private DateTime? _fechaInicioCiclo = null;

    private Color ColorNormal => Application.Current.RequestedTheme == AppTheme.Dark
        ? Color.FromArgb("#1E1E1E") : Colors.White;
    private Color BordeNormal => Application.Current.RequestedTheme == AppTheme.Dark
        ? Color.FromArgb("#3C3C3C") : Colors.Transparent;

    private Dictionary<string, Frame> _fases;
    private Dictionary<string, Frame> _sintomas;
    private Dictionary<string, Frame> _sentimientos;

    // ── MAPEOS entre claves de UI y valores de BBDD ─────────────────
    // En la BBDD guardamos texto "limpio" (sin el prefijo "Fase" o "Sintoma")
    // para que sea más fácil de leer y reutilizar en otras pantallas.
    private static readonly Dictionary<string, string> FaseUiToDb = new()
    {
        { "FaseMenstruacion", "Menstruacion" },
        { "FaseFolicular",    "Folicular"    },
        { "FaseOvulacion",    "Ovulacion"    },
        { "FaseLutea",        "Lutea"        }
    };

    private static readonly Dictionary<string, string> SintomaUiToDb = new()
    {
        { "SintomaColicos",      "Cólicos"          },
        { "SintomaCabeza",       "Dolor de cabeza"  },
        { "SintomaHinchazón",    "Hinchazón"        },
        { "SintomaCambiosAnimo", "Cambios de ánimo" },
        { "SintomaFatiga",       "Fatiga"           },
        { "SintomaAcne",         "Acné"             }
    };

    private static readonly Dictionary<string, string> SentimientoUiToDb = new()
    {
        { "SentimientoTriste",     "Triste"     },
        { "SentimientoPreocupado", "Preocupado" },
        { "SentimientoNeutral",    "Neutral"    },
        { "SentimientoBien",       "Bien"       },
        { "SentimientoFeliz",      "Feliz"      }
    };

    // ── CONSTRUCTOR ─────────────────────────────────────────────────
    // Recibe la BBDD por DI igual que RegistroActividadPage y RetosPage.
    public MenstrualPage(SaludDatabase database)
    {
        InitializeComponent();
        _database = database;

        _fases = new Dictionary<string, Frame>
        {
            { "FaseMenstruacion", FaseMenstruacion },
            { "FaseFolicular",    FaseFolicular    },
            { "FaseOvulacion",    FaseOvulacion    },
            { "FaseLutea",        FaseLutea        }
        };

        _sintomas = new Dictionary<string, Frame>
        {
            { "SintomaColicos",      SintomaColicos      },
            { "SintomaCabeza",       SintomaCabeza       },
            { "SintomaHinchazón",    SintomaHinchazón    },
            { "SintomaCambiosAnimo", SintomaCambiosAnimo },
            { "SintomaFatiga",       SintomaFatiga       },
            { "SintomaAcne",         SintomaAcne         }
        };

        _sentimientos = new Dictionary<string, Frame>
        {
            { "SentimientoTriste",      SentimientoTriste      },
            { "SentimientoPreocupado",  SentimientoPreocupado  },
            { "SentimientoNeutral",     SentimientoNeutral     },
            { "SentimientoBien",        SentimientoBien        },
            { "SentimientoFeliz",       SentimientoFeliz       }
        };
    }

    // Se ejecuta cada vez que se entra a la página: cargamos los datos.
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

        await CargarDatosAsync();
    }

    // ── CARGA INICIAL ───────────────────────────────────────────────
    private async Task CargarDatosAsync()
    {
        // 1) Tarjeta superior: día del ciclo, progreso y días para el próximo periodo
        var estado = await _database.CalcularEstadoCicloAsync(_usuarioActual.ID_Usuario);
        ActualizarTarjetaCiclo(estado);

        // 2) Si HOY ya hay un registro, lo precargamos en la UI
        var registroHoy = await _database.ObtenerMenstruacionHoyAsync(_usuarioActual.ID_Usuario);
        if (registroHoy != null)
        {
            PrecargarRegistro(registroHoy);
        }
        else if (estado.TieneDatos)
        {
            // Si no hay registro de hoy pero sí ciclos previos, marcamos la fase
            // calculada como sugerencia (la usuaria puede cambiarla).
            string keyFase = FaseUiToDb.FirstOrDefault(p => p.Value == estado.FaseActual).Key;
            if (!string.IsNullOrEmpty(keyFase))
                SeleccionarFasePorKey(keyFase);
        }
    }

    private void ActualizarTarjetaCiclo(SaludDatabase.EstadoCiclo estado)
    {
        if (!estado.TieneDatos)
        {
            LblDiaCiclo.Text = "Aún no has registrado tu ciclo";
            LblProximoPeriodo.Text = "Marca tu primer día para empezar";
            BarraProgreso.Progress = 0;
            return;
        }

        LblDiaCiclo.Text = $"Día {estado.DiaActual} de {estado.DuracionCiclo}";
        LblProximoPeriodo.Text = estado.DiasParaProximoPeriodo == 0
            ? "Tu periodo podría empezar hoy"
            : $"Próximo periodo estimado: en {estado.DiasParaProximoPeriodo} días";
        BarraProgreso.Progress = estado.Progreso;

        // Guardamos la fecha de inicio para reusarla al guardar
        _fechaInicioCiclo = estado.FechaInicioCiclo;
    }

    private void PrecargarRegistro(Menstruacion m)
    {
        // Fase
        string keyFase = FaseUiToDb.FirstOrDefault(p => p.Value == m.Fase).Key;
        if (!string.IsNullOrEmpty(keyFase)) SeleccionarFasePorKey(keyFase);

        // Sentimiento
        string keySent = SentimientoUiToDb.FirstOrDefault(p => p.Value == m.Estado_Animo).Key;
        if (!string.IsNullOrEmpty(keySent)) SeleccionarSentimientoPorKey(keySent);

        // Síntomas (CSV → varios)
        if (!string.IsNullOrEmpty(m.Sintomas))
        {
            var nombresGuardados = m.Sintomas.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var nombre in nombresGuardados)
            {
                string keySint = SintomaUiToDb.FirstOrDefault(p => p.Value == nombre.Trim()).Key;
                if (!string.IsNullOrEmpty(keySint)) SeleccionarSintomaPorKey(keySint);
            }
        }

        // Notas
        EditorNotas.Text = m.Notas;

        // Fecha de inicio guardada
        if (!string.IsNullOrEmpty(m.Fecha_Inicio_Ciclo))
            _fechaInicioCiclo = DateTime.Parse(m.Fecha_Inicio_Ciclo);
    }

    // ── SELECCIÓN DESDE CÓDIGO (reaprovecha la lógica visual) ───────
    private void SeleccionarFasePorKey(string key)
    {
        if (!_fases.ContainsKey(key)) return;
        if (_faseSeleccionada != null)
        {
            _faseSeleccionada.BackgroundColor = ColorNormal;
            _faseSeleccionada.BorderColor = BordeNormal;
        }
        var frame = _fases[key];
        frame.BackgroundColor = Color.FromArgb("#2A1A1F");
        frame.BorderColor = Color.FromArgb("#F29EBB");
        _faseSeleccionada = frame;
        _faseSeleccionadaKey = key;
    }

    private void SeleccionarSentimientoPorKey(string key)
    {
        if (!_sentimientos.ContainsKey(key)) return;
        if (_sentimientoSeleccionado != null)
        {
            _sentimientoSeleccionado.BackgroundColor = ColorNormal;
            _sentimientoSeleccionado.BorderColor = BordeNormal;
        }
        var frame = _sentimientos[key];
        frame.BackgroundColor = Color.FromArgb("#2A1A1F");
        frame.BorderColor = Color.FromArgb("#F29EBB");
        _sentimientoSeleccionado = frame;
        _sentimientoSeleccionadoKey = key;
    }

    private void SeleccionarSintomaPorKey(string key)
    {
        if (!_sintomas.ContainsKey(key)) return;
        var frame = _sintomas[key];
        _sintomasSeleccionados.Add(key);
        frame.BackgroundColor = Color.FromArgb("#2A1A1F");
        frame.BorderColor = Color.FromArgb("#F29EBB");
    }

    // ── HANDLERS DE TAP ─────────────────────────────────────────────
    private void OnFaseTapped(object sender, TappedEventArgs e)
    {
        var key = e.Parameter?.ToString();
        if (key == null || !_fases.ContainsKey(key)) return;
        SeleccionarFasePorKey(key);

        // Si selecciona "Menstruación" y no había fecha de inicio guardada,
        // asumimos que HOY empieza un ciclo nuevo. Esto inicializa el
        // cálculo del ciclo la primera vez que se usa la pantalla.
        if (key == "FaseMenstruacion" && _fechaInicioCiclo == null)
        {
            _fechaInicioCiclo = DateTime.Today;
        }
    }

    private void OnSintomaTapped(object sender, TappedEventArgs e)
    {
        var key = e.Parameter?.ToString();
        if (key == null || !_sintomas.ContainsKey(key)) return;
        var frame = _sintomas[key];

        if (_sintomasSeleccionados.Contains(key))
        {
            _sintomasSeleccionados.Remove(key);
            frame.BackgroundColor = ColorNormal;
            frame.BorderColor = BordeNormal;
        }
        else
        {
            _sintomasSeleccionados.Add(key);
            frame.BackgroundColor = Color.FromArgb("#2A1A1F");
            frame.BorderColor = Color.FromArgb("#F29EBB");
        }
    }

    private void OnSentimientoTapped(object sender, TappedEventArgs e)
    {
        var key = e.Parameter?.ToString();
        if (key == null || !_sentimientos.ContainsKey(key)) return;
        SeleccionarSentimientoPorKey(key);
    }

    // ── BOTÓN GUARDAR ───────────────────────────────────────────────
    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        if (_usuarioActual == null) return;

        // Validación mínima: necesitamos al menos saber la fecha de inicio del ciclo.
        // Si la usuaria nunca ha registrado nada y tampoco ha tocado "Menstruación",
        // le pedimos que confirme que hoy es el primer día.
        if (_fechaInicioCiclo == null)
        {
            bool esHoy = await DisplayAlert(
                "Primer registro",
                "¿Hoy es el primer día de tu periodo? Si no, marca primero la fase en la que estás.",
                "Sí, hoy es día 1", "Cancelar");

            if (!esHoy) return;
            _fechaInicioCiclo = DateTime.Today;
        }

        // Convertimos la selección de UI a los valores que se guardan en BBDD
        string fase = FaseUiToDb.GetValueOrDefault(_faseSeleccionadaKey, "");
        string sentimiento = SentimientoUiToDb.GetValueOrDefault(_sentimientoSeleccionadoKey, "");

        var sintomasSeleccionados = _sintomasSeleccionados
            .Select(k => SintomaUiToDb.GetValueOrDefault(k, ""))
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();

        try
        {
            await _database.GuardarMenstruacionAsync(
                idUsuario:        _usuarioActual.ID_Usuario,
                fechaInicioCiclo: _fechaInicioCiclo.Value.ToString("yyyy-MM-dd"),
                fase:             fase,
                estadoAnimo:      sentimiento,
                sintomas:         sintomasSeleccionados,
                notas:            EditorNotas.Text ?? "");

            // Recargamos la tarjeta superior para reflejar el día actual
            var estado = await _database.CalcularEstadoCicloAsync(_usuarioActual.ID_Usuario);
            ActualizarTarjetaCiclo(estado);

            await DisplayAlert("✓", "Tu registro se ha guardado correctamente", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo guardar: {ex.Message}", "OK");
        }
    }

    // ── NAVEGACIÓN ──────────────────────────────────────────────────
    private async void OnInicioTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//MainPage");

    private async void OnRetosTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//RetosPage");

    private async void OnPerfilTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//PerfilPage");

    private async void OnVolverClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//MainPage");
}