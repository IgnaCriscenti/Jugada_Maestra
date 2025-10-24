namespace TableroApuestas.Models
{
    public class Fixture
    {
        public int IdFixture { get; set; }
        public int IdLiga { get; set; }
        public DateTime Fecha { get; set; }
        public string Descripcion { get; set; } = "";

        public Liga? Liga { get; set; }
    }
}
