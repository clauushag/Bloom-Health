using SQLite;

namespace app.Models
{
    public class ProgresoReto
    {
        [PrimaryKey, AutoIncrement]
        public int ID_Progreso {get;set;}

        //Fk a Usuario
        public int ID_Usuario {get;set;}
        //FK a retos
        public int ID_Retos {get;set;}

        private string FechaInicio;
        private string FechaFin;
        public int progreso {get;set;}
        
        public string Estado {get;set;}

        public DateTime GetFechaInicio()
        {
            return DateTime.Parse(FechaInicio);
        }
        public void SetFechaInicio(DateTime fecha)
        {
            FechaInicio = fecha.ToString("yyyy-MM-dd");
        }
        public DateTime GetFechaFIN()
        {
            return DateTime.Parse(FechaFin);
        }
        public void SetFechaFin(DateTime fecha)
        {
            FechaFin = fecha.ToString("yyyy-MM-dd");
        }


    }
}