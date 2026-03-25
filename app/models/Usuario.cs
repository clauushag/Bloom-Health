using SQLite;

namespace app.Models
{
    public class Usuario
    {
        [PrimaryKey, AutoIncrement]
        public int ID_Usuario { get; set; }

        public string Nombre { get; set; }

        public double Peso { get; set; }

        public double Altura { get; set; }

        private string FechaNacimiento;

        public string Genero { get; set; }



        
        // Foreign Key hacia Avatar (no requiere atributo)
        [Ignore]//temporal
        public int ID_Avatar { get; set; }


        public DateTime GetFechaNacimiento()
        {
            return DateTime.Parse(FechaNacimiento);
        }
        public void SetFechaNacimiento(DateTime fecha)
        {
            FechaNacimiento = fecha.ToString("yyyy-MM-dd");
        }
        public int CalcularEdad()
        {
            DateTime fechaNacimiento = GetFechaNacimiento();
            DateTime hoy = DateTime.Today;
            int edad = hoy.Year - fechaNacimiento.Year;

            if (hoy < fechaNacimiento.AddYears(edad))
                edad--;

            return edad;
        }
    }
}