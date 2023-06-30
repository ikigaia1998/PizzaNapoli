using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PizzaNapoli.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PizzaNapoli.Controllers
{
    public class AuthController : Controller
    {
        string cadena = @"server = DESKTOP-G2J1P97;database = PizzaVentaNetCore;Trusted_Connection = True;" +
    "MultipleActiveResultSets = True;TrustServerCertificate = False;Encrypt = False";

        IEnumerable<Usuario> usuarios()
        {
            List<Usuario> temporal = new List<Usuario>();
            using (SqlConnection cn = new SqlConnection(cadena))
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("usp_users", cn);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    temporal.Add(new Usuario
                    {
                        id_usuario = dr.GetInt32(0),
                        nombreusuario = dr.GetString(1),
                        contrasenausuario = dr.GetString(2),
                        emailusuario = dr.GetString(3),
                    });
                }
                dr.Close();
            }
            return temporal;
        }

        public async Task<IActionResult> Index()
        { return View(await Task.Run(() => new Usuario())); }

        [HttpPost]
        public async Task<IActionResult> Index(Usuario reg)
        {
            //buscar el usuario por email y clave
            Usuario item = usuarios().FirstOrDefault(u => u.emailusuario == reg.emailusuario && u.contrasenausuario == reg.contrasenausuario);
            if (item == null)
            {
                ViewBag.mensaje = "Email o clave incorrecta";
                return View();
            }
            else
            {
                var claim = new List<Claim>
        {
          new Claim(ClaimTypes.Name,item.nombreusuario),
          new Claim("Email",item.emailusuario),
          new Claim("IdUsu",item.id_usuario.ToString()),
        };
                var userIdentity = new ClaimsIdentity(claim, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                  new ClaimsPrincipal(userIdentity));
                return RedirectToAction("Portal", "Ecommerce");
            }
        }

        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Auth");
        }

        public async Task<IActionResult> Registrar(int id)
        {
            Usuario usuario;
            if (id == 0) { usuario = new Usuario(); }
            else
            {
                usuario = Buscar(id);
            }
            return View(await Task.Run(() => usuario));
        }


        [HttpPost]
        public async Task<IActionResult> Registrar(Usuario reg)
        {
            if (!ModelState.IsValid)
            {
                return View(await Task.Run(() => reg));
            }
            ViewBag.mensaje = agregar(reg);
            return View(await Task.Run(() => reg));
        }


        string agregar(Usuario usuario)
        {
            string mensaje = "";
            SqlConnection cn = new SqlConnection(cadena);
            try
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("usp_register_users", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@nombre", usuario.nombreusuario);
                cmd.Parameters.AddWithValue("@email", usuario.emailusuario);
                cmd.Parameters.AddWithValue("@clave", usuario.contrasenausuario);
                int c = cmd.ExecuteNonQuery();
                mensaje = $"Se ha agregado {c} usuario";
            }
            catch (SqlException ex) { mensaje = ex.Message; }
            finally { cn.Close(); }
            return mensaje;
        }

        Usuario Buscar(int idclient)
        {
            return usuarios().FirstOrDefault(x => x.id_usuario == idclient);
        }
    }
}
