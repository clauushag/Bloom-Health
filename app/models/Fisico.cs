using SQLite;

namespace app.Models
{
    public class Fisico : RegistroDiario
    {

        public int Distancia { get; set; }

        public int Kcal_Quemadas { get; set; }

        public int Tiempo_Ejercicio { get; set; }

    }
}