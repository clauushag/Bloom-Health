namespace app; // Recuerda cambiar "app" si tu proyecto se llama diferente

public partial class EstadoAnimicoPage : ContentPage
{
    // Guarda el estado de ánimo principal seleccionado (solo uno)
    private Frame _estadoPrincipalSeleccionado;

    public EstadoAnimicoPage()
    {
        InitializeComponent();
    }

    // Lógica para SELECCIÓN ÚNICA (Increíble, Bien, Normal...)
    private void OnMoodTapped(object sender, TappedEventArgs e)
    {
        // 1. Si había uno seleccionado antes, lo volvemos blanco
        if (_estadoPrincipalSeleccionado != null)
        {
            _estadoPrincipalSeleccionado.BackgroundColor = Colors.White;
            _estadoPrincipalSeleccionado.BorderColor = Colors.Transparent;
        }

        // 2. Pintamos el nuevo de verde
        var frameActual = (Frame)sender;
        frameActual.BackgroundColor = Color.FromArgb("#F0F5F1"); // Verde de fondo
        frameActual.BorderColor = Color.FromArgb("#8EB497");     // Borde verde

        // 3. Guardamos cuál es el actual para desmarcarlo luego si se elige otro
        _estadoPrincipalSeleccionado = frameActual;
    }

    // Lógica para SELECCIÓN MÚLTIPLE (Con energía, cansado, motivado...)
    private void OnFeelingTapped(object sender, TappedEventArgs e)
    {
        var frame = (Frame)sender;

        // Comprobamos si está seleccionado (si tiene el borde verde)
        if (frame.BorderColor == Color.FromArgb("#8EB497"))
        {
            // Si ya estaba seleccionado, lo "apagamos" (lo volvemos blanco)
            frame.BackgroundColor = Colors.White;
            frame.BorderColor = Colors.Transparent;
        }
        else
        {
            // Si NO estaba seleccionado, lo "encendemos" (lo volvemos verde)
            frame.BackgroundColor = Color.FromArgb("#F0F5F1");
            frame.BorderColor = Color.FromArgb("#8EB497");
        }
    }

    // Lógica para el botón de la flecha de atrás
    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
    private async void OnInicioTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }

    private async void OnRetosTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//RetosPage");
    }

    private async void OnPerfilTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PerfilPage");
    }
}