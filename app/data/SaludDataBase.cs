using SQLite;
using app.Models;

namespace app.Data;

public class SaludDatabase
{
    private SQLiteAsyncConnection _conexion;

    public SaludDatabase(string rutaBD)
    {
        bool existe = File.Exists(rutaBD);
        _conexion = new SQLiteAsyncConnection(rutaBD);
        // Esto crea la tabla la primera vez que se ejecuta la app
        if (!existe)
        {
            InicializarBBDD().Wait(); 
            _conexion.CreateTableAsync<Paciente>().Wait();
            
        }
        
    }

    public Task<List<Paciente>> ObtenerPacientesAsync()
    {
        return _conexion.Table<Paciente>().ToListAsync();

    }

    public Task<int> GuardarPacienteAsync(Paciente paciente)
    {
        if (paciente.Id != 0)
            return _conexion.UpdateAsync(paciente);
        else
            return _conexion.InsertAsync(paciente);
    }


    private async Task InicializarBBDD()
{
    // Activar claves foráneas
    await _conexion.ExecuteAsync("PRAGMA foreign_keys = ON;");

    //tabla Usuario
    await _conexion.ExecuteAsync(@"
        CREATE TABLE IF NOT EXISTS Usuario (
            ID_Usuario INTEGER PRIMARY KEY AUTOINCREMENT,
            Nombre TEXT NOT NULL,
            Peso REAL NOT NULL UNIQUE,
            Altura REAL NOT NULL,
            FechaNacimiento TEXT NOT NULL,
            Genero TEXT NOT NULL
        );");
    await _conexion.ExecuteAsync(@"
        CREATE TABLE IF NOT EXISTS Retos (
            ID_Reto INTEGER PRIMARY KEY AUTOINCREMENT,
            Nombre TEXT NOT NULL,
            Descripcion TEXT NOT NULL,
            Puntos_Recompensa INTEGER NOT NULL
        );");
     await _conexion.ExecuteAsync(@"
        CREATE TABLE IF NOT EXISTS ProgresoReto (
            ID_ProgresoReto INTEGER PRIMARY KEY AUTOINCREMENT,
            ID_Usuario INTEGER NOT NULL,
            ID_Reto INTEGER NOT NULL,
            FechaInicio TEXT NOT NULL,
            FechaFin TEXT,
            progreso INTEGER NOT NULL,
            Estado TEXT NOT NULL,
            FOREIGN KEY (ID_Usuario) REFERENCES Usuario(ID_Usuario),
            FOREIGN KEY (ID_Reto) REFERENCES Retos(ID_Reto)
        );");

    
}
}