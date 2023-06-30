namespace PizzaNapoli.Models
{
    public class Registro
    {
        public int id_pizza { get; set; }

        public string nombrepizza { get; set; }

        public string descripcion { get; set; }

        public decimal precio { get; set; }
        public int cantidad { get; set; }

        public decimal monto { get { return precio * cantidad; } }
        public Registro()
        {

            id_pizza = 0;

            descripcion = string.Empty;

            nombrepizza = string.Empty;

            precio = 0;

            cantidad = 0;

        }

    }
}
