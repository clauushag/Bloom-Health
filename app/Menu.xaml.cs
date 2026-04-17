using System;
using Microsoft.Maui.Controls;

namespace app
{
    public partial class Menu : ContentPage
    {
        public Menu()
        {
            InitializeComponent();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private void OnActividadTapped(object sender, EventArgs e)
        {
            // Lógica para Actividad
        }

        private void OnComidaTapped(object sender, EventArgs e)
        {
            // Lógica para Comida
        }

        private void OnEstadoTapped(object sender, EventArgs e)
        {
            // Lógica para Estado Anímico
        }

        private void OnSaludMenstrualTapped(object sender, EventArgs e)
        {
            // Lógica para el registro de salud menstrual
        }
    }
}