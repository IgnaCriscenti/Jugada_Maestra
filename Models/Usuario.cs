using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography;

namespace TableroApuestas.Models
{
    public class Usuario
    {
        public int ID { get; set; }
        public string? nombre { get; set; }
        public string? apellido { get; set; }
        public string? password { get; set; }

        public static string CalcularSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2")); // formato hexadecimal
                }
                return builder.ToString();
            }
        }
    }
}
