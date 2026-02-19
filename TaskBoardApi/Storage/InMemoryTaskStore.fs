namespace TaskBoardApi.Storage

open System
open System.Collections.Generic
open TaskBoardApi.Models

/// In-memory storage for tasks
type InMemoryTaskStore() =
    let mutable tasks = Dictionary<Guid, TaskItem>()
    let lockObj = obj()

    /// Get all tasks
    member _.GetAll() =
        lock lockObj (fun () ->
            tasks.Values |> Seq.toList
        )

    /// Get a task by ID
    member _.GetById(id: Guid) =
        lock lockObj (fun () ->
            match tasks.TryGetValue(id) with
            | true, task -> Some task
            | false, _ -> None
        )

    /// Add a new task
    member _.Add(task: TaskItem) =
        lock lockObj (fun () ->
            tasks.[task.Id] <- task
            task
        )

    /// Update an existing task
    member _.Update(task: TaskItem) =
        lock lockObj (fun () ->
            if tasks.ContainsKey(task.Id) then
                tasks.[task.Id] <- task
                Some task
            else
                None
        )

    /// Delete a task by ID
    member _.Delete(id: Guid) =
        lock lockObj (fun () ->
            tasks.Remove(id)
        )

    /// Clear all tasks (used for loading from JSON)
    member _.Clear() =
        lock lockObj (fun () ->
            tasks.Clear()
        )

    /// Load tasks from a list (used for JSON restoration)
    member _.LoadTasks(taskList: TaskItem list) =
        lock lockObj (fun () ->
            tasks.Clear()
            for task in taskList do
                tasks.[task.Id] <- task
        )

    /// Get all unique tags across all tasks
    member _.GetAllTags() =
        lock lockObj (fun () ->
            tasks.Values
            |> Seq.collect (fun t -> t.Tags)
            |> Seq.distinct
            |> Seq.sort
            |> Seq.toList
        )

    /// Filter, sort, and paginate tasks
    member _.FilterSortAndPaginate(options: FilterOptions) =
        lock lockObj (fun () ->
            let filtered =
                tasks.Values
                |> Seq.toList
                // Filter by status
                |> fun items ->
                    match options.Status with
                    | Some status ->
                        let statusEnum =
                            match status.ToLower() with
                            | "notstarted" -> Some NotStarted
                            | "inprogress" -> Some InProgress
                            | "completed" -> Some Completed
                            | "onhold" -> Some OnHold
                            | _ -> None
                        match statusEnum with
                        | Some s -> items |> List.filter (fun t -> t.Status = s)
                        | None -> items
                    | None -> items
                // Filter by tag
                |> fun items ->
                    match options.Tag with
                    | Some tag -> items |> List.filter (fun t -> t.Tags |> List.exists (fun tg -> tg.ToLower() = tag.ToLower()))
                    | None -> items
                // Filter by keyword (search in title and description)
                |> fun items ->
                    match options.Keyword with
                    | Some keyword ->
                        let kw = keyword.ToLower()
                        items |> List.filter (fun t ->
                            t.Title.ToLower().Contains(kw) ||
                            (t.Description |> Option.exists (fun d -> d.ToLower().Contains(kw))))
                    | None -> items
                // Filter by due date range
                |> fun items ->
                    match options.DueDateFrom, options.DueDateTo with
                    | Some fromDate, Some toDate ->
                        items |> List.filter (fun t ->
                            match t.DueDate with
                            | Some dd -> dd >= fromDate && dd <= toDate
                            | None -> false)
                    | Some fromDate, None ->
                        items |> List.filter (fun t ->
                            match t.DueDate with
                            | Some dd -> dd >= fromDate
                            | None -> false)
                    | None, Some toDate ->
                        items |> List.filter (fun t ->
                            match t.DueDate with
                            | Some dd -> dd <= toDate
                            | None -> false)
                    | None, None -> items

            // Sort
            let sorted =
                match options.SortBy with
                | Some "Title" | Some "title" ->
                    if options.SortDescending then
                        filtered |> List.sortByDescending (fun t -> t.Title)
                    else
                        filtered |> List.sortBy (fun t -> t.Title)
                | Some "Status" | Some "status" ->
                    if options.SortDescending then
                        filtered |> List.sortByDescending (fun t -> t.Status)
                    else
                        filtered |> List.sortBy (fun t -> t.Status)
                | Some "DueDate" | Some "duedate" ->
                    if options.SortDescending then
                        filtered |> List.sortByDescending (fun t -> t.DueDate |> Option.defaultValue DateTime.MaxValue)
                    else
                        filtered |> List.sortBy (fun t -> t.DueDate |> Option.defaultValue DateTime.MaxValue)
                | Some "UpdatedAt" | Some "updatedat" ->
                    if options.SortDescending then
                        filtered |> List.sortByDescending (fun t -> t.UpdatedAt)
                    else
                        filtered |> List.sortBy (fun t -> t.UpdatedAt)
                | _ -> // Default to CreatedAt
                    if options.SortDescending then
                        filtered |> List.sortByDescending (fun t -> t.CreatedAt)
                    else
                        filtered |> List.sortBy (fun t -> t.CreatedAt)

            let totalCount = sorted.Length
            let totalPages = int (Math.Ceiling(float totalCount / float options.PageSize))

            // Paginate
            let paged =
                sorted
                |> List.skip ((options.Page - 1) * options.PageSize)
                |> List.truncate options.PageSize

            { Items = paged
              TotalCount = totalCount
              Page = options.Page
              PageSize = options.PageSize
              TotalPages = totalPages }
        )
