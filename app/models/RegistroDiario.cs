using SQLite;

namespace app.Models
{
    public abstract class RegistroDiario
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
                return DateTime.Today; // Valor por defecto seguro
            return DateTime.Parse(Fecha);
        }
        public void SetFecha(DateTime fecha)
        {
            Fecha = fecha.ToString("yyyy-MM-dd-HH:mm:ss");
        }
    }
}