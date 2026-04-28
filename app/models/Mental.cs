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
    }
}