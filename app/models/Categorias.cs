using SQLite;

namespace app.Models
{
    public class Categorias
    {
        [PrimaryKey, AutoIncrement]
        public int ID_Categoria { get; set; }
        public string Nombre { get; set; }
    }
}