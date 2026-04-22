using System;
using Microsoft.Maui.Controls;
using app.Data;
using app.Models;

namespace app
{
    public partial class PerfilPage : ContentPage
    {
        private SaludDatabase _database;
        public PerfilViewModel ViewModel { get; set; }
        public PerfilPage(SaludDatabase database)
        {
            InitializeComponent();
            _database = database;
            ViewModel = new PerfilViewModel();
            BindingContext = ViewModel;

            // Al abrir la página, comprobamos si el modo oscuro ya está activo
            // para poner el Switch en la posición correcta
            if (Application.Current.UserAppTheme == AppTheme.Dark)
            {
                SwitchModoOscuro.IsToggled = true;
                LblEstadoModoOscuro.Text = "Activado";
            }
        }

        // Este es el nuevo evento que se ejecuta al tocar el Switch
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
        private void OnModoOscuroToggled(object sender, ToggledEventArgs e)
        {
            if (e.Value) // Si el switch se enciende (true)
            {
                Application.Current.UserAppTheme = AppTheme.Dark;
                LblEstadoModoOscuro.Text = "Activado";
            }
            else // Si el switch se apaga (false)
            {
                Application.Current.UserAppTheme = AppTheme.Light;
                LblEstadoModoOscuro.Text = "Desactivado";
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            // Lógica para ir a la pantalla de edición
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Cerrar Sesión", "¿Estás seguro de que quieres salir?", "Sí", "No");
            if (answer)
            {
                // Lógica para cerrar sesión y volver al Login
            }
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