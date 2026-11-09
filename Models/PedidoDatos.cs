using Microsoft.Data.SqlClient;
using System.Data;

namespace TechStoreMVC.Models
{
    public class PedidoDatos
    {
        private readonly string conexion;

        public PedidoDatos(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("ConexionTechStore")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión ConexionTechStore.");
        }

        public async Task<int> RegistrarPedido(
            int idUsuario,
            List<DetallePedido> detalles)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El usuario no es válido.");

            if (detalles == null || detalles.Count == 0)
                throw new ArgumentException("El pedido no contiene productos.");

            using (SqlConnection connection = new SqlConnection(conexion))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaccion = connection.BeginTransaction())
                {
                    try
                    {
                        decimal total = 0;

                        foreach (var detalle in detalles)
                        {
                            if (detalle.IdProducto <= 0 || detalle.Cantidad <= 0)
                                throw new InvalidOperationException(
                                    "Uno de los productos del pedido no es válido.");

                            string consultaPrecio = @"
                                SELECT Precio
                                FROM Productos
                                WHERE IdProducto = @IdProducto";

                            using (SqlCommand command = new SqlCommand(
                                consultaPrecio, connection, transaccion))
                            {
                                command.Parameters.Add(
                                    "@IdProducto", SqlDbType.Int).Value =
                                    detalle.IdProducto;

                                object? resultado =
                                    await command.ExecuteScalarAsync();

                                if (resultado == null || resultado == DBNull.Value)
                                {
                                    throw new InvalidOperationException(
                                        "Uno de los productos ya no existe.");
                                }

                                detalle.PrecioUnitario =
                                    Convert.ToDecimal(resultado);

                                total += detalle.PrecioUnitario * detalle.Cantidad;
                            }
                        }

                        string consultaPedido = @"
                            INSERT INTO Pedidos
                                (IdUsuario, Total, Estado)
                            OUTPUT INSERTED.IdPedido
                            VALUES
                                (@IdUsuario, @Total, 'Pendiente')";

                        int idPedido;

                        using (SqlCommand command = new SqlCommand(
                            consultaPedido, connection, transaccion))
                        {
                            command.Parameters.Add(
                                "@IdUsuario", SqlDbType.Int).Value = idUsuario;

                            SqlParameter parametroTotal =
                                command.Parameters.Add(
                                    "@Total", SqlDbType.Decimal);

                            parametroTotal.Precision = 10;
                            parametroTotal.Scale = 2;
                            parametroTotal.Value = total;

                            idPedido = Convert.ToInt32(
                                await command.ExecuteScalarAsync());
                        }

                        string consultaDetalle = @"
                            INSERT INTO DetallePedido
                                (IdPedido, IdProducto, Cantidad, PrecioUnitario)
                            VALUES
                                (@IdPedido, @IdProducto, @Cantidad, @PrecioUnitario)";

                        foreach (var detalle in detalles)
                        {
                            using (SqlCommand command = new SqlCommand(
                                consultaDetalle, connection, transaccion))
                            {
                                command.Parameters.Add(
                                    "@IdPedido", SqlDbType.Int).Value = idPedido;

                                command.Parameters.Add(
                                    "@IdProducto", SqlDbType.Int).Value =
                                    detalle.IdProducto;

                                command.Parameters.Add(
                                    "@Cantidad", SqlDbType.Int).Value =
                                    detalle.Cantidad;

                                SqlParameter parametroPrecio =
                                    command.Parameters.Add(
                                        "@PrecioUnitario", SqlDbType.Decimal);

                                parametroPrecio.Precision = 10;
                                parametroPrecio.Scale = 2;
                                parametroPrecio.Value = detalle.PrecioUnitario;

                                await command.ExecuteNonQueryAsync();
                            }
                        }

                        transaccion.Commit();
                        return idPedido;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<List<Pedido>> ObtenerPedidosPorUsuario(int idUsuario)
        {
            List<Pedido> pedidos = new List<Pedido>();

            string consulta = @"
                SELECT
                    IdPedido,
                    IdUsuario,
                    FechaPedido,
                    Total,
                    Estado
                FROM Pedidos
                WHERE IdUsuario = @IdUsuario
                ORDER BY FechaPedido DESC";

            using (SqlConnection connection = new SqlConnection(conexion))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(consulta, connection))
                {
                    command.Parameters.Add(
                        "@IdUsuario", SqlDbType.Int).Value = idUsuario;

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            pedidos.Add(new Pedido
                            {
                                IdPedido = reader.GetInt32(0),
                                IdUsuario = reader.GetInt32(1),
                                FechaPedido = reader.GetDateTime(2),
                                Total = reader.GetDecimal(3),
                                Estado = reader.GetString(4)
                            });
                        }
                    }
                }
            }

            return pedidos;
        }

        public async Task<Pedido?> ObtenerPedidoPorId(
            int idPedido,
            int idUsuario)
        {
            Pedido? pedido = null;

            string consultaPedido = @"
                SELECT
                    IdPedido,
                    IdUsuario,
                    FechaPedido,
                    Total,
                    Estado
                FROM Pedidos
                WHERE IdPedido = @IdPedido
                  AND IdUsuario = @IdUsuario";

            using (SqlConnection connection = new SqlConnection(conexion))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(
                    consultaPedido, connection))
                {
                    command.Parameters.Add(
                        "@IdPedido", SqlDbType.Int).Value = idPedido;

                    command.Parameters.Add(
                        "@IdUsuario", SqlDbType.Int).Value = idUsuario;

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            pedido = new Pedido
                            {
                                IdPedido = reader.GetInt32(0),
                                IdUsuario = reader.GetInt32(1),
                                FechaPedido = reader.GetDateTime(2),
                                Total = reader.GetDecimal(3),
                                Estado = reader.GetString(4)
                            };
                        }
                    }
                }

                if (pedido == null)
                    return null;

                string consultaDetalles = @"
                    SELECT
                        dp.IdDetalle,
                        dp.IdPedido,
                        dp.IdProducto,
                        p.Nombre,
                        dp.Cantidad,
                        dp.PrecioUnitario
                    FROM DetallePedido dp
                    INNER JOIN Productos p
                        ON dp.IdProducto = p.IdProducto
                    WHERE dp.IdPedido = @IdPedido";

                using (SqlCommand command = new SqlCommand(
                    consultaDetalles, connection))
                {
                    command.Parameters.Add(
                        "@IdPedido", SqlDbType.Int).Value = idPedido;

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            pedido.Detalles.Add(new DetallePedido
                            {
                                IdDetalle = reader.GetInt32(0),
                                IdPedido = reader.GetInt32(1),
                                IdProducto = reader.GetInt32(2),
                                NombreProducto = reader.GetString(3),
                                Cantidad = reader.GetInt32(4),
                                PrecioUnitario = reader.GetDecimal(5)
                            });
                        }
                    }
                }
            }

            return pedido;
        }

        public async Task<List<Pedido>> ObtenerTodosLosPedidos()
        {
            List<Pedido> pedidos = new List<Pedido>();

            string consulta = @"
                SELECT
                    p.IdPedido,
                    p.IdUsuario,
                    p.FechaPedido,
                    p.Total,
                    p.Estado,
                    u.Nombre,
                    u.Correo
                FROM Pedidos p
                INNER JOIN Usuarios u
                    ON p.IdUsuario = u.IdUsuario
                ORDER BY p.FechaPedido DESC";

            using (SqlConnection connection = new SqlConnection(conexion))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(consulta, connection))
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        pedidos.Add(new Pedido
                        {
                            IdPedido = reader.GetInt32(0),
                            IdUsuario = reader.GetInt32(1),
                            FechaPedido = reader.GetDateTime(2),
                            Total = reader.GetDecimal(3),
                            Estado = reader.GetString(4),
                            NombreCliente = reader.GetString(5),
                            CorreoCliente = reader.GetString(6)
                        });
                    }
                }
            }

            return pedidos;
        }

        public async Task<bool> ActualizarEstadoPedido(
            int idPedido,
            string nuevoEstado)
        {
            string[] estadosPermitidos =
            {
                "Pendiente",
                "Procesando",
                "Enviado",
                "Entregado",
                "Cancelado"
            };

            if (idPedido <= 0 || !estadosPermitidos.Contains(nuevoEstado))
                return false;

            string consulta = @"
                UPDATE Pedidos
                SET Estado = @Estado
                WHERE IdPedido = @IdPedido";

            using (SqlConnection connection = new SqlConnection(conexion))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(consulta, connection))
                {
                    command.Parameters.Add(
                        "@Estado", SqlDbType.NVarChar, 30).Value = nuevoEstado;

                    command.Parameters.Add(
                        "@IdPedido", SqlDbType.Int).Value = idPedido;

                    int filasAfectadas = await command.ExecuteNonQueryAsync();

                    return filasAfectadas > 0;
                }
            }
        }
    }
}
