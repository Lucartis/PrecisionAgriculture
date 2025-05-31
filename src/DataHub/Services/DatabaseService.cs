// src/DataHub/Services/DatabaseService.cs
using Npgsql;
using NpgsqlTypes;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PrecisionAgriculture.DataHub.Services
{
    public interface IDatabaseService
    {
        Task SaveSensorDataAsync(SensorData sensorData, DataAnalysisResult analysisResult);
        Task<IEnumerable<SensorData>> GetSensorDataAsync(string sensorId, DateTime startDate, DateTime endDate);
    }

    public class PostgresDatabaseService : IDatabaseService
    {
        private readonly string _connectionString;
        private readonly ILogger<PostgresDatabaseService> _logger;

        public PostgresDatabaseService(IConfiguration configuration, ILogger<PostgresDatabaseService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task SaveSensorDataAsync(SensorData sensorData, DataAnalysisResult analysisResult)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Guardar datos del sensor
                string sensorDataSql = @"
                    INSERT INTO sensor_data (sensor_id, sensor_type, timestamp, location, raw_data, is_anomaly)
                    VALUES (@sensorId, @sensorType, @timestamp, @location, @rawData, @isAnomaly)
                    RETURNING id";

                await using var sensorDataCmd = new NpgsqlCommand(sensorDataSql, connection);
                sensorDataCmd.Parameters.AddWithValue("sensorId", sensorData.SensorId);
                sensorDataCmd.Parameters.AddWithValue("sensorType", sensorData.SensorType);
                sensorDataCmd.Parameters.AddWithValue("timestamp", sensorData.Timestamp);
                sensorDataCmd.Parameters.AddWithValue("location", sensorData.Location);
                sensorDataCmd.Parameters.AddWithValue("rawData", NpgsqlTypes.NpgsqlDbType.Jsonb, System.Text.Json.JsonSerializer.Serialize(sensorData.Values));
                sensorDataCmd.Parameters.AddWithValue("isAnomaly", analysisResult.IsAnomaly);

                var sensorDataId = await sensorDataCmd.ExecuteScalarAsync();

                // Si hay anomalías, guardarlas
                if (analysisResult.IsAnomaly && analysisResult.Anomalies.Count > 0)
                {
                    foreach (var anomaly in analysisResult.Anomalies)
                    {
                        string anomalySql = @"
                            INSERT INTO anomalies (sensor_data_id, description, detected_at)
                            VALUES (@sensorDataId, @description, @detectedAt)";

                        await using var anomalyCmd = new NpgsqlCommand(anomalySql, connection);
                        anomalyCmd.Parameters.AddWithValue("sensorDataId", sensorDataId);
                        anomalyCmd.Parameters.AddWithValue("description", anomaly);
                        anomalyCmd.Parameters.AddWithValue("detectedAt", DateTime.UtcNow);

                        await anomalyCmd.ExecuteNonQueryAsync();
                    }
                }

                _logger.LogInformation("✅ Datos del sensor {SensorId} guardados correctamente en PostgreSQL", sensorData.SensorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al guardar datos del sensor {SensorId} en PostgreSQL", sensorData.SensorId);
                throw;
            }
        }

        public async Task<IEnumerable<SensorData>> GetSensorDataAsync(string sensorId, DateTime startDate, DateTime endDate)
        {
            var result = new List<SensorData>();
            
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                string sql = @"
                    SELECT sensor_id, sensor_type, timestamp, location, raw_data
                    FROM sensor_data
                    WHERE sensor_id = @sensorId 
                      AND timestamp BETWEEN @startDate AND @endDate
                    ORDER BY timestamp DESC";
                
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("sensorId", sensorId);
                cmd.Parameters.AddWithValue("startDate", startDate);
                cmd.Parameters.AddWithValue("endDate", endDate);
                
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var sensorData = new SensorData
                    {
                        SensorId = reader.GetString(0),
                        SensorType = reader.GetString(1),
                        Timestamp = reader.GetDateTime(2),
                        Location = reader.GetString(3),
                        Values = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(4))
                    };
                    
                    result.Add(sensorData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al obtener datos del sensor {SensorId}", sensorId);
                throw;
            }
            
            return result;
        }
    }
}