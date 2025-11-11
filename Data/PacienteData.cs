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
    public class PacienteData
    {
        private readonly string _connectionString;

        public PacienteData(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("GecoConnection");
        }

        public List<PacienteDto> ListarTodos(bool soloActivos = true)
        {
            List<PacienteDto> pacientes = new List<PacienteDto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ListarPacientes", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SoloActivos", soloActivos);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pacientes.Add(MapearPacienteDesdeReader(reader));
                        }
                    }
                }
            }

            return pacientes;
        }

        public PacienteDto ObtenerPorId(int pacienteId)
        {
            PacienteDto paciente = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ObtenerPacientePorId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PacienteId", pacienteId);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            paciente = MapearPacienteDesdeReader(reader);
                        }
                    }
                }
            }

            return paciente;
        }

        public int Crear(CrearPacienteDto dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_CrearPaciente", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    AgregarParametrosPaciente(cmd, dto);

                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        public bool Actualizar(ActualizarPacienteDto dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ActualizarPaciente", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PacienteId", dto.PacienteId);
                    AgregarParametrosPaciente(cmd, dto);
                    cmd.Parameters.AddWithValue("@Activo", dto.Activo);

                    conn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }

        public bool Eliminar(int pacienteId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_EliminarPaciente", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PacienteId", pacienteId);

                    conn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }

        public List<PacienteDto> Buscar(string termino, bool soloActivos = true)
        {
            List<PacienteDto> pacientes = new List<PacienteDto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_BuscarPacientes", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Termino", termino);
                    cmd.Parameters.AddWithValue("@SoloActivos", soloActivos);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pacientes.Add(MapearPacienteDesdeReader(reader));
                        }
                    }
                }
            }

            return pacientes;
        }

        public bool ExisteDocumento(string tipoDocumento, string numeroDocumento, int? pacienteIdExcluir = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM Pacientes 
                    WHERE TipoDocumento = @TipoDocumento 
                        AND NumeroDocumento = @NumeroDocumento
                        AND (@PacienteIdExcluir IS NULL OR PacienteId != @PacienteIdExcluir)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TipoDocumento", tipoDocumento);
                    cmd.Parameters.AddWithValue("@NumeroDocumento", numeroDocumento);
                    cmd.Parameters.AddWithValue("@PacienteIdExcluir",
                        pacienteIdExcluir.HasValue ? (object)pacienteIdExcluir.Value : DBNull.Value);

                    conn.Open();

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        private void AgregarParametrosPaciente(SqlCommand cmd, dynamic dto)
        {
            cmd.Parameters.AddWithValue("@Nombre", dto.Nombre);
            cmd.Parameters.AddWithValue("@Apellido", dto.Apellido);
            cmd.Parameters.AddWithValue("@TipoDocumento", dto.TipoDocumento);
            cmd.Parameters.AddWithValue("@NumeroDocumento", dto.NumeroDocumento);
            cmd.Parameters.AddWithValue("@FechaNacimiento",
                dto.FechaNacimiento.HasValue ? (object)dto.FechaNacimiento.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Sexo",
                string.IsNullOrWhiteSpace(dto.Sexo) ? (object)DBNull.Value : dto.Sexo);
            cmd.Parameters.AddWithValue("@Telefono",
                string.IsNullOrWhiteSpace(dto.Telefono) ? (object)DBNull.Value : dto.Telefono);
            cmd.Parameters.AddWithValue("@TelefonoAlternativo",
                string.IsNullOrWhiteSpace(dto.TelefonoAlternativo) ? (object)DBNull.Value : dto.TelefonoAlternativo);
            cmd.Parameters.AddWithValue("@Email",
                string.IsNullOrWhiteSpace(dto.Email) ? (object)DBNull.Value : dto.Email);
            cmd.Parameters.AddWithValue("@Direccion",
                string.IsNullOrWhiteSpace(dto.Direccion) ? (object)DBNull.Value : dto.Direccion);
            cmd.Parameters.AddWithValue("@Localidad",
                string.IsNullOrWhiteSpace(dto.Localidad) ? (object)DBNull.Value : dto.Localidad);
            cmd.Parameters.AddWithValue("@Provincia",
                string.IsNullOrWhiteSpace(dto.Provincia) ? (object)DBNull.Value : dto.Provincia);
            cmd.Parameters.AddWithValue("@CodigoPostal",
                string.IsNullOrWhiteSpace(dto.CodigoPostal) ? (object)DBNull.Value : dto.CodigoPostal);
            cmd.Parameters.AddWithValue("@ObraSocialId",
                dto.ObraSocialId.HasValue ? (object)dto.ObraSocialId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@PlanId",
                dto.PlanId.HasValue ? (object)dto.PlanId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@NumeroAfiliado",
                string.IsNullOrWhiteSpace(dto.NumeroAfiliado) ? (object)DBNull.Value : dto.NumeroAfiliado);
            cmd.Parameters.AddWithValue("@Observaciones",
                string.IsNullOrWhiteSpace(dto.Observaciones) ? (object)DBNull.Value : dto.Observaciones);
        }

        private PacienteDto MapearPacienteDesdeReader(SqlDataReader reader)
        {
            return new PacienteDto
            {
                PacienteId = reader.GetInt32(reader.GetOrdinal("PacienteId")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Apellido = reader.GetString(reader.GetOrdinal("Apellido")),
                TipoDocumento = reader.GetString(reader.GetOrdinal("TipoDocumento")),
                NumeroDocumento = reader.GetString(reader.GetOrdinal("NumeroDocumento")),
                FechaNacimiento = reader.IsDBNull(reader.GetOrdinal("FechaNacimiento"))
                    ? null : reader.GetDateTime(reader.GetOrdinal("FechaNacimiento")),
                Sexo = reader.IsDBNull(reader.GetOrdinal("Sexo"))
                    ? null : reader.GetString(reader.GetOrdinal("Sexo")),
                Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono"))
                    ? null : reader.GetString(reader.GetOrdinal("Telefono")),
                TelefonoAlternativo = reader.IsDBNull(reader.GetOrdinal("TelefonoAlternativo"))
                    ? null : reader.GetString(reader.GetOrdinal("TelefonoAlternativo")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                    ? null : reader.GetString(reader.GetOrdinal("Email")),
                Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion"))
                    ? null : reader.GetString(reader.GetOrdinal("Direccion")),
                Localidad = reader.IsDBNull(reader.GetOrdinal("Localidad"))
                    ? null : reader.GetString(reader.GetOrdinal("Localidad")),
                Provincia = reader.IsDBNull(reader.GetOrdinal("Provincia"))
                    ? null : reader.GetString(reader.GetOrdinal("Provincia")),
                CodigoPostal = reader.IsDBNull(reader.GetOrdinal("CodigoPostal"))
                    ? null : reader.GetString(reader.GetOrdinal("CodigoPostal")),
                ObraSocialId = reader.IsDBNull(reader.GetOrdinal("ObraSocialId"))
                    ? null : reader.GetInt32(reader.GetOrdinal("ObraSocialId")),
                PlanId = reader.IsDBNull(reader.GetOrdinal("PlanId"))
                    ? null : reader.GetInt32(reader.GetOrdinal("PlanId")),
                NumeroAfiliado = reader.IsDBNull(reader.GetOrdinal("NumeroAfiliado"))
                    ? null : reader.GetString(reader.GetOrdinal("NumeroAfiliado")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                FechaAlta = reader.GetDateTime(reader.GetOrdinal("FechaAlta")),
                Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones"))
                    ? null : reader.GetString(reader.GetOrdinal("Observaciones")),
                ObraSocialNombre = reader.IsDBNull(reader.GetOrdinal("ObraSocialNombre"))
                    ? null : reader.GetString(reader.GetOrdinal("ObraSocialNombre")),
                PlanNombre = reader.IsDBNull(reader.GetOrdinal("PlanNombre"))
                    ? null : reader.GetString(reader.GetOrdinal("PlanNombre"))
            };
        }
    }
}