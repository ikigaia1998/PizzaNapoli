namespace PizzaNapoli.Models
{
    public class Pizza
    {
        public int id_pizza { get; set; }

        public string nombrepizza { get; set; }

        public string descripcion { get; set; }

        public decimal precio { get; set; }
        public Pizza()
        {

            id_pizza = 0;

            descripcion = string.Empty;

            nombrepizza = string.Empty;

            precio = 0;

        }
    }
}
