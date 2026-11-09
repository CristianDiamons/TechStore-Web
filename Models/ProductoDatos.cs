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
                        Productos.IdCategoria,
                        Productos.Icono,
                        Categorias.Nombre AS Categoria
                    FROM Productos
                    INNER JOIN Categorias
                        ON Productos.IdCategoria = Categorias.IdCategoria";

                using (SqlCommand command = new SqlCommand(consulta, connection))
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Producto producto = new Producto
                        {
                            IdProducto = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Descripcion = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Precio = reader.GetDecimal(3),
                            IdCategoria = reader.GetInt32(4),
                            Icono = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            Categoria = reader.GetString(6)
                        };

                        producto.Imagen = ObtenerImagen(producto.IdProducto);

                        productos.Add(producto);
                    }
                }
            }

            return productos;
        }

        public async Task<Producto?> ObtenerProductoPorId(int id)
        {
            Producto? producto = null;

            using (SqlConnection connection = new SqlConnection(conexion))
            {
                await connection.OpenAsync();

                string consulta = @"
                    SELECT
                        Productos.IdProducto,
                        Productos.Nombre,
                        Productos.Descripcion,
                        Productos.Precio,
                        Productos.IdCategoria,
                        Productos.Icono,
                        Categorias.Nombre AS Categoria
                    FROM Productos
                    INNER JOIN Categorias
                        ON Productos.IdCategoria = Categorias.IdCategoria
                    WHERE Productos.IdProducto = @IdProducto";

                using (SqlCommand command = new SqlCommand(consulta, connection))
                {
                    command.Parameters.AddWithValue("@IdProducto", id);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            producto = new Producto
                            {
                                IdProducto = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Descripcion = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Precio = reader.GetDecimal(3),
                                IdCategoria = reader.GetInt32(4),
                                Icono = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                Categoria = reader.GetString(6)
                            };

                            producto.Imagen = ObtenerImagen(producto.IdProducto);
                        }
                    }
                }
            }

            return producto;
        }

        public async Task<List<Categoria>> ObtenerCategorias()
        {
            List<Categoria> categorias = new List<Categoria>();

            using (SqlConnection connection = new SqlConnection(conexion))
            {
                await connection.OpenAsync();

                string consulta = @"
                    SELECT IdCategoria, Nombre
                    FROM Categorias
                    ORDER BY Nombre";

                using (SqlCommand command = new SqlCommand(consulta, connection))
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        categorias.Add(new Categoria
                        {
                            IdCategoria = reader.GetInt32(0),
                            Nombre = reader.GetString(1)
                        });
                    }
                }
            }

            return categorias;
        }

        public async Task<int> RegistrarProducto(Producto producto)
        {
            using (SqlConnection connection = new SqlConnection(conexion))
            {
                await connection.OpenAsync();

                string consulta = @"
                    INSERT INTO Productos
                        (Nombre, Descripcion, Precio, IdCategoria, Icono)
                    VALUES
                        (@Nombre, @Descripcion, @Precio, @IdCategoria, @Icono);

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (SqlCommand command = new SqlCommand(consulta, connection))
                {
                    command.Parameters.AddWithValue("@Nombre", producto.Nombre);

                    command.Parameters.AddWithValue(
                        "@Descripcion",
                        string.IsNullOrWhiteSpace(producto.Descripcion)
                            ? DBNull.Value
                            : producto.Descripcion);

                    command.Parameters.AddWithValue("@Precio", producto.Precio);
                    command.Parameters.AddWithValue("@IdCategoria", producto.IdCategoria);

                    command.Parameters.AddWithValue(
                        "@Icono",
                        string.IsNullOrWhiteSpace(producto.Icono)
                            ? DBNull.Value
                            : producto.Icono);

                    object? resultado = await command.ExecuteScalarAsync();

                    return Convert.ToInt32(resultado);
                }
            }
        }

        public async Task<bool> EditarProducto(Producto producto)
        {
            using (SqlConnection connection = new SqlConnection(conexion))
            {
                await connection.OpenAsync();

                string consulta = @"
                    UPDATE Productos
                    SET
                        Nombre = @Nombre,
                        Descripcion = @Descripcion,
                        Precio = @Precio,
                        IdCategoria = @IdCategoria,
                        Icono = @Icono
                    WHERE IdProducto = @IdProducto";

                using (SqlCommand command = new SqlCommand(consulta, connection))
                {
                    command.Parameters.AddWithValue("@IdProducto", producto.IdProducto);
                    command.Parameters.AddWithValue("@Nombre", producto.Nombre);

                    command.Parameters.AddWithValue(
                        "@Descripcion",
                        string.IsNullOrWhiteSpace(producto.Descripcion)
                            ? DBNull.Value
                            : producto.Descripcion);

                    command.Parameters.AddWithValue("@Precio", producto.Precio);
                    command.Parameters.AddWithValue("@IdCategoria", producto.IdCategoria);

                    command.Parameters.AddWithValue(
                        "@Icono",
                        string.IsNullOrWhiteSpace(producto.Icono)
                            ? DBNull.Value
                            : producto.Icono);

                    int filas = await command.ExecuteNonQueryAsync();

                    return filas > 0;
                }
            }
        }

        public async Task<bool> EliminarProducto(int id)
        {
            using (SqlConnection connection = new SqlConnection(conexion))
            {
                await connection.OpenAsync();

                string consulta = @"
                    DELETE FROM Productos
                    WHERE IdProducto = @IdProducto";

                using (SqlCommand command = new SqlCommand(consulta, connection))
                {
                    command.Parameters.AddWithValue("@IdProducto", id);

                    int filas = await command.ExecuteNonQueryAsync();

                    return filas > 0;
                }
            }
        }

        private string ObtenerImagen(int idProducto)
        {
            return idProducto switch
            {
                1 => "teclado-rgb.jpg",
                2 => "mouse-gamer.jpg",
                3 => "audifonos.jpg",
                4 => "memoria-usb.jpg",
                5 => "mousepad.jpg",
                6 => "teclado-inalambrico.jpg",
                7 => "mouse-inalambrico.jpg",
                8 => "bocinas.jpg",
                _ => ""
            };
        }
    }
}
