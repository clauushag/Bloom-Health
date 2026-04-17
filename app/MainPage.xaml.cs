using app.Data;
using app.Models;

namespace app 
{
    public partial class MainPage : ContentPage
    {
        private SaludDatabase _database;
        public MainPage(SaludDatabase database)
        {
            InitializeComponent();
            _database = database;
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
        }
        private void OnAddClicked(object sender, EventArgs e)
        {
            // Aquí irá la lógica para abrir el menú
        }
    }
}