using SQLite;

namespace app.Models;

[Table("Menstruacion")]
public class Menstruacion
{
    [PrimaryKey]
    [Column("ID_Registro")]
    public int ID_Registro { get; set; }

    // Fecha en la que empezó EL CICLO actual (primer día de la última regla).
    [Column("Fecha_Inicio_Ciclo")]
    public string Fecha_Inicio_Ciclo { get; set; } = "";

    // Fase seleccionada por la usuaria: "Menstruacion" | "Folicular" | "Ovulacion" | "Lutea"
    [Column("Fase")]
    public string Fase { get; set; } = "";

    // Sentimiento elegido (un solo valor): "Triste", "Preocupado", "Neutral", "Bien", "Feliz"
    [Column("Estado_Animo")]
    public string Estado_Animo { get; set; } = "";

    // Síntomas como CSV: "Cólicos,Fatiga,Acné" (varios posibles)
    [Column("Sintomas")]
    public string Sintomas { get; set; } = "";

    // Notas personales que escribe la usuaria
    [Column("Notas")]
    public string Notas { get; set; } = "";

    // Duración media del ciclo en días (28 por defecto)
    [Column("Duracion_Ciclo")]
    public int Duracion_Ciclo { get; set; } = 28;

    // Duración media del periodo en días (5 por defecto)
    [Column("Duracion_Periodo")]
    public int Duracion_Periodo { get; set; } = 5;
}