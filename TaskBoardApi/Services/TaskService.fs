namespace TaskBoardApi.Services

open System
open TaskBoardApi.Models
open TaskBoardApi.Storage

/// Business logic service for task operations
type TaskService(store: InMemoryTaskStore) =
    
    /// Get all tasks with filtering, sorting, and pagination
    member _.GetTasks(options: FilterOptions) =
        store.FilterSortAndPaginate(options)
    
    /// Get a task by ID
    member _.GetTaskById(id: Guid) =
        store.GetById(id)
    
    /// Create a new task
    member _.CreateTask(dto: CreateTaskDto) =
        let now = DateTime.UtcNow
        let task =
            { Id = Guid.NewGuid()
              Title = dto.Title
              Description = dto.Description
              Status = dto.Status |> Option.defaultValue NotStarted
              Tags = dto.Tags |> Option.defaultValue []
              DueDate = dto.DueDate
              CreatedAt = now
              UpdatedAt = now }
        store.Add(task)
    
    /// Update an existing task
    member _.UpdateTask(id: Guid, dto: UpdateTaskDto) =
        match store.GetById(id) with
        | Some existingTask ->
            let updatedTask =
                { existingTask with
                    Title = dto.Title |> Option.defaultValue existingTask.Title
                    Description = dto.Description |> Option.orElse existingTask.Description
                    Status = dto.Status |> Option.defaultValue existingTask.Status
                    Tags = dto.Tags |> Option.defaultValue existingTask.Tags
                    DueDate = dto.DueDate |> Option.orElse existingTask.DueDate
                    UpdatedAt = DateTime.UtcNow }
            store.Update(updatedTask)
        | None -> None
    
    /// Delete a task
    member _.DeleteTask(id: Guid) =
        store.Delete(id)
    
    /// Get all unique tags
    member _.GetAllTags() =
        store.GetAllTags()
    
    /// Export tasks to CSV format
    member _.ExportToCsv() =
        let tasks = store.GetAll()
        let sb = System.Text.StringBuilder()
        
        // CSV Header
        sb.AppendLine("Id,Title,Description,Status,Tags,DueDate,CreatedAt,UpdatedAt") |> ignore
        
        // CSV Rows
        for task in tasks do
            let escapeCsv (value: string) =
                if value.Contains(",") || value.Contains("\"") || value.Contains("\n") then
                    sprintf "\"%s\"" (value.Replace("\"", "\"\""))
                else
                    value
            
            let description = task.Description |> Option.defaultValue ""
            let tags = String.Join(";", task.Tags)
            let dueDate = task.DueDate |> Option.map (fun d -> d.ToString("yyyy-MM-dd")) |> Option.defaultValue ""
            let status =
                match task.Status with
                | NotStarted -> "NotStarted"
                | InProgress -> "InProgress"
                | Completed -> "Completed"
                | OnHold -> "OnHold"
            
            sb.AppendLine(sprintf "%s,%s,%s,%s,%s,%s,%s,%s"
                (task.Id.ToString())
                (escapeCsv task.Title)
                (escapeCsv description)
                status
                (escapeCsv tags)
                dueDate
                (task.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                (task.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"))) |> ignore
        
        sb.ToString()
