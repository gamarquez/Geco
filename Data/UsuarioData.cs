using Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Data
{
    /// <summary>
    /// Capa de acceso a datos para Usuarios
    /// </summary>
    public class UsuarioData
    {
        private readonly string _connectionString;

        public UsuarioData(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("GecoConnection");
        }

        /// <summary>
        /// Obtiene un usuario por nombre de usuario
        /// </summary>
        public UsuarioDto ObtenerPorNombreUsuario(string nombreUsuario)
        {
            UsuarioDto usuario = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ObtenerUsuarioPorNombre", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = MapearUsuarioDesdeReader(reader);
                        }
                    }
                }
            }

            return usuario;
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        public int Crear(CrearUsuarioDto dto, string passwordHash)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_CrearUsuario", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@NombreUsuario", dto.NombreUsuario);
                    cmd.Parameters.AddWithValue("@Email", dto.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    cmd.Parameters.AddWithValue("@Nombre", dto.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", dto.Apellido);
                    cmd.Parameters.AddWithValue("@TipoUsuario", dto.TipoUsuario);

                    if (dto.ProfesionalId.HasValue)
                        cmd.Parameters.AddWithValue("@ProfesionalId", dto.ProfesionalId.Value);
                    else
                        cmd.Parameters.AddWithValue("@ProfesionalId", DBNull.Value);

                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        /// <summary>
        /// Actualiza la fecha de último acceso
        /// </summary>
        public void ActualizarUltimoAcceso(int usuarioId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ActualizarUltimoAcceso", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        public bool CambiarPassword(int usuarioId, string nuevoPasswordHash)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_CambiarPassword", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                    cmd.Parameters.AddWithValue("@NuevoPasswordHash", nuevoPasswordHash);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int filasActualizadas = reader.GetInt32(0);
                            return filasActualizadas > 0;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Lista todos los usuarios
        /// </summary>
        public List<UsuarioDto> ListarTodos(bool soloActivos = true)
        {
            List<UsuarioDto> usuarios = new List<UsuarioDto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ListarUsuarios", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SoloActivos", soloActivos);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usuarios.Add(MapearUsuarioDesdeReader(reader));
                        }
                    }
                }
            }

            return usuarios;
        }

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        public UsuarioDto ObtenerPorId(int usuarioId)
        {
            var usuarios = ListarTodos(false);
            return usuarios.Find(u => u.UsuarioId == usuarioId);
        }

        /// <summary>
        /// Mapea un SqlDataReader a UsuarioDto
        /// </summary>
        private UsuarioDto MapearUsuarioDesdeReader(SqlDataReader reader)
        {
            return new UsuarioDto
            {
                UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                NombreUsuario = reader.GetString(reader.GetOrdinal("NombreUsuario")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
                TipoUsuario = reader.GetString(reader.GetOrdinal("TipoUsuario")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                UltimoAcceso = reader.IsDBNull(reader.GetOrdinal("UltimoAcceso"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("UltimoAcceso")),
                ProfesionalId = reader.IsDBNull(reader.GetOrdinal("ProfesionalId"))
                    ? null
                    : reader.GetInt32(reader.GetOrdinal("ProfesionalId"))
            };
        }
    }
}

