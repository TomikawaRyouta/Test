#!/bin/bash
# Task Board API - Manual Test Script

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

API_URL="http://localhost:5000/api"

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}Task Board API - Manual Test Script${NC}"
echo -e "${BLUE}========================================${NC}\n"

# Test 1: Create tasks
echo -e "${GREEN}1. Creating test tasks...${NC}"
TASK1=$(curl -s -X POST ${API_URL}/task \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Implement user authentication",
    "description": "Add JWT authentication to the API",
    "status": "InProgress",
    "tags": ["backend", "security"],
    "dueDate": "2026-03-15T00:00:00Z"
  }' | jq -r '.id')
echo "Created task 1: $TASK1"

TASK2=$(curl -s -X POST ${API_URL}/task \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Design homepage UI",
    "description": "Create mockups for the homepage",
    "status": "NotStarted",
    "tags": ["frontend", "design"],
    "dueDate": "2026-02-28T00:00:00Z"
  }' | jq -r '.id')
echo "Created task 2: $TASK2"

TASK3=$(curl -s -X POST ${API_URL}/task \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Write API documentation",
    "status": "Completed",
    "tags": ["documentation"]
  }' | jq -r '.id')
echo "Created task 3: $TASK3"

echo ""

# Test 2: Get all tasks
echo -e "${GREEN}2. Getting all tasks...${NC}"
curl -s "${API_URL}/task" | jq '{totalCount, items: [.items[] | {title, status}]}'
echo ""

# Test 3: Filter by status
echo -e "${GREEN}3. Filtering tasks by status (InProgress)...${NC}"
curl -s "${API_URL}/task?status=InProgress" | jq '[.items[] | .title]'
echo ""

# Test 4: Filter by tag
echo -e "${GREEN}4. Filtering tasks by tag (backend)...${NC}"
curl -s "${API_URL}/task?tag=backend" | jq '[.items[] | .title]'
echo ""

# Test 5: Search by keyword
echo -e "${GREEN}5. Searching tasks with keyword (API)...${NC}"
curl -s "${API_URL}/task?keyword=API" | jq '[.items[] | .title]'
echo ""

# Test 6: Get all tags
echo -e "${GREEN}6. Getting all tags...${NC}"
curl -s "${API_URL}/tag" | jq .
echo ""

# Test 7: Update a task
echo -e "${GREEN}7. Updating task status...${NC}"
curl -s -X PUT ${API_URL}/task/$TASK1 \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Completed"
  }' | jq '{title, status, updatedAt}'
echo ""

# Test 8: Get specific task
echo -e "${GREEN}8. Getting specific task details...${NC}"
curl -s "${API_URL}/task/$TASK2" | jq '{title, description, status, tags, dueDate}'
echo ""

# Test 9: Sort and paginate
echo -e "${GREEN}9. Getting tasks sorted by title (page 1, size 2)...${NC}"
curl -s "${API_URL}/task?sortBy=Title&page=1&pageSize=2" | jq '{totalCount, page, pageSize, items: [.items[] | .title]}'
echo ""

# Test 10: Export to CSV
echo -e "${GREEN}10. Exporting tasks to CSV...${NC}"
curl -s "${API_URL}/task/export/csv" > /tmp/tasks_export.csv
echo "Saved to /tmp/tasks_export.csv"
head -3 /tmp/tasks_export.csv
echo ""

# Test 11: Delete a task
echo -e "${GREEN}11. Deleting a task...${NC}"
curl -s -X DELETE ${API_URL}/task/$TASK3 -w "\nHTTP Status: %{http_code}\n"
echo ""

# Test 12: Verify deletion
echo -e "${GREEN}12. Verifying deletion (remaining tasks)...${NC}"
curl -s "${API_URL}/task" | jq '{totalCount, items: [.items[] | .title]}'
echo ""

echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}All tests completed!${NC}"
echo -e "${BLUE}========================================${NC}"
