namespace PizzaNapoli.Models
{
    public class HistorialPedido
    {
        public int idpedido { get; set; }
        public DateTime fechapedido { get; set; }
        public string nombrepizza { get; set; }
        public int cantidadpedido { get; set; }
        public decimal precio { get; set; }

        public decimal monto { get { return precio * cantidadpedido; } }

        public HistorialPedido()
        {
            idpedido = 0;
            fechapedido = DateTime.Now;
            nombrepizza = string.Empty;
            cantidadpedido = 0;
            precio = 0;
        }

    }
}
