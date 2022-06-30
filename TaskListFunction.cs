using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace RESTApiWithAzureFunction
{
    public static class TaskListFunction
    {
        [FunctionName("CreateTask")]
        public static async Task<IActionResult> CreateTask(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "task")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Creating a new task list item.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<CreateTaskModel>(requestBody);
            try
            {
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    connection.Open();
                    var query = $"INSERT INTO [TaskList] ([Description],[CreatedOn],[IsDone]) VALUES(@TaskDescription, '{input.CreatedOn}' , '{false}')";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@TaskDescription", input.Description);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                return new BadRequestResult();
            }
            return new OkResult();
        }

        [FunctionName("GetTasks")]
        public static async Task<IActionResult> GetTasks(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "task")] HttpRequest req, ILogger log)
        {
            log.LogInformation("Getting task list items.");
            List<TaskModel> taskList = new List<TaskModel>();
            try
            {
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    connection.Open();
                    var query = @"Select * from TaskList";
                    SqlCommand command = new SqlCommand(query, connection);
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        TaskModel task = new TaskModel()
                        {
                            Id = (int)reader["Id"],
                            Description = reader["Description"].ToString(),
                            CreatedOn = (DateTime)reader["CreatedOn"],
                            IsDone = (bool)reader["IsDone"]
                        };
                        taskList.Add(task);
                    }
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                return new BadRequestResult();
            }
            return new OkObjectResult(taskList);
        }

        [FunctionName("GetTaskById")]
        public static IActionResult GetTaskById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "task/{id}")] HttpRequest req, ILogger log, int id)
        {
            DataTable dt = new DataTable();
            log.LogInformation("Getting task by Id items.");
            try
            {
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    connection.Open();
                    var query = @"Select * from TaskList Where Id = @Id";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", id);
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    da.Fill(dt);
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
            }
            if (dt.Rows.Count == 0)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(dt);
        }

        [FunctionName("DeleteTask")]
        public static IActionResult DeleteTask(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "task/{id}")] HttpRequest req, ILogger log, int id)
        {
            log.LogInformation("Deleting a new task list item.");
            try
            {
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    connection.Open();
                    var query = @"Delete from TaskList Where Id = @Id";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                return new BadRequestResult();
            }
            return new OkResult();
        }

        [FunctionName("UpdateTask")]
        public static async Task<IActionResult> UpdateTask(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "task/{id}")] HttpRequest req, ILogger log, int id)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<UpdateTaskModel>(requestBody);
            try
            {
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    connection.Open();
                    var query = @"Update TaskList Set Description = @Description , IsDone = @IsDone Where Id = @Id";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Description", input.Description);
                    command.Parameters.AddWithValue("@IsDone", input.IsDone);
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                return new BadRequestResult();
            }
            return new OkResult();
        }
    }
}
