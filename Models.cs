using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AzFunctionProxy
{
    public class Todo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("n");
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
    public class TodoEntity : TableEntity
    {
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}