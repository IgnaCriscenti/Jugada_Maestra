namespace TableroApuestas.Models
{
    public class Fixture
    {
        public int Id { get; set; }
        public int DeporteId { get; set; }
        public DateTime Fecha { get; set; }
        public int EquipoLocalId { get; set; }
        public int EquipoVisitanteId { get; set; }
        public string? Descripcion { get; set; }
    }
}
