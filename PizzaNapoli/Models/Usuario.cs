namespace PizzaNapoli.Models
{
    public class Usuario
    {
        public int id_usuario { get; set; }
        public string nombreusuario { get; set; }
        public string contrasenausuario { get; set; }
        public string emailusuario { get; set; }

        public Usuario()
        {
            id_usuario = 0;
            nombreusuario = "";
            contrasenausuario = "";
            emailusuario = "";
        }
    }
}
