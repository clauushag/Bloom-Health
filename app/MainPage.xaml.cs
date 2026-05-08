using app.Data;
using app.Models;
using app.Services;
namespace app
{
    public partial class MainPage : ContentPage
    {
        private SaludDatabase _database;
        public PerfilViewModel ViewModel { get; set; }
        private static readonly string[] _tips =
        {
            "Beber agua regularmente ayuda a tu planta.",
            "Una caminata de 20 minutos al día mejora tu energía.",
            "Dormir 7-8 horas recarga tu estado de ánimo.",
            "Comer frutas de temporada refuerza tu sistema inmune.",
            "Tomarte 5 minutos para respirar reduce el estrés."
        };
        public MainPage(SaludDatabase database)
        {
            InitializeComponent();
            _database = database;
            ViewModel = new PerfilViewModel();
            BindingContext = ViewModel;
            ViewModel.TipDelDia = _tips[DateTime.Now.DayOfYear % _tips.Length];
            _ = NotificacionService.InicializarAsync();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();


            try
            {
                await _database.InicializarAsync();
                Usuario usuario = await _database.ObtenerUsuarioAsync();

                if (usuario == null)
                {
                    await Shell.Current.GoToAsync("//crearPerfil");
                }
                else
                {
                    ViewModel.UsuarioActual = usuario;
                    ViewModel.AvatarActual = await _database.ObtenerAvatarAsync(ViewModel.UsuarioActual.ID_Usuario);
                    System.Diagnostics.Debug.WriteLine($"[Avatar] ID: {ViewModel.AvatarActual?.ID_Avatar}, Nivel: {ViewModel.AvatarActual?.Nivel_Evolucion}, Imagen: {ViewModel.AvatarActual?.ImagenPlanta}");

                }
            }
            catch (Exception ex)
            {
                // CAMBIO: En debug te muestra el error; en release lo puedes loguear
                // en tu sistema de telemetría (AppCenter, Firebase, etc.)
                System.Diagnostics.Debug.WriteLine($"[MainPage] Error en OnAppearing: {ex.Message}");
                await DisplayAlert("Error", "No se pudo cargar tu perfil. Inténtalo de nuevo.", "OK");
            }

        }
        // Abre el menú
        private void OnAddClicked(object sender, EventArgs e)
        {
            MenuOverlay.IsVisible = true;
            MenuPanel.IsVisible = true;
        }

        // Cierra el menú (botón X o fondo)
        private void OnCerrarMenuTapped(object sender, EventArgs e)
        {
            MenuOverlay.IsVisible = false;
            MenuPanel.IsVisible = false;
        }
        private async Task CerrarMenuYNavegar(string ruta)
        {
            MenuOverlay.IsVisible = false;
            MenuPanel.IsVisible = false;
            await Shell.Current.GoToAsync(ruta);
        }

        // Navegaciones desde el menú
        private async void OnActividadTapped(object sender, EventArgs e)
            => await CerrarMenuYNavegar("//RegistroActividadPage");

        private async void OnComidaTapped(object sender, EventArgs e)
            => await CerrarMenuYNavegar("//NutricionPage");

        private async void OnEstadoTapped(object sender, EventArgs e)
            => await CerrarMenuYNavegar("//EstadoAnimicoPage");

        private async void OnMenstrualTapped(object sender, EventArgs e)
            => await CerrarMenuYNavegar("//MenstrualPage");

        private async void OnInicioTapped(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//MainPage");

        private async void OnRetosTapped(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//RetosPage");

        private async void OnPerfilTapped(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("//PerfilPage");
    }
}