﻿namespace TableroApuestas.Data
{
    using System.Data;
    using System;
    using System.Data.OleDb;
    using System.Reflection;
    using TableroApuestas.Models;
    using System.Data.SqlClient;

    public class AccesoDatos
    {
        private readonly string connectionString;

        public AccesoDatos(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public DataTable ObtenerTodosLosUsuarios()
        {
            DataTable dataTable = new DataTable();

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM usuarios";
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection))
                {
                    adapter.Fill(dataTable);
                }
            }

            return dataTable;
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
        public bool ValidarUsuario(string nombre, string password)
        {
            using (var connection = new OleDbConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT PasswordHash FROM usuarios WHERE nombre = @nombre";
                using (var command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@nombre", nombre);

                    object result = command.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                        return false;

                    string hash = result.ToString();
                    return BCrypt.Net.BCrypt.Verify(password, hash);
                }
            }
        }

        // Obtener usuario por nombre
        public DataTable ObtenerUsuarioPorNombre(string nombreUsuario)
        {
            DataTable dataTable = new DataTable();

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM usuarios WHERE nombre = @NombreUsuario";

                using (OleDbCommand cmd = new OleDbCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }

            return dataTable;
        }

        // ===================== CONSULTAS DE DEPORTES =====================
        // (dejé tal cual los métodos que vos ya tenías, no se tocan)

        private DataTable ConsultarDatos(string consulta)
        {
            DataTable dataTable = new DataTable();

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(consulta, connection))
                {
                    adapter.Fill(dataTable);
                }
            }

            return dataTable;
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


        // ======= SCHEMA AUTÓMATICO (Access / Jet-ACE) =======
        public async Task EnsureSchemaAsync()
        {
            // Tablas
            await EnsureTableAsync("Equipos", @"
        CREATE TABLE [Equipos](
          [id_equipo] AUTOINCREMENT PRIMARY KEY,
          [id_liga]   LONG NOT NULL,
          [nombre]    TEXT(100) NOT NULL
        )");

            await EnsureTableAsync("Jugadores", @"
        CREATE TABLE [Jugadores](
          [id_jugador] AUTOINCREMENT PRIMARY KEY,
          [id_equipo]  LONG NOT NULL,
          [nombre]     TEXT(100) NOT NULL,
          [posicion]   TEXT(50)
        )");

            await EnsureTableAsync("Fixtures", @"
        CREATE TABLE [Fixtures](
          [id_fixture]         AUTOINCREMENT PRIMARY KEY,
          [id_liga]            LONG NOT NULL,
          [fecha]              DATETIME NOT NULL,
          [id_equipo_local]    LONG NOT NULL,
          [id_equipo_visitante] LONG NOT NULL,
          [descripcion]        TEXT(255)
        )");

            await EnsureTableAsync("Apuestas", @"
        CREATE TABLE [Apuestas](
          [id_apuesta]     AUTOINCREMENT PRIMARY KEY,
          [id_usuario]     LONG NOT NULL,
          [id_deporte]     LONG NOT NULL,
          [id_liga]        LONG NOT NULL,
          [id_equipo]      LONG NOT NULL,
          [id_fixture]     LONG NOT NULL,
          [id_jugador]     LONG NOT NULL,
          [tipo_apuesta]   BYTE NOT NULL,
          [monto]          CURRENCY NOT NULL,
          [fecha_creacion] DATETIME NOT NULL,
          [estado]         BYTE NOT NULL
        )");

            // FKs (si ya existen se ignoran)
            await TryExecAsync(@"ALTER TABLE [Equipos]
                         ADD CONSTRAINT [fk_equ_liga]
                         FOREIGN KEY ([id_liga]) REFERENCES [ligas]([id_liga])");

            await TryExecAsync(@"ALTER TABLE [Jugadores]
                         ADD CONSTRAINT [fk_jug_equ]
                         FOREIGN KEY ([id_equipo]) REFERENCES [Equipos]([id_equipo])");

            await TryExecAsync(@"ALTER TABLE [Fixtures]
                         ADD CONSTRAINT [fk_fix_liga]
                         FOREIGN KEY ([id_liga]) REFERENCES [ligas]([id_liga])");

            await TryExecAsync(@"ALTER TABLE [Fixtures]
                         ADD CONSTRAINT [fk_fix_local]
                         FOREIGN KEY ([id_equipo_local]) REFERENCES [Equipos]([id_equipo])");

            await TryExecAsync(@"ALTER TABLE [Fixtures]
                         ADD CONSTRAINT [fk_fix_visit]
                         FOREIGN KEY ([id_equipo_visitante]) REFERENCES [Equipos]([id_equipo])");

            await TryExecAsync(@"ALTER TABLE [Apuestas]
                         ADD CONSTRAINT [fk_ap_dep]
                         FOREIGN KEY ([id_deporte]) REFERENCES [deportes]([id_deporte])");

            await TryExecAsync(@"ALTER TABLE [Apuestas]
                         ADD CONSTRAINT [fk_ap_liga]
                         FOREIGN KEY ([id_liga]) REFERENCES [ligas]([id_liga])");

            await TryExecAsync(@"ALTER TABLE [Apuestas]
                         ADD CONSTRAINT [fk_ap_equ]
                         FOREIGN KEY ([id_equipo]) REFERENCES [Equipos]([id_equipo])");

            await TryExecAsync(@"ALTER TABLE [Apuestas]
                         ADD CONSTRAINT [fk_ap_fix]
                         FOREIGN KEY ([id_fixture]) REFERENCES [Fixtures]([id_fixture])");

            await TryExecAsync(@"ALTER TABLE [Apuestas]
                         ADD CONSTRAINT [fk_ap_jug]
                         FOREIGN KEY ([id_jugador]) REFERENCES [Jugadores]([id_jugador])");
        }

        // ---- Helpers ----
        private async Task<bool> TableExistsAsync(string tableName)
        {
            return await Task.Run(() =>
            {
                using var cn = new OleDbConnection(connectionString);
                cn.Open();
                var schema = cn.GetSchema("Tables", new[] { null, null, tableName, "TABLE" });
                return schema.Rows.Count > 0;
            });
        }

        private async Task EnsureTableAsync(string name, string createSql)
        {
            if (await TableExistsAsync(name)) return;

            await Task.Run(() =>
            {
                using var cn = new OleDbConnection(connectionString);
                cn.Open();
                using var cmd = new OleDbCommand(createSql, cn);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Esto ayuda a ver EXACTAMENTE qué SQL falló
                    throw new Exception($"Error creando tabla [{name}]: {ex.Message}\nSQL:\n{createSql}");
                }
            });
        }

        private async Task TryExecAsync(string sql)
        {
            await Task.Run(() =>
            {
                using var cn = new OleDbConnection(connectionString);
                cn.Open();
                using var cmd = new OleDbCommand(sql, cn);
                try { cmd.ExecuteNonQuery(); }
                catch { /* FK ya existe u otro detalle: ignorar */ }
            });
        }



        // ===================== PASO 3 – NUEVOS MÉTODOS PARA APUESTAS =====================

        // ⚽ Devolver deportes (id_deporte, nombre, objetivo)
        public async Task<DataTable> ObtenerDeportesBasicosAsync()
        {
            const string sql = "SELECT id_deporte, nombre, objetivo FROM deportes ORDER BY nombre";
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
        SELECT l.id_liga, l.nombre, l.cant_equipos
        FROM ligas AS l
        INNER JOIN deportes_liga AS dl ON dl.id_liga = l.id_liga
        WHERE dl.id_deporte = ?
        ORDER BY l.nombre";
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();
            using var da = new OleDbDataAdapter(sql, cn);
            da.SelectCommand!.Parameters.AddWithValue("@p1", idDeporte);
            var dt = new DataTable();
            await Task.Run(() => da.Fill(dt));
            return dt;
        }

        // 🧾 Fixtures por liga
        public async Task<DataTable> ObtenerFixturesPorLigaAsyncDT(int idLiga)
        {
            const string sql = @"
        SELECT id_fixture, id_liga, fecha, id_equipo_local, id_equipo_visitante, descripcion
        FROM Fixtures
        WHERE id_liga = ?
        ORDER BY fecha DESC";
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();
            using var da = new OleDbDataAdapter(sql, cn);
            da.SelectCommand!.Parameters.AddWithValue("@p1", idLiga);
            var dt = new DataTable();
            await Task.Run(() => da.Fill(dt));
            return dt;
        }

        // 👥 Equipos por liga
        public async Task<DataTable> ObtenerEquiposPorLigaAsyncDT(int idLiga)
        {
            const string sql = @"SELECT id_equipo, id_liga, nombre FROM Equipos WHERE id_liga = ? ORDER BY nombre";
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();
            using var da = new OleDbDataAdapter(sql, cn);
            da.SelectCommand!.Parameters.AddWithValue("@p1", idLiga);
            var dt = new DataTable();
            await Task.Run(() => da.Fill(dt));
            return dt;
        }

        // 🧑‍🦱 Jugadores por equipo
        public async Task<DataTable> ObtenerJugadoresPorEquipoAsyncDT(int idEquipo)
        {
            const string sql = @"SELECT id_jugador, id_equipo, nombre, posicion FROM Jugadores WHERE id_equipo = ? ORDER BY nombre";
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();
            using var da = new OleDbDataAdapter(sql, cn);
            da.SelectCommand!.Parameters.AddWithValue("@p1", idEquipo);
            var dt = new DataTable();
            await Task.Run(() => da.Fill(dt));
            return dt;
        }

        // 💾 Insertar apuesta (parámetros simples para no depender de modelos)
        public async Task<int> InsertarApuestaAsync(
            int idUsuario,
            int idDeporte,
            int idLiga,
            int idEquipo,
            int idFixture,
            int idJugador,
            byte tipoApuesta,            // 0=Goles,1=Asistencias,2=Tarjetas
            decimal monto,
            DateTime fechaCreacion,
            byte estado = 0              // 0=Pendiente
        )
        {
            const string sql = @"
        INSERT INTO Apuestas
        (id_usuario, id_deporte, id_liga, id_equipo, id_fixture, id_jugador, tipo_apuesta, monto, fecha_creacion, estado)
        VALUES (?,?,?,?,?,?,?,?,?,?)";

            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();
            using var cmd = new OleDbCommand(sql, cn);
            cmd.Parameters.AddWithValue("@p1", idUsuario);
            cmd.Parameters.AddWithValue("@p2", idDeporte);
            cmd.Parameters.AddWithValue("@p3", idLiga);
            cmd.Parameters.AddWithValue("@p4", idEquipo);
            cmd.Parameters.AddWithValue("@p5", idFixture);
            cmd.Parameters.AddWithValue("@p6", idJugador);
            cmd.Parameters.AddWithValue("@p7", tipoApuesta);
            cmd.Parameters.AddWithValue("@p8", monto);
            cmd.Parameters.AddWithValue("@p9", fechaCreacion);
            cmd.Parameters.AddWithValue("@p10", estado);
            await cmd.ExecuteNonQueryAsync();

            using var idCmd = new OleDbCommand("SELECT @@IDENTITY", cn);
            return Convert.ToInt32(await idCmd.ExecuteScalarAsync());
        }

        // 📊 Total recaudado por deporte desde Apuestas (para tu “Ver detalle”)
        public async Task<decimal> ObtenerMontoTotalDeporteDesdeApuestasAsync(string nombreDeporte)
        {
            const string sql = @"
        SELECT Nz(SUM(a.monto),0)
        FROM (Apuestas a INNER JOIN deportes d ON a.id_deporte = d.id_deporte)
        WHERE LCASE(d.nombre) = LCASE(?)";
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();
            using var cmd = new OleDbCommand(sql, cn);
            cmd.Parameters.AddWithValue("@p1", nombreDeporte);
            var v = await cmd.ExecuteScalarAsync();
            return (v == null || v == DBNull.Value) ? 0 : Convert.ToDecimal(v);
        }

        // 🔎 Helper: obtener id_deporte por nombre (futbol/basquet/tenis)
        public async Task<int?> ObtenerIdDeportePorNombreAsync(string nombreDeporte)
        {
            const string sql = "SELECT id_deporte FROM deportes WHERE LCASE(nombre) = LCASE(?)";
            using var cn = new OleDbConnection(connectionString);
            await cn.OpenAsync();
            using var cmd = new OleDbCommand(sql, cn);
            cmd.Parameters.AddWithValue("@p1", nombreDeporte);
            var v = await cmd.ExecuteScalarAsync();
            return (v == null || v == DBNull.Value) ? null : (int?)Convert.ToInt32(v);
        }





    }
}
