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
            await Shell.Current.GoToAsync("//RegistroActividadPage");
        }

        private async void  OnComidaTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//NutricionPage");
        }

        private async void OnEstadoTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//EstadoAnimoPage");
        }

        private async void OnSaludMenstrualTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MenstrualPage");
        }
    }
}