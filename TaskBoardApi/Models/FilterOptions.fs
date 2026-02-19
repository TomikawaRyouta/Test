namespace TaskBoardApi.Models

open System

/// Represents filter options for task queries
type FilterOptions =
    { Status: string option
      Tag: string option
      Keyword: string option
      DueDateFrom: DateTime option
      DueDateTo: DateTime option
      SortBy: string option
      SortDescending: bool
      Page: int
      PageSize: int }

    static member Default =
        { Status = None
          Tag = None
          Keyword = None
          DueDateFrom = None
          DueDateTo = None
          SortBy = Some "CreatedAt"
          SortDescending = false
          Page = 1
          PageSize = 10 }

/// Represents a paginated result
type PaginatedResult<'T> =
    { Items: 'T list
      TotalCount: int
      Page: int
      PageSize: int
      TotalPages: int }
