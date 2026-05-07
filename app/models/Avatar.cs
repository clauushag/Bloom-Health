using SQLite;

namespace app.Models
{
    public class Avatar
    {
        public const int XP_POR_NIVEL = 100;
        public const int NIVEL_MAXIMO = 3;

        [PrimaryKey, AutoIncrement]
        public int ID_Avatar { get; set; }

        public int Nivel_Evolucion { get; set; } = 1;
        public int XP { get; set; }


        public int ID_Usuario { get; set; }

        public string Estado_Salud { get; set; }

        [Ignore]
        public static List<string> Tipos_Estados_Salud { get; set; } = new List<string> {
               "Marchita",
               "Creciendo",
               "Florecida"
            };
        public string ImagenPlanta => Nivel_Evolucion switch
        {
            1 => "plantamarchita.png",
            2 => "plantacreciendo.png",
            3 => "plantaflorecida.png",
            _ => "plantamarchita.png"
        };



    }
}