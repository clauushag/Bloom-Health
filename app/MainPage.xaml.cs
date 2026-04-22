using app.Data;
using app.Models;

namespace app
{
    public partial class MainPage : ContentPage
    {
        private SaludDatabase _database;
        public PerfilViewModel ViewModel { get; set; }
        public MainPage(SaludDatabase database)
        {
            InitializeComponent();
            _database = database;
            ViewModel = new PerfilViewModel();
            BindingContext = ViewModel;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
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

        // Navegaciones desde el menú
        private async void OnActividadTapped(object sender, EventArgs e)
        {
            MenuOverlay.IsVisible = false;
            MenuPanel.IsVisible = false;
            await Shell.Current.GoToAsync("//RegistroActividadPage");
        }

        private async void OnComidaTapped(object sender, EventArgs e)
        {
            MenuOverlay.IsVisible = false;
            MenuPanel.IsVisible = false;
            await Shell.Current.GoToAsync("//NutricionPage");
        }

        private async void OnEstadoTapped(object sender, EventArgs e)
        {
            MenuOverlay.IsVisible = false;
            MenuPanel.IsVisible = false;
            await Shell.Current.GoToAsync("//EstadoAnimicoPage");
        }

        private async void OnMenstrualTapped(object sender, EventArgs e)
        {
            MenuOverlay.IsVisible = false;
            MenuPanel.IsVisible = false;
            await Shell.Current.GoToAsync("//MenstrualPage");
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
}