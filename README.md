# Todo List MVC Application

An ASP.NET MVC application for managing task lists with full CRUD operations. Uses Serilog and SQLite-backed audit logging system. 

---

## Features
### CRUD Operations
- Add, view, update, and delete Todo items, including filtering.

### SQLite Integration 
  - Primary database for managing Todos.
  - Logging database for storing application logs.
  
### Serilog Logging
  - Logs stored in `logs.db` file, `audit-log-.txt` file and also written in console in runtime.
  - Configured to write logs with levels and timestamps.
  - All crucial operations are logged and stored for auditing.
  - Logs can also be accessed via the `Logs` page in the application, including filtering.
---


