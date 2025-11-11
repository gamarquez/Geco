using Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    /// <summary>
    /// Capa de acceso a datos para Profesionales
    /// </summary>
    public class ProfesionalData
    {
        private readonly string _connectionString;

        public ProfesionalData(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("GecoConnection");
        }

        /// <summary>
        /// Lista todos los profesionales
        /// </summary>
        public List<ProfesionalDto> ListarTodos(bool soloActivos = true)
        {
            List<ProfesionalDto> profesionales = new List<ProfesionalDto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ListarProfesionales", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SoloActivos", soloActivos);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            profesionales.Add(MapearProfesionalDesdeReader(reader));
                        }
                    }
                }
            }

            return profesionales;
        }

        /// <summary>
        /// Obtiene un profesional por ID
        /// </summary>
        public ProfesionalDto ObtenerPorId(int profesionalId)
        {
            ProfesionalDto profesional = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ObtenerProfesionalPorId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProfesionalId", profesionalId);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            profesional = MapearProfesionalDesdeReader(reader);
                        }
                    }
                }
            }

            return profesional;
        }

        /// <summary>
        /// Crea un nuevo profesional
        /// </summary>
        public int Crear(CrearProfesionalDto dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_CrearProfesional", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Nombre", dto.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", dto.Apellido);
                    cmd.Parameters.AddWithValue("@Matricula", dto.Matricula);
                    cmd.Parameters.AddWithValue("@Especialidad",
                        string.IsNullOrWhiteSpace(dto.Especialidad) ? (object)DBNull.Value : dto.Especialidad);
                    cmd.Parameters.AddWithValue("@Telefono",
                        string.IsNullOrWhiteSpace(dto.Telefono) ? (object)DBNull.Value : dto.Telefono);
                    cmd.Parameters.AddWithValue("@Email",
                        string.IsNullOrWhiteSpace(dto.Email) ? (object)DBNull.Value : dto.Email);
                    cmd.Parameters.AddWithValue("@Direccion",
                        string.IsNullOrWhiteSpace(dto.Direccion) ? (object)DBNull.Value : dto.Direccion);
                    cmd.Parameters.AddWithValue("@Observaciones",
                        string.IsNullOrWhiteSpace(dto.Observaciones) ? (object)DBNull.Value : dto.Observaciones);

                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        /// <summary>
        /// Actualiza un profesional existente
        /// </summary>
        public bool Actualizar(ActualizarProfesionalDto dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ActualizarProfesional", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ProfesionalId", dto.ProfesionalId);
                    cmd.Parameters.AddWithValue("@Nombre", dto.Nombre);
                    cmd.Parameters.AddWithValue("@Apellido", dto.Apellido);
                    cmd.Parameters.AddWithValue("@Matricula", dto.Matricula);
                    cmd.Parameters.AddWithValue("@Especialidad",
                        string.IsNullOrWhiteSpace(dto.Especialidad) ? (object)DBNull.Value : dto.Especialidad);
                    cmd.Parameters.AddWithValue("@Telefono",
                        string.IsNullOrWhiteSpace(dto.Telefono) ? (object)DBNull.Value : dto.Telefono);
                    cmd.Parameters.AddWithValue("@Email",
                        string.IsNullOrWhiteSpace(dto.Email) ? (object)DBNull.Value : dto.Email);
                    cmd.Parameters.AddWithValue("@Direccion",
                        string.IsNullOrWhiteSpace(dto.Direccion) ? (object)DBNull.Value : dto.Direccion);
                    cmd.Parameters.AddWithValue("@Observaciones",
                        string.IsNullOrWhiteSpace(dto.Observaciones) ? (object)DBNull.Value : dto.Observaciones);
                    cmd.Parameters.AddWithValue("@Activo", dto.Activo);

                    conn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }

        /// <summary>
        /// Elimina (desactiva) un profesional
        /// </summary>
        public bool Eliminar(int profesionalId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_EliminarProfesional", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProfesionalId", profesionalId);

                    conn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }

        /// <summary>
        /// Verifica si existe una matrícula
        /// </summary>
        public bool ExisteMatricula(string matricula, int? profesionalIdExcluir = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM Profesionales 
                    WHERE Matricula = @Matricula 
                        AND (@ProfesionalIdExcluir IS NULL OR ProfesionalId != @ProfesionalIdExcluir)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Matricula", matricula);
                    cmd.Parameters.AddWithValue("@ProfesionalIdExcluir",
                        profesionalIdExcluir.HasValue ? (object)profesionalIdExcluir.Value : DBNull.Value);

                    conn.Open();

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        /// <summary>
        /// Busca profesionales por nombre, apellido o matrícula
        /// </summary>
        public List<ProfesionalDto> Buscar(string termino, bool soloActivos = true)
        {
            List<ProfesionalDto> profesionales = new List<ProfesionalDto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT * FROM Profesionales
                    WHERE (@SoloActivos = 0 OR Activo = 1)
                        AND (
                            Nombre LIKE @Termino 
                            OR Apellido LIKE @Termino 
                            OR Matricula LIKE @Termino
                            OR Especialidad LIKE @Termino
                        )
                    ORDER BY Apellido, Nombre";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Termino", "%" + termino + "%");
                    cmd.Parameters.AddWithValue("@SoloActivos", soloActivos);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            profesionales.Add(MapearProfesionalDesdeReader(reader));
                        }
                    }
                }
            }

            return profesionales;
        }

        /// <summary>
        /// Mapea un SqlDataReader a ProfesionalDto
        /// </summary>
        private ProfesionalDto MapearProfesionalDesdeReader(SqlDataReader reader)
        {
            return new ProfesionalDto
            {
                ProfesionalId = reader.GetInt32(reader.GetOrdinal("ProfesionalId")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
                Matricula = reader.GetString(reader.GetOrdinal("Matricula")),
                Especialidad = reader.IsDBNull(reader.GetOrdinal("Especialidad"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Especialidad")),
                Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Telefono")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Email")),
                Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Direccion")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                FechaAlta = reader.GetDateTime(reader.GetOrdinal("FechaAlta")),
                Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Observaciones"))
            };
        }
    }
}