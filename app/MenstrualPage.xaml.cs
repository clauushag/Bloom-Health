namespace app;

public partial class MenstrualPage : ContentPage
{
    private Frame _faseSeleccionada = null;
    private Frame _sentimientoSeleccionado = null;
    private HashSet<string> _sintomasSeleccionados = new();

    private Color ColorNormal => Application.Current.RequestedTheme == AppTheme.Dark
        ? Color.FromArgb("#1E1E1E") : Colors.White;
    private Color BordeNormal => Application.Current.RequestedTheme == AppTheme.Dark
        ? Color.FromArgb("#3C3C3C") : Colors.Transparent;

    private Dictionary<string, Frame> _fases;
    private Dictionary<string, Frame> _sintomas;
    private Dictionary<string, Frame> _sentimientos;

    public MenstrualPage()
    {
        InitializeComponent();

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

    // ── Fases (solo una) ─────────────────────────────────────────────
    private void OnFaseTapped(object sender, TappedEventArgs e)
    {
        if (_faseSeleccionada != null)
        {
            _faseSeleccionada.BackgroundColor = ColorNormal;
            _faseSeleccionada.BorderColor = BordeNormal;
        }

        var key = e.Parameter?.ToString();
        if (key == null || !_fases.ContainsKey(key)) return;

        var frame = _fases[key];
        frame.BackgroundColor = Color.FromArgb("#2A1A1F");
        frame.BorderColor = Color.FromArgb("#F29EBB");
        _faseSeleccionada = frame;
    }

    // ── Síntomas (varios) ────────────────────────────────────────────
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

    // ── Sentimientos (solo uno) ──────────────────────────────────────
    private void OnSentimientoTapped(object sender, TappedEventArgs e)
    {
        if (_sentimientoSeleccionado != null)
        {
            _sentimientoSeleccionado.BackgroundColor = ColorNormal;
            _sentimientoSeleccionado.BorderColor = BordeNormal;
        }

        var key = e.Parameter?.ToString();
        if (key == null || !_sentimientos.ContainsKey(key)) return;

        var frame = _sentimientos[key];
        frame.BackgroundColor = Color.FromArgb("#2A1A1F");
        frame.BorderColor = Color.FromArgb("#F29EBB");
        _sentimientoSeleccionado = frame;
    }

    // ── Navegación ───────────────────────────────────────────────────
    private async void OnInicioTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//MainPage");

    private async void OnRetosTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//RetosPage");

    private async void OnPerfilTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//PerfilPage");

    private async void OnVolverClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//MainPage");
}