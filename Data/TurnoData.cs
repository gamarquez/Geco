using Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;

namespace Data
{
    /// <summary>
    /// Capa de acceso a datos para Turnos
    /// </summary>
    public class TurnoData
    {
        private readonly string _connectionString;

        public TurnoData(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("GecoConnection");
        }

        /// <summary>
        /// Lista turnos con filtros opcionales
        /// </summary>
        public List<TurnoDto> Listar(TurnoFiltroDto filtro, out int totalRegistros)
        {
            List<TurnoDto> turnos = new List<TurnoDto>();
            totalRegistros = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ListarTurnos", conn))
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
                    cmd.Parameters.AddWithValue("@Estado",
                        string.IsNullOrWhiteSpace(filtro.Estado) ? (object)DBNull.Value : filtro.Estado);
                    cmd.Parameters.AddWithValue("@PageNumber", filtro.PageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", filtro.PageSize);
                    cmd.Parameters.AddWithValue("@SoloActivos", filtro.SoloActivos);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            turnos.Add(MapearTurnoDesdeReader(reader));
                        }

                        // Leer el total de registros
                        if (reader.NextResult() && reader.Read())
                        {
                            totalRegistros = reader.GetInt32(0);
                        }
                    }
                }
            }

            return turnos;
        }

        /// <summary>
        /// Obtiene un turno por ID
        /// </summary>
        public TurnoDto ObtenerPorId(int turnoId)
        {
            TurnoDto turno = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ObtenerTurnoPorId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TurnoId", turnoId);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            turno = MapearTurnoDesdeReader(reader);
                        }
                    }
                }
            }

            return turno;
        }

        /// <summary>
        /// Obtiene los turnos de un paciente
        /// </summary>
        public List<TurnoDto> ObtenerTurnosPorPaciente(int pacienteId)
        {
            List<TurnoDto> turnos = new List<TurnoDto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ObtenerTurnosPorPaciente", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PacienteId", pacienteId);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            turnos.Add(MapearTurnoDesdeReader(reader));
                        }
                    }
                }
            }

            return turnos;
        }

        /// <summary>
        /// Obtiene la agenda de un profesional para una fecha específica
        /// </summary>
        public List<TurnoDto> ObtenerAgendaProfesional(int profesionalId, DateTime fecha)
        {
            List<TurnoDto> turnos = new List<TurnoDto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ObtenerAgendaProfesional", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProfesionalId", profesionalId);
                    cmd.Parameters.AddWithValue("@Fecha", fecha.Date);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            turnos.Add(MapearTurnoDesdeReader(reader));
                        }
                    }
                }
            }

            return turnos;
        }

        /// <summary>
        /// Verifica si hay disponibilidad horaria para un turno
        /// </summary>
        public bool VerificarDisponibilidad(int profesionalId, DateTime fechaTurno, TimeSpan horaInicio, int duracionMinutos, int? turnoIdExcluir = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_VerificarDisponibilidad", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProfesionalId", profesionalId);
                    cmd.Parameters.AddWithValue("@FechaTurno", fechaTurno.Date);
                    cmd.Parameters.AddWithValue("@HoraInicio", horaInicio);
                    cmd.Parameters.AddWithValue("@DuracionMinutos", duracionMinutos);
                    cmd.Parameters.AddWithValue("@TurnoIdExcluir",
                        turnoIdExcluir.HasValue ? (object)turnoIdExcluir.Value : DBNull.Value);

                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    return Convert.ToBoolean(result);
                }
            }
        }

        /// <summary>
        /// Crea un nuevo turno
        /// </summary>
        public int Crear(CrearTurnoDto dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_CrearTurno", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PacienteId", dto.PacienteId);
                    cmd.Parameters.AddWithValue("@ProfesionalId", dto.ProfesionalId);
                    cmd.Parameters.AddWithValue("@FechaTurno", dto.FechaTurno.Date);
                    cmd.Parameters.AddWithValue("@HoraInicio", dto.HoraInicio);
                    cmd.Parameters.AddWithValue("@DuracionMinutos", dto.DuracionMinutos);
                    cmd.Parameters.AddWithValue("@MotivoConsulta", dto.MotivoConsulta);
                    cmd.Parameters.AddWithValue("@Observaciones",
                        string.IsNullOrWhiteSpace(dto.Observaciones) ? (object)DBNull.Value : dto.Observaciones);
                    cmd.Parameters.AddWithValue("@Estado", dto.Estado);

                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        /// <summary>
        /// Actualiza un turno existente
        /// </summary>
        public bool Actualizar(ActualizarTurnoDto dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ActualizarTurno", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@TurnoId", dto.TurnoId);
                    cmd.Parameters.AddWithValue("@PacienteId", dto.PacienteId);
                    cmd.Parameters.AddWithValue("@ProfesionalId", dto.ProfesionalId);
                    cmd.Parameters.AddWithValue("@FechaTurno", dto.FechaTurno.Date);
                    cmd.Parameters.AddWithValue("@HoraInicio", dto.HoraInicio);
                    cmd.Parameters.AddWithValue("@DuracionMinutos", dto.DuracionMinutos);
                    cmd.Parameters.AddWithValue("@MotivoConsulta", dto.MotivoConsulta);
                    cmd.Parameters.AddWithValue("@Estado", dto.Estado);
                    cmd.Parameters.AddWithValue("@MotivoCancelacion",
                        string.IsNullOrWhiteSpace(dto.MotivoCancelacion) ? (object)DBNull.Value : dto.MotivoCancelacion);
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
        /// Cancela un turno
        /// </summary>
        public bool Cancelar(int turnoId, string motivoCancelacion)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_CancelarTurno", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TurnoId", turnoId);
                    cmd.Parameters.AddWithValue("@MotivoCancelacion", motivoCancelacion);

                    conn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }

        /// <summary>
        /// Mapea un SqlDataReader a TurnoDto
        /// </summary>
        private TurnoDto MapearTurnoDesdeReader(SqlDataReader reader)
        {
            return new TurnoDto
            {
                TurnoId = reader.GetInt32(reader.GetOrdinal("TurnoId")),
                PacienteId = reader.GetInt32(reader.GetOrdinal("PacienteId")),
                ProfesionalId = reader.GetInt32(reader.GetOrdinal("ProfesionalId")),
                FechaTurno = reader.GetDateTime(reader.GetOrdinal("FechaTurno")),
                HoraInicio = reader.GetTimeSpan(reader.GetOrdinal("HoraInicio")),
                HoraFin = reader.GetTimeSpan(reader.GetOrdinal("HoraFin")),
                DuracionMinutos = reader.GetInt32(reader.GetOrdinal("DuracionMinutos")),
                MotivoConsulta = reader.GetString(reader.GetOrdinal("MotivoConsulta")),
                Estado = reader.GetString(reader.GetOrdinal("Estado")),
                MotivoCancelacion = reader.IsDBNull(reader.GetOrdinal("MotivoCancelacion"))
                    ? null : reader.GetString(reader.GetOrdinal("MotivoCancelacion")),
                Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones"))
                    ? null : reader.GetString(reader.GetOrdinal("Observaciones")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                FechaAlta = reader.GetDateTime(reader.GetOrdinal("FechaAlta")),
                FechaModificacion = reader.IsDBNull(reader.GetOrdinal("FechaModificacion"))
                    ? null : reader.GetDateTime(reader.GetOrdinal("FechaModificacion")),
                PacienteNombre = reader.GetString(reader.GetOrdinal("PacienteNombre")),
                PacienteApellido = reader.GetString(reader.GetOrdinal("PacienteApellido")),
                PacienteDocumento = reader.GetString(reader.GetOrdinal("PacienteDocumento")),
                PacienteTelefono = reader.IsDBNull(reader.GetOrdinal("PacienteTelefono"))
                    ? null : reader.GetString(reader.GetOrdinal("PacienteTelefono")),
                ProfesionalNombre = reader.GetString(reader.GetOrdinal("ProfesionalNombre")),
                ProfesionalApellido = reader.GetString(reader.GetOrdinal("ProfesionalApellido")),
                ProfesionalMatricula = reader.GetString(reader.GetOrdinal("ProfesionalMatricula"))
            };
        }
    }
}
