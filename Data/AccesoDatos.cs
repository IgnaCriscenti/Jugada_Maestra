using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;
using TableroApuestas.Models;
using static TableroApuestas.Pages.MisApuestas;

namespace TableroApuestas.Data
{
    public class AccesoDatos
    {
        private readonly string connectionString;

        public AccesoDatos(string connectionString)
        {
            this.connectionString = connectionString;
        }



        // Registrar usuario con bcrypt
        public void RegistrarUsuario(string nombre, string apellido, string password)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            using (var connection = new OleDbConnection(connectionString))
            {
                connection.Open();

                string sql = "INSERT INTO usuarios (nombre, apellido, PasswordHash) VALUES (@nombre, @apellido, @hash)";
                using (var command = new OleDbCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", nombre);
                    command.Parameters.AddWithValue("@apellido", apellido);
                    command.Parameters.AddWithValue("@hash", hash);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Validar login con bcrypt
        public int ValidarUsuarioYObtenerId(string nombre, string password)
        {
            using (var connection = new OleDbConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT id_usuario, PasswordHash FROM usuarios WHERE nombre = @nombre";
                using (var command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@nombre", nombre);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string hash = reader["PasswordHash"].ToString()!;
                            if (BCrypt.Net.BCrypt.Verify(password, hash))
                                return Convert.ToInt32(reader["id_usuario"]);
                        }
                    }
                }
            }

            return 0; // 0 indica usuario no válido
        }

        

        public async Task<DataTable> ObtenerDatosDeporteCompletoPorNombreAsync(string nombreDeporte)
        {
            string consulta = "SELECT * FROM deportes WHERE nombre = @nombreDeporte";
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                await connection.OpenAsync();
                using (OleDbCommand command = new OleDbCommand(consulta, connection))
                {
                    command.Parameters.AddWithValue("@nombreDeporte", nombreDeporte);
                    DataTable dataTable = new DataTable();
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                    {
                        await Task.Run(() => adapter.Fill(dataTable));
                    }
                    return dataTable;
                }
            }
        }

        public async Task<decimal> ObtenerMontoTotalDeporteAsync(string nombreDeporte)
        {
            string consulta = @"
            SELECT SUM(ml.monto)
            FROM ((mes_ligas AS ml
            INNER JOIN ligas AS l ON ml.id_liga = l.id_liga)
            INNER JOIN deportes_liga AS dl ON l.id_liga = dl.id_liga)
            INNER JOIN deportes AS d ON dl.id_deporte = d.id_deporte
            WHERE d.nombre = @nombreDeporte";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                await connection.OpenAsync();
                using (OleDbCommand command = new OleDbCommand(consulta, connection))
                {
                    command.Parameters.Add(new OleDbParameter("@nombreDeporte", OleDbType.VarChar)).Value = nombreDeporte;
                    object result = await command.ExecuteScalarAsync();
                    return (result != DBNull.Value) ? Convert.ToDecimal(result) : 0;
                }
            }
        }

        public async Task<DataTable> ObtenerDatosDeportesAsync()
        {
            string consulta = "SELECT id_deporte, nombre, objetivo FROM deportes";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                await connection.OpenAsync();

                using (OleDbCommand command = new OleDbCommand(consulta, connection))
                {
                    DataTable dataTable = new DataTable();

                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                    {
                        await Task.Run(() => adapter.Fill(dataTable));
                    }

                    return dataTable;
                }
            }
        }

        public async Task ActualizarObjetivoDeporteAsync(int idDeporte, decimal nuevoObjetivo)
        {
            string consulta = "UPDATE deportes SET objetivo = @nuevoObjetivo WHERE id_deporte = @idDeporte";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                await connection.OpenAsync();

                using (OleDbCommand command = new OleDbCommand(consulta, connection))
                {
                    command.Parameters.AddWithValue("@nuevoObjetivo", nuevoObjetivo);
                    command.Parameters.AddWithValue("@idDeporte", idDeporte);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<Mes>> ObtenerDatosMesesFutbolAsync()
        {
            string consulta = @"
            SELECT m.id_mes, m.nombre, SUM(ml.monto) AS monto_mes
            FROM (((mes_ligas AS ml
            INNER JOIN ligas AS l ON ml.id_liga = l.id_liga)
            INNER JOIN deportes_liga AS dl ON l.id_liga = dl.id_liga)
            INNER JOIN deportes AS d ON dl.id_deporte = d.id_deporte)
            INNER JOIN meses AS m ON ml.id_mes = m.id_mes
            WHERE d.nombre = 'futbol'
            GROUP BY m.id_mes, m.nombre
            ORDER BY m.id_mes";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                await connection.OpenAsync();
                using (OleDbCommand command = new OleDbCommand(consulta, connection))
                {
                    DataTable dataTable = new DataTable();
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                    List<Mes> meses = new List<Mes>();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        meses.Add(new Mes
                        {
                            Id = Convert.ToInt32(row["id_mes"]),
                            Nombre = row["nombre"].ToString(),
                            Monto = Convert.ToDecimal(row["monto_mes"])
                        });
                    }
                    return meses;
                }
            }
        }

        public async Task<List<Liga>> ObtenerDatosLigasFutbolPorMesAsync(int idMes)
        {
            string consulta = @"
            SELECT l.id_liga, l.nombre, SUM(ml.monto) AS monto_generado
            FROM ((mes_ligas AS ml
            INNER JOIN ligas AS l ON ml.id_liga = l.id_liga)
            INNER JOIN deportes_liga AS dl ON l.id_liga = dl.id_liga)
            INNER JOIN deportes AS d ON dl.id_deporte = d.id_deporte
            WHERE d.nombre = 'futbol' AND ml.id_mes = @idMes
            GROUP BY l.id_liga, l.nombre
            ORDER BY l.id_liga";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                await connection.OpenAsync();
                using (OleDbCommand command = new OleDbCommand(consulta, connection))
                {
                    command.Parameters.AddWithValue("@idMes", idMes);
                    DataTable dataTable = new DataTable();
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                    List<Liga> ligas = new List<Liga>();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        ligas.Add(new Liga
                        {
                            Id = Convert.ToInt32(row["id_liga"]),
                            Nombre = row["nombre"].ToString(),
                            Monto = Convert.ToDecimal(row["monto_generado"])
                        });
                    }
                    return ligas;
                }
            }
        }

        public async Task<List<Mes>> ObtenerDatosMesesBasquetAsync()
        {
            string consulta = @"
        SELECT m.id_mes, m.nombre, SUM(ml.monto) AS monto_mes
        FROM (((mes_ligas AS ml
        INNER JOIN ligas AS l ON ml.id_liga = l.id_liga)
        INNER JOIN deportes_liga AS dl ON l.id_liga = dl.id_liga)
        INNER JOIN deportes AS d ON dl.id_deporte = d.id_deporte)
        INNER JOIN meses AS m ON ml.id_mes = m.id_mes
        WHERE d.nombre = 'basquet'
        GROUP BY m.id_mes, m.nombre
        ORDER BY m.id_mes";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                await connection.OpenAsync();
                using (OleDbCommand command = new OleDbCommand(consulta, connection))
                {
                    DataTable dataTable = new DataTable();
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                    List<Mes> meses = new List<Mes>();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        meses.Add(new Mes
                        {
                            Id = Convert.ToInt32(row["id_mes"]),
                            Nombre = row["nombre"].ToString(),
                            Monto = Convert.ToDecimal(row["monto_mes"])
                        });
                    }
                    return meses;
                }
            }
        }

        public async Task<List<Liga>> ObtenerDatosLigasBasquetPorMesAsync(int idMes)
        {
            string consulta = @"
        SELECT l.id_liga, l.nombre, SUM(ml.monto) AS monto_generado
        FROM ((mes_ligas AS ml
        INNER JOIN ligas AS l ON ml.id_liga = l.id_liga)
        INNER JOIN deportes_liga AS dl ON l.id_liga = dl.id_liga)
        INNER JOIN deportes AS d ON dl.id_deporte = d.id_deporte
        WHERE d.nombre = 'basquet' AND ml.id_mes = @idMes
        GROUP BY l.id_liga, l.nombre
        ORDER BY l.id_liga";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                await connection.OpenAsync();
                using (OleDbCommand command = new OleDbCommand(consulta, connection))
                {
                    command.Parameters.AddWithValue("@idMes", idMes);
                    DataTable dataTable = new DataTable();
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                    List<Liga> ligas = new List<Liga>();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        ligas.Add(new Liga
                        {
                            Id = Convert.ToInt32(row["id_liga"]),
                            Nombre = row["nombre"].ToString(),
                            Monto = Convert.ToDecimal(row["monto_generado"])
                        });
                    }
                    return ligas;
                }
            }
        }

        public async Task<List<Mes>> ObtenerDatosMesesTenisAsync()
        {
            string consulta = @"
            SELECT m.id_mes, m.nombre, SUM(ml.monto) AS monto_mes
            FROM (((mes_ligas AS ml
            INNER JOIN ligas AS l ON ml.id_liga = l.id_liga)
            INNER JOIN deportes_liga AS dl ON l.id_liga = dl.id_liga)
            INNER JOIN deportes AS d ON dl.id_deporte = d.id_deporte)
            INNER JOIN meses AS m ON ml.id_mes = m.id_mes
            WHERE d.nombre = 'tenis'
            GROUP BY m.id_mes, m.nombre
            ORDER BY m.id_mes";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                await connection.OpenAsync();
                using (OleDbCommand command = new OleDbCommand(consulta, connection))
                {
                    DataTable dataTable = new DataTable();
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                    List<Mes> meses = new List<Mes>();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        meses.Add(new Mes
                        {
                            Id = Convert.ToInt32(row["id_mes"]),
                            Nombre = row["nombre"].ToString(),
                            Monto = Convert.ToDecimal(row["monto_mes"])
                        });
                    }
                    return meses;
                }
            }
        }
        public async Task<List<Liga>> ObtenerDatosLigasTenisPorMesAsync(int idMes)
        {
            string consulta = @"
            SELECT l.id_liga, l.nombre, SUM(ml.monto) AS monto_generado
            FROM ((mes_ligas AS ml
            INNER JOIN ligas AS l ON ml.id_liga = l.id_liga)
            INNER JOIN deportes_liga AS dl ON l.id_liga = dl.id_liga)
            INNER JOIN deportes AS d ON dl.id_deporte = d.id_deporte
            WHERE d.nombre = 'tenis' AND ml.id_mes = @idMes
            GROUP BY l.id_liga, l.nombre
            ORDER BY l.id_liga";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                await connection.OpenAsync();
                using (OleDbCommand command = new OleDbCommand(consulta, connection))
                {
                    command.Parameters.AddWithValue("@idMes", idMes);
                    DataTable dataTable = new DataTable();
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                    List<Liga> ligas = new List<Liga>();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        ligas.Add(new Liga
                        {
                            Id = Convert.ToInt32(row["id_liga"]),
                            Nombre = row["nombre"].ToString(),
                            Monto = Convert.ToDecimal(row["monto_generado"])
                        });
                    }
                    return ligas;
                }
            }
        }

        // ==============================================
        // 🔹 VERIFICAR TABLAS Y CAMPOS NECESARIOS
        // ==============================================
        public async Task EnsureSchemaAsync()
        {
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();

            // =============================================================
            // 🔹 CREAR TABLAS BASE
            // =============================================================
            if (!await TableExistsAsync("deportes"))
                new OleDbCommand("CREATE TABLE deportes (id_deporte AUTOINCREMENT PRIMARY KEY, nombre TEXT(100))", cn).ExecuteNonQuery();

            if (!await TableExistsAsync("ligas"))
                new OleDbCommand("CREATE TABLE ligas (id_liga AUTOINCREMENT PRIMARY KEY, nombre TEXT(100))", cn).ExecuteNonQuery();

            if (!await TableExistsAsync("equipos"))
                new OleDbCommand("CREATE TABLE equipos (id_equipo AUTOINCREMENT PRIMARY KEY, id_liga LONG, nombre TEXT(100))", cn).ExecuteNonQuery();

            if (!await TableExistsAsync("fixture"))
                new OleDbCommand("CREATE TABLE fixture (id_fixture AUTOINCREMENT PRIMARY KEY, id_liga LONG, id_deporte LONG, fecha DATETIME, descripcion TEXT(255))", cn).ExecuteNonQuery();

            if (!await TableExistsAsync("usuarios"))
                new OleDbCommand("CREATE TABLE usuarios (id_usuario AUTOINCREMENT PRIMARY KEY, nombre TEXT(100), password TEXT(100))", cn).ExecuteNonQuery();

            if (!await TableExistsAsync("apuesta"))
                new OleDbCommand("CREATE TABLE apuesta (id_apuesta AUTOINCREMENT PRIMARY KEY, id_usuario LONG, monto CURRENCY, estado TEXT(50), fecha DATETIME, fecha_partido DATETIME)", cn).ExecuteNonQuery();

            if (!await TableExistsAsync("apuesta_detalle"))
                new OleDbCommand("CREATE TABLE apuesta_detalle (id_apuestaDetalle AUTOINCREMENT PRIMARY KEY, id_apuesta LONG, id_fixture LONG, id_jugador LONG, tipo_apuesta TEXT(100))", cn).ExecuteNonQuery();

            if (!await TableExistsAsync("fixture_equipo"))
                new OleDbCommand("CREATE TABLE fixture_equipo (id_fixtureEquipo AUTOINCREMENT PRIMARY KEY, id_fixture LONG, id_equipo LONG, rol TEXT(20))", cn).ExecuteNonQuery();

            if (!await TableExistsAsync("deportes_liga"))
                new OleDbCommand("CREATE TABLE deportes_liga (id_deporteLiga AUTOINCREMENT PRIMARY KEY, id_deporte LONG, id_liga LONG)", cn).ExecuteNonQuery();

            if (!await TableExistsAsync("mes_ligas"))
                new OleDbCommand("CREATE TABLE mes_ligas (id_mesliga AUTOINCREMENT PRIMARY KEY, id_liga LONG, id_mes LONG, monto CURRENCY)", cn).ExecuteNonQuery();

            // =============================================================
            // 🔹 SEED DEPORTES
            // =============================================================
            if ((int)new OleDbCommand("SELECT COUNT(*) FROM deportes", cn).ExecuteScalar() == 0)
            {
                string[] deportes = { "Futbol", "Basquet", "Tenis" };
                foreach (var d in deportes)
                {
                    using var cmd = new OleDbCommand("INSERT INTO deportes (nombre) VALUES (?)", cn);
                    cmd.Parameters.AddWithValue("?", d);
                    cmd.ExecuteNonQuery();
                }
            }

            // =============================================================
            // 🔹 SEED LIGAS
            // =============================================================
            if ((int)new OleDbCommand("SELECT COUNT(*) FROM ligas", cn).ExecuteScalar() == 0)
            {
                string[] ligas = {
            "Serie A", "Premier League", "La Liga",
            "NBA", "ACB", "Euroleague",
            "ATP Masters", "ATP 500", "Grand Slam"
        };
                foreach (var l in ligas)
                {
                    using var cmd = new OleDbCommand("INSERT INTO ligas (nombre) VALUES (?)", cn);
                    cmd.Parameters.AddWithValue("?", l);
                    cmd.ExecuteNonQuery();
                }
            }

            // =============================================================
            // 🔹 SEED DEPORTES_LIGA
            // =============================================================
            if ((int)new OleDbCommand("SELECT COUNT(*) FROM deportes_liga", cn).ExecuteScalar() == 0)
            {
                for (int i = 1; i <= 3; i++)
                    new OleDbCommand($"INSERT INTO deportes_liga (id_deporte, id_liga) VALUES (2,{i})", cn).ExecuteNonQuery();
                for (int i = 4; i <= 6; i++)
                    new OleDbCommand($"INSERT INTO deportes_liga (id_deporte, id_liga) VALUES (3,{i})", cn).ExecuteNonQuery();
                for (int i = 7; i <= 9; i++)
                    new OleDbCommand($"INSERT INTO deportes_liga (id_deporte, id_liga) VALUES (4,{i})", cn).ExecuteNonQuery();

                Console.WriteLine("✅ Tabla deportes_liga creada y vinculada correctamente.");
            }

            // =============================================================
            // 🔹 SEED EQUIPOS
            // =============================================================
            if ((int)new OleDbCommand("SELECT COUNT(*) FROM equipos", cn).ExecuteScalar() == 0)
            {
                var equiposPorLiga = new Dictionary<int, string[]>
        {
            { 1, new[] { "Inter", "Juventus", "Milan" } },
            { 2, new[] { "Manchester City", "Manchester United", "Liverpool" } },
            { 3, new[] { "Barcelona", "Atletico de Madrid", "Real Madrid" } },
            { 4, new[] { "Angeles Lakers", "Boston Celtics", "San Antonio Spurs" } },
            { 5, new[] { "Basquet Zaragoza", "Basquet Girona", "Club Baloncesto Gran Canaria" } },
            { 6, new[] { "Panathinaikos Athens", "Olympiacos Piraeus", "Baskonia" } },
            { 7, new[] { "Jannik Sinner", "Carlos Alcaraz", "Novak Djokovic" } },
            { 8, new[] { "Taylor Fritz", "Francisco Cerundolo", "Alexander Zverev" } },
            { 9, new[] { "Joao Fonseca", "Ben Shelton", "Casper Ruud" } }
        };
                foreach (var kv in equiposPorLiga)
                {
                    foreach (var nombre in kv.Value)
                    {
                        using var cmd = new OleDbCommand("INSERT INTO equipos (id_liga, nombre) VALUES (?,?)", cn);
                        cmd.Parameters.AddWithValue("?", kv.Key);
                        cmd.Parameters.AddWithValue("?", nombre);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // =============================================================
            // 🔹 SEED FIXTURES (2025)
            // =============================================================
            if ((int)new OleDbCommand("SELECT COUNT(*) FROM fixture", cn).ExecuteScalar() == 0)
            {
                Console.WriteLine("🔧 Generando fixtures automáticos...");

                var ligas = new Dictionary<int, (int dep, string[] eq)>
        {
            { 1, (2, new[] { "Inter", "Juventus", "Milan" }) },
            { 2, (2, new[] { "Manchester City", "Manchester United", "Liverpool" }) },
            { 3, (2, new[] { "Barcelona", "Atletico de Madrid", "Real Madrid" }) },
            { 4, (3, new[] { "Angeles Lakers", "Boston Celtics", "San Antonio Spurs" }) },
            { 5, (3, new[] { "Basquet Zaragoza", "Basquet Girona", "Club Baloncesto Gran Canaria" }) },
            { 6, (3, new[] { "Panathinaikos Athens", "Olympiacos Piraeus", "Baskonia" }) },
            { 7, (4, new[] { "Jannik Sinner", "Carlos Alcaraz", "Novak Djokovic" }) },
            { 8, (4, new[] { "Taylor Fritz", "Francisco Cerundolo", "Alexander Zverev" }) },
            { 9, (4, new[] { "Joao Fonseca", "Ben Shelton", "Casper Ruud" }) }
        };

                var fechas = new[]
                {
            new DateTime(2025, 8, 13),
            new DateTime(2025, 8, 20),
            new DateTime(2025, 9, 3),
            new DateTime(2025, 9, 13),
            new DateTime(2025, 9, 20),
            new DateTime(2025, 10, 3)
        };

                foreach (var kv in ligas)
                {
                    int idLiga = kv.Key;
                    int idDep = kv.Value.dep;
                    var eqs = kv.Value.eq;

                    for (int i = 0; i < eqs.Length; i++)
                    {
                        for (int j = i + 1; j < eqs.Length; j++)
                        {
                            string desc = $"{eqs[i]} vs {eqs[j]}";
                            DateTime fecha = fechas[(i + j) % fechas.Length];

                            using var cmd = new OleDbCommand(
                                "INSERT INTO fixture (id_liga, id_deporte, fecha, descripcion) VALUES (?,?,?,?)", cn);
                            cmd.Parameters.AddWithValue("?", idLiga);
                            cmd.Parameters.AddWithValue("?", idDep);
                            cmd.Parameters.AddWithValue("?", fecha);
                            cmd.Parameters.AddWithValue("?", desc);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                Console.WriteLine("✅ Fixtures generados correctamente.");
            }

            // =============================================================
            // 🔹 CREAR TABLA EVENTO_PARTIDO (si no existe) Y GENERAR DATOS USANDO fixture_equipo
            // =============================================================
            if (!await TableExistsAsync("evento_partido"))
            {
                new OleDbCommand("CREATE TABLE evento_partido (id_evento AUTOINCREMENT PRIMARY KEY, id_fixture LONG, id_jugador LONG, id_tipoevento LONG, minuto INTEGER)", cn).ExecuteNonQuery();
            }

            int countEventosSeed;
            using (var cmdCountEv = new OleDbCommand("SELECT COUNT(*) FROM evento_partido", cn))
                countEventosSeed = Convert.ToInt32(cmdCountEv.ExecuteScalar());

            if (countEventosSeed == 0)
            {
                Console.WriteLine("⚙️ Generando eventos usando fixture_equipo...");

                var rand = new Random();

                // Tipos de evento agrupados por deporte
                var tiposPorDep = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
                using (var cmd = new OleDbCommand("SELECT id_tipoevento, deporte FROM tipo_evento", cn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string dep = r["deporte"].ToString() ?? "";
                        int idTipo = Convert.ToInt32(r["id_tipoevento"]);
                        if (!tiposPorDep.ContainsKey(dep))
                            tiposPorDep[dep] = new List<int>();
                        tiposPorDep[dep].Add(idTipo);
                    }
                }

                string DeporteNombre(int idDep) =>
                    idDep == 2 ? "Futbol" :
                    idDep == 3 ? "Basquet" :
                    idDep == 4 ? "Tenis" : "";

                // Jugadores por equipo
                var jugadoresPorEquipo = new Dictionary<int, List<int>>();
                using (var cmd = new OleDbCommand("SELECT id_jugador, id_equipo FROM jugador", cn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        int idJug = Convert.ToInt32(r["id_jugador"]);
                        int idEq = Convert.ToInt32(r["id_equipo"]);
                        if (!jugadoresPorEquipo.ContainsKey(idEq))
                            jugadoresPorEquipo[idEq] = new List<int>();
                        jugadoresPorEquipo[idEq].Add(idJug);
                    }
                }

                // fixture_equipo: fixture -> equipos
                var equiposPorFixture = new Dictionary<int, List<int>>();
                using (var cmd = new OleDbCommand("SELECT id_fixture, id_equipo FROM fixture_equipo ORDER BY id_fixture, id_fixtureEquipo", cn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        int idFix = Convert.ToInt32(r["id_fixture"]);
                        int idEq = Convert.ToInt32(r["id_equipo"]);
                        if (!equiposPorFixture.ContainsKey(idFix))
                            equiposPorFixture[idFix] = new List<int>();
                        if (!equiposPorFixture[idFix].Contains(idEq))
                            equiposPorFixture[idFix].Add(idEq);
                    }
                }

                using (var cmdFix = new OleDbCommand("SELECT id_fixture, id_deporte FROM fixture", cn))
                using (var rFix = cmdFix.ExecuteReader())
                {
                    while (rFix.Read())
                    {
                        int idFix = Convert.ToInt32(rFix["id_fixture"]);
                        int idDep = Convert.ToInt32(rFix["id_deporte"]);
                        string depNombre = DeporteNombre(idDep);
                        if (string.IsNullOrEmpty(depNombre)) continue;

                        if (!equiposPorFixture.TryGetValue(idFix, out var equiposDelPartido) || equiposDelPartido.Count == 0)
                            continue;

                        if (!tiposPorDep.TryGetValue(depNombre, out var tiposValidos) || tiposValidos.Count == 0)
                            continue;

                        var equiposConJugadores = equiposDelPartido
                            .Where(eq => jugadoresPorEquipo.ContainsKey(eq) && jugadoresPorEquipo[eq].Count > 0)
                            .ToList();

                        if (equiposConJugadores.Count == 0)
                            continue;

                        int cantidadEventos = rand.Next(3, 7);

                        for (int i = 0; i < cantidadEventos; i++)
                        {
                            int idEquipo = equiposConJugadores[rand.Next(equiposConJugadores.Count)];
                            var jugadoresDeEseEquipo = jugadoresPorEquipo[idEquipo];
                            int idJugador = jugadoresDeEseEquipo[rand.Next(jugadoresDeEseEquipo.Count)];

                            int idTipo = tiposValidos[rand.Next(tiposValidos.Count)];
                            int minuto =
                                depNombre == "Futbol" ? rand.Next(1, 91) :
                                depNombre == "Basquet" ? rand.Next(1, 49) :
                                rand.Next(1, 181);

                            using var cmdIns = new OleDbCommand(
                                "INSERT INTO evento_partido (id_fixture, id_jugador, id_tipoevento, minuto) VALUES (?,?,?,?)", cn);
                            cmdIns.Parameters.AddWithValue("?", idFix);
                            cmdIns.Parameters.AddWithValue("?", idJugador);
                            cmdIns.Parameters.AddWithValue("?", idTipo);
                            cmdIns.Parameters.AddWithValue("?", minuto);
                            cmdIns.ExecuteNonQuery();
                        }
                    }
                }

                Console.WriteLine("✅ Eventos generados correctamente usando fixture_equipo.");
            }

            // =============================================================
            // 🔹 USUARIO BASE
            // =============================================================
            if ((int)new OleDbCommand("SELECT COUNT(*) FROM usuarios", cn).ExecuteScalar() == 0)
            {
                using var cmd = new OleDbCommand("INSERT INTO usuarios (nombre, password) VALUES (?,?)", cn);
                cmd.Parameters.AddWithValue("?", "admin");
                cmd.Parameters.AddWithValue("?", "123");
                cmd.ExecuteNonQuery();
            }
        }

        // ---- Helpers ----
        private async Task<bool> TableExistsAsync(string name)
        {
            return await Task.Run(() =>
            {
                using var cn = new OleDbConnection(connectionString);
                cn.Open();
                var schema = cn.GetSchema("Tables", new[] { null, null, name, "TABLE" });
                return schema.Rows.Count > 0;
            });
        }


        // ===================== PASO 3 – NUEVOS MÉTODOS PARA APUESTAS =====================

        // ⚽ Devolver deportes (id_deporte, nombre, objetivo)
        public async Task<DataTable> ObtenerDeportesBasicosAsync()
        {
            const string sql = "SELECT id_deporte, nombre FROM deportes ORDER BY nombre";
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();
            using var da = new OleDbDataAdapter(sql, cn);
            var dt = new DataTable();
            await Task.Run(() => da.Fill(dt));
            return dt;
        }

        // 🏆 Ligas de un deporte
        public async Task<DataTable> ObtenerLigasPorDeporteAsyncDT(int idDeporte)
        {
            const string sql = @"
        SELECT l.id_liga, l.nombre
        FROM ligas AS l
        INNER JOIN deportes_liga AS dl ON l.id_liga = dl.id_liga
        WHERE dl.id_deporte = ?";
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();
            using var da = new OleDbDataAdapter(sql, cn);
            da.SelectCommand!.Parameters.AddWithValue("@p1", idDeporte);
            var dt = new DataTable();
            await Task.Run(() => da.Fill(dt));
            return dt;
        }

        // ================= EQUIPOS =================
        public async Task<DataTable> ObtenerEquiposPorLigaAsyncDT(int idLiga)
        {
            string sql = "SELECT id_equipo, nombre FROM Equipos WHERE id_liga = ?";
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();
            using var da = new OleDbDataAdapter(sql, cn);
            da.SelectCommand!.Parameters.AddWithValue("@p1", idLiga);
            var dt = new DataTable();
            await Task.Run(() => da.Fill(dt));
            return dt;
        }

        public async Task<DataTable> ObtenerJugadoresPorEquipoAsyncDT(int idEquipo)
        {
            string sql = "SELECT id_jugador, nombre FROM jugador WHERE id_equipo = ?";
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();
            using var da = new OleDbDataAdapter(sql, cn);
            da.SelectCommand!.Parameters.AddWithValue("@p1", idEquipo);
            var dt = new DataTable();
            await Task.Run(() => da.Fill(dt));
            return dt;
        }


        public async Task<DataTable> ObtenerFixturesPorEquipoYDeporteAsyncDT(int idEquipo, int idDeporte)
        {
            DataTable dt = new();

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                await conn.OpenAsync();

                string sql = @"
            SELECT f.id_fixture, f.descripcion, f.fecha
            FROM fixture f
            INNER JOIN fixture_equipo fe ON f.id_fixture = fe.id_fixture
            WHERE fe.id_equipo = ? AND f.id_deporte = ?
            ORDER BY f.fecha ASC";

                using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("?", idEquipo);
                    cmd.Parameters.AddWithValue("?", idDeporte);

                    using (OleDbDataAdapter da = new OleDbDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }


        // ✅ Tipos de evento filtrados por deport
        public async Task<DataTable> ObtenerTiposEventoPorDeporteAsync(string nombreDeporte)
        {
            const string sql = "SELECT id_tipoEvento, descripcion FROM tipo_evento WHERE LCASE(deporte) = LCASE(?) ORDER BY descripcion";
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();
            using var da = new OleDbDataAdapter(sql, cn);
            da.SelectCommand!.Parameters.AddWithValue("@p1", nombreDeporte);
            var dt = new DataTable();
            await Task.Run(() => da.Fill(dt));
            return dt;
        }

        // ✅ INSERTAR APUESTA (corrige error de tipo y formato de fecha)

        public async Task<int> InsertarApuestaAsync(int idUsuario, decimal monto, int idFixture, string estado = "Pendiente")
        {
            int idApuesta = 0;

            using var cn = new OleDbConnection(connectionString + ";Mode=Share Deny None;");
            await cn.OpenAsync();

            // 🔹 Obtener la fecha del partido (fixture)
            DateTime fechaPartido = DateTime.Now;
            using (var cmdFecha = new OleDbCommand("SELECT fecha FROM fixture WHERE id_fixture = ?", cn))
            {
                cmdFecha.Parameters.AddWithValue("?", idFixture);
                var result = await cmdFecha.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                    fechaPartido = Convert.ToDateTime(result);
            }

            // 🔹 Insertar apuesta usando la fecha del partido
            using (var cmd = new OleDbCommand(
                "INSERT INTO apuesta (id_usuario, monto, estado, fecha, fecha_partido) VALUES (?,?,?,?,?)", cn))
            {
                cmd.Parameters.Add("@p1", OleDbType.Integer).Value = idUsuario;
                cmd.Parameters.Add("@p2", OleDbType.Currency).Value = monto;
                cmd.Parameters.Add("@p3", OleDbType.VarChar).Value = estado;
                cmd.Parameters.Add("@p4", OleDbType.Date).Value = DateTime.Now; // fecha real del sistema
                cmd.Parameters.Add("@p5", OleDbType.Date).Value = fechaPartido; // fecha del fixture
                await cmd.ExecuteNonQueryAsync();
            }

            using (var cmdId = new OleDbCommand("SELECT @@IDENTITY", cn))
            {
                object result = await cmdId.ExecuteScalarAsync();
                if (result != DBNull.Value && result != null)
                    idApuesta = Convert.ToInt32(result);
            }

            return idApuesta;
        }

        // ✅ INSERTAR DETALLE (sin campo fecha)
        public async Task InsertarApuestaDetalleAsync(int idApuesta, int idFixture, int idJugador, string tipoApuesta)
        {
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();

            const string sql = @"INSERT INTO apuesta_detalle (id_apuesta, id_fixture, id_jugador, tipo_apuesta)
                         VALUES (?,?,?,?)";
            using var cmd = new OleDbCommand(sql, cn);
            cmd.Parameters.Add("@p1", OleDbType.Integer).Value = idApuesta;
            cmd.Parameters.Add("@p2", OleDbType.Integer).Value = idFixture;
            cmd.Parameters.Add("@p3", OleDbType.Integer).Value = idJugador;
            cmd.Parameters.Add("@p4", OleDbType.VarChar).Value = tipoApuesta ?? "";

            int rows = await cmd.ExecuteNonQueryAsync();
            if (rows != 1)
                throw new Exception("No se pudo insertar el detalle de la apuesta (apuesta_detalle). Revisión de esquema/nombres.");
        }

        public async Task<List<ApuestaDetalle>> ObtenerApuestasDetalladasPorUsuarioAsync(int idUsuario)
        {
            var lista = new List<ApuestaDetalle>();

            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();

                    string sql = @"
            SELECT 
                a.id_apuesta, a.id_usuario, a.monto, a.estado, a.fecha,
                ad.id_apuestaDetalle, ad.id_fixture, ad.id_jugador, ad.tipo_apuesta,
                IIf(f.descripcion Is Null, '', f.descripcion) AS partido,
                a.fecha_partido AS fecha_partido,
                IIf(d.nombre Is Null, '', d.nombre) AS deporte,
                IIf(j.nombre Is Null, '', j.nombre) AS jugador,
                IIf(e.nombre Is Null, '', e.nombre) AS equipo
            FROM (((((apuesta AS a
            LEFT JOIN apuesta_detalle AS ad ON a.id_apuesta = ad.id_apuesta)
            LEFT JOIN fixture AS f ON ad.id_fixture = f.id_fixture)
            LEFT JOIN jugador AS j ON ad.id_jugador = j.id_jugador)
            LEFT JOIN equipos AS e ON j.id_equipo = e.id_equipo)
            LEFT JOIN deportes AS d ON f.id_deporte = d.id_deporte)
            WHERE a.id_usuario = ?
            ORDER BY a.fecha DESC";

            using var cmd = new OleDbCommand(sql, cn);
            cmd.Parameters.Add("@p1", OleDbType.Integer).Value = idUsuario;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var apuesta = new Apuesta
                {
                    IdApuesta = Convert.ToInt32(reader["id_apuesta"]),
                    IdUsuario = Convert.ToInt32(reader["id_usuario"]),
                    Monto = Convert.ToDecimal(reader["monto"]),
                    Estado = reader["estado"]?.ToString() ?? "",
                    Fecha = Convert.ToDateTime(reader["fecha"]),
                    FechaPartido = reader["fecha_partido"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["fecha_partido"]),
                    Deporte = reader["deporte"].ToString() ?? ""
                };

                var detalle = new ApuestaDetalle
                {
                    IdApuestaDetalle = reader["id_apuestaDetalle"] == DBNull.Value ? 0 : Convert.ToInt32(reader["id_apuestaDetalle"]),
                    IdApuesta = apuesta.IdApuesta,
                    IdFixture = reader["id_fixture"] == DBNull.Value ? 0 : Convert.ToInt32(reader["id_fixture"]),
                    IdJugador = reader["id_jugador"] == DBNull.Value ? 0 : Convert.ToInt32(reader["id_jugador"]),
                    TipoApuestaTexto = reader["tipo_apuesta"]?.ToString() ?? "",
                    Apuesta = apuesta,
                    Fixture = new Fixture
                    {
                        IdFixture = reader["id_fixture"] == DBNull.Value ? 0 : Convert.ToInt32(reader["id_fixture"]),
                        Descripcion = reader["partido"]?.ToString() ?? "",
                        Fecha = reader["fecha_partido"] == DBNull.Value ? apuesta.Fecha : Convert.ToDateTime(reader["fecha_partido"])
                    },
                    Jugador = new Jugador
                    {
                        Id = reader["id_jugador"] == DBNull.Value ? 0 : Convert.ToInt32(reader["id_jugador"]),
                        Nombre = reader["jugador"]?.ToString() ?? ""
                    },
                    Equipo = new Equipo
                    {
                        Nombre = reader["equipo"]?.ToString() ?? ""
                    }
                };

                lista.Add(detalle);
            }
            lista = lista.OrderBy(d => d.Apuesta.FechaPartido).ToList();
            return lista;
        }

        public async Task ActualizarMontosMesLigasAsync()
        {
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();

            // 🧹 Limpiar la tabla antes de recalcular
            using (var cmdDel = new OleDbCommand("DELETE FROM mes_ligas", cn))
                await cmdDel.ExecuteNonQueryAsync();

            // 🔹 Traer todas las apuestas junto con sus fixtures y ligas
            string sql = @"
        SELECT 
            a.id_apuesta,
            a.monto,
            f.id_liga,
            a.fecha_partido
        FROM (apuesta AS a
        INNER JOIN apuesta_detalle AS ad ON a.id_apuesta = ad.id_apuesta)
        INNER JOIN fixture AS f ON ad.id_fixture = f.id_fixture
    ";

            var datos = new List<(int idLiga, DateTime fecha, decimal monto)>();

            using (var cmd = new OleDbCommand(sql, cn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idLiga = Convert.ToInt32(reader["id_liga"]);
                    decimal monto = Convert.ToDecimal(reader["monto"]);

                    // 🧠 Usamos SÓLO la fecha del partido
                    DateTime fecha = reader["fecha_partido"] != DBNull.Value
                        ? Convert.ToDateTime(reader["fecha_partido"])
                        : DateTime.Now;

                    datos.Add((idLiga, fecha, monto));
                }
            }

            // 🔹 Agrupar los montos por MES y LIGA
            var grupos = datos
                .GroupBy(x =>
                {
                    int mesId = 0;
                    switch (x.fecha.Month)
                    {
                        case 8: mesId = 1; break;  // Agosto
                        case 9: mesId = 2; break;  // Septiembre
                        case 10: mesId = 3; break; // Octubre
                    }
                    return new { x.idLiga, mesId };
                })
                .Where(g => g.Key.mesId != 0)
                .Select(g => new
                {
                    Liga = g.Key.idLiga,
                    Mes = g.Key.mesId,
                    Total = g.Sum(x => x.monto)
                });

            // 🔹 Insertar resultados en mes_ligas
            foreach (var item in grupos)
            {
                using var cmdIns = new OleDbCommand(
                    "INSERT INTO mes_ligas (id_liga, id_mes, monto) VALUES (?,?,?)", cn);
                cmdIns.Parameters.AddWithValue("?", item.Liga);
                cmdIns.Parameters.AddWithValue("?", item.Mes);
                cmdIns.Parameters.AddWithValue("?", item.Total);
                await cmdIns.ExecuteNonQueryAsync();
            }

            Console.WriteLine("✅ Tabla mes_ligas actualizada según fecha_partido correctamente.");
        }

        public async Task ActualizarEstadosApuestasAsync(List<ApuestaDetalle> detalles)
        {
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();

            DateTime hoy = DateTime.Now;

            foreach (var det in detalles)
            {
                if (det.Apuesta == null || det.Fixture == null)
                    continue;

                int idApuesta = det.Apuesta.IdApuesta;
                int idFixture = det.Fixture.IdFixture;
                string tipoApuesta = det.TipoApuestaTexto?.Trim() ?? "";

                string estado = "Pendiente"; // valor por defecto

                // 🧭 Determinar si el partido ya ocurrió o no
                if (det.Fixture.Fecha <= hoy)
                {
                    // Buscar eventos reales del fixture
                    using var cmdEv = new OleDbCommand(
                        "SELECT COUNT(*) FROM evento_partido ep " +
                        "INNER JOIN tipo_evento te ON ep.id_tipoevento = te.id_tipoevento " +
                        "WHERE ep.id_fixture = ? AND te.descripcion = ?", cn);
                    cmdEv.Parameters.AddWithValue("?", idFixture);
                    cmdEv.Parameters.AddWithValue("?", tipoApuesta);

                    int coincidencias = Convert.ToInt32(await cmdEv.ExecuteScalarAsync());

                    if (coincidencias > 0)
                        estado = "Ganada";
                    else
                        estado = "Perdida";
                }
                else
                {
                    estado = "Pendiente";
                }

                // Actualizar el estado de la apuesta
                using var cmdUpdate = new OleDbCommand(
                    "UPDATE apuesta SET estado = ? WHERE id_apuesta = ?", cn);
                cmdUpdate.Parameters.AddWithValue("?", estado);
                cmdUpdate.Parameters.AddWithValue("?", idApuesta);
                await cmdUpdate.ExecuteNonQueryAsync();

                // También actualizamos el objeto en memoria (por si se refresca en UI)
                det.Apuesta.Estado = estado;
            }

            Console.WriteLine("✅ Estados de apuestas actualizados correctamente.");
        }


        public async Task<string> VerificarResultadoApuestaAsync(int idFixture, int idJugador, string tipoApuesta)
        {
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();

            // Obtener el id_tipoevento que corresponde al texto de tipoApuesta (por ejemplo, "Gol", "Asistencia", etc.)
            int idTipoEvento = 0;
            using (var cmdTipo = new OleDbCommand(
                "SELECT id_tipoevento FROM tipo_evento WHERE LCASE(descripcion) = LCASE(?)", cn))
            {
                cmdTipo.Parameters.AddWithValue("?", tipoApuesta);
                var result = await cmdTipo.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                    idTipoEvento = Convert.ToInt32(result);
            }

            if (idTipoEvento == 0)
                return "Perdida"; // si el tipo no existe, la damos por perdida

            // Buscar si hubo algún evento que coincida con el fixture, jugador y tipo_evento
            using (var cmd = new OleDbCommand(
                "SELECT COUNT(*) FROM evento_partido WHERE id_fixture = ? AND id_jugador = ? AND id_tipoevento = ?", cn))
            {
                cmd.Parameters.AddWithValue("?", idFixture);
                cmd.Parameters.AddWithValue("?", idJugador);
                cmd.Parameters.AddWithValue("?", idTipoEvento);

                int coincidencias = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return coincidencias > 0 ? "Ganada" : "Perdida";
            }
        }



    }
}

