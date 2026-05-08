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
        // ── PROGRESO RETO ────────────────────────────────────────────────────────
        var columnasProgreso = await _conexion.QueryAsync<ColumnInfo>(
            "PRAGMA table_info(ProgresoReto);");
        var nombresProgreso = columnasProgreso.Select(c => c.Name).ToHashSet();

        if (!nombresProgreso.Contains("ID_Reto"))
            await _conexion.ExecuteAsync(
                "ALTER TABLE ProgresoReto ADD COLUMN ID_Reto INTEGER NOT NULL DEFAULT 0;");

        if (!nombresProgreso.Contains("FechaInicio"))
            await _conexion.ExecuteAsync(
                "ALTER TABLE ProgresoReto ADD COLUMN FechaInicio TEXT NOT NULL DEFAULT '';");

        if (!nombresProgreso.Contains("FechaFin"))
            await _conexion.ExecuteAsync(
                "ALTER TABLE ProgresoReto ADD COLUMN FechaFin TEXT DEFAULT '';");

        // ── FISICO ───────────────────────────────────────────────────────────────
        var columnas = await _conexion.QueryAsync<ColumnInfo>(
            "PRAGMA table_info(Fisico);");
        var nombres = columnas.Select(c => c.Name).ToHashSet();

        bool necesitaRecrear = !nombres.Contains("XP")
                            || !nombres.Contains("Tipo_Actividad")
                            || !nombres.Contains("Distancia")
                            || !nombres.Contains("Kcal_Quemadas")
                            || !nombres.Contains("Tiempo_Ejercicio");

        if (necesitaRecrear)
        {
            await _conexion.ExecuteAsync("PRAGMA foreign_keys = OFF;");
            await _conexion.ExecuteAsync("ALTER TABLE Fisico RENAME TO Fisico_old;");
            await _conexion.ExecuteAsync(@"
            CREATE TABLE Fisico (
                ID_Registro      INTEGER PRIMARY KEY,
                Distancia        REAL    NOT NULL DEFAULT 0,
                Tipo_Actividad   TEXT    NOT NULL DEFAULT '',
                XP               INTEGER NOT NULL DEFAULT 0,
                Kcal_Quemadas    REAL    NOT NULL DEFAULT 0,
                Tiempo_Ejercicio REAL    NOT NULL DEFAULT 0,
                Pasos            INTEGER NOT NULL DEFAULT 0,
                FOREIGN KEY (ID_Registro) REFERENCES RegistroDiario(ID_Registro)
            );");
            await _conexion.ExecuteAsync(@"
            INSERT INTO Fisico (ID_Registro, Distancia, Tipo_Actividad, XP,
                                Kcal_Quemadas, Tiempo_Ejercicio, Pasos)
            SELECT ID_Registro,
                   COALESCE(Distancia, 0),
                   COALESCE(Tipo_Actividad, ''),
                   COALESCE(XP, 0),
                   COALESCE(Kcal_Quemadas, 0),
                   COALESCE(Tiempo_Ejercicio, 0),
                   COALESCE(Pasos, 0)
            FROM Fisico_old;");
            await _conexion.ExecuteAsync("DROP TABLE Fisico_old;");
            await _conexion.ExecuteAsync("PRAGMA foreign_keys = ON;");
        }
        // ── MENSTRUACIÓN ───────────────────────────────────────────────────────────────
        var columnasMenstruacion = await _conexion.QueryAsync<ColumnInfo>("PRAGMA table_info(Menstruacion);");
        var nombresMenstruacion = columnasMenstruacion.Select(c => c.Name).ToHashSet();
        if (!nombresMenstruacion.Contains("Fase"))
            await _conexion.ExecuteAsync(
                "ALTER TABLE Menstruacion ADD COLUMN Fase TEXT NOT NULL DEFAULT '';");

        if (!nombresMenstruacion.Contains("Notas"))
            await _conexion.ExecuteAsync(
                "ALTER TABLE Menstruacion ADD COLUMN Notas TEXT DEFAULT '';");

        if (!nombresMenstruacion.Contains("Duracion_Ciclo"))
            await _conexion.ExecuteAsync(
                "ALTER TABLE Menstruacion ADD COLUMN Duracion_Ciclo INTEGER NOT NULL DEFAULT 28;");

        if (!nombresMenstruacion.Contains("Duracion_Periodo"))
            await _conexion.ExecuteAsync(
                "ALTER TABLE Menstruacion ADD COLUMN Duracion_Periodo INTEGER NOT NULL DEFAULT 5;");
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
                ID_Progreso INTEGER PRIMARY KEY AUTOINCREMENT,
                ID_Usuario INTEGER NOT NULL,
                ID_Reto INTEGER NOT NULL,
                FechaInicio TEXT NOT NULL DEFAULT '',
                FechaFin TEXT DEFAULT '',
                progreso INTEGER NOT NULL DEFAULT 0,
                Estado TEXT NOT NULL DEFAULT '',
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
                Fase TEXT NOT NULL DEFAULT '',
                Estado_Animo TEXT NOT NULL DEFAULT '',
                Sintomas TEXT NOT NULL DEFAULT '',
                Notas TEXT DEFAULT '',
                Duracion_Ciclo INTEGER NOT NULL DEFAULT 28,
                Duracion_Periodo INTEGER NOT NULL DEFAULT 5,
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
        var count = await _conexion.Table<Retos>().CountAsync();
        if (count > 0) return;

        var retos = new List<Retos>
    {
        // ── ACTIVIDAD FÍSICA (ID_Categoria = 1) ─────────────────────────
        new Retos { Nombre = "Primer paso",             Descripcion = "Registra tu primera actividad física",         Puntos_Recompensa = 10,  ID_Categoria = 1, Objetivo = 1 },
        new Retos { Nombre = "Caminar 5 km",            Descripcion = "Acumula 5 km caminando",                       Puntos_Recompensa = 15,  ID_Categoria = 1, Objetivo = 5 },
        new Retos { Nombre = "Caminar 10 km",           Descripcion = "Acumula 10 km caminando",                      Puntos_Recompensa = 25,  ID_Categoria = 1, Objetivo = 10 },
        new Retos { Nombre = "Caminar 25 km",           Descripcion = "Acumula 25 km caminando",                      Puntos_Recompensa = 40,  ID_Categoria = 1, Objetivo = 25 },
        new Retos { Nombre = "Caminar 50 km",           Descripcion = "Acumula 50 km caminando",                      Puntos_Recompensa = 60,  ID_Categoria = 1, Objetivo = 50 },
        new Retos { Nombre = "Caminar 100 km",          Descripcion = "Acumula 100 km caminando",                     Puntos_Recompensa = 100, ID_Categoria = 1, Objetivo = 100 },
        new Retos { Nombre = "Correr 1 km",             Descripcion = "Corre al menos 1 km",                          Puntos_Recompensa = 10,  ID_Categoria = 1, Objetivo = 1 },
        new Retos { Nombre = "Correr 5 km",             Descripcion = "Corre al menos 5 km",                          Puntos_Recompensa = 30,  ID_Categoria = 1, Objetivo = 5 },
        new Retos { Nombre = "Correr 10 km",            Descripcion = "Corre 10 km de una vez",                       Puntos_Recompensa = 60,  ID_Categoria = 1, Objetivo = 10 },
        new Retos { Nombre = "Correr 21 km",            Descripcion = "Completa una media maratón",                   Puntos_Recompensa = 120, ID_Categoria = 1, Objetivo = 21 },
        new Retos { Nombre = "Correr 42 km",            Descripcion = "Completa una maratón completa",                Puntos_Recompensa = 250, ID_Categoria = 1, Objetivo = 42 },
        new Retos { Nombre = "Ciclismo 10 km",          Descripcion = "Pedalea al menos 10 km",                       Puntos_Recompensa = 20,  ID_Categoria = 1, Objetivo = 10 },
        new Retos { Nombre = "Ciclismo 25 km",          Descripcion = "Pedalea al menos 25 km",                       Puntos_Recompensa = 40,  ID_Categoria = 1, Objetivo = 25 },
        new Retos { Nombre = "Ciclismo 50 km",          Descripcion = "Pedalea al menos 50 km",                       Puntos_Recompensa = 70,  ID_Categoria = 1, Objetivo = 50 },
        new Retos { Nombre = "Ciclismo 100 km",         Descripcion = "Completa una ruta de 100 km",                  Puntos_Recompensa = 150, ID_Categoria = 1, Objetivo = 100 },
        new Retos { Nombre = "Nadar 500 m",             Descripcion = "Nada al menos 500 metros",                     Puntos_Recompensa = 20,  ID_Categoria = 1, Objetivo = 0.5 },
        new Retos { Nombre = "Nadar 1 km",              Descripcion = "Nada al menos 1 km",                           Puntos_Recompensa = 35,  ID_Categoria = 1, Objetivo = 1 },
        new Retos { Nombre = "Nadar 5 km",              Descripcion = "Acumula 5 km nadando",                         Puntos_Recompensa = 80,  ID_Categoria = 1, Objetivo = 5 },
        new Retos { Nombre = "30 min de ejercicio",     Descripcion = "Haz 30 minutos de actividad física",           Puntos_Recompensa = 15,  ID_Categoria = 1, Objetivo = 30 },
        new Retos { Nombre = "1 hora de ejercicio",     Descripcion = "Haz 60 minutos de actividad física",           Puntos_Recompensa = 30,  ID_Categoria = 1, Objetivo = 60 },
        new Retos { Nombre = "2 horas de ejercicio",    Descripcion = "Acumula 120 minutos de actividad",             Puntos_Recompensa = 50,  ID_Categoria = 1, Objetivo = 120 },
        new Retos { Nombre = "Semana activa",           Descripcion = "Registra actividad 7 días seguidos",           Puntos_Recompensa = 70,  ID_Categoria = 1, Objetivo = 7 },
        new Retos { Nombre = "Mes activo",              Descripcion = "Registra actividad 30 días seguidos",          Puntos_Recompensa = 200, ID_Categoria = 1, Objetivo = 30 },
        new Retos { Nombre = "Quemar 500 kcal",         Descripcion = "Quema 500 kcal en una sesión",                 Puntos_Recompensa = 40,  ID_Categoria = 1, Objetivo = 500 },
        new Retos { Nombre = "Quemar 1000 kcal",        Descripcion = "Quema 1000 kcal en un día",                    Puntos_Recompensa = 80,  ID_Categoria = 1, Objetivo = 1000 },
        new Retos { Nombre = "Quemar 5000 kcal",        Descripcion = "Acumula 5000 kcal quemadas",                   Puntos_Recompensa = 150, ID_Categoria = 1, Objetivo = 5000 },
        new Retos { Nombre = "Yoga 1 vez",              Descripcion = "Completa una sesión de yoga",                  Puntos_Recompensa = 15,  ID_Categoria = 1, Objetivo = 1 },
        new Retos { Nombre = "Yoga 10 veces",           Descripcion = "Completa 10 sesiones de yoga",                 Puntos_Recompensa = 60,  ID_Categoria = 1, Objetivo = 10 },
        new Retos { Nombre = "Yoga 30 veces",           Descripcion = "Completa 30 sesiones de yoga",                 Puntos_Recompensa = 150, ID_Categoria = 1, Objetivo = 30 },
        new Retos { Nombre = "Gimnasio 1 vez",          Descripcion = "Ve al gimnasio por primera vez",               Puntos_Recompensa = 15,  ID_Categoria = 1, Objetivo = 1 },
        new Retos { Nombre = "Gimnasio 10 veces",       Descripcion = "Ve al gimnasio 10 veces",                      Puntos_Recompensa = 60,  ID_Categoria = 1, Objetivo = 10 },
        new Retos { Nombre = "Gimnasio 30 veces",       Descripcion = "Ve al gimnasio 30 veces",                      Puntos_Recompensa = 150, ID_Categoria = 1, Objetivo = 30 },
        new Retos { Nombre = "Bailar 1 vez",            Descripcion = "Registra una sesión de baile",                 Puntos_Recompensa = 15,  ID_Categoria = 1, Objetivo = 1 },
        new Retos { Nombre = "Bailar 10 veces",         Descripcion = "Registra 10 sesiones de baile",                Puntos_Recompensa = 50,  ID_Categoria = 1, Objetivo = 10 },
        new Retos { Nombre = "Estirar cada día",        Descripcion = "Registra 7 sesiones de estiramiento",          Puntos_Recompensa = 30,  ID_Categoria = 1, Objetivo = 7 },
        new Retos { Nombre = "Sin excusas",             Descripcion = "Registra 3 actividades en un día",             Puntos_Recompensa = 50,  ID_Categoria = 1, Objetivo = 3 },
        new Retos { Nombre = "Madrugadora",             Descripcion = "Registra 5 actividades antes de las 9h",       Puntos_Recompensa = 60,  ID_Categoria = 1, Objetivo = 5 },

        // ── SUEÑO (ID_Categoria = 2) ─────────────────────────────────────
        new Retos { Nombre = "Primera noche",           Descripcion = "Registra tus horas de sueño por primera vez",  Puntos_Recompensa = 10,  ID_Categoria = 2, Objetivo = 1 },
        new Retos { Nombre = "Dormir bien",             Descripcion = "Duerme 8 horas una noche",                     Puntos_Recompensa = 15,  ID_Categoria = 2, Objetivo = 8 },
        new Retos { Nombre = "Semana de sueño",         Descripcion = "Registra tu sueño 7 días seguidos",            Puntos_Recompensa = 40,  ID_Categoria = 2, Objetivo = 7 },
        new Retos { Nombre = "Mes de sueño",            Descripcion = "Registra tu sueño 30 días seguidos",           Puntos_Recompensa = 120, ID_Categoria = 2, Objetivo = 30 },
        new Retos { Nombre = "Sueño perfecto",          Descripcion = "Duerme entre 7-9 horas 5 noches seguidas",     Puntos_Recompensa = 60,  ID_Categoria = 2, Objetivo = 5 },
        new Retos { Nombre = "Sin trasnochar",          Descripcion = "Acuéstate antes de las 23h durante 7 días",    Puntos_Recompensa = 50,  ID_Categoria = 2, Objetivo = 7 },
        new Retos { Nombre = "100 noches registradas",  Descripcion = "Registra 100 noches de sueño",                 Puntos_Recompensa = 100, ID_Categoria = 2, Objetivo = 100 },

        // ── NUTRICIÓN (ID_Categoria = 3) ─────────────────────────────────
        new Retos { Nombre = "Primera comida",          Descripcion = "Registra tu primera comida",                   Puntos_Recompensa = 10,  ID_Categoria = 3, Objetivo = 1 },
        new Retos { Nombre = "Registro diario",         Descripcion = "Registra comidas 7 días seguidos",             Puntos_Recompensa = 40,  ID_Categoria = 3, Objetivo = 7 },
        new Retos { Nombre = "Mes saludable",           Descripcion = "Registra comidas 30 días seguidos",            Puntos_Recompensa = 120, ID_Categoria = 3, Objetivo = 30 },
        new Retos { Nombre = "500 kcal en un día",      Descripcion = "Registra al menos 500 kcal en un día",         Puntos_Recompensa = 10,  ID_Categoria = 3, Objetivo = 500 },
        new Retos { Nombre = "Control calórico",        Descripcion = "Registra menos de 2000 kcal en un día",        Puntos_Recompensa = 20,  ID_Categoria = 3, Objetivo = 2000 },
        new Retos { Nombre = "Proteínas al día",        Descripcion = "Registra más de 50g de proteínas en un día",   Puntos_Recompensa = 25,  ID_Categoria = 3, Objetivo = 50 },
        new Retos { Nombre = "10 alimentos distintos",  Descripcion = "Registra 10 alimentos diferentes",             Puntos_Recompensa = 30,  ID_Categoria = 3, Objetivo = 10 },
        new Retos { Nombre = "50 alimentos distintos",  Descripcion = "Registra 50 alimentos diferentes",             Puntos_Recompensa = 80,  ID_Categoria = 3, Objetivo = 50 },
        new Retos { Nombre = "100 registros de comida", Descripcion = "Registra 100 comidas en total",                Puntos_Recompensa = 100, ID_Categoria = 3, Objetivo = 100 },
        new Retos { Nombre = "Escáner de comida",       Descripcion = "Escanea 5 productos con el escáner",           Puntos_Recompensa = 20,  ID_Categoria = 3, Objetivo = 5 },
        new Retos { Nombre = "Escáner experta",         Descripcion = "Escanea 25 productos con el escáner",          Puntos_Recompensa = 60,  ID_Categoria = 3, Objetivo = 25 },

        // ── HIDRATACIÓN (ID_Categoria = 4) ───────────────────────────────
        new Retos { Nombre = "Primera gota",            Descripcion = "Registra tu primer vaso de agua",              Puntos_Recompensa = 10,  ID_Categoria = 4, Objetivo = 1 },
        new Retos { Nombre = "2 litros al día",         Descripcion = "Bebe 2 litros de agua en un día",              Puntos_Recompensa = 20,  ID_Categoria = 4, Objetivo = 2 },
        new Retos { Nombre = "Hidratación semanal",     Descripcion = "Registra agua 7 días seguidos",                Puntos_Recompensa = 40,  ID_Categoria = 4, Objetivo = 7 },
        new Retos { Nombre = "Hidratación mensual",     Descripcion = "Registra agua 30 días seguidos",               Puntos_Recompensa = 100, ID_Categoria = 4, Objetivo = 30 },
        new Retos { Nombre = "Super hidratada",         Descripcion = "Bebe 3 litros en un día",                      Puntos_Recompensa = 30,  ID_Categoria = 4, Objetivo = 3 },

        // ── BIENESTAR (ID_Categoria = 5) ─────────────────────────────────
        new Retos { Nombre = "Bienvenida",              Descripcion = "Completa tu perfil por primera vez",           Puntos_Recompensa = 20,  ID_Categoria = 5, Objetivo = 1 },
        new Retos { Nombre = "Racha de 3 días",         Descripcion = "Mantén una racha de 3 días activos",           Puntos_Recompensa = 20,  ID_Categoria = 5, Objetivo = 3 },
        new Retos { Nombre = "Racha de 7 días",         Descripcion = "Mantén una racha de 7 días activos",           Puntos_Recompensa = 50,  ID_Categoria = 5, Objetivo = 7 },
        new Retos { Nombre = "Racha de 14 días",        Descripcion = "Mantén una racha de 14 días activos",          Puntos_Recompensa = 100, ID_Categoria = 5, Objetivo = 14 },
        new Retos { Nombre = "Racha de 30 días",        Descripcion = "Mantén una racha de 30 días activos",          Puntos_Recompensa = 200, ID_Categoria = 5, Objetivo = 30 },
        new Retos { Nombre = "Racha de 100 días",       Descripcion = "Mantén una racha de 100 días activos",         Puntos_Recompensa = 500, ID_Categoria = 5, Objetivo = 100 },
        new Retos { Nombre = "100 XP",                  Descripcion = "Acumula 100 puntos de XP",                     Puntos_Recompensa = 10,  ID_Categoria = 5, Objetivo = 100 },
        new Retos { Nombre = "500 XP",                  Descripcion = "Acumula 500 puntos de XP",                     Puntos_Recompensa = 25,  ID_Categoria = 5, Objetivo = 500 },
        new Retos { Nombre = "1000 XP",                 Descripcion = "Acumula 1000 puntos de XP",                    Puntos_Recompensa = 50,  ID_Categoria = 5, Objetivo = 1000 },
        new Retos { Nombre = "5000 XP",                 Descripcion = "Acumula 5000 puntos de XP",                    Puntos_Recompensa = 150, ID_Categoria = 5, Objetivo = 5000 },
        new Retos { Nombre = "Exploradora",             Descripcion = "Visita todas las secciones de la app",         Puntos_Recompensa = 30,  ID_Categoria = 5, Objetivo = 5 },

        // ── SALUD MENTAL (ID_Categoria = 6) ──────────────────────────────
        new Retos { Nombre = "Primera emoción",         Descripcion = "Registra tu estado de ánimo por primera vez",  Puntos_Recompensa = 10,  ID_Categoria = 6, Objetivo = 1 },
        new Retos { Nombre = "Diario semanal",          Descripcion = "Registra tu estado de ánimo 7 días seguidos",  Puntos_Recompensa = 40,  ID_Categoria = 6, Objetivo = 7 },
        new Retos { Nombre = "Diario mensual",          Descripcion = "Registra tu estado de ánimo 30 días seguidos", Puntos_Recompensa = 120, ID_Categoria = 6, Objetivo = 30 },
        new Retos { Nombre = "Día increíble",           Descripcion = "Registra un estado de ánimo 'Increíble'",      Puntos_Recompensa = 15,  ID_Categoria = 6, Objetivo = 1 },
        new Retos { Nombre = "Semana positiva",         Descripcion = "Registra 7 días con estado positivo",          Puntos_Recompensa = 60,  ID_Categoria = 6, Objetivo = 7 },
        new Retos { Nombre = "100 registros mentales",  Descripcion = "Registra tu estado de ánimo 100 veces",        Puntos_Recompensa = 100, ID_Categoria = 6, Objetivo = 100 },
        new Retos { Nombre = "Escritora",               Descripcion = "Escribe una nota en el diario 10 veces",       Puntos_Recompensa = 40,  ID_Categoria = 6, Objetivo = 10 },
        new Retos { Nombre = "Mindfulness",             Descripcion = "Registra 5 sesiones de yoga o estiramiento",   Puntos_Recompensa = 35,  ID_Categoria = 6, Objetivo = 5 },
    };

        await _conexion.InsertAllAsync(retos);
    }

    public async Task<List<RetoConProgreso>> ObtenerRetosPageAsync(
    int idUsuario, int offset = 0, int limit = 20)
    {
        return await _conexion.QueryAsync<RetoConProgreso>(@"
        SELECT
            r.ID_Reto,
            r.Nombre,
            r.Descripcion,
            r.Puntos_Recompensa,
            r.Objetivo,
            r.ID_Categoria,
            c.Nombre                        AS NombreCategoria,
            COALESCE(p.ID_Progreso, 0)      AS ID_Progreso,
            COALESCE(p.progreso, 0)         AS ProgresoActual,
            COALESCE(p.Estado, 'Pendiente') AS Estado
        FROM Retos r
        LEFT JOIN Categorias c ON r.ID_Categoria = c.ID_Categoria
        LEFT JOIN ProgresoReto p
               ON p.ID_Reto = r.ID_Reto AND p.ID_Usuario = ?
        ORDER BY
            CASE COALESCE(p.Estado, 'Pendiente')
                WHEN 'En progreso' THEN 1
                WHEN 'Pendiente'   THEN 2
                WHEN 'Completado'  THEN 3
            END,
            r.Puntos_Recompensa DESC
        LIMIT ? OFFSET ?",
            idUsuario, limit, offset);
    }

    public async Task AceptarRetoAsync(int idUsuario, int idReto)
    {
        var existente = await _conexion.Table<ProgresoReto>()
            .FirstOrDefaultAsync(p => p.ID_Usuario == idUsuario && p.ID_Reto == idReto);

        if (existente != null) return; // ya aceptado

        var progreso = new ProgresoReto
        {
            ID_Usuario = idUsuario,
            ID_Reto = idReto,
            progreso = 0,
            Estado = "En progreso"
        };
        progreso.SetFechaInicio(DateTime.Now);
        await _conexion.InsertAsync(progreso);
    }
    public async Task<(int Completados, int EnProgreso, int Pendientes)>
    ObtenerResumenRetosAsync(int idUsuario)
    {
        var completados = await _conexion.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM ProgresoReto WHERE ID_Usuario = ? AND Estado = 'Completado'",
            idUsuario);

        var enProgreso = await _conexion.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM ProgresoReto WHERE ID_Usuario = ? AND Estado = 'En progreso'",
            idUsuario);

        var totalRetos = await _conexion.Table<Retos>().CountAsync();
        var pendientes = totalRetos - completados - enProgreso;

        return (completados, enProgreso, pendientes);
    }


    public async Task<List<RetoConProgreso>> ComprobarRetosFisicoAsync(
    int idUsuario, Fisico fisico)
    {

        var retosCompletadosAhora = new List<RetoConProgreso>();

        // Traemos todos los retos que el usuario tiene en progreso
        var retosEnProgreso = await _conexion.QueryAsync<RetoConProgreso>(@"
        SELECT
            r.ID_Reto, r.Nombre, r.Descripcion,
            r.Puntos_Recompensa, r.Objetivo, r.ID_Categoria,
            p.ID_Progreso, p.progreso AS ProgresoActual, p.Estado
        FROM ProgresoReto p
        INNER JOIN Retos r ON r.ID_Reto = p.ID_Reto
        WHERE p.ID_Usuario = ? AND p.Estado = 'En progreso'",
            idUsuario);

        foreach (var reto in retosEnProgreso)
        {
            double incremento = CalcularIncrementoReto(reto, fisico);
            if (incremento <= 0) continue;

            double nuevoProgreso = reto.ProgresoActual + incremento;
            bool completado = nuevoProgreso >= reto.Objetivo;

            if (completado)
            {
                // Marcamos como completado y damos los puntos de recompensa
                await _conexion.ExecuteAsync(@"
                UPDATE ProgresoReto
                SET progreso = ?, Estado = 'Completado', FechaFin = ?
                WHERE ID_Progreso = ?",
                    reto.Objetivo,
                    DateTime.Now.ToString("yyyy-MM-dd"),
                    reto.ID_Progreso);

                await SumarXPAsync(idUsuario, reto.Puntos_Recompensa);
                retosCompletadosAhora.Add(reto);
            }
            else
            {
                // Solo actualizamos el progreso
                await _conexion.ExecuteAsync(@"
                UPDATE ProgresoReto SET progreso = ?
                WHERE ID_Progreso = ?",
                    nuevoProgreso, reto.ID_Progreso);
            }
        }

        return retosCompletadosAhora;
    }

    public async Task<List<RetoConProgreso>> ObtenerRetosEnProgresoAsync(int idUsuario)
    {
        return await _conexion.QueryAsync<RetoConProgreso>(@"
        SELECT
            r.ID_Reto,
            r.Nombre,
            r.Descripcion,
            r.Puntos_Recompensa,
            r.Objetivo,
            r.ID_Categoria,
            c.Nombre AS NombreCategoria,
            p.ID_Progreso,          
            p.progreso AS ProgresoActual,
            p.Estado
        FROM ProgresoReto p
        INNER JOIN Retos r ON r.ID_Reto = p.ID_Reto
        LEFT JOIN Categorias c ON r.ID_Categoria = c.ID_Categoria
        WHERE p.ID_Usuario = ? AND p.Estado = 'En progreso'
        ORDER BY p.progreso DESC",
            idUsuario);
    }

    private double CalcularIncrementoReto(RetoConProgreso reto, Fisico fisico)
    {
        // Categoría 1 = Actividad Física
        if (reto.ID_Categoria != 1) return 0;

        string nombre = reto.Nombre.ToLower();

        // Retos de distancia — solo cuentan actividades con distancia
        if (nombre.Contains("km") && (
            nombre.Contains("caminar") || nombre.Contains("correr") ||
            nombre.Contains("ciclismo") || nombre.Contains("nadar")))
        {
            // Verificamos que la actividad sea del tipo correcto
            if (nombre.Contains("caminar") && fisico.Tipo_Actividad != "Caminar") return 0;
            if (nombre.Contains("correr") && fisico.Tipo_Actividad != "Correr") return 0;
            if (nombre.Contains("ciclismo") && fisico.Tipo_Actividad != "Ciclismo") return 0;
            if (nombre.Contains("nadar") && fisico.Tipo_Actividad != "Natación") return 0;
            return fisico.Distancia;
        }

        // Retos de tiempo (minutos)
        if (nombre.Contains("min") || nombre.Contains("hora"))
            return fisico.Tiempo_Ejercicio;

        // Retos de kcal quemadas
        if (nombre.Contains("kcal") || nombre.Contains("quemar"))
            return fisico.Kcal_Quemadas;

        // Retos de sesiones (yoga, gimnasio, baile, estiramiento, actividades)
        if (nombre.Contains("yoga") && fisico.Tipo_Actividad == "Yoga") return 1;
        if (nombre.Contains("gimnasio") && fisico.Tipo_Actividad == "Gimnasio") return 1;
        if (nombre.Contains("bail") && fisico.Tipo_Actividad == "Baile") return 1;
        if (nombre.Contains("estirar") || nombre.Contains("estiramiento"))
        {
            if (fisico.Tipo_Actividad == "Estiramiento") return 1;
            return 0;
        }

        // Retos de racha — los gestiona ObtenerRachaAsync, no se incrementan aquí
        if (nombre.Contains("racha")) return 0;

        // Reto "primer paso" y "sin excusas" — cualquier actividad cuenta
        if (nombre.Contains("primer paso")) return 1;
        if (nombre.Contains("sin excusas")) return 1;
        if (nombre.Contains("actividad")) return 1;

        return 0;
    }

    public class RetoConProgreso
    {
        public int ID_Reto { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Puntos_Recompensa { get; set; }
        public double Objetivo { get; set; }
        public int ID_Categoria { get; set; }
        public int ID_Progreso { get; set; }
        public string NombreCategoria { get; set; }
        public double ProgresoActual { get; set; }
        public string Estado { get; set; }

        // Propiedades calculadas para el binding en XAML
        [SQLite.Ignore]
        public double PorcentajeProgreso =>
            Objetivo > 0 ? Math.Min(ProgresoActual / Objetivo, 1.0) : 0;

        [SQLite.Ignore]
        public string TextoProgreso =>
            $"{ProgresoActual:F0} / {Objetivo:F0}";

        [SQLite.Ignore]
        public bool EstaCompletado => Estado == "Completado";

        [SQLite.Ignore]
        public bool EstaEnProgreso => Estado == "En progreso";

        [SQLite.Ignore]
        public bool EstaPendiente => Estado == "Pendiente";

        [SQLite.Ignore]
        public Color ColorEstado => Estado switch
        {
            "Completado" => Color.FromArgb("#8EB497"),
            "En progreso" => Color.FromArgb("#F5C842"),
            _ => Color.FromArgb("#C0C0C0")
        };

        [SQLite.Ignore]
        public string EmojiCategoria => ID_Categoria switch
        {
            1 => "🏃",
            2 => "😴",
            3 => "🥗",
            4 => "💧",
            5 => "🌱",
            6 => "🧠",
            _ => "⭐"
        };

        [SQLite.Ignore]
        public string TextoBoton => Estado switch
        {
            "Completado" => "✅ Completado",
            "En progreso" => "En curso",
            _ => "Aceptar reto"
        };
    }
    public async Task CrearAvatarAsync()
    {
        Avatar avatar = new Avatar
        {
            ID_Usuario = 1, // Aquí deberías poner el ID del usuario actual
            Nivel_Evolucion = 2,
            XP = 15,
            Estado_Salud = Avatar.Tipos_Estados_Salud[1]
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
        Avatar avatar = await _conexion.Table<Avatar>()
        .Where(a => a.ID_Usuario == idUsuario)
        .FirstOrDefaultAsync();
        if (avatar == null || avatar.XP < 100) return;
        if (avatar.Nivel_Evolucion < 3)
        {
            await _conexion.ExecuteAsync(
                "UPDATE Avatar SET XP = 0, Nivel_Evolucion = Nivel_Evolucion + 1 WHERE ID_Usuario = ?",
                idUsuario);
        }
        else
        {
            // Ya está en nivel 3 (florecida): dejamos el XP capado a 100
            await _conexion.ExecuteAsync(
                "UPDATE Avatar SET XP = 100 WHERE ID_Usuario = ?",
                idUsuario);
        }

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
    //mental
    public async Task GuardarRegistroAsync(Mental registro)
    {

        await _conexion.InsertAsync(registro);
    }
    /// <summary>
    /// Devuelve el registro Mental de hoy para el usuario, o null si no existe.
    /// </summary>
    public async Task<Mental?> ObtenerMentalHoyAsync(int idUsuario)
    {
        var hoy = DateTime.Now.ToString("yyyy-MM-dd");

        var resultado = await _conexion.QueryAsync<Mental>(
            @"SELECT m.* FROM Mental m
            INNER JOIN RegistroDiario r ON m.ID_Registro = r.ID_Registro
            WHERE r.ID_Usuario = ? AND r.Fecha LIKE ?",
            idUsuario, hoy + "%");

        return resultado.FirstOrDefault();
    }

    /// <summary>
    /// Devuelve los registros Mental de los últimos 7 días con su fecha.
    /// </summary>
    public class MentalConFecha
    {
        public string Estado_Animo { get; set; } = "";
        public double Horas_Sueno { get; set; }
        public string Fecha { get; set; } = "";
    }

    public async Task<List<MentalConFecha>> ObtenerMentalSemanaAsync(int idUsuario)
    {
        var hace7dias = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd");

        return await _conexion.QueryAsync<MentalConFecha>(
            @"SELECT m.Estado_Animo, m.Horas_Sueno, substr(r.Fecha, 1, 10) AS Fecha
            FROM Mental m
            INNER JOIN RegistroDiario r ON m.ID_Registro = r.ID_Registro
            WHERE r.ID_Usuario = ? AND r.Fecha >= ?
            ORDER BY r.Fecha ASC",
            idUsuario, hace7dias);
    }

    // ─── CICLO MENSTRUAL ────────────────────────────────────────────────

    /// Guarda (o actualiza) el registro menstrual de HOY para la usuaria.
    public async Task<Menstruacion> GuardarMenstruacionAsync(
        int idUsuario,
        string fechaInicioCiclo,
        string fase,
        string estadoAnimo,
        List<string> sintomas,
        string notas,
        int duracionCiclo = 28,
        int duracionPeriodo = 5)
    {
        await InicializarAsync();

        var hoy = DateTime.Now.ToString("yyyy-MM-dd");

        // Buscamos si ya hay un RegistroDiario de hoy para esta usuaria.
        // Si no, lo creamos (toda fila de Menstruacion necesita un ID_Registro padre).
        var registroHoy = await _conexion.QueryAsync<RegistroDiario>(
            @"SELECT * FROM RegistroDiario
            WHERE ID_Usuario = ? AND Fecha LIKE ?
            LIMIT 1",
            idUsuario, hoy + "%");

        int idRegistro;
        if (registroHoy.Count == 0)
        {
            var nuevo = new RegistroDiario
            {
                ID_Usuario = idUsuario,
                Fecha = hoy
            };
            await _conexion.InsertAsync(nuevo);
            idRegistro = nuevo.ID_Registro;
        }
        else
        {
            idRegistro = registroHoy[0].ID_Registro;
        }

        // Comprobamos si ya hay un Menstruacion enganchado a ese ID_Registro
        var existente = await _conexion.FindAsync<Menstruacion>(idRegistro);

        var menstruacion = new Menstruacion
        {
            ID_Registro = idRegistro,
            Fecha_Inicio_Ciclo = fechaInicioCiclo,
            Fase = fase ?? "",
            Estado_Animo = estadoAnimo ?? "",
            Sintomas = sintomas != null ? string.Join(",", sintomas) : "",
            Notas = notas ?? "",
            Duracion_Ciclo = duracionCiclo,
            Duracion_Periodo = duracionPeriodo
        };

        if (existente == null)
            await _conexion.InsertAsync(menstruacion);
        else
            await _conexion.UpdateAsync(menstruacion);

        return menstruacion;
    }

    /// Devuelve el registro de Menstruacion de HOY (o null si no existe).
    public async Task<Menstruacion?> ObtenerMenstruacionHoyAsync(int idUsuario)
    {
        await InicializarAsync();
        var hoy = DateTime.Now.ToString("yyyy-MM-dd");

        var resultado = await _conexion.QueryAsync<Menstruacion>(
            @"SELECT m.* FROM Menstruacion m
            INNER JOIN RegistroDiario r ON m.ID_Registro = r.ID_Registro
            WHERE r.ID_Usuario = ? AND r.Fecha LIKE ?",
            idUsuario, hoy + "%");

        return resultado.FirstOrDefault();
    }

    /// Devuelve el ÚLTIMO registro menstrual de la usuaria (el más reciente),
    /// sea de hoy o no. Sirve para saber cuándo empezó el ciclo actual y poder
    /// calcular en qué día del ciclo está hoy.
    public async Task<Menstruacion?> ObtenerUltimaMenstruacionAsync(int idUsuario)
    {
        await InicializarAsync();

        var resultado = await _conexion.QueryAsync<Menstruacion>(
            @"SELECT m.* FROM Menstruacion m
            INNER JOIN RegistroDiario r ON m.ID_Registro = r.ID_Registro
            WHERE r.ID_Usuario = ?
            ORDER BY r.Fecha DESC
            LIMIT 1",
            idUsuario);

        return resultado.FirstOrDefault();
    }

    /// Devuelve el historial de registros menstruales de la usuaria
    public async Task<List<Menstruacion>> ObtenerHistorialMenstruacionAsync(int idUsuario)
    {
        await InicializarAsync();

        return await _conexion.QueryAsync<Menstruacion>(
            @"SELECT m.* FROM Menstruacion m
            INNER JOIN RegistroDiario r ON m.ID_Registro = r.ID_Registro
            WHERE r.ID_Usuario = ?
            ORDER BY r.Fecha DESC
            LIMIT 12",
            idUsuario);
    }

    // ─── CÁLCULOS DEL CICLO ────
    /// Datos calculados sobre el estado actual del ciclo menstrual.
    public class EstadoCiclo
    {
        public int DiaActual { get; set; }              // Día 12 de 28
        public int DuracionCiclo { get; set; }          // 28
        public double Progreso { get; set; }            // 0.0 a 1.0 (para la barra)
        public int DiasParaProximoPeriodo { get; set; } // 16
        public string FaseActual { get; set; } = "";    // "Menstruacion", "Folicular"...
        public DateTime FechaInicioCiclo { get; set; }
        public DateTime FechaProximoPeriodo { get; set; }
        public bool TieneDatos { get; set; }            // false = aún no ha registrado nada
    }

    /// Calcula en qué día del ciclo está la usuaria hoy a partir del último registro.
    /// Si no hay datos previos devuelve un EstadoCiclo "vacío" para que la pantalla
    /// muestre valores por defecto.
    public async Task<EstadoCiclo> CalcularEstadoCicloAsync(int idUsuario)
    {
        var ultima = await ObtenerUltimaMenstruacionAsync(idUsuario);

        if (ultima == null || string.IsNullOrEmpty(ultima.Fecha_Inicio_Ciclo))
        {
            return new EstadoCiclo
            {
                TieneDatos = false,
                DuracionCiclo = 28
            };
        }

        var fechaInicio = DateTime.Parse(ultima.Fecha_Inicio_Ciclo);
        int duracion = ultima.Duracion_Ciclo > 0 ? ultima.Duracion_Ciclo : 28;
        int duracionPeriodo = ultima.Duracion_Periodo > 0 ? ultima.Duracion_Periodo : 5;

        // Día del ciclo (1 = primer día de la regla)
        int diasDesdeInicio = (DateTime.Today - fechaInicio.Date).Days;

        // Si han pasado más días que la duración del ciclo, asumimos que ya empezó
        // un ciclo nuevo (la usuaria simplemente no lo ha registrado aún).
        // Lo "rebobinamos" para mostrar el día relativo al ciclo actual estimado.
        int diaActual = (diasDesdeInicio % duracion) + 1;

        int diasParaProximo = duracion - (diasDesdeInicio % duracion);

        return new EstadoCiclo
        {
            TieneDatos = true,
            DiaActual = diaActual,
            DuracionCiclo = duracion,
            Progreso = (double)(diaActual - 1) / duracion,
            DiasParaProximoPeriodo = diasParaProximo,
            FaseActual = CalcularFase(diaActual, duracion, duracionPeriodo),
            FechaInicioCiclo = fechaInicio,
            FechaProximoPeriodo = fechaInicio.AddDays(duracion)
        };
    }

    /// Devuelve la fase del ciclo en función del día actual.
    /// Reglas estándar (basadas en un ciclo de 28 días, escaladas a la duración real):
    ///   - Menstruación: días 1 a 5 (duracionPeriodo)
    ///   - Folicular: del día 6 hasta 2 días antes de la ovulación
    ///   - Ovulación: día 14 ± 2  (centrado en duracion - 14)
    ///   - Lútea: desde el final de la ovulación hasta el final del ciclo
    private static string CalcularFase(int diaActual, int duracionCiclo, int duracionPeriodo)
    {
        // La ovulación ocurre típicamente 14 días ANTES del próximo periodo
        int diaOvulacion = duracionCiclo - 14;
        int inicioOvulacion = diaOvulacion - 2;
        int finOvulacion = diaOvulacion + 2;

        if (diaActual <= duracionPeriodo) return "Menstruacion";
        if (diaActual < inicioOvulacion) return "Folicular";
        if (diaActual <= finOvulacion) return "Ovulacion";
        return "Lutea";
    }
}