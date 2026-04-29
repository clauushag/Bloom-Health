using SQLite;

namespace app.Models
{
    public class Fisico
    {
        public int ID_Registro { get; set; }

        public string Tipo_Actividad { get; set; }

        // double en vez de int → permite decimales (ej: 3.5 km, 250.5 kcal)
        public double Distancia { get; set; }
        public double Kcal_Quemadas { get; set; }
        public int Tiempo_Ejercicio { get; set; }  // en minutos
        // Ahora existe en el modelo Y en la BD — se persiste al guardar
        public int XP { get; set; }

        [Ignore]
        public List<string> TiposActividad =>
    new() { "Correr", "Caminar", "Ciclismo", "Natación", "Yoga", "Gimnasio", "Baile", "Estiramiento" };

        private static readonly HashSet<string> ActividadesSinDistancia =
            new() { "Yoga", "Gimnasio", "Baile", "Estiramiento" };

        /// <summary>
        /// Calcula el XP que merece esta sesión según la duración.
        /// Mínimo 10 XP, máximo 100 XP, escala lineal cada 5 minutos = 5 XP.
        /// Ejemplos: 10 min → 10 XP | 30 min → 30 XP | 60 min → 60 XP | 120 min → 100 XP
        /// </summary>
        public int CalcularXP()
        {
            int xp = (Tiempo_Ejercicio / 5) * 5; // cada bloque de 5 min vale 5 XP
            return Math.Clamp(xp, 10, 100);
        }

        /// <summary>
        /// Valida los campos obligatorios.
        /// La distancia solo es obligatoria para actividades que la tienen sentido.
        /// </summary>
        public bool EsValido()
        {
            // Tipo y tiempo son siempre obligatorios
            if (string.IsNullOrEmpty(Tipo_Actividad) || Tiempo_Ejercicio <= 0)
                return false;

            // Kcal quemadas siempre obligatorio (el usuario las introduce o las estimamos)
            if (Kcal_Quemadas <= 0)
                return false;

            // Distancia solo obligatoria si la actividad la requiere
            bool requiereDistancia = !ActividadesSinDistancia.Contains(Tipo_Actividad);
            if (requiereDistancia && Distancia <= 0)
                return false;

            return true;
        }

        /// <summary>
        /// Indica si esta actividad concreta requiere que el usuario introduzca distancia.
        /// Lo usaremos en la página para mostrar/ocultar el campo de distancia.
        /// </summary>
        [Ignore]
        public bool RequiereDistancia =>
            !ActividadesSinDistancia.Contains(Tipo_Actividad ?? "");

        [Ignore]
        public string EmojiActividad => Tipo_Actividad switch
        {
            "Correr" => "🏃",
            "Caminar" => "🚶",
            "Ciclismo" => "🚴",
            "Natación" => "🏊",
            "Yoga" => "🧘",
            "Gimnasio" => "💪",
            "Baile" => "💃",
            "Estiramiento" => "🤸",
            _ => "🏅"
        };

        [Ignore]
        public string ResumenCorto => $"{Tiempo_Ejercicio} min • {Kcal_Quemadas:F0} kcal";

        [Ignore]
        public string XpTexto => $"+{XP} XP";
    }

}