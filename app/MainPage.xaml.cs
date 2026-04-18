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
        private void OnAddClicked(object sender, EventArgs e)
        {
            // Aquí irá la lógica para abrir el menú
        }
    }
}