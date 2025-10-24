namespace TableroApuestas.Models
{
    public enum TipoApuesta : byte
    {
        Goles = 0,
        Asistencias = 1,
        Tarjetas = 2
    }
    public enum EstadoApuesta : byte
    {
        Pendiente = 0,
        Ganada = 1,
        Perdida = 2
    }
    public class Apuesta
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }         // por ahora fijo (ej: 1)
        public int DeporteId { get; set; }
        public int EquipoId { get; set; }
        public int FixtureId { get; set; }
        public int JugadorId { get; set; }
        public TipoApuesta Tipo { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaCreacion { get; set; }
        public EstadoApuesta Estado { get; set; } = EstadoApuesta.Pendiente;
    }
}
