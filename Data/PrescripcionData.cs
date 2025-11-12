using Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;

namespace Data
{
    /// <summary>
    /// Capa de acceso a datos para Prescripciones
    /// </summary>
    public class PrescripcionData
    {
        private readonly string _connectionString;

        public PrescripcionData(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("GecoConnection");
        }

        /// <summary>
        /// Lista prescripciones con filtros opcionales
        /// </summary>
        public List<PrescripcionDto> Listar(PrescripcionFiltroDto filtro, out int totalRegistros)
        {
            List<PrescripcionDto> prescripciones = new List<PrescripcionDto>();
            totalRegistros = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ListarPrescripciones", conn))
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
                    cmd.Parameters.AddWithValue("@SoloVigentes",
                        filtro.SoloVigentes.HasValue ? (object)filtro.SoloVigentes.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@PageNumber", filtro.PageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", filtro.PageSize);
                    cmd.Parameters.AddWithValue("@SoloActivas", filtro.SoloActivas);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            prescripciones.Add(MapearPrescripcionDesdeReader(reader));
                        }

                        // Leer el total de registros
                        if (reader.NextResult() && reader.Read())
                        {
                            totalRegistros = reader.GetInt32(0);
                        }
                    }
                }
            }

            return prescripciones;
        }

        /// <summary>
        /// Obtiene una prescripción por ID con sus items
        /// </summary>
        public PrescripcionDto ObtenerPorId(int prescripcionId)
        {
            PrescripcionDto prescripcion = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ObtenerPrescripcionPorId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PrescripcionId", prescripcionId);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            prescripcion = MapearPrescripcionDesdeReader(reader);
                        }

                        // Leer items
                        if (prescripcion != null && reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                prescripcion.Items.Add(MapearItemPrescripcionDesdeReader(reader));
                            }
                        }
                    }
                }
            }

            return prescripcion;
        }

        /// <summary>
        /// Obtiene las prescripciones de un paciente
        /// </summary>
        public List<PrescripcionDto> ObtenerPorPaciente(int pacienteId)
        {
            List<PrescripcionDto> prescripciones = new List<PrescripcionDto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ObtenerPrescripcionesPorPaciente", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PacienteId", pacienteId);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            prescripciones.Add(MapearPrescripcionDesdeReader(reader));
                        }
                    }
                }
            }

            return prescripciones;
        }

        /// <summary>
        /// Crea una nueva prescripción con sus items
        /// </summary>
        public int Crear(CrearPrescripcionDto dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    int prescripcionId;

                    // Crear prescripción
                    using (SqlCommand cmd = new SqlCommand("SP_CrearPrescripcion", conn, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@PacienteId", dto.PacienteId);
                        cmd.Parameters.AddWithValue("@ProfesionalId", dto.ProfesionalId);
                        cmd.Parameters.AddWithValue("@TurnoId",
                            dto.TurnoId.HasValue ? (object)dto.TurnoId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@HistoriaClinicaId",
                            dto.HistoriaClinicaId.HasValue ? (object)dto.HistoriaClinicaId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@FechaPrescripcion", dto.FechaPrescripcion);
                        cmd.Parameters.AddWithValue("@Diagnostico", dto.Diagnostico);
                        cmd.Parameters.AddWithValue("@Indicaciones",
                            string.IsNullOrWhiteSpace(dto.Indicaciones) ? (object)DBNull.Value : dto.Indicaciones);
                        cmd.Parameters.AddWithValue("@FechaVencimiento",
                            dto.FechaVencimiento.HasValue ? (object)dto.FechaVencimiento.Value : DBNull.Value);

                        object result = cmd.ExecuteScalar();
                        prescripcionId = Convert.ToInt32(result);
                    }

                    // Crear items
                    int orden = 1;
                    foreach (var item in dto.Items)
                    {
                        using (SqlCommand cmd = new SqlCommand("SP_CrearItemPrescripcion", conn, transaction))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("@PrescripcionId", prescripcionId);
                            cmd.Parameters.AddWithValue("@Medicamento", item.Medicamento);
                            cmd.Parameters.AddWithValue("@PrincipioActivo",
                                string.IsNullOrWhiteSpace(item.PrincipioActivo) ? (object)DBNull.Value : item.PrincipioActivo);
                            cmd.Parameters.AddWithValue("@Presentacion",
                                string.IsNullOrWhiteSpace(item.Presentacion) ? (object)DBNull.Value : item.Presentacion);
                            cmd.Parameters.AddWithValue("@Dosis", item.Dosis);
                            cmd.Parameters.AddWithValue("@Frecuencia", item.Frecuencia);
                            cmd.Parameters.AddWithValue("@Duracion", item.Duracion);
                            cmd.Parameters.AddWithValue("@ViaAdministracion",
                                string.IsNullOrWhiteSpace(item.ViaAdministracion) ? (object)DBNull.Value : item.ViaAdministracion);
                            cmd.Parameters.AddWithValue("@IndicacionesEspeciales",
                                string.IsNullOrWhiteSpace(item.IndicacionesEspeciales) ? (object)DBNull.Value : item.IndicacionesEspeciales);
                            cmd.Parameters.AddWithValue("@Orden", orden);

                            cmd.ExecuteNonQuery();
                            orden++;
                        }
                    }

                    transaction.Commit();
                    return prescripcionId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Anula una prescripción
        /// </summary>
        public bool Anular(int prescripcionId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_AnularPrescripcion", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PrescripcionId", prescripcionId);

                    conn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }

        /// <summary>
        /// Mapea un SqlDataReader a PrescripcionDto
        /// </summary>
        private PrescripcionDto MapearPrescripcionDesdeReader(SqlDataReader reader)
        {
            return new PrescripcionDto
            {
                PrescripcionId = reader.GetInt32(reader.GetOrdinal("PrescripcionId")),
                PacienteId = reader.GetInt32(reader.GetOrdinal("PacienteId")),
                ProfesionalId = reader.GetInt32(reader.GetOrdinal("ProfesionalId")),
                TurnoId = reader.IsDBNull(reader.GetOrdinal("TurnoId"))
                    ? null : reader.GetInt32(reader.GetOrdinal("TurnoId")),
                HistoriaClinicaId = reader.IsDBNull(reader.GetOrdinal("HistoriaClinicaId"))
                    ? null : reader.GetInt32(reader.GetOrdinal("HistoriaClinicaId")),
                FechaPrescripcion = reader.GetDateTime(reader.GetOrdinal("FechaPrescripcion")),
                Diagnostico = reader.GetString(reader.GetOrdinal("Diagnostico")),
                Indicaciones = reader.IsDBNull(reader.GetOrdinal("Indicaciones"))
                    ? null : reader.GetString(reader.GetOrdinal("Indicaciones")),
                Vigente = reader.GetBoolean(reader.GetOrdinal("Vigente")),
                FechaVencimiento = reader.IsDBNull(reader.GetOrdinal("FechaVencimiento"))
                    ? null : reader.GetDateTime(reader.GetOrdinal("FechaVencimiento")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                FechaAlta = reader.GetDateTime(reader.GetOrdinal("FechaAlta")),
                FechaModificacion = reader.IsDBNull(reader.GetOrdinal("FechaModificacion"))
                    ? null : reader.GetDateTime(reader.GetOrdinal("FechaModificacion")),
                PacienteNombre = reader.GetString(reader.GetOrdinal("PacienteNombre")),
                PacienteApellido = reader.GetString(reader.GetOrdinal("PacienteApellido")),
                PacienteDocumento = reader.GetString(reader.GetOrdinal("PacienteDocumento")),
                PacienteFechaNacimiento = reader.IsDBNull(reader.GetOrdinal("PacienteFechaNacimiento"))
                    ? null : reader.GetDateTime(reader.GetOrdinal("PacienteFechaNacimiento")),
                ProfesionalNombre = reader.GetString(reader.GetOrdinal("ProfesionalNombre")),
                ProfesionalApellido = reader.GetString(reader.GetOrdinal("ProfesionalApellido")),
                ProfesionalMatricula = reader.GetString(reader.GetOrdinal("ProfesionalMatricula")),
                ProfesionalEspecialidad = reader.IsDBNull(reader.GetOrdinal("ProfesionalEspecialidad"))
                    ? null : reader.GetString(reader.GetOrdinal("ProfesionalEspecialidad"))
            };
        }

        /// <summary>
        /// Mapea un SqlDataReader a ItemPrescripcionDto
        /// </summary>
        private ItemPrescripcionDto MapearItemPrescripcionDesdeReader(SqlDataReader reader)
        {
            return new ItemPrescripcionDto
            {
                ItemPrescripcionId = reader.GetInt32(reader.GetOrdinal("ItemPrescripcionId")),
                PrescripcionId = reader.GetInt32(reader.GetOrdinal("PrescripcionId")),
                Medicamento = reader.GetString(reader.GetOrdinal("Medicamento")),
                PrincipioActivo = reader.IsDBNull(reader.GetOrdinal("PrincipioActivo"))
                    ? null : reader.GetString(reader.GetOrdinal("PrincipioActivo")),
                Presentacion = reader.IsDBNull(reader.GetOrdinal("Presentacion"))
                    ? null : reader.GetString(reader.GetOrdinal("Presentacion")),
                Dosis = reader.GetString(reader.GetOrdinal("Dosis")),
                Frecuencia = reader.GetString(reader.GetOrdinal("Frecuencia")),
                Duracion = reader.GetString(reader.GetOrdinal("Duracion")),
                ViaAdministracion = reader.IsDBNull(reader.GetOrdinal("ViaAdministracion"))
                    ? null : reader.GetString(reader.GetOrdinal("ViaAdministracion")),
                IndicacionesEspeciales = reader.IsDBNull(reader.GetOrdinal("IndicacionesEspeciales"))
                    ? null : reader.GetString(reader.GetOrdinal("IndicacionesEspeciales")),
                Orden = reader.GetInt32(reader.GetOrdinal("Orden"))
            };
        }
    }
}
