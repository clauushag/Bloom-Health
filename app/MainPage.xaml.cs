using app.Data;
using app.Models;

namespace app 
{
    public partial class MainPage : ContentPage
    {
        private SaludDatabase _database;
        public Usuario UsuarioActual { get; set; }  
        public MainPage(SaludDatabase database)
        {
            InitializeComponent();
            _database = database;
            UsuarioActual = new Usuario();
            BindingContext = UsuarioActual; 
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _database.InicializarAsync();
            Usuario usuario = await _database.ObtenerUsuarioAsync();
            UsuarioActual = usuario;
            BindingContext = UsuarioActual; // Actualizamos el BindingContext con el usuario obtenido
            if (usuario == null)
            {
                await Shell.Current.GoToAsync("//crearPerfil");
            }
        }
        private void OnAddClicked(object sender, EventArgs e)
        {
            // Aquí irá la lógica para abrir el menú
        }
    }
}