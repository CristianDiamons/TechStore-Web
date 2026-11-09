using Microsoft.Data.SqlClient;

namespace TechStoreMVC.Models
{
    public class UsuarioDatos
    {
        private readonly string conexion;

        public UsuarioDatos(IConfiguration configuration)
        {
            conexion = configuration.GetConnectionString("ConexionTechStore")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión ConexionTechStore.");
        }

        public async Task<bool> CorreoExiste(string correo)
        {
            using (SqlConnection connection = new SqlConnection(conexion))
            {
                await connection.OpenAsync();

                string consulta = @"
                    SELECT COUNT(1)
                    FROM Usuarios
                    WHERE Correo = @Correo";

                using (SqlCommand command = new SqlCommand(consulta, connection))
                {
                    command.Parameters.AddWithValue("@Correo", correo);

                    int cantidad = Convert.ToInt32(
                        await command.ExecuteScalarAsync());

                    return cantidad > 0;
                }
            }
        }

        public async Task<Usuario?> ObtenerUsuarioPorCorreo(string correo)
        {
            Usuario? usuario = null;

            using (SqlConnection connection = new SqlConnection(conexion))
            {
                await connection.OpenAsync();

                string consulta = @"
                    SELECT
                        IdUsuario,
                        Nombre,
                        Correo,
                        ContrasenaHash,
                        Rol,
                        FechaRegistro
                    FROM Usuarios
                    WHERE Correo = @Correo";

                using (SqlCommand command = new SqlCommand(consulta, connection))
                {
                    command.Parameters.AddWithValue("@Correo", correo);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            usuario = new Usuario
                            {
                                IdUsuario = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Correo = reader.GetString(2),
                                ContrasenaHash = reader.GetString(3),
                                Rol = reader.GetString(4),
                                FechaRegistro = reader.GetDateTime(5)
                            };
                        }
                    }
                }
            }

            return usuario;
        }

        public async Task<bool> RegistrarUsuario(Usuario usuario)
        {
            using (SqlConnection connection = new SqlConnection(conexion))
            {
                await connection.OpenAsync();

                string consulta = @"
                    INSERT INTO Usuarios
                        (Nombre, Correo, ContrasenaHash, Rol)
                    VALUES
                        (@Nombre, @Correo, @ContrasenaHash, @Rol)";

                using (SqlCommand command = new SqlCommand(consulta, connection))
                {
                    command.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                    command.Parameters.AddWithValue("@Correo", usuario.Correo);

                    command.Parameters.AddWithValue(
                        "@ContrasenaHash", usuario.ContrasenaHash);

                    command.Parameters.AddWithValue("@Rol", "Cliente");

                    int filas = await command.ExecuteNonQueryAsync();

                    return filas > 0;
                }
            }
        }
    }
}
