namespace TaskBoardApi.Models

open System

/// Represents the status of a task
type TaskStatus =
    | NotStarted
    | InProgress
    | Completed
    | OnHold

/// Represents a task item
type TaskItem =
    { Id: Guid
      Title: string
      Description: string option
      Status: TaskStatus
      Tags: string list
      DueDate: DateTime option
      CreatedAt: DateTime
      UpdatedAt: DateTime }

/// DTO for creating a new task
type CreateTaskDto =
    { Title: string
      Description: string option
      Status: TaskStatus option
      Tags: string list option
      DueDate: DateTime option }

/// DTO for updating a task
type UpdateTaskDto =
    { Title: string option
      Description: string option
      Status: TaskStatus option
      Tags: string list option
      DueDate: DateTime option }
