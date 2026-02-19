namespace TaskBoardApi.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open TaskBoardApi.Services

/// Controller for tag operations
[<ApiController>]
[<Route("api/[controller]")>]
type TagController(taskService: TaskService, logger: ILogger<TagController>) =
    inherit ControllerBase()
    
    /// Get all unique tags across all tasks
    [<HttpGet>]
    member this.GetAllTags() =
        logger.LogInformation("Getting all tags")
        let tags = taskService.GetAllTags()
        this.Ok(tags) :> IActionResult
