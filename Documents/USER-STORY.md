# US-001 — Personal Task Management

**Priority:** High  
**Methodology:** Behavior-Driven Development (BDD)  
**Application:** TaskBoard

## User Story

**As a** registered user,  
**I want to** securely create, organize, update, and track my personal tasks through a Kanban board,  
**So that** I can efficiently manage my responsibilities, monitor progress, and identify overdue tasks.

---

## 1. Domain Model

### User Entity

| Property | Type | Rules |
|---|---|---|
| Id | Guid | Required, Primary Key, server-generated |
| Name | string | Required, cannot be empty |
| Email | string | Required, valid email, unique |
| TimeZone | string | Required, valid IANA timezone identifier |
| PasswordHash | string | Required, securely hashed on the backend |
| Tasks | Collection of Task | Zero or many tasks |

The frontend must collect a password during registration. The backend generates the password hash before storing the user. Neither the password nor the hash should appear in API responses.

### Task Entity

| Property | Type | Rules |
|---|---|---|
| Id | Guid | Required, Primary Key, server-generated |
| Title | string | Required, maximum 50 characters |
| Description | string? | Optional, maximum 400 characters |
| Status | TaskStatus | Required, defaults to To Do |
| DueDate | DateTime | Required, stored in UTC |
| UserId | Guid | Required, Foreign Key to User |

### Relationships

- One User can have zero or many Tasks.
- Every Task must belong to exactly one User.
- A Task cannot exist without an associated User.
- Users can only access their own tasks.

### Task Status

The Kanban board must contain exactly three columns:

1. **To Do**
2. **In Progress**
3. **Done**

A newly created task must default to **To Do**.

---

## 2. Business Rules

**BR-001 — Authentication:** Users must authenticate before accessing the Kanban board or task operations.

**BR-002 — Ownership:** Users can only retrieve, modify, and delete tasks they own.

**BR-003 — Validation:** Required fields and maximum lengths must be validated by both Angular and the backend.

**BR-004 — Task status:** Tasks must have one of three allowed statuses: To Do, In Progress, or Done.

**BR-005 — Default status:** Newly created tasks must start with To Do unless a valid status is explicitly provided by an authorized operation.

**BR-006 — Overdue detection:** A task is overdue when its due date/time is earlier than the current instant in UTC and its status is not Done.

**BR-007 — Overdue presentation:** Overdue tasks must appear at the top of their respective Kanban columns and display a red pill containing "Overdue".

**BR-008 — Timezone handling:** Task due dates must be stored in UTC and displayed in the authenticated user's configured timezone.

**BR-009 — Password security:** Passwords must never be stored in plaintext or returned through the API.

**BR-010 — Data persistence:** Users and tasks must persist in the SQLite database.

---

# 3. Acceptance Criteria — BDD Scenarios

## Feature: User Registration and Authentication

### Scenario 1 — Register a New User

```gherkin
Given I am an unregistered visitor
When I provide a valid name, email, timezone and password
And I submit the registration form
Then my user account should be created
And a unique User Id should be generated
And my password should be securely hashed
And my user information should be persisted
```

### Scenario 2 — Required Registration Field Validation

```gherkin
Scenario Outline: Validate required registration fields
  Given I am on the registration page
  When I leave the "<field>" field empty
  And I submit the form
  Then the "<field>" field should be highlighted in red
  And a required field error message should be displayed
  And the registration should not be submitted

Examples:
  | field    |
  | Name     |
  | Email    |
  | TimeZone |
  | Password |
```

### Scenario 3 — Register With an Existing Email

```gherkin
Given a user already exists with email "user@example.com"
When I attempt to register using the same email
Then the API should return HTTP 409 Conflict
And the registration should be rejected
And an appropriate error message should be displayed
```

### Scenario 4 — User Login

```gherkin
Given I am a registered user
When I provide valid credentials
Then I should be authenticated
And I should receive an authentication token
And I should be redirected to my Kanban board
```

### Scenario 5 — Invalid Login Credentials

```gherkin
Given I am on the login page
When I provide invalid credentials
Then authentication should fail
And an appropriate error message should be displayed
And I should remain unauthenticated
```

---

## Feature: Kanban Board

### Scenario 6 — Display Kanban Columns

```gherkin
Given I am an authenticated user
When I navigate to my Kanban board
Then I should see exactly three columns
And the columns should be named:
  | To Do       |
  | In Progress |
  | Done        |
And my tasks should appear in their respective columns
```

### Scenario 7 — Display an Empty Kanban Board

```gherkin
Given I am an authenticated user
And I have no tasks
When I navigate to my Kanban board
Then all three Kanban columns should be displayed
And the board should show an appropriate empty state
And I should be able to create my first task
```

### Scenario 8 — Create a Task With Default Status

```gherkin
Given I am an authenticated user
And I am viewing my Kanban board
When I create a task with:
  | Title       | Prepare technical interview |
  | Description | Review Clean Architecture   |
  | Due Date    | A valid future date/time    |
And I save the task
Then a unique Task Id should be generated
And the task should be associated with my User Id
And the task status should default to "To Do"
And the task should be persisted in SQLite
And the task should appear in the "To Do" column
```

### Scenario 9 — Create a Task Without a Description

```gherkin
Given I am an authenticated user
When I create a task with a valid title and due date
And I leave the description empty
Then the task should be successfully created
And the description should remain empty
And the task should appear in the "To Do" column
```

---

## Feature: Form Validation

### Scenario 10 — Validate Required Task Fields

```gherkin
Scenario Outline: Validate required task fields
  Given I am authenticated
  And I am on the Create Task form
  When I leave the "<field>" field empty
  And I submit the form
  Then the "<field>" field should be highlighted in red
  And the error message "<message>" should be displayed
  And the task should not be created

Examples:
  | field    | message              |
  | Title    | Title is required    |
  | Due Date | Due date is required |
```

### Scenario 11 — Validate Maximum Field Lengths

```gherkin
Scenario Outline: Validate task field character limits
  Given I am authenticated
  And I am creating or editing a task
  When I enter more than "<max>" characters in "<field>"
  Then the field should be highlighted in red
  And an appropriate validation message should be displayed
  And the form should not be submitted

Examples:
  | field       | max |
  | Title       | 50  |
  | Description | 400 |
```

### Scenario 12 — Validate Fields on the Backend

```gherkin
Given I am an authenticated user
When I send an API request with invalid task data
Then the API should return HTTP 400 Bad Request
And the response should contain validation details
And invalid data should not be persisted
```

Validation must also happen on the backend, regardless of Angular form validation.

---

## Feature: Task Management

### Scenario 13 — Edit an Existing Task

```gherkin
Given I am authenticated
And I own an existing task
When I modify the title, description or due date
And I save the changes
Then the task should be updated
And the updated information should be persisted
And the Kanban board should reflect the changes
```

### Scenario 14 — Change Task Status Using Drag and Drop

```gherkin
Given I am authenticated
And I have a task in the "To Do" column
When I drag the task to the "In Progress" column
Then its status should be updated to "In Progress"
And the change should be persisted through the API
And the task should appear in the "In Progress" column
```

### Scenario 15 — Persist Status After Refresh

```gherkin
Given I am authenticated
And I previously moved a task to the "Done" column
When I refresh the browser
Then the task should remain in the "Done" column
And its persisted status should be "Done"
```

### Scenario 16 — Delete a Task

```gherkin
Given I am authenticated
And I own an existing task
When I delete the task
Then the task should be removed from the database
And the task should disappear from the Kanban board
```

---

## Feature: Overdue Task Management

### Scenario 17 — Highlight Overdue Tasks

```gherkin
Given I am authenticated
And I have a task with status "To Do"
And its due date/time is earlier than the current time
When I view my Kanban board
Then the task should display a red pill labeled "Overdue"
And the task should appear at the top of the "To Do" column
```

### Scenario 18 — Prioritize Multiple Overdue Tasks

```gherkin
Given I am authenticated
And I have multiple overdue tasks in "In Progress"
And I have other tasks that are not overdue
When I view my Kanban board
Then all overdue tasks should appear before non-overdue tasks
And each overdue task should display a red "Overdue" pill
And overdue tasks should be ordered by their due date,
    with the earliest due date first
```

### Scenario 19 — Completed Tasks Are Not Overdue

```gherkin
Given I am authenticated
And I have a task with a due date in the past
And the task status is "Done"
When I view my Kanban board
Then the task should not display an "Overdue" pill
And it should remain in the "Done" column
```

### Scenario 20 — Moving an Overdue Task to Done

```gherkin
Given I am authenticated
And I have an overdue task in "In Progress"
When I drag the task into the "Done" column
Then its status should be persisted as "Done"
And the "Overdue" pill should disappear
And the task should no longer be considered overdue
```

---

## Feature: Timezone Handling

### Scenario 21 — Store Task Due Date in UTC

```gherkin
Given I am authenticated
And my configured timezone is "America/Sao_Paulo"
When I create a task with a local due date/time
  of "2026-10-21 15:00"
Then the backend should convert the due date/time to UTC
And the stored value should be "2026-10-21T18:00:00Z"
```

### Scenario 22 — Display Task Due Date in User Timezone

```gherkin
Given I am authenticated
And my configured timezone is "America/Sao_Paulo"
And a task has a UTC due date of "2026-10-21T18:00:00Z"
When I view the task on my Kanban board
Then its due date/time should be displayed as
  "2026-10-21 15:00"
And the original UTC value should remain unchanged
  in the database
```

### Scenario 23 — Overdue Detection Across Timezones

```gherkin
Given a task has a UTC due date of "2026-10-21T18:00:00Z"
And the current UTC time is "2026-10-21T18:01:00Z"
And the task status is "To Do"
When I view the task from any supported timezone
Then the task should be considered overdue
And the task should display the red "Overdue" pill
```

---

## Feature: Task Ownership and Authorization

### Scenario 24 — Prevent Access to Another User's Tasks

```gherkin
Given I am authenticated as User A
And User B owns an existing task
When I attempt to retrieve, update or delete User B's task
Then the API should not disclose the task information
And the task should remain unchanged
And the API should return HTTP 404 Not Found
```

### Scenario 25 — Reject Unauthenticated Requests

```gherkin
Given I am not authenticated
When I request a protected task endpoint
Then the API should return HTTP 401 Unauthorized
And no protected task information should be returned
```

---

# 4. Definition of Done

- [ ] Users can register and authenticate.
- [ ] Passwords are securely hashed and never exposed.
- [ ] User and Task relationships are correctly implemented.
- [ ] All required field validations work in Angular and .NET.
- [ ] Title and Description character limits are enforced.
- [ ] Tasks default to To Do.
- [ ] Kanban columns support drag-and-drop.
- [ ] Overdue tasks are displayed first with a red pill.
- [ ] Completed tasks are excluded from overdue detection.
- [ ] Due dates are stored in UTC and displayed in the user's timezone.
- [ ] Users can only access their own tasks.
- [ ] Data persists in SQLite.
- [ ] Backend development follows TDD using xUnit.
- [ ] Unit and integration tests cover critical business behaviors.
- [ ] The application is responsive on desktop and mobile.
- [ ] The README contains setup instructions and demo credentials.
