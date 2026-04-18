using SQLite;

namespace app.Models
{
    public class Nutricional
    {
        [PrimaryKey, AutoIncrement]
        public string Comida { get; set; }

        public int Kcal_Ingeridos { get; set; }

        public int Vasos_Agua { get; set; }

        // Relación 1:1 con RegistroDiario
        public int ID_Registro { get; set; }
    }
}