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
        await MigrarAsync();
        await InsertarCategoriasIniciales();
        await InsertarRetosIniciales();
        _isInitialized = true;
    }

    private async Task MigrarAsync()
    {
        // ── FISICO ──────────────────────────────────────────────────────────────
        // Obtenemos las columnas reales de la tabla en disco
        var columnas = await _conexion.QueryAsync<ColumnInfo>(
            "PRAGMA table_info(Fisico);");
        var nombres = columnas.Select(c => c.Name).ToHashSet();

        // Si faltan columnas clave del modelo actual → recreamos la tabla entera
        // Esto cubre cualquier combinación de columnas antiguas desconocidas
        bool necesitaRecrear = !nombres.Contains("XP")
                            || !nombres.Contains("Tipo_Actividad")
                            || !nombres.Contains("Distancia")
                            || !nombres.Contains("Kcal_Quemadas")
                            || !nombres.Contains("Tiempo_Ejercicio");

        if (necesitaRecrear)
        {
            // 1. Desactivamos FK para poder hacer el DROP sin violar restricciones
            await _conexion.ExecuteAsync("PRAGMA foreign_keys = OFF;");

            // 2. Renombramos la tabla vieja (no la borramos por si acaso)
            await _conexion.ExecuteAsync(
                "ALTER TABLE Fisico RENAME TO Fisico_old;");

            // 3. Creamos la tabla nueva con la estructura correcta
            await _conexion.ExecuteAsync(@"
            CREATE TABLE Fisico (
                ID_Registro INTEGER PRIMARY KEY,
                Distancia   REAL    NOT NULL DEFAULT 0,
                Tipo_Actividad TEXT NOT NULL DEFAULT '',
                XP          INTEGER NOT NULL DEFAULT 0,
                Kcal_Quemadas REAL  NOT NULL DEFAULT 0,
                Tiempo_Ejercicio REAL NOT NULL DEFAULT 0,
                FOREIGN KEY (ID_Registro) REFERENCES RegistroDiario(ID_Registro)
            );");

            // 4. Copiamos los datos que podamos rescatar de la tabla vieja
            //    COALESCE maneja columnas que no existían en la versión antigua
            await _conexion.ExecuteAsync(@"
            INSERT INTO Fisico (ID_Registro, Distancia, Tipo_Actividad, XP,
                                Kcal_Quemadas, Tiempo_Ejercicio)
            SELECT
                ID_Registro,
                COALESCE(Distancia, 0),
                COALESCE(Tipo_Actividad, ''),
                COALESCE(XP, 0),
                COALESCE(Kcal_Quemadas, 0),
                COALESCE(Tiempo_Ejercicio, 0)
            FROM Fisico_old;");

            // 5. Borramos la tabla vieja
            await _conexion.ExecuteAsync("DROP TABLE Fisico_old;");

            // 6. Reactivamos FK
            await _conexion.ExecuteAsync("PRAGMA foreign_keys = ON;");
        }
    }

    // Clase auxiliar para mapear el resultado de PRAGMA table_info
    private class ColumnInfo
    {
        [SQLite.Column("name")]
        public string Name { get; set; }
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
    public async Task<int> InsertarRegistroAsync(RegistroDiario registro)
    {
        await _conexion.InsertAsync(registro);
        return registro.ID_Registro; // Devolvemos el ID generado
    }
    public async Task InsertarFisicoAsync(Fisico fisico)
    {
        await _conexion.InsertAsync(fisico);
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
            CREATE TABLE IF NOT EXISTS Avatar (
                ID_Avatar INTEGER PRIMARY KEY AUTOINCREMENT,
                ID_Usuario INTEGER NOT NULL UNIQUE,
                XP INTEGER NOT NULL,
                Nivel_Evolucion INTEGER NOT NULL,
                Estado_Salud TEXT NOT NULL,
                FOREIGN KEY (ID_Usuario) REFERENCES Usuario(ID_Usuario)
            );");


        await _conexion.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS RegistroDiario (
                ID_Registro INTEGER PRIMARY KEY AUTOINCREMENT,
                ID_Usuario INTEGER NOT NULL,
                Fecha TEXT NOT NULL,
                FOREIGN KEY (ID_Usuario) REFERENCES Usuario(ID_Usuario)
            );");

        // Distancia ahora es REAL (antes INTEGER) para admitir decimales
        await _conexion.ExecuteAsync(@"
        CREATE TABLE IF NOT EXISTS Fisico (
            ID_Registro INTEGER PRIMARY KEY,
            Distancia REAL NOT NULL,
            Tipo_Actividad TEXT NOT NULL,
            XP INTEGER NOT NULL,
            Kcal_Quemadas REAL NOT NULL,
            Tiempo_Ejercicio REAL NOT NULL,
            FOREIGN KEY (ID_Registro) REFERENCES RegistroDiario(ID_Registro)
        );");

        await _conexion.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Nutricional (
                ID_Registro INTEGER PRIMARY KEY,
                Comida TEXT NOT NULL,
                Kcal REAL NOT NULL,
                Nombre TEXT NOT NULL,
                Marca TEXT NOT NULL,
                Proteinas REAL NOT NULL,
                Carbos REAL NOT NULL,
                Grasas REAL NOT NULL,
                Fibra REAL NOT NULL,
                Imagen TEXT,
                FOREIGN KEY (ID_Registro) REFERENCES RegistroDiario(ID_Registro)
            );");

        await _conexion.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Mental (
                ID_Registro INTEGER PRIMARY KEY,
                Estado_Animo TEXT NOT NULL,
                Horas_Sueno REAL NOT NULL,
                Notas_diario TEXT,
                FOREIGN KEY (ID_Registro) REFERENCES RegistroDiario(ID_Registro)
            );");

        await _conexion.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Menstruacion (
                ID_Registro INTEGER PRIMARY KEY,
                Fecha_Inicio_Ciclo TEXT NOT NULL,
                Estado_Animo TEXT NOT NULL,
                Sintomas TEXT NOT NULL,
                FOREIGN KEY (ID_Registro) REFERENCES RegistroDiario(ID_Registro)
            );");

    }

    public async Task InsertarNutricionalAsync(Nutricional nutricional)
    {
        await _conexion.InsertAsync(nutricional);
    }

    public async Task<List<Nutricional>> ObtenerHistorialNutricionalAsync(int idUsuario)
    {
        // Une RegistroDiario con Nutricional para obtener solo los del usuario actual
        return await _conexion.QueryAsync<Nutricional>(
            @"SELECT n.* FROM Nutricional n
          INNER JOIN RegistroDiario r ON n.ID_Registro = r.ID_Registro
          WHERE r.ID_Usuario = ?
          ORDER BY r.ID_Registro DESC
          LIMIT 20", idUsuario);
    }

    public async Task<double> ObtenerKcalHoyAsync(int idUsuario)
    {
        var hoy = DateTime.Now.ToString("yyyy-MM-dd");
        var resultado = await _conexion.QueryAsync<Nutricional>(
            @"SELECT n.* FROM Nutricional n
          INNER JOIN RegistroDiario r ON n.ID_Registro = r.ID_Registro
          WHERE r.ID_Usuario = ? AND r.Fecha LIKE ?",
            idUsuario, hoy + "%");
        return resultado.Sum(n => n.Kcal);
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

    public async Task CrearAvatarAsync()
    {
        Avatar avatar = new Avatar
        {
            ID_Usuario = 1, // Aquí deberías poner el ID del usuario actual
            Nivel_Evolucion = 1,
            XP = 0,
            Estado_Salud = Avatar.Tipos_Estados_Salud[2] // "Brotando"
        };
        await _conexion.InsertAsync(avatar);
    }
    public async Task<Avatar> ObtenerAvatarAsync(int idUsuario)
    {
        return await _conexion.Table<Avatar>()
                               .FirstOrDefaultAsync(a => a.ID_Usuario == idUsuario);
    }

    public async Task SumarXPAsync(int idUsuario, int xpGanado)
    {
        await _conexion.ExecuteAsync(
            "UPDATE Avatar SET XP = XP + ? WHERE ID_Usuario = ?",
            xpGanado, idUsuario);
    }

    // ─── ACTIVIDAD FÍSICA ────────────────────────────────────────────────────────

    // Clase auxiliar interna: agrupa los datos que la página necesita mostrar
    // al usuario tras guardar o al cargar la pantalla.
    public class ResumenActividadHoy
    {
        public double KcalQuemadas { get; set; }   // suma de kcal de todos los Fisico de hoy
        public int MinutosTotales { get; set; } // suma de Tiempo_Ejercicio de hoy
        public int XpGanado { get; set; }       // suma de XP de los registros de hoy
        public int Sesiones { get; set; }       // cuántas actividades se han guardado hoy
    }

    /// <summary>
    /// Devuelve las estadísticas de actividad física del día actual para un usuario.
    /// Une RegistroDiario (para filtrar por usuario y fecha) con Fisico (datos de la actividad).
    /// </summary>
    public async Task<ResumenActividadHoy> ObtenerResumenActividadHoyAsync(int idUsuario)
    {
        // Prefijo de la fecha de hoy en formato "yyyy-MM-dd" — mismo formato que usa SetFecha()
        var hoy = DateTime.Now.ToString("yyyy-MM-dd");

        // Traemos todos los registros Fisico del usuario para hoy
        var registros = await _conexion.QueryAsync<Fisico>(
            @"SELECT f.* FROM Fisico f
          INNER JOIN RegistroDiario r ON f.ID_Registro = r.ID_Registro
          WHERE r.ID_Usuario = ? AND r.Fecha LIKE ?",
            idUsuario, hoy + "%");

        // Agregamos en memoria (son pocos registros diarios, no merece la pena hacer SUM en SQL)
        return new ResumenActividadHoy
        {
            KcalQuemadas = registros.Sum(f => f.Kcal_Quemadas),
            MinutosTotales = registros.Sum(f => f.Tiempo_Ejercicio),
            XpGanado = registros.Sum(f => f.XP),
            Sesiones = registros.Count
        };
    }

    /// <summary>
    /// Calcula la racha actual de días consecutivos con al menos una actividad física.
    /// Lógica: obtiene todas las fechas distintas con actividad, las ordena DESC,
    /// y cuenta cuántos días seguidos hay desde hoy hacia atrás sin ningún hueco.
    /// </summary>
    public async Task<int> ObtenerRachaAsync(int idUsuario)
    {
        // Obtenemos las fechas únicas (solo "yyyy-MM-dd", sin hora) en las que hay actividad
        var filas = await _conexion.QueryAsync<FechaRow>(
            @"SELECT DISTINCT substr(r.Fecha, 1, 10) AS Fecha
          FROM Fisico f
          INNER JOIN RegistroDiario r ON f.ID_Registro = r.ID_Registro
          WHERE r.ID_Usuario = ?
          ORDER BY Fecha DESC",
            idUsuario);

        if (filas.Count == 0) return 0;

        // Convertimos a DateTime para poder restar días fácilmente
        var fechas = filas
            .Select(f => DateTime.Parse(f.Fecha))
            .ToList();

        int racha = 0;
        // Punto de partida: hoy. Si hoy no hay actividad, empezamos desde ayer
        // (permitimos que el usuario todavía no haya entrenado hoy sin romper la racha)
        var diaEsperado = fechas[0].Date == DateTime.Today
            ? DateTime.Today
            : DateTime.Today.AddDays(-1);

        foreach (var fecha in fechas)
        {
            if (fecha.Date == diaEsperado)
            {
                racha++;
                diaEsperado = diaEsperado.AddDays(-1); // siguiente día esperado = el anterior
            }
            else
            {
                break; // hueco encontrado → la racha se rompe
            }
        }

        return racha;
    }

    // Clase auxiliar privada para mapear la columna "Fecha" de la query de racha
    // SQLite-net necesita una clase con propiedades para QueryAsync<T>
    private class FechaRow
    {
        public string Fecha { get; set; }
    }

    public async Task<List<Fisico>> ObtenerHistorialFisicoAsync(int idUsuario)
    {
        return await _conexion.QueryAsync<Fisico>(
            @"SELECT f.* FROM Fisico f
          INNER JOIN RegistroDiario r ON f.ID_Registro = r.ID_Registro
          WHERE r.ID_Usuario = ?
          ORDER BY r.ID_Registro DESC
          LIMIT 10", idUsuario);
    }
}