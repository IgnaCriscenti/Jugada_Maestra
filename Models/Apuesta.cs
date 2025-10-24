namespace TableroApuestas.Models
{
    public class Apuesta
    {
        public int IdApuesta { get; set; }
        public int IdUsuario { get; set; }
        public decimal Monto { get; set; }
        public string Estado { get; set; } = "";
    }
}
