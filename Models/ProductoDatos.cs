using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TechStoreMVC.Models
{
    public class ProductoDatos
    {
        private readonly string conexion;

        public ProductoDatos(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("ConexionTechStore")!;
        }

        public async Task<List<Producto>> ObtenerProductos()
        {
            List<Producto> productos = new List<Producto>();

            using (SqlConnection connection = new SqlConnection(conexion))
            {
                await connection.OpenAsync();

                string consulta = @"
                    SELECT
                        Productos.IdProducto,
                        Productos.Nombre,
                        Productos.Descripcion,
                        Productos.Precio,
                        Productos.Icono,
                        Categorias.Nombre AS Categoria
                    FROM Productos
                    INNER JOIN Categorias
                        ON Productos.IdCategoria = Categorias.IdCategoria";

                using (SqlCommand command = new SqlCommand(consulta, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Producto producto = new Producto();

                            producto.IdProducto = reader.GetInt32(0);
                            producto.Nombre = reader.GetString(1);
                            producto.Descripcion = reader.IsDBNull(2)
                                ? ""
                                : reader.GetString(2);
                            producto.Precio = reader.GetDecimal(3);
                            producto.Icono = reader.IsDBNull(4)
                                ? ""
                                : reader.GetString(4);
                            producto.Categoria = reader.GetString(5);

                            productos.Add(producto);
                        }
                    }
                }
            }

            return productos;
        }
    }
}
