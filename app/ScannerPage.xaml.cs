using ZXing.Net.Maui;
using System.Text.Json;
using System.Globalization;
namespace app;

public partial class ScannerPage : ContentPage
{
    private bool _yaEscaneado = false;
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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _yaEscaneado = false;
        barcodeReader.IsDetecting = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        barcodeReader.IsDetecting = false;
    }

    // Este evento salta automáticamente en cuanto la cámara ve un código
    private void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_yaEscaneado) return;
        var resultados = e.Results;
        if (resultados != null && resultados.Length > 0)
        {
            _yaEscaneado = true;
            string codigo = resultados[0].Value;
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                barcodeReader.IsDetecting = false;
                await BuscarProducto(codigo);
            });
        }
    }

    private async Task BuscarProducto(string codigoBarras)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "BloomHealth/1.0");
            client.Timeout = TimeSpan.FromSeconds(10);

            string url = $"https://world.openfoodfacts.org/api/v2/product/{codigoBarras}.json";
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Error de red", "No se pudo conectar con la base de datos.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            string json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("status", out var statusProp) && statusProp.GetInt32() == 0)
            {
                bool reintentar = await DisplayAlert(
                    "No encontrado",
                    "Este producto no está en la base de datos. ¿Escanear otro?",
                    "Sí", "No");
                if (reintentar) { _yaEscaneado = false; barcodeReader.IsDetecting = true; }
                else await Shell.Current.GoToAsync("..");
                return;
            }

            var product = root.GetProperty("product");

            string nombre = ObtenerString(product, "product_name_es")
                         ?? ObtenerString(product, "product_name")
                         ?? "Producto desconocido";
            string marca = ObtenerString(product, "brands") ?? "Sin marca";
            string imagen = ObtenerString(product, "image_front_small_url") ?? "";

            double kcal = 0, proteinas = 0, carbos = 0, grasas = 0, fibra = 0;
            if (product.TryGetProperty("nutriments", out var nutriments))
            {
                kcal = ObtenerDouble(nutriments, "energy-kcal_100g");
                proteinas = ObtenerDouble(nutriments, "proteins_100g");
                carbos = ObtenerDouble(nutriments, "carbohydrates_100g");
                grasas = ObtenerDouble(nutriments, "fat_100g");
                fibra = ObtenerDouble(nutriments, "fiber_100g");
            }

            var ci = CultureInfo.InvariantCulture;
            await Shell.Current.GoToAsync(
                $"..?nombre={Uri.EscapeDataString(nombre)}" +
                $"&marca={Uri.EscapeDataString(marca)}" +
                $"&kcal={kcal.ToString(ci)}" +
                $"&proteinas={proteinas.ToString(ci)}" +
                $"&carbos={carbos.ToString(ci)}" +
                $"&grasas={grasas.ToString(ci)}" +
                $"&fibra={fibra.ToString(ci)}" +
                $"&imagen={Uri.EscapeDataString(imagen)}");
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("HttpRequestException",
                $"Mensaje: {ex.Message}\nInner: {ex.InnerException?.Message}", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Exception general",
                $"Tipo: {ex.GetType().Name}\nMensaje: {ex.Message}\nInner: {ex.InnerException?.Message}", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    private string? ObtenerString(JsonElement el, string key) =>
        el.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() : null;

    private double ObtenerDouble(JsonElement el, string key) =>
        el.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.Number
            ? v.GetDouble() : 0;

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
