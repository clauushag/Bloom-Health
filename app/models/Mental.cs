using SQLite;

namespace app.Models
{
    public class Mental
    {
        [PrimaryKey, AutoIncrement]
        public int ID_Registro { get; set; }
        public string Estado_Animo { get; set; }="";

        public double Horas_Sueno { get; set; }

        public string Notas_diario { get; set; }="";

        public bool EstaCompleto()
        {
            return !string.IsNullOrEmpty(Estado_Animo) && Horas_Sueno > 0;
        }

        public int CalcularXP()
        {
            int xp = 0;

            // XP por estado de ánimo
            xp += Estado_Animo switch
            {
                "Muy mal" => 0,
                "Mal"     => 5,
                "Regular" => 10,
                "Bien"    => 15,
                "Genial"  => 20,
                _         => 0
            };

            // XP por horas de sueño (cada hora completa vale 5 XP, máximo 40 XP)
            xp += (int)(Horas_Sueno) * 5;
            return Math.Clamp(xp, 0, 60); // Máximo total de XP para esta sección
        }

        public static readonly List<string> EstadosAnimo = new() { "Muy mal", "Mal", "Regular", "Bien", "Genial" };
        

    }
}