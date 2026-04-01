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
        //usar getFechaNacimiento para obtener la fecha en formato DateTime y setFechaNacimiento para guardar la fecha en formato string
        public string FechaNacimiento { get; set; }

        public string Genero { get; set; }



        
        // Foreign Key hacia Avatar (no requiere atributo)
        [Ignore]//temporal
        public int ID_Avatar { get; set; }


        [Ignore]
        public DateTime FechaNacDate
        {
            get => GetFechaNacimiento();
            set => SetFechaNacimiento(value);
        }

        [Ignore]
        public List<string> ListaGeneros { get; set; } = new(){"Hombre","Mujer","Otro"};




        public DateTime GetFechaNacimiento()
        {

            if (string.IsNullOrEmpty(FechaNacimiento))
                return DateTime.Today; // Valor por defecto seguro
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
        //comprobar que todos los campost esten completos
        public bool EsValido()
        {
            //false si algun campo esta vacio
            return !string.IsNullOrWhiteSpace(Nombre) &&
                   Peso > 0 &&
                   Altura > 0 &&
                   !string.IsNullOrWhiteSpace(Genero) &&
                   GetFechaNacimiento() < DateTime.Today; // La fecha de nacimiento debe ser en el pasado
        }
    }
}