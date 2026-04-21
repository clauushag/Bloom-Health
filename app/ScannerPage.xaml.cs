using ZXing.Net.Maui;

namespace app;

public partial class ScannerPage : ContentPage
{
    public ScannerPage()
    {
        InitializeComponent();

        // Le decimos que busque códigos de barras estándar (EAN)
        barcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.OneDimensional,
            AutoRotate = true,
            Multiple = false
        };
    }

    // Este evento salta automáticamente en cuanto la cámara ve un código
    private void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        var resultados = e.Results;
        if (resultados != null && resultados.Length > 0)
        {
            string codigoEscaneado = resultados[0].Value;

            // Paramos la cámara y volveamos a la pantalla anterior, pasando el código
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                barcodeReader.IsDetecting = false; // Detenemos la lectura
                
                // Opción 1: Enseñar una alerta con el número
                await DisplayAlert("¡Escaneado!", $"El código es: {codigoEscaneado}", "OK");

                // Volvemos a la página de Nutrición
                await Navigation.PopModalAsync();
            });
        }
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}