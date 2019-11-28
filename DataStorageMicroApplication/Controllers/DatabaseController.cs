using SensorEntries;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DataStorageMicroApplication.Controllers
{
    [RoutePrefix("api/sensors")]
    public class DatabaseController : ApiController
    {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DataStorageMicroApplication.Properties.Settings.ConnectionString"].ConnectionString;
        
        [Route("")]
        public IEnumerable<Sensor> GetAllTemperature()
        {
            List<Sensor> sensors = new List<Sensor>();
            SqlConnection connectionSQL = null;

            try
            {
                connectionSQL = new SqlConnection(connectionString);
                connectionSQL.Open();

                SqlCommand command = new SqlCommand("SELECT * FROM Sensor ORDER BY Id", connectionSQL);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Sensor sensor = new Sensor();

                    sensor.id = (int)reader["Id"];
                    sensor.temperature = (double)reader["Temperature"];
                    sensor.humidity = (double)reader["Humidity"];
                    sensor.battery = (int)reader["Battery"];
                    sensor.timestamp = (int)reader["Timestamp"];

                    sensors.Add(sensor);
                }
                reader.Close();
                connectionSQL.Close();
            }
            catch (Exception)
            {
                if (connectionSQL.State == System.Data.ConnectionState.Open)
                    connectionSQL.Close();
                throw;
            }

            return sensors;
        }

        [Route("{id}")]
        public IHttpActionResult GetSensor(long id)
        {
            Sensor sensor = new Sensor();
            SqlConnection connectionSQL = null;

            try
            {
                connectionSQL = new SqlConnection(connectionString);
                connectionSQL.Open();

                SqlCommand command = new SqlCommand();
                command.CommandText = "SELECT * FROM Sensor WHERE Id = @id";
                command.Parameters.AddWithValue("@name", id);
                command.CommandType = System.Data.CommandType.Text;
                command.Connection = connectionSQL;

                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    sensor.id = (int)reader["Id"];
                    sensor.temperature = (double)reader["Temperature"];
                    sensor.humidity = (double)reader["Humidity"];
                    sensor.battery = (int)reader["Battery"];
                    sensor.timestamp = (int)reader["Timestamp"];
                }
                reader.Close();
                connectionSQL.Close();

                if (sensor != null)
                {
                    return Ok(sensor);
                }
                return NotFound();
            }
            catch (Exception)
            {
                return NotFound();
            }

        }

        [Route("")]
        public IHttpActionResult PostSensor(Sensor sensor)
        {
            SqlConnection connectionSQL = null;
            try
            {
                connectionSQL = new SqlConnection(connectionString);
                connectionSQL.Open();


                string str = "INSERT INTO Sensor(Id,Temperature,Humidity,Battery,Timestamp) values(@id,@temperature,@humidity,@battery,@timestamp);";

                SqlCommand command = new SqlCommand(str, connectionSQL);
                command.Parameters.AddWithValue("@id", (int)sensor.id);
                command.Parameters.AddWithValue("@temperature", (float)sensor.temperature);
                command.Parameters.AddWithValue("@humidity", (float)sensor.humidity);
                command.Parameters.AddWithValue("@battery", (int)sensor.battery);
                command.Parameters.AddWithValue("@timestamp", (int)sensor.timestamp);

                int nrows = command.ExecuteNonQuery();
                connectionSQL.Close();

                if (nrows > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Sensor added");
                    return Ok(sensor);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return NotFound();
            }
        }

        [Route("{id}")]
        public IHttpActionResult PutSensor(long id, Sensor sensor)
        {
            SqlConnection connectionSQL = null;
            try
            {
                connectionSQL = new SqlConnection(connectionString);
                connectionSQL.Open();

                string str = "UPDATE Sensor SET Temperature = @temperature, Humidity = @humidity, Battery = @battery, Timestamp = @timestamp WHERE Id = @id";

                SqlCommand command = new SqlCommand(str, connectionSQL);

                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@temperature", sensor.temperature);
                command.Parameters.AddWithValue("@humidity", sensor.humidity);
                command.Parameters.AddWithValue("@battery", sensor.battery);
                command.Parameters.AddWithValue("@timestamp", sensor.timestamp);

                int nrows = command.ExecuteNonQuery();
                connectionSQL.Close();

                if (nrows > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Sensor added");
                    return Ok(sensor);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        }

}
