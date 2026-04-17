using System;
using Microsoft.Maui.Controls;

namespace app
{
    public partial class PerfilPage : ContentPage
    {
        public PerfilPage()
        {
            InitializeComponent();
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