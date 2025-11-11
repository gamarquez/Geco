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
    public class ObraSocialData
    {
        private readonly string _connectionString;

        public ObraSocialData(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("GecoConnection");
        }

        public List<ObraSocialDto> ListarTodas(bool soloActivas = true)
        {
            List<ObraSocialDto> obrasSociales = new List<ObraSocialDto>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ListarObrasSociales", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SoloActivos", soloActivas);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            obrasSociales.Add(MapearObraSocialDesdeReader(reader));
                        }
                    }
                }
            }

            return obrasSociales;
        }

        public ObraSocialDto ObtenerPorId(int obraSocialId)
        {
            ObraSocialDto obraSocial = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ObtenerObraSocialPorId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ObraSocialId", obraSocialId);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            obraSocial = MapearObraSocialDesdeReader(reader, incluirCantidadPlanes: false);
                        }
                    }
                }
            }

            return obraSocial;
        }

        public int Crear(CrearObraSocialDto dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_CrearObraSocial", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Nombre", dto.Nombre);
                    cmd.Parameters.AddWithValue("@RazonSocial",
                        string.IsNullOrWhiteSpace(dto.RazonSocial) ? (object)DBNull.Value : dto.RazonSocial);
                    cmd.Parameters.AddWithValue("@CUIT",
                        string.IsNullOrWhiteSpace(dto.CUIT) ? (object)DBNull.Value : dto.CUIT);
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

        public bool Actualizar(ActualizarObraSocialDto dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_ActualizarObraSocial", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ObraSocialId", dto.ObraSocialId);
                    cmd.Parameters.AddWithValue("@Nombre", dto.Nombre);
                    cmd.Parameters.AddWithValue("@RazonSocial",
                        string.IsNullOrWhiteSpace(dto.RazonSocial) ? (object)DBNull.Value : dto.RazonSocial);
                    cmd.Parameters.AddWithValue("@CUIT",
                        string.IsNullOrWhiteSpace(dto.CUIT) ? (object)DBNull.Value : dto.CUIT);
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

        public bool Eliminar(int obraSocialId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_EliminarObraSocial", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ObraSocialId", obraSocialId);

                    conn.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }

        private ObraSocialDto MapearObraSocialDesdeReader(SqlDataReader reader, bool incluirCantidadPlanes = true)
        {
            var dto = new ObraSocialDto
            {
                ObraSocialId = reader.GetInt32(reader.GetOrdinal("ObraSocialId")),
                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                RazonSocial = reader.IsDBNull(reader.GetOrdinal("RazonSocial"))
                    ? null : reader.GetString(reader.GetOrdinal("RazonSocial")),
                CUIT = reader.IsDBNull(reader.GetOrdinal("CUIT"))
                    ? null : reader.GetString(reader.GetOrdinal("CUIT")),
                Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono"))
                    ? null : reader.GetString(reader.GetOrdinal("Telefono")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email"))
                    ? null : reader.GetString(reader.GetOrdinal("Email")),
                Direccion = reader.IsDBNull(reader.GetOrdinal("Direccion"))
                    ? null : reader.GetString(reader.GetOrdinal("Direccion")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                FechaAlta = reader.GetDateTime(reader.GetOrdinal("FechaAlta")),
                Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones"))
                    ? null : reader.GetString(reader.GetOrdinal("Observaciones"))
            };

            if (incluirCantidadPlanes)
            {
                dto.CantidadPlanes = reader.GetInt32(reader.GetOrdinal("CantidadPlanes"));
            }

            return dto;
        }
    }
}

