using SQLite;

namespace app.Models
{
    public class Nutricional
    {
        [PrimaryKey, AutoIncrement]
        public int ID_Registro { get; set; }
        public string Comida { get; set; } = "";
        public double Kcal { get; set; }
        public string Nombre { get; set; } = "";
        public string Marca { get; set; } = "";
        public double Proteinas { get; set; }
        public double Carbos { get; set; }
        public double Grasas { get; set; }
        public double Fibra { get; set; }
        public string Imagen { get; set; } = "";
    }
}