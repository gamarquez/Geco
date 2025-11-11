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
    public class PlanData
    {
        private readonly string _connectionString;

        public PlanData(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("GecoConnection");
        }

        public List<PlanDto> ListarTodos(bool soloActivos = true)
        {
            List<PlanDto> planes = new List<PlanDto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ListarPlanes", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SoloActivos", soloActivos);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            planes.Add(MapearPlanDesdeReader(reader));
                        }
                    }
                }
            }

            return planes;
        }

        public List<PlanDto> ListarPorObraSocial(int obraSocialId, bool soloActivos = true)
        {
            List<PlanDto> planes = new List<PlanDto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ListarPlanesPorObraSocial", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ObraSocialId", obraSocialId);
                    cmd.Parameters.AddWithValue("@SoloActivos", soloActivos);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            planes.Add(MapearPlanDesdeReader(reader));
                        }
                    }
                }
            }

            return planes;
        }

        public PlanDto ObtenerPorId(int planId)
        {
            PlanDto plan = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ObtenerPlanPorId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PlanId", planId);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            plan = MapearPlanDesdeReader(reader);
                        }
                    }
                }
            }

            return plan;
        }

        public int Crear(CrearPlanDto dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_CrearPlan", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ObraSocialId", dto.ObraSocialId);
                    cmd.Parameters.AddWithValue("@Nombre", dto.Nombre);
                    cmd.Parameters.AddWithValue("@Codigo",
                        string.IsNullOrWhiteSpace(dto.Codigo) ? (object)DBNull.Value : dto.Codigo);
                    cmd.Parameters.AddWithValue("@Descripcion",
                        string.IsNullOrWhiteSpace(dto.Descripcion) ? (object)DBNull.Value : dto.Descripcion);
                    cmd.Parameters.AddWithValue("@PorcentajeCobertura",
                        dto.PorcentajeCobertura.HasValue ? (object)dto.PorcentajeCobertura.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Copago",
                        dto.Copago.HasValue ? (object)dto.Copago.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Observaciones",
                        string.IsNullOrWhiteSpace(dto.Observaciones) ? (object)DBNull.Value : dto.Observaciones);

                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        public bool Actualizar(ActualizarPlanDto dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ActualizarPlan", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PlanId", dto.PlanId);
                    cmd.Parameters.AddWithValue("@ObraSocialId", dto.ObraSocialId);
                    cmd.Parameters.AddWithValue("@Nombre", dto.Nombre);
                    cmd.Parameters.AddWithValue("@Codigo",
                        string.IsNullOrWhiteSpace(dto.Codigo) ? (object)DBNull.Value : dto.Codigo);
                    cmd.Parameters.AddWithValue("@Descripcion",
                        string.IsNullOrWhiteSpace(dto.Descripcion) ? (object)DBNull.Value : dto.Descripcion);
                    cmd.Parameters.AddWithValue("@PorcentajeCobertura",
                        dto.PorcentajeCobertura.HasValue ? (object)dto.PorcentajeCobertura.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Copago",
                        dto.Copago.HasValue ? (object)dto.Copago.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Observaciones",
                        string.IsNullOrWhiteSpace(dto.Observaciones) ? (object)DBNull.Value : dto.Observaciones);
                    cmd.Parameters.AddWithValue("@Activo", dto.Activo);

                    conn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }

        public bool Eliminar(int planId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_EliminarPlan", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PlanId", planId);

                    conn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }

        private PlanDto MapearPlanDesdeReader(SqlDataReader reader)
        {
            return new PlanDto
            {
                PlanId = reader.GetInt32(reader.GetOrdinal("PlanId")),
                ObraSocialId = reader.GetInt32(reader.GetOrdinal("ObraSocialId")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                Codigo = reader.IsDBNull(reader.GetOrdinal("Codigo"))
                    ? null : reader.GetString(reader.GetOrdinal("Codigo")),
                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion"))
                    ? null : reader.GetString(reader.GetOrdinal("Descripcion")),
                PorcentajeCobertura = reader.IsDBNull(reader.GetOrdinal("PorcentajeCobertura"))
                    ? null : reader.GetDecimal(reader.GetOrdinal("PorcentajeCobertura")),
                Copago = reader.IsDBNull(reader.GetOrdinal("Copago"))
                    ? null : reader.GetDecimal(reader.GetOrdinal("Copago")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                FechaAlta = reader.GetDateTime(reader.GetOrdinal("FechaAlta")),
                Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones"))
                    ? null : reader.GetString(reader.GetOrdinal("Observaciones")),
                ObraSocialNombre = reader.GetString(reader.GetOrdinal("ObraSocialNombre"))
            };
        }
    }
}

