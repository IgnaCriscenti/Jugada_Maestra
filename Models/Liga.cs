using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TableroApuestas.Models
{
    public class Liga
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Monto { get; set; }
    }
}
