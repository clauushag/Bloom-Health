using System;
using Microsoft.Maui.Controls;

namespace app
{
    public partial class PerfilPage : ContentPage
    {
        public PerfilPage()
        {
            InitializeComponent();

            // Al abrir la página, comprobamos si el modo oscuro ya está activo
            // para poner el Switch en la posición correcta
            if (Application.Current.UserAppTheme == AppTheme.Dark)
            {
                SwitchModoOscuro.IsToggled = true;
                LblEstadoModoOscuro.Text = "Activado";
            }
        }

        // Este es el nuevo evento que se ejecuta al tocar el Switch
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
            await Navigation.PopAsync();
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
    }
}