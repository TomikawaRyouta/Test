namespace TaskBoardApi.Services

open System
open System.IO
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open TaskBoardApi.Models
open TaskBoardApi.Storage

/// JSON serialization converter for TaskStatus
type TaskStatusConverter() =
    inherit JsonConverter<TaskStatus>()
    
    override _.Read(reader: byref<Utf8JsonReader>, typeToConvert: Type, options: JsonSerializerOptions) =
        let value = reader.GetString()
        match value with
        | "NotStarted" -> NotStarted
        | "InProgress" -> InProgress
        | "Completed" -> Completed
        | "OnHold" -> OnHold
        | _ -> NotStarted
    
    override _.Write(writer: Utf8JsonWriter, value: TaskStatus, options: JsonSerializerOptions) =
        let str =
            match value with
            | NotStarted -> "NotStarted"
            | InProgress -> "InProgress"
            | Completed -> "Completed"
            | OnHold -> "OnHold"
        writer.WriteStringValue(str)

/// Service for JSON persistence of tasks
type JsonPersistenceService(store: InMemoryTaskStore, logger: ILogger<JsonPersistenceService>) =
    let filePath = "./data/tasks.json"
    let mutable timer: Timer option = None
    
    let jsonOptions =
        let options = JsonSerializerOptions()
        options.WriteIndented <- true
        options.Converters.Add(JsonStringEnumConverter())
        options.Converters.Add(TaskStatusConverter())
        options
    
    /// Ensure the data directory exists
    let ensureDirectory() =
        let dir = Path.GetDirectoryName(filePath)
        if not (Directory.Exists(dir)) then
            Directory.CreateDirectory(dir) |> ignore
    
    /// Save tasks to JSON file
    member _.SaveToFile() =
        try
            ensureDirectory()
            let tasks = store.GetAll()
            let json = JsonSerializer.Serialize(tasks, jsonOptions)
            // Write without BOM
            File.WriteAllText(filePath, json, UTF8Encoding(false))
            logger.LogInformation("Saved {Count} tasks to {FilePath}", tasks.Length, filePath)
        with ex ->
            logger.LogError(ex, "Error saving tasks to file")
    
    /// Load tasks from JSON file
    member _.LoadFromFile() =
        try
            if File.Exists(filePath) then
                let json = File.ReadAllText(filePath, UTF8Encoding(false))
                let tasks = JsonSerializer.Deserialize<TaskItem list>(json, jsonOptions)
                store.LoadTasks(tasks)
                logger.LogInformation("Loaded {Count} tasks from {FilePath}", tasks.Length, filePath)
            else
                logger.LogInformation("No existing tasks file found at {FilePath}", filePath)
        with ex ->
            logger.LogError(ex, "Error loading tasks from file")
    
    /// Start periodic save timer (every 1 minute)
    member this.StartPeriodicSave() =
        let callback = TimerCallback(fun _ ->
            this.SaveToFile()
        )
        timer <- Some (new Timer(callback, null, TimeSpan.FromMinutes(1.0), TimeSpan.FromMinutes(1.0)))
        logger.LogInformation("Started periodic save timer (every 1 minute)")
    
    /// Stop the periodic save timer
    member _.StopPeriodicSave() =
        match timer with
        | Some t ->
            t.Dispose()
            timer <- None
            logger.LogInformation("Stopped periodic save timer")
        | None -> ()
    
    interface IHostedService with
        member this.StartAsync(cancellationToken: CancellationToken) =
            logger.LogInformation("JsonPersistenceService starting...")
            this.LoadFromFile()
            this.StartPeriodicSave()
            Task.CompletedTask
        
        member this.StopAsync(cancellationToken: CancellationToken) =
            logger.LogInformation("JsonPersistenceService stopping...")
            this.StopPeriodicSave()
            this.SaveToFile()
            Task.CompletedTask
