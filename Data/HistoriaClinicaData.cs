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
    /// Capa de acceso a datos para Historias Clínicas
    /// </summary>
    public class HistoriaClinicaData
    {
        private readonly string _connectionString;

        public HistoriaClinicaData(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("GecoConnection");
        }

        /// <summary>
        /// Lista historias clínicas con filtros opcionales
        /// </summary>
        public List<HistoriaClinicaDto> Listar(HistoriaClinicaFiltroDto filtro, out int totalRegistros)
        {
            List<HistoriaClinicaDto> historias = new List<HistoriaClinicaDto>();
            totalRegistros = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ListarHistoriasClinicas", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PacienteId",
                        filtro.PacienteId.HasValue ? (object)filtro.PacienteId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProfesionalId",
                        filtro.ProfesionalId.HasValue ? (object)filtro.ProfesionalId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaDesde",
                        filtro.FechaDesde.HasValue ? (object)filtro.FechaDesde.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaHasta",
                        filtro.FechaHasta.HasValue ? (object)filtro.FechaHasta.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Diagnostico",
                        string.IsNullOrWhiteSpace(filtro.Diagnostico) ? (object)DBNull.Value : filtro.Diagnostico);
                    cmd.Parameters.AddWithValue("@PageNumber", filtro.PageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", filtro.PageSize);
                    cmd.Parameters.AddWithValue("@SoloActivas", filtro.SoloActivas);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            historias.Add(MapearHistoriaClinicaDesdeReader(reader));
                        }

                        // Leer el total de registros
                        if (reader.NextResult() && reader.Read())
                        {
                            totalRegistros = reader.GetInt32(0);
                        }
                    }
                }
            }

            return historias;
        }

        /// <summary>
        /// Obtiene una historia clínica por ID
        /// </summary>
        public HistoriaClinicaDto ObtenerPorId(int historiaClinicaId)
        {
            HistoriaClinicaDto historia = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ObtenerHistoriaClinicaPorId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@HistoriaClinicaId", historiaClinicaId);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            historia = MapearHistoriaClinicaDesdeReader(reader);
                        }
                    }
                }
            }

            return historia;
        }

        /// <summary>
        /// Obtiene el historial completo de un paciente
        /// </summary>
        public List<HistoriaClinicaDto> ObtenerHistorialPaciente(int pacienteId)
        {
            List<HistoriaClinicaDto> historias = new List<HistoriaClinicaDto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ObtenerHistorialPaciente", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PacienteId", pacienteId);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            historias.Add(MapearHistoriaClinicaDesdeReader(reader));
                        }
                    }
                }
            }

            return historias;
        }

        /// <summary>
        /// Crea una nueva historia clínica
        /// </summary>
        public int Crear(CrearHistoriaClinicaDto dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_CrearHistoriaClinica", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    AgregarParametrosHistoriaClinica(cmd, dto);

                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        /// <summary>
        /// Actualiza una historia clínica existente
        /// </summary>
        public bool Actualizar(ActualizarHistoriaClinicaDto dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ActualizarHistoriaClinica", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@HistoriaClinicaId", dto.HistoriaClinicaId);
                    AgregarParametrosHistoriaClinica(cmd, dto);
                    cmd.Parameters.AddWithValue("@Activo", dto.Activo);

                    conn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }

        /// <summary>
        /// Elimina (desactiva) una historia clínica
        /// </summary>
        public bool Eliminar(int historiaClinicaId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_EliminarHistoriaClinica", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@HistoriaClinicaId", historiaClinicaId);

                    conn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }

        /// <summary>
        /// Agrega parámetros comunes de historia clínica al comando
        /// </summary>
        private void AgregarParametrosHistoriaClinica(SqlCommand cmd, dynamic dto)
        {
            cmd.Parameters.AddWithValue("@PacienteId", dto.PacienteId);
            cmd.Parameters.AddWithValue("@ProfesionalId", dto.ProfesionalId);
            cmd.Parameters.AddWithValue("@FechaConsulta", dto.FechaConsulta);
            cmd.Parameters.AddWithValue("@MotivoConsulta", dto.MotivoConsulta);
            cmd.Parameters.AddWithValue("@Anamnesis",
                string.IsNullOrWhiteSpace(dto.Anamnesis) ? (object)DBNull.Value : dto.Anamnesis);
            cmd.Parameters.AddWithValue("@ExamenFisico",
                string.IsNullOrWhiteSpace(dto.ExamenFisico) ? (object)DBNull.Value : dto.ExamenFisico);
            cmd.Parameters.AddWithValue("@Diagnostico", dto.Diagnostico);
            cmd.Parameters.AddWithValue("@Tratamiento",
                string.IsNullOrWhiteSpace(dto.Tratamiento) ? (object)DBNull.Value : dto.Tratamiento);
            cmd.Parameters.AddWithValue("@Observaciones",
                string.IsNullOrWhiteSpace(dto.Observaciones) ? (object)DBNull.Value : dto.Observaciones);
            cmd.Parameters.AddWithValue("@Peso",
                dto.Peso.HasValue ? (object)dto.Peso.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Altura",
                dto.Altura.HasValue ? (object)dto.Altura.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@PresionArterial",
                dto.PresionArterial.HasValue ? (object)dto.PresionArterial.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Temperatura",
                dto.Temperatura.HasValue ? (object)dto.Temperatura.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@FrecuenciaCardiaca",
                dto.FrecuenciaCardiaca.HasValue ? (object)dto.FrecuenciaCardiaca.Value : DBNull.Value);
        }

        /// <summary>
        /// Mapea un SqlDataReader a HistoriaClinicaDto
        /// </summary>
        private HistoriaClinicaDto MapearHistoriaClinicaDesdeReader(SqlDataReader reader)
        {
            return new HistoriaClinicaDto
            {
                HistoriaClinicaId = reader.GetInt32(reader.GetOrdinal("HistoriaClinicaId")),
                PacienteId = reader.GetInt32(reader.GetOrdinal("PacienteId")),
                ProfesionalId = reader.GetInt32(reader.GetOrdinal("ProfesionalId")),
                FechaConsulta = reader.GetDateTime(reader.GetOrdinal("FechaConsulta")),
                MotivoConsulta = reader.GetString(reader.GetOrdinal("MotivoConsulta")),
                Anamnesis = reader.IsDBNull(reader.GetOrdinal("Anamnesis"))
                    ? null : reader.GetString(reader.GetOrdinal("Anamnesis")),
                ExamenFisico = reader.IsDBNull(reader.GetOrdinal("ExamenFisico"))
                    ? null : reader.GetString(reader.GetOrdinal("ExamenFisico")),
                Diagnostico = reader.GetString(reader.GetOrdinal("Diagnostico")),
                Tratamiento = reader.IsDBNull(reader.GetOrdinal("Tratamiento"))
                    ? null : reader.GetString(reader.GetOrdinal("Tratamiento")),
                Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones"))
                    ? null : reader.GetString(reader.GetOrdinal("Observaciones")),
                Peso = reader.IsDBNull(reader.GetOrdinal("Peso"))
                    ? null : reader.GetDecimal(reader.GetOrdinal("Peso")),
                Altura = reader.IsDBNull(reader.GetOrdinal("Altura"))
                    ? null : reader.GetDecimal(reader.GetOrdinal("Altura")),
                PresionArterial = reader.IsDBNull(reader.GetOrdinal("PresionArterial"))
                    ? null : reader.GetDecimal(reader.GetOrdinal("PresionArterial")),
                Temperatura = reader.IsDBNull(reader.GetOrdinal("Temperatura"))
                    ? null : reader.GetDecimal(reader.GetOrdinal("Temperatura")),
                FrecuenciaCardiaca = reader.IsDBNull(reader.GetOrdinal("FrecuenciaCardiaca"))
                    ? null : reader.GetDecimal(reader.GetOrdinal("FrecuenciaCardiaca")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                FechaAlta = reader.GetDateTime(reader.GetOrdinal("FechaAlta")),
                PacienteNombre = reader.GetString(reader.GetOrdinal("PacienteNombre")),
                PacienteApellido = reader.GetString(reader.GetOrdinal("PacienteApellido")),
                PacienteDocumento = reader.GetString(reader.GetOrdinal("PacienteDocumento")),
                ProfesionalNombre = reader.GetString(reader.GetOrdinal("ProfesionalNombre")),
                ProfesionalApellido = reader.GetString(reader.GetOrdinal("ProfesionalApellido")),
                ProfesionalMatricula = reader.GetString(reader.GetOrdinal("ProfesionalMatricula"))
            };
        }
    }
}

