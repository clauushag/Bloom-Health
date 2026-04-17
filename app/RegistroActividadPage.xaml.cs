namespace app;

public partial class RegistroActividadPage : ContentPage
{
    // Variable para recordar cuál fue la última tarjeta que tocamos
    private Frame _frameSeleccionadoAnteriormente;

    public RegistroActividadPage()
    {
        InitializeComponent();
    }

    private void OnActividadTapped(object sender, TappedEventArgs e)
    {
        // 1. Si ya había una tarjeta seleccionada antes, la volvemos a poner blanca
        if (_frameSeleccionadoAnteriormente != null)
        {
            _frameSeleccionadoAnteriormente.BackgroundColor = Colors.White;
            _frameSeleccionadoAnteriormente.BorderColor = Colors.Transparent;
        }

        // 2. Obtenemos la tarjeta (Frame) exacta que acabas de tocar ahora mismo
        var frameActual = (Frame)sender;

        // 3. Pintamos la tarjeta tocada de verde
        frameActual.BackgroundColor = Color.FromArgb("#F0F5F1"); 
        frameActual.BorderColor = Color.FromArgb("#8EB497");     

        // 4. Guardamos esta tarjeta en la memoria para "deseleccionarla" la próxima vez
        _frameSeleccionadoAnteriormente = frameActual;

        // 5. Mostramos el formulario de abajo
        ContenedorFormulario.IsVisible = true;
        
        // 6. Cambiamos el título del formulario
        if (e.Parameter != null)
        {
            LabelActividadSeleccionada.Text = "Registrar " + e.Parameter.ToString();
        }
    }

    private async void OnVolverClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}