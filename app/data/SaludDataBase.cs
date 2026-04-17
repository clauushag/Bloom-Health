using SQLite;
using app.Models;

namespace app.Data;

public class SaludDatabase
{
    private SQLiteAsyncConnection _conexion;
    private bool _isInitialized = false; 
    public SaludDatabase(string rutaBD)
    {
        _conexion = new SQLiteAsyncConnection(rutaBD);
        
    }

    public async Task InicializarAsync()
    {
        if (_isInitialized) return;
        await InicializarBBDD();
        await _conexion.CreateTableAsync<Paciente>();
        await InsertarCategoriasIniciales();
        await InsertarRetosIniciales();
        _isInitialized = true;
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
    public async Task InsertarUsuarioAsync(Usuario usuario)
    {
        await _conexion.InsertAsync(usuario);
    }
    public async Task<Usuario?> ObtenerUsuarioAsync()
    {
        await InicializarAsync();

        return await _conexion.Table<Usuario>()
                               .FirstOrDefaultAsync();
    }


    //encatgado de crear todas las tablas de la BBDD
    private async Task InicializarBBDD()
    {
        // Activar claves foráneas
        await _conexion.ExecuteAsync("PRAGMA foreign_keys = ON;");

        //tabla Usuario
        await _conexion.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Usuario (
                ID_Usuario INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Peso REAL NOT NULL,
                Altura REAL NOT NULL,
                FechaNacimiento TEXT NOT NULL,
                Genero TEXT NOT NULL
            );");
        await _conexion.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Categorias (
                ID_Categoria INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL UNIQUE
            );
            ");
        await _conexion.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Retos (
                ID_Reto INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Descripcion TEXT NOT NULL,
                Puntos_Recompensa INTEGER NOT NULL,
                ID_Categoria INTEGER NOT NULL,
                Objetivo REAL NOT NULL,
                FOREIGN KEY (ID_Categoria) REFERENCES Categorias(ID_Categoria)
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


        await _conexion.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS RegistroDiario (
                ID_Registro INTEGER PRIMARY KEY AUTOINCREMENT,
                ID_Usuario INTEGER NOT NULL,
                Fecha TEXT NOT NULL,
                FOREIGN KEY (ID_Usuario) REFERENCES Usuario(ID_Usuario)
            );");

        await _conexion.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Fisico (
                ID_Registro INTEGER PRIMARY KEY,
                Pasos INTEGER NOT NULL,
                Kcal_Quemadas REAL NOT NULL,
                Tiempo_Ejercicio REAL NOT NULL,
                FOREIGN KEY (ID_Registro) REFERENCES RegistroDiario(ID_Registro)
            );");




    }
    //categorias por defecto
    private async Task InsertarCategoriasIniciales()
    {
        var count = await _conexion.Table<Categorias>().CountAsync();
        if (count > 0) return;

        var categorias = new List<Categorias>
        {
            new Categorias { Nombre = "Actividad Física" },
            new Categorias { Nombre = "Sueño" },
            new Categorias { Nombre = "Nutrición" },
            new Categorias { Nombre = "Hidratación" },
            new Categorias { Nombre = "Bienestar" },
            new Categorias { Nombre = "Salud Mental" }
        };

        await _conexion.InsertAllAsync(categorias);
    }
    private async Task InsertarRetosIniciales()
    {
        // Si ya hay retos, no hacemos nada
        var count = await _conexion.Table<Retos>().CountAsync();
        if (count > 0) return;

        // Lista de retos iniciales
        var retos = new List<Retos>
        {
            new Retos { Nombre = "Caminar 5.000 pasos", Descripcion = "Alcanza 5.000 pasos en un día", Puntos_Recompensa = 10, ID_Categoria =1, Objetivo = 5000 },
            new Retos { Nombre = "Caminar 10.000 pasos", Descripcion = "Alcanza 10.000 pasos en un día", Puntos_Recompensa = 25, ID_Categoria =1, Objetivo = 10000 },
            new Retos { Nombre = "Caminar 15.000 pasos", Descripcion = "Alcanza 15.000 pasos en un día", Puntos_Recompensa = 40, ID_Categoria =1, Objetivo = 15000 },
            new Retos { Nombre = "Caminar 20.000 pasos", Descripcion = "Alcanza 20.000 pasos en un día", Puntos_Recompensa = 50, ID_Categoria =1, Objetivo = 20000 }

        };

        // Inserción masiva
        await _conexion.InsertAllAsync(retos);
    }
}