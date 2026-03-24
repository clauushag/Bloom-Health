using SQLite;
using app.Models;

namespace app.Data;

public class SaludDatabase
{
    private SQLiteAsyncConnection _conexion;

    public SaludDatabase(string rutaBD)
    {
        _conexion = new SQLiteAsyncConnection(rutaBD);
        // Esto crea la tabla la primera vez que se ejecuta la app
        _conexion.CreateTableAsync<Paciente>().Wait();
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
}