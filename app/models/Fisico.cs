using SQLite;

namespace app.Models
{
    public class Fisico 
    {

        public int Distancia { get; set; }
        public string Tipo_Actividad { get; set; }
        public int Kcal_Quemadas { get; set; }

        public int Tiempo_Ejercicio { get; set; }

        public int ID_Registro { get; set; }
        [Ignore]
        public List<string> TiposActividad { 
            get
            {
                return new List<string> { "Correr", "Caminar", "Bicicleta", "Natación", "Yoga", "Gimnasio" };
            }
        }

        public bool EsValido()
        {
            return Distancia > 0 && !string.IsNullOrEmpty(Tipo_Actividad) && Kcal_Quemadas > 0 && Tiempo_Ejercicio > 0;
        }
    }
}