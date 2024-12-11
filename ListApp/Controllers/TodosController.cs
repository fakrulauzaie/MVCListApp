using ListApp.Data;
using ListApp.Models;
using ListApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace ListApp.Controllers
{
    public class TodosController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Serilog.ILogger _logger;

        public TodosController(ApplicationDbContext dbContext, Serilog.ILogger logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger.ForContext<TodosController>();
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddTodoViewModel viewModel)
        {
            try
            {
                _logger.Information("Adding a new Todo item. Title: {Title}, DueDate: {DueDate}",
                    viewModel.Title, viewModel.DueDate);

                var todo = new Todo
                {
                    Title = viewModel.Title,
                    Description = viewModel.Description,
                    IsCompleted = viewModel.IsCompleted,
                    CreatedDate = DateTime.Now,
                    DueDate = viewModel.DueDate,
                };

                await _dbContext.Todos.AddAsync(todo);
                await _dbContext.SaveChangesAsync();

                // Log success
                _logger.Information("Successfully added Todo with ID {TodoId}, Title: {Title}, CreatedDate: {CreatedDate}",
                    todo.Id, todo.Title, todo.CreatedDate);

                return RedirectToAction("List", "Todos");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while adding a new Todo item. Title: {Title}, DueDate: {DueDate}",
                    viewModel.Title, viewModel.DueDate);
                return StatusCode(500, "An error occurred while processing your request. Please try again.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> List(string searchQuery = "")
        {
            var todos = await _dbContext.Todos
                .Where(t => !t.IsDeleted && (string.IsNullOrEmpty(searchQuery) || t.Title.Contains(searchQuery)))
                .ToListAsync();
            return View(todos);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var todo = await _dbContext.Todos.FindAsync(id);

            return View(todo);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Todo viewModel)
        {

            try
            {
                var todo = await _dbContext.Todos.FindAsync(viewModel.Id);

                if (todo is null)
                {
                    _logger.Warning("Edit operation failed: Todo with ID {TodoId} not found.", viewModel.Id);
                    return NotFound();
                }

                _logger.Information("Editing Todo with ID {TodoId}. Original values: {OriginalTodo}.",
                    todo.Id,
                    new { todo.Title, todo.Description, todo.IsCompleted, todo.DueDate });

                // Update fields
                todo.Title = viewModel.Title;
                todo.Description = viewModel.Description;
                todo.IsCompleted = viewModel.IsCompleted;
                todo.DueDate = viewModel.DueDate;

                await _dbContext.SaveChangesAsync();

                _logger.Information("Successfully updated Todo with ID {TodoId}. Updated values: {UpdatedTodo}.",
                    todo.Id,
                    new { todo.Title, todo.Description, todo.IsCompleted, todo.DueDate });

                return RedirectToAction("List", "Todos");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while editing Todo with ID {TodoId}.", viewModel.Id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SoftDelete(Todo viewModel)
        {

            try
            {
                var todo = await _dbContext.Todos.FindAsync(viewModel.Id);
                if (todo == null)
                {
                    _logger.Warning("Delete operation failed: Todo with ID {TodoId} not found.", viewModel.Id);
                    return NotFound();
                }

                _logger.Information("Deleting Todo with ID {TodoId}, Title: {Title}",
                    todo.Id, todo.Title);

                todo.IsDeleted = true;
                _dbContext.Update(todo);
                await _dbContext.SaveChangesAsync();

                // Log success
                _logger.Information("Successfully deleted Todo with ID {TodoId}, Title: {Title}",
                    todo.Id, todo.Title);

                return RedirectToAction("List", "Todos");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while deleting Todo with ID {TodoId}.", viewModel.Id);
                return StatusCode(500, "An error occurred while processing your request.");
            }


        }

    }
}

