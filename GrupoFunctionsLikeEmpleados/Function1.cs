using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace GrupoFunctionsLikeEmpleados
{
    public static class Function1
    {
        [FunctionName("functionlikeempleado")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string idempleado = req.Query["idempleado"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            idempleado = idempleado ?? data?.idempleado;

            if (idempleado == null)
            {
                return new BadRequestObjectResult("El ID de empleado no es correcto o es obligatorio");
            }

            string connectionString = Environment.GetEnvironmentVariable("SqlHospitalAzure");
            string sqlupdate = "UPDATE EMP SET SALARIO = SALARIO + 1 WHERE EMP_NO=@IDEMPLEADO";
            SqlParameter pamid = new SqlParameter("@IDEMPLEADO", idempleado);
            SqlConnection cn = new SqlConnection(connectionString);
            SqlCommand com = new SqlCommand();
            com.Parameters.Add(pamid);
            com.Connection = cn;
            com.CommandType = System.Data.CommandType.Text;
            com.CommandText = sqlupdate;
            cn.Open();
            com.ExecuteNonQuery();
            string sqlselect = "SELECT * FROM EMP WHERE EMP_NO=@IDEMPLEADO";
            com.CommandText = sqlselect;
            SqlDataReader reader = com.ExecuteReader();
            string apellido = "" , oficio = "";
            int salario = -1;
            if (reader.Read())
            {
                apellido = reader["APELLIDO"].ToString();
                oficio = reader["OFICIO"].ToString();
                salario = int.Parse(reader["SALARIO"].ToString());
                reader.Close();
            }
            cn.Close();
            com.Parameters.Clear();
            if (salario ==-1)
            {
                return new BadRequestObjectResult("El id del empleado "+ idempleado + " no existe.");
            }
            else
            {
                return new OkObjectResult("El empleado " + apellido + " con oficio " + oficio + " ha incrementado su salario en 1 ." + " Su nuevo salario es " + salario);
            }

            string responseMessage = string.IsNullOrEmpty(idempleado)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {idempleado}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
