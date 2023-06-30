
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Data;
using PizzaNapoli.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using System;

namespace PizzaNapoli.Controllers
{
    [Authorize]
    public class EcommerceController : Controller
    {
        private readonly ILogger<EcommerceController> _logger;





        string cadena = @"server = DESKTOP-G2J1P97;database = PizzaVentaNetCore;Trusted_Connection = True;" +

            "MultipleActiveResultSets = True;TrustServerCertificate = False;Encrypt = False";

        IEnumerable<Pizza> listado()
        {

            List<Pizza> pizza = new List<Pizza>();

            using (SqlConnection connec = new SqlConnection(cadena))

            {

                SqlCommand cmd = new SqlCommand("exec usp_pizza", connec);

                connec.Open();

                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())

                {

                    pizza.Add(new Pizza()

                    {

                        id_pizza = rd.GetInt32(0),

                        nombrepizza = rd.GetString(1),

                        precio = rd.GetDecimal(2),

                        descripcion = rd.GetString(3)

                    });

                }

            }

            return pizza;

        }

        Pizza buscar(int id)

        {

            return listado().FirstOrDefault(x => x.id_pizza == id);

        }

        public async Task<IActionResult> Portal(string nombre = "")
        {
            if (HttpContext.Session.GetString("canasta") == null)

            {
                HttpContext.Session.SetString("canasta",
                  JsonConvert.SerializeObject(new List<Registro>()));
            }


            string searchTerm = nombre;

            return View(await Task.Run(() => listado().Where(p => p.nombrepizza.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList()));
        }

        public async Task<IActionResult> Seleccionar(int? id = null)

        {

            if (id == null) return RedirectToAction("Portal");


            return View(await Task.Run(() => buscar(id.Value)));

        }

        [HttpPost]
        public async Task<IActionResult> Seleccionar(int codigo, int cantidad)
        {
            List<Registro> temporal = JsonConvert.DeserializeObject<List<Registro>>(HttpContext.Session.GetString("canasta"));
            bool oculta = false;

            Registro reg = temporal.FirstOrDefault(x => x.id_pizza == codigo);
            if (reg == null)
            {

                Pizza it = buscar(codigo);
                reg = new Registro()
                {
                    id_pizza = it.id_pizza,
                    nombrepizza = it.nombrepizza,
                    precio = it.precio,
                    descripcion = it.descripcion,
                    cantidad = cantidad
                };
                temporal.Add(reg);
                oculta = true;
            }
            else
            {
                reg.cantidad += cantidad;

            }

            ViewBag.DeshabilitarBoton = oculta;

            ViewBag.mensaje = "Ya se Registro La pizza";

            HttpContext.Session.SetString("canasta", JsonConvert.SerializeObject(temporal));

            return View(await Task.Run(() => buscar(codigo)));

        }

        public async Task<IActionResult> Canasta()

        {

            if (HttpContext.Session.GetString("canasta") == null)

                return RedirectToAction("Portal");
            IEnumerable<Registro> temporal =

              JsonConvert.DeserializeObject<IEnumerable<Registro>>(HttpContext.Session.GetString("canasta"));



            return View(await Task.Run(() => temporal));
        }

        public IActionResult Eliminar(int id)
        {
            List<Registro> temporal =

              JsonConvert.DeserializeObject<List<Registro>>(HttpContext.Session.GetString("canasta"));


            temporal.RemoveAll(x => x.id_pizza == id);


            HttpContext.Session.SetString("canasta", JsonConvert.SerializeObject(temporal));



            return RedirectToAction("Canasta");



        }

        public IActionResult Actualizar(int codigo, int cantidad)
        {

            List<Registro> canasta = JsonConvert.DeserializeObject<List<Registro>>(HttpContext.Session.GetString("canasta"));

            Registro Registro = canasta.FirstOrDefault(p => p.id_pizza == codigo);
            if (Registro != null)
            {
                Registro.cantidad = cantidad;
            }

            HttpContext.Session.SetString("canasta", JsonConvert.SerializeObject(canasta));

            return RedirectToAction("Canasta");
        }

        public IActionResult Compra()
        {
            int idusu = int.Parse(User.FindFirstValue("IdUsu"));
            List<Registro> temporal = JsonConvert.DeserializeObject<List<Registro>>(HttpContext.Session.GetString("canasta"));
            if (temporal.Count() == 0) return RedirectToAction("Portal", new { buscar = "" });
            string mensajes = "";
            SqlConnection cn = new SqlConnection(cadena);
            cn.Open();
            SqlTransaction t = cn.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {
                SqlCommand cmd = new SqlCommand("usp_add_pedido", cn, t);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idusu", idusu);

                SqlParameter idPedidoParam = new SqlParameter("@idpedido", SqlDbType.Int);
                idPedidoParam.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(idPedidoParam);
                cmd.ExecuteNonQuery();

                int idpedido = (int)idPedidoParam.Value;

                foreach (var item in temporal)
                {
                    cmd = new SqlCommand("usp_detallepedido", cn, t);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", idpedido);
                    cmd.Parameters.AddWithValue("@idpizza", item.id_pizza);
                    cmd.Parameters.AddWithValue("@cantidad", item.cantidad);
                    cmd.Parameters.AddWithValue("@pre", item.precio);
                    cmd.ExecuteNonQuery();
                }
                t.Commit();
                mensajes = $"Se ha registrado el pedido {idpedido}";
            }
            catch (SqlException ex)
            {
                mensajes = ex.Message;
                t.Rollback();
            }
            finally
            {
                cn.Close();
            }
            return RedirectToAction("Mensaje", new { msg = mensajes });
        }

        public IActionResult Mensaje(string msg = "")
        {
            HttpContext.Session.Remove("canasta");
            ViewBag.msg = msg;
            return View();
        }


        public async Task<IActionResult> HistorialPedido()
        {
            return View(await Task.Run(() => listadoHistorial()));
        }

        IEnumerable<HistorialPedido> listadoHistorial()
        {
            int idusu = int.Parse(User.FindFirstValue("IdUsu"));
            List<HistorialPedido> historial = new List<HistorialPedido>();
            using (SqlConnection connec = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("USP_Historial_pedido", connec);
                cmd.Parameters.AddWithValue("@idusuario", idusu);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                connec.Open();
                SqlDataReader rd = cmd.ExecuteReader();
                while (rd.Read())
                {
                    historial.Add(new HistorialPedido()
                    {
                        idpedido = rd.GetInt32(0),
                        fechapedido = rd.GetDateTime(1),
                        nombrepizza = rd.GetString(2),
                        cantidadpedido = rd.GetInt32(3),
                        precio = rd.GetDecimal(4)
                    });
                }
            }
            return historial;

        }

        //PRA GENERAR EXCEL
        public IActionResult GenerarExcel()
        {
            // Obtener los datos para el reporte
            IEnumerable<HistorialPedido> historial = listadoHistorial();

            // Crear el archivo Excel
            using (var package = new ExcelPackage())
            {
                // Crear una hoja de trabajo en el archivo
                var worksheet = package.Workbook.Worksheets.Add("Reporte");

                // Establecer los encabezados de las columnas
                var headers = new string[] { "ID Pedido", "Fecha Pedido", "Nombre Pizza", "Cantidad", "Precio" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Llenar los datos del reporte
                int row = 2;
                foreach (var item in historial)
                {
                    worksheet.Cells[row, 1].Value = item.idpedido;
                    worksheet.Cells[row, 2].Value = item.fechapedido;
                    worksheet.Cells[row, 3].Value = item.nombrepizza;
                    worksheet.Cells[row, 4].Value = item.cantidadpedido;
                    worksheet.Cells[row, 5].Value = item.precio;
                    row++;
                }

                // Establecer el formato de fecha en la columna de fecha pedido
                var fechaPedidoColumn = worksheet.Column(2);
                fechaPedidoColumn.Style.Numberformat.Format = "dd/MM/yyyy";

                // Autoajustar el ancho de las columnas
                worksheet.Cells.AutoFitColumns();

                // Establecer bordes para todo el rango de celdas
                var cells = worksheet.Cells[1, 1, row - 1, 5];
                var border = cells.Style.Border;
                border.Top.Style = border.Bottom.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;

                // Guardar el archivo Excel en la memoria
                var stream = new MemoryStream(package.GetAsByteArray());

                // Devolver el archivo Excel como respuesta
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "reporte.xlsx");
            }

        }
    }
}
