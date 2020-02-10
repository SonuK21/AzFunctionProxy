using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;
using Microsoft.WindowsAzure.Storage;

namespace AzFunctionProxy
{
    public static class TodoApi
    {
        [FunctionName("CreateTodo")]
        public static async Task<IActionResult> CreateTodo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todo")] HttpRequest req,
            [Table("todosample", Connection = "AzureWebJobsStorage")] IAsyncCollector<TodoEntity> todoEntity,
            [Blob("todo-sample-container", FileAccess.Write, Connection = "AzureWebJobsStorage")] CloudBlobContainer cloudBlobContainer,
            ILogger log) {

            log.LogInformation("Add new todo in storage table");
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var addTodo = JsonConvert.DeserializeObject<Todo>(requestBody);

            await todoEntity.AddAsync(new TodoEntity() {
                PartitionKey = "TODO",
                RowKey = addTodo.Id,
                Description = addTodo.TaskDescription,
                CreatedDate = addTodo.CreatedDate,
                IsCompleted = addTodo.IsCompleted
            });

            await cloudBlobContainer.CreateIfNotExistsAsync();
            var blockBlob = cloudBlobContainer.GetBlockBlobReference(addTodo.Id);
            await blockBlob.UploadTextAsync($"Created a new task: {addTodo.TaskDescription} at: {addTodo.CreatedDate}");

            return new OkObjectResult(addTodo);
        }

        [FunctionName("Todos")]
        public static async Task<IActionResult> Todos([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todo")] HttpRequest httpRequest,
            [Table("todosample", Connection = "AzureWebJobsStorage")] CloudTable cloudTable,
            ILogger logger) {
            logger.LogInformation("Get all todos");
            var tableQuery = new TableQuery<TodoEntity>();
            var segment = await cloudTable.ExecuteQuerySegmentedAsync(tableQuery, null);
            return new OkObjectResult(segment.Select(x => new Todo() {
                CreatedDate = x.CreatedDate,
                TaskDescription = x.Description,
                Id = x.RowKey,
                IsCompleted = x.IsCompleted
            }));
        }

        [FunctionName("DeleteTodo")]
        public static async Task<IActionResult> DeleteTodo([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todo/{id}")] HttpRequest httpRequest,
            [Table("todosample", Connection = "AzureWebJobsStorage")] CloudTable cloudTable,
            string id,
            ILogger logger) {
            TableOperation tableOperation = TableOperation.Delete(new TodoEntity() {
                PartitionKey = "TODO",
                RowKey = id,
                ETag = "*"
            });

            try {
                await cloudTable.ExecuteAsync(tableOperation);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == (int)System.Net.HttpStatusCode.NotFound) {
                return new NotFoundResult();
            }
            return new OkResult();
        }
    }
}