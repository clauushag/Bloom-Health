using SQLite;

namespace app.Models
{
    public class Avatar
    {
        [PrimaryKey, AutoIncrement]
        public int ID_Avatar { get; set; }

        public int Nivel_Evolucion { get; set; }
        public int XP { get; set; }


        public int ID_Usuario { get; set; }

        public string Estado_Salud { get; set; }

        [Ignore]
        public static List<string> Tipos_Estados_Salud { get; set; } = new List<string> {
                "Apagada",
                "Débil",
                "Brotando",
                "Estable",
                "En flor",
                "Fuerte",
            };
    }
}