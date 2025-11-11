namespace TableroApuestas.Models
{
    public class ApuestaDetalle
    {
        public int IdApuestaDetalle { get; set; }
        public int IdApuesta { get; set; }
        public int IdFixture { get; set; }
        public int IdJugador { get; set; }
        public string TipoApuestaTexto { get; set; } = "";

        public Apuesta? Apuesta { get; set; }
        public Fixture? Fixture { get; set; }
        public Jugador? Jugador { get; set; }
        public Equipo? Equipo { get; set; }
    }
}
