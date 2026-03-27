using SQLite;

namespace app.Models
{
    public class Retos
    {
        [PrimaryKey, AutoIncrement]
        public int ID_Reto { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Puntos_Recompensa { get; set; }
        public int ID_Categoria {get;set;}
        public double Objetivo {get;set;}
    }
}