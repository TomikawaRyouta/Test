namespace TaskBoardApi.Controllers

open System
open System.Text
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open TaskBoardApi.Models
open TaskBoardApi.Services

/// Controller for task operations
[<ApiController>]
[<Route("api/[controller]")>]
type TaskController(taskService: TaskService, logger: ILogger<TaskController>) =
    inherit ControllerBase()
    
    /// Get tasks with optional filtering, sorting, and pagination
    [<HttpGet>]
    member this.GetTasks(
        [<FromQuery>] status: string,
        [<FromQuery>] tag: string,
        [<FromQuery>] keyword: string,
        [<FromQuery>] dueDateFrom: Nullable<DateTime>,
        [<FromQuery>] dueDateTo: Nullable<DateTime>,
        [<FromQuery>] sortBy: string,
        [<FromQuery>] sortDescending: Nullable<bool>,
        [<FromQuery>] page: Nullable<int>,
        [<FromQuery>] pageSize: Nullable<int>) =
        
        let options =
            { Status = if String.IsNullOrWhiteSpace(status) then None else Some status
              Tag = if String.IsNullOrWhiteSpace(tag) then None else Some tag
              Keyword = if String.IsNullOrWhiteSpace(keyword) then None else Some keyword
              DueDateFrom = if dueDateFrom.HasValue then Some dueDateFrom.Value else None
              DueDateTo = if dueDateTo.HasValue then Some dueDateTo.Value else None
              SortBy = if String.IsNullOrWhiteSpace(sortBy) then Some "CreatedAt" else Some sortBy
              SortDescending = if sortDescending.HasValue then sortDescending.Value else false
              Page = if page.HasValue && page.Value > 0 then page.Value else 1
              PageSize = if pageSize.HasValue && pageSize.Value > 0 then pageSize.Value else 10 }
        
        logger.LogInformation("Getting tasks with filters")
        let result = taskService.GetTasks(options)
        this.Ok(result) :> IActionResult
    
    /// Get a specific task by ID
    [<HttpGet("{id}")>]
    member this.GetTaskById(id: Guid) =
        logger.LogInformation("Getting task {TaskId}", id)
        match taskService.GetTaskById(id) with
        | Some task -> this.Ok(task) :> IActionResult
        | None -> this.NotFound() :> IActionResult
    
    /// Create a new task
    [<HttpPost>]
    member this.CreateTask([<FromBody>] dto: CreateTaskDto) =
        logger.LogInformation("Creating new task: {Title}", dto.Title)
        
        if String.IsNullOrWhiteSpace(dto.Title) then
            this.BadRequest("Title is required") :> IActionResult
        else
            let task = taskService.CreateTask(dto)
            this.CreatedAtAction("GetTaskById", {| id = task.Id |}, task) :> IActionResult
    
    /// Update an existing task
    [<HttpPut("{id}")>]
    member this.UpdateTask(id: Guid, [<FromBody>] dto: UpdateTaskDto) =
        logger.LogInformation("Updating task {TaskId}", id)
        match taskService.UpdateTask(id, dto) with
        | Some task -> this.Ok(task) :> IActionResult
        | None -> this.NotFound() :> IActionResult
    
    /// Delete a task
    [<HttpDelete("{id}")>]
    member this.DeleteTask(id: Guid) =
        logger.LogInformation("Deleting task {TaskId}", id)
        if taskService.DeleteTask(id) then
            this.NoContent() :> IActionResult
        else
            this.NotFound() :> IActionResult
    
    /// Export tasks to CSV
    [<HttpGet("export/csv")>]
    member this.ExportToCsv() =
        logger.LogInformation("Exporting tasks to CSV")
        let csv = taskService.ExportToCsv()
        let bytes = (new UTF8Encoding(false)).GetBytes(csv)
        this.File(bytes, "text/csv", "tasks.csv") :> IActionResult
