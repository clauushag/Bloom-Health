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
            await Shell.Current.GoToAsync("..");
        }

        private async void OnActividadTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(RegistroActividadPage));
        }

        private async Task OnComidaTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(NutricionPage));
        }

        private async Task OnEstadoTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(EstadoAnimicoPage));
        }

        private async Task OnSaludMenstrualTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(MenstrualPage));
        }
    }
}