using SQLite;

namespace app.Models
{
    public class RegistroDiario
    {
        [PrimaryKey, AutoIncrement]
        public int ID_Registro { get; set; }
        //usar getFecha para obtener la fecha en formato DateTime y setFecha para guardar la fecha en formato string
        public string Fecha { get; set; }

        // FK a Usuario
        public int ID_Usuario { get; set; }

        [Ignore]
        public DateTime FechaDate
        {
            get => GetFecha();
            set => SetFecha(value);
        }

        public DateTime GetFecha()
        {

            if (string.IsNullOrEmpty(Fecha))
                return DateTime.Today;

            // Convierte "2026-04-29-10:43:53" → "2026-04-29 10:43:53"
            var normalizado = Fecha.Length > 10
                ? Fecha[..10] + " " + Fecha[11..]
                : Fecha;

            return DateTime.Parse(normalizado);
        }
        public void SetFecha(DateTime fecha)
        {
            Fecha = fecha.ToString("yyyy-MM-dd-HH:mm:ss");
        }
    }
}