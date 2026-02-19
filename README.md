# Task Board API

A RESTful API for task management built with **F#** and **ASP.NET Core 8.0**, featuring in-memory storage with JSON persistence and Swagger/OpenAPI documentation.

## Features

- ✅ **Full CRUD Operations** - Create, Read, Update, and Delete tasks
- 🔍 **Advanced Filtering** - Filter by status, tags, keywords, and due date ranges
- 📊 **Sorting & Pagination** - Customizable sorting and paginated results
- 🏷️ **Tag Management** - Automatic tag extraction and listing
- 💾 **JSON Persistence** - Automatic save/load with UTF-8 encoding (no BOM)
- 📤 **CSV Export** - Export tasks to CSV format
- 📖 **Swagger UI** - Interactive API documentation

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Language**: F#
- **Documentation**: Swagger/OpenAPI (Swashbuckle)
- **Persistence**: JSON file storage (UTF-8, no BOM)
- **Storage**: In-memory with periodic snapshots

## Quick Start

### Prerequisites

- .NET 8.0 SDK or later

### Running the Application

```bash
cd TaskBoardApi
dotnet run
```

The API will start at `https://localhost:5001` (or `http://localhost:5000`)

Access Swagger UI at: `https://localhost:5001/swagger`

## API Endpoints

### Tasks

#### Get All Tasks (with filtering, sorting, and pagination)

```http
GET /api/task?status={status}&tag={tag}&keyword={keyword}&dueDateFrom={date}&dueDateTo={date}&sortBy={field}&sortDescending={bool}&page={number}&pageSize={size}
```

**Query Parameters:**
- `status` - Filter by task status: `NotStarted`, `InProgress`, `Completed`, `OnHold`
- `tag` - Filter by tag name (case-insensitive)
- `keyword` - Search in title and description (case-insensitive)
- `dueDateFrom` - Filter tasks with due date from this date
- `dueDateTo` - Filter tasks with due date until this date
- `sortBy` - Sort field: `Title`, `Status`, `DueDate`, `CreatedAt` (default), `UpdatedAt`
- `sortDescending` - Sort in descending order (default: false)
- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 10)

**Example:**
```bash
curl "http://localhost:5000/api/task?status=InProgress&page=1&pageSize=10"
```

**Response:**
```json
{
  "items": [ /* array of tasks */ ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

#### Get Task by ID

```http
GET /api/task/{id}
```

**Example:**
```bash
curl "http://localhost:5000/api/task/d6ecf9d3-4aea-48c2-8f60-14db9cecf609"
```

#### Create Task

```http
POST /api/task
Content-Type: application/json
```

**Request Body:**
```json
{
  "title": "Implement authentication",
  "description": "Add JWT authentication to the API",
  "status": "NotStarted",
  "tags": ["backend", "security"],
  "dueDate": "2026-03-15T00:00:00Z"
}
```

**Note:** All fields except `title` are optional. Default status is `NotStarted`.

#### Update Task

```http
PUT /api/task/{id}
Content-Type: application/json
```

**Request Body:**
```json
{
  "title": "Implement authentication (updated)",
  "status": "InProgress"
}
```

**Note:** All fields are optional. Only provided fields will be updated.

#### Delete Task

```http
DELETE /api/task/{id}
```

**Example:**
```bash
curl -X DELETE "http://localhost:5000/api/task/{id}"
```

#### Export Tasks to CSV

```http
GET /api/task/export/csv
```

**Example:**
```bash
curl "http://localhost:5000/api/task/export/csv" > tasks.csv
```

### Tags

#### Get All Tags

```http
GET /api/tag
```

Returns a sorted list of all unique tags used across all tasks.

**Example:**
```bash
curl "http://localhost:5000/api/tag"
```

**Response:**
```json
["api", "backend", "frontend", "security", "testing"]
```

## Data Model

### TaskItem

```json
{
  "id": "guid",
  "title": "string (required)",
  "description": "string (optional)",
  "status": "NotStarted | InProgress | Completed | OnHold",
  "tags": ["string"],
  "dueDate": "datetime (optional)",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### Task Status Values

- `NotStarted` - Task has not been started
- `InProgress` - Task is currently being worked on
- `Completed` - Task has been completed
- `OnHold` - Task is temporarily paused

## Data Persistence

The API uses JSON file persistence with the following behavior:

- **Location**: `./data/tasks.json`
- **Encoding**: UTF-8 without BOM
- **Load on Startup**: Tasks are automatically loaded when the application starts
- **Periodic Save**: Tasks are saved every 1 minute
- **Save on Shutdown**: Tasks are saved when the application stops gracefully

## Project Structure

```
TaskBoardApi/
├── Controllers/
│   ├── TaskController.fs      # Task CRUD and search endpoints
│   └── TagController.fs        # Tag listing endpoint
├── Models/
│   ├── TaskItem.fs            # Task data models and DTOs
│   └── FilterOptions.fs       # Filtering and pagination models
├── Services/
│   ├── TaskService.fs         # Business logic for task operations
│   └── JsonPersistenceService.fs  # JSON save/load and periodic save
├── Storage/
│   └── InMemoryTaskStore.fs   # In-memory task storage with filtering
├── Program.fs                 # Application entry point and configuration
└── appsettings.json          # Application settings
```

## Design Principles

### Separation of Concerns

- **Controllers**: Handle HTTP requests/responses and input validation
- **Services**: Contain business logic and orchestration
- **Storage**: Manage in-memory data storage and queries
- **Models**: Define data structures and DTOs

### Thread Safety

The `InMemoryTaskStore` uses lock-based synchronization to ensure thread-safe operations.

### Dependency Injection

All services are registered with the DI container and injected into controllers:
- `InMemoryTaskStore` - Singleton
- `TaskService` - Singleton
- `JsonPersistenceService` - Hosted Service

### Error Handling

The API returns appropriate HTTP status codes:
- `200 OK` - Successful GET/PUT requests
- `201 Created` - Successful POST requests
- `204 No Content` - Successful DELETE requests
- `400 Bad Request` - Validation errors
- `404 Not Found` - Resource not found

## Examples

### Create and Update a Task Workflow

```bash
# Create a task
TASK_ID=$(curl -X POST http://localhost:5000/api/task \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Write documentation",
    "status": "NotStarted",
    "tags": ["documentation"]
  }' | jq -r '.id')

# Update the task
curl -X PUT http://localhost:5000/api/task/$TASK_ID \
  -H "Content-Type: application/json" \
  -d '{
    "status": "InProgress"
  }'

# Get the task
curl http://localhost:5000/api/task/$TASK_ID
```

### Search and Filter Tasks

```bash
# Get all in-progress tasks
curl "http://localhost:5000/api/task?status=InProgress"

# Search for tasks containing "documentation"
curl "http://localhost:5000/api/task?keyword=documentation"

# Get tasks with specific tag
curl "http://localhost:5000/api/task?tag=backend"

# Get tasks due before a certain date
curl "http://localhost:5000/api/task?dueDateTo=2026-03-01T00:00:00Z"

# Combine filters and sort by title
curl "http://localhost:5000/api/task?status=InProgress&sortBy=Title&sortDescending=false"
```

## Development

### Building

```bash
dotnet build
```

### Running in Development Mode

```bash
dotnet run --environment Development
```

In development mode, Swagger UI is automatically enabled.

## Future Enhancements

Potential improvements for future versions:
- Authentication and authorization
- Database persistence (SQL Server, PostgreSQL, etc.)
- Task assignments and user management
- Task comments and attachments
- Webhooks for task events
- Real-time updates via SignalR

## License

This project is provided as-is for demonstration and educational purposes.
