namespace TableroApuestas.Models
{
    public class Jugador
    {
        public int Id { get; set; }
        public int EquipoId { get; set; }
        public string Nombre { get; set; } = "";
        public string? Posicion { get; set; }
    }
}
