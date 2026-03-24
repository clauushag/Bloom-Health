using SQLite;

namespace app.Models;

public class Paciente
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string? Nombre { get; set; } // <-- Añadimos el ?
    
    public string? Sintomas { get; set; } // <-- Añadimos el ?
}