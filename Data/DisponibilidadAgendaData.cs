using Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;

namespace Data
{
    /// <summary>
    /// Capa de acceso a datos para Disponibilidades de Agenda
    /// </summary>
    public class DisponibilidadAgendaData
    {
        private readonly string _connectionString;

        public DisponibilidadAgendaData(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("GecoConnection");
        }

        /// <summary>
        /// Lista las disponibilidades con filtros
        /// </summary>
        public List<DisponibilidadAgendaDto> Listar(DisponibilidadAgendaFiltroDto filtro, out int totalRegistros)
        {
            var lista = new List<DisponibilidadAgendaDto>();
            totalRegistros = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ListarDisponibilidadesAgenda", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProfesionalId", (object)filtro.ProfesionalId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DiaSemana", (object)filtro.DiaSemana ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SoloVigentes", (object)filtro.SoloVigentes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PageNumber", filtro.PageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", filtro.PageSize);
                    cmd.Parameters.AddWithValue("@SoloActivas", filtro.SoloActivas);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Primera consulta: datos de disponibilidades
                        while (reader.Read())
                        {
                            lista.Add(MapearDesdeReader(reader));
                        }

                        // Segunda consulta: total de registros
                        if (reader.NextResult() && reader.Read())
                        {
                            totalRegistros = reader.GetInt32(0);
                        }
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Obtiene una disponibilidad por ID
        /// </summary>
        public DisponibilidadAgendaDto ObtenerPorId(int disponibilidadAgendaId)
        {
            DisponibilidadAgendaDto disponibilidad = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ObtenerDisponibilidadPorId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DisponibilidadAgendaId", disponibilidadAgendaId);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            disponibilidad = MapearDesdeReader(reader);
                        }
                    }
                }
            }

            return disponibilidad;
        }

        /// <summary>
        /// Obtiene las disponibilidades de un profesional
        /// </summary>
        public List<DisponibilidadAgendaDto> ObtenerPorProfesional(int profesionalId, bool soloVigentes = true)
        {
            var lista = new List<DisponibilidadAgendaDto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ObtenerDisponibilidadesPorProfesional", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProfesionalId", profesionalId);
                    cmd.Parameters.AddWithValue("@SoloVigentes", soloVigentes);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearDesdeReader(reader));
                        }
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Crea una nueva disponibilidad
        /// </summary>
        public int Crear(CrearDisponibilidadAgendaDto dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_CrearDisponibilidadAgenda", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProfesionalId", dto.ProfesionalId);
                    cmd.Parameters.AddWithValue("@DiaSemana", dto.DiaSemana);
                    cmd.Parameters.AddWithValue("@HoraInicio", dto.HoraInicio);
                    cmd.Parameters.AddWithValue("@HoraFin", dto.HoraFin);
                    cmd.Parameters.AddWithValue("@IntervaloMinutos", dto.IntervaloMinutos);
                    cmd.Parameters.AddWithValue("@FechaVigenciaDesde", dto.FechaVigenciaDesde);
                    cmd.Parameters.AddWithValue("@FechaVigenciaHasta", (object)dto.FechaVigenciaHasta ?? DBNull.Value);

                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }

        /// <summary>
        /// Actualiza una disponibilidad
        /// </summary>
        public bool Actualizar(ActualizarDisponibilidadAgendaDto dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ActualizarDisponibilidadAgenda", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DisponibilidadAgendaId", dto.DisponibilidadAgendaId);
                    cmd.Parameters.AddWithValue("@ProfesionalId", dto.ProfesionalId);
                    cmd.Parameters.AddWithValue("@DiaSemana", dto.DiaSemana);
                    cmd.Parameters.AddWithValue("@HoraInicio", dto.HoraInicio);
                    cmd.Parameters.AddWithValue("@HoraFin", dto.HoraFin);
                    cmd.Parameters.AddWithValue("@IntervaloMinutos", dto.IntervaloMinutos);
                    cmd.Parameters.AddWithValue("@FechaVigenciaDesde", dto.FechaVigenciaDesde);
                    cmd.Parameters.AddWithValue("@FechaVigenciaHasta", (object)dto.FechaVigenciaHasta ?? DBNull.Value);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int filasAfectadas = reader.GetInt32(0);
                            return filasAfectadas > 0;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Elimina (soft delete) una disponibilidad
        /// </summary>
        public bool Eliminar(int disponibilidadAgendaId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_EliminarDisponibilidadAgenda", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DisponibilidadAgendaId", disponibilidadAgendaId);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int filasAfectadas = reader.GetInt32(0);
                            return filasAfectadas > 0;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica si existe disponibilidad para un turno específico
        /// </summary>
        public bool VerificarDisponibilidad(int profesionalId, DateTime fechaTurno, TimeSpan horaTurno, int duracionMinutos)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_VerificarDisponibilidadParaTurno", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProfesionalId", profesionalId);
                    cmd.Parameters.AddWithValue("@FechaTurno", fechaTurno.Date);
                    cmd.Parameters.AddWithValue("@HoraTurno", horaTurno);
                    cmd.Parameters.AddWithValue("@DuracionMinutos", duracionMinutos);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(reader.GetOrdinal("EstaDisponible")) == 1;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Obtiene los horarios disponibles para un profesional en una fecha determinada
        /// </summary>
        public (List<(TimeSpan HoraInicio, TimeSpan HoraFin, int Intervalo)> Disponibilidades,
                List<(TimeSpan HoraInicio, TimeSpan HoraFin)> TurnosOcupados)
            ObtenerHorariosDisponibles(int profesionalId, DateTime fechaTurno)
        {
            var disponibilidades = new List<(TimeSpan HoraInicio, TimeSpan HoraFin, int Intervalo)>();
            var turnosOcupados = new List<(TimeSpan HoraInicio, TimeSpan HoraFin)>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ObtenerHorariosDisponibles", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProfesionalId", profesionalId);
                    cmd.Parameters.AddWithValue("@FechaTurno", fechaTurno.Date);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Primera consulta: disponibilidades
                        while (reader.Read())
                        {
                            disponibilidades.Add((
                                reader.GetTimeSpan(reader.GetOrdinal("HoraInicio")),
                                reader.GetTimeSpan(reader.GetOrdinal("HoraFin")),
                                reader.GetInt32(reader.GetOrdinal("IntervaloMinutos"))
                            ));
                        }

                        // Segunda consulta: turnos ocupados
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                turnosOcupados.Add((
                                    reader.GetTimeSpan(reader.GetOrdinal("HoraInicio")),
                                    reader.GetTimeSpan(reader.GetOrdinal("HoraFin"))
                                ));
                            }
                        }
                    }
                }
            }

            return (disponibilidades, turnosOcupados);
        }

        /// <summary>
        /// Mapea un SqlDataReader a DisponibilidadAgendaDto
        /// </summary>
        private DisponibilidadAgendaDto MapearDesdeReader(SqlDataReader reader)
        {
            return new DisponibilidadAgendaDto
            {
                DisponibilidadAgendaId = reader.GetInt32(reader.GetOrdinal("DisponibilidadAgendaId")),
                ProfesionalId = reader.GetInt32(reader.GetOrdinal("ProfesionalId")),
                DiaSemana = reader.GetInt32(reader.GetOrdinal("DiaSemana")),
                HoraInicio = reader.GetTimeSpan(reader.GetOrdinal("HoraInicio")),
                HoraFin = reader.GetTimeSpan(reader.GetOrdinal("HoraFin")),
                IntervaloMinutos = reader.GetInt32(reader.GetOrdinal("IntervaloMinutos")),
                FechaVigenciaDesde = reader.GetDateTime(reader.GetOrdinal("FechaVigenciaDesde")),
                FechaVigenciaHasta = reader.IsDBNull(reader.GetOrdinal("FechaVigenciaHasta"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("FechaVigenciaHasta")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                FechaAlta = reader.GetDateTime(reader.GetOrdinal("FechaAlta")),
                FechaModificacion = reader.IsDBNull(reader.GetOrdinal("FechaModificacion"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("FechaModificacion")),

                // Datos del profesional
                ProfesionalNombre = reader.GetString(reader.GetOrdinal("ProfesionalNombre")),
                ProfesionalApellido = reader.GetString(reader.GetOrdinal("ProfesionalApellido")),
                ProfesionalMatricula = reader.GetString(reader.GetOrdinal("ProfesionalMatricula")),
                ProfesionalEspecialidad = reader.GetString(reader.GetOrdinal("ProfesionalEspecialidad"))
            };
        }
    }
}
